using Dapper;
using Microsoft.Extensions.Logging;
using Sels.Core.Data.SQL.Query.Statement;
using System.Collections;
using System.Reflection;

namespace Sels.Core.Data.SQL.SearchCriteria
{
    /// <inheritdoc cref="ISearchCriteriaConverter{T, TSearchCriteria}"/>
    public class SearchCriteriaConverter<T, TSearchCriteria> : ISearchCriteriaConverter<T, TSearchCriteria>, ISearchCriteriaConverterBuilder<T, TSearchCriteria>
    {
        // Fields
        private readonly List<ExplicitConverter> _explicitConverters = new List<ExplicitConverter>();
        private readonly List<string> _excludedProperties = new List<string>();
        private readonly ILogger? _logger;

        private Func<PropertyInfo, object?>? _aliasSelector;
        private Func<PropertyInfo, string?>? _nameSelector;
        private Func<PropertyInfo, IStatementConditionOperatorExpressionBuilder<T>, IStatementConditionRightExpressionBuilder<T>?>? _operatorSelector;
        private Func<PropertyInfo, object, DynamicParameters?, IStatementConditionRightExpressionBuilder<T>, IChainedBuilder<T, IStatementConditionExpressionBuilder<T>>?>? _valueSelector;

        /// <inheritdoc cref="SearchCriteriaConverter{T, TSearchCriteria}"/>
        /// <param name="configurator">Optional delegate for configuring the current instance</param>
        /// <param name="logger">Optional logger for tracing</param>
        public SearchCriteriaConverter(Action<ISearchCriteriaConverterBuilder<T, TSearchCriteria>>? configurator = null, ILogger? logger = null)
        {
            configurator?.Invoke(this);
            this._logger = logger;
        }

        /// <inheritdoc/>
        public IChainedBuilder<T, IStatementConditionExpressionBuilder<T>>? Build(IStatementConditionExpressionBuilder<T> builder, TSearchCriteria searchCriteria, DynamicParameters? parameters = null)
        {
            using var logger = _logger.TraceMethod(this);
            Guard.IsNotNull(builder);
            Guard.IsNotNull(searchCriteria);

            IChainedBuilder<T, IStatementConditionExpressionBuilder<T>>? lastBuilder = null;

            _logger.Debug($"Converting <{searchCriteria}> to SQL conditions");

            foreach(var property in typeof(TSearchCriteria).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => !_excludedProperties.Contains(x.Name)))
            {
                var value = property.GetValue(searchCriteria);
                _logger.Trace($"Converting property <{property.Name}> with value <{value}>");

                var explicitConverter = _explicitConverters.FirstOrDefault(x => x.Property.AreEqual(property));
                var allowNull = explicitConverter != null ? explicitConverter.AllowNull : false;
                if(!allowNull && value == null)
                {
                    _logger.Debug($"Nulls are not allowed for property <{property.Name}>. Skipping");
                    continue;
                }

                IStatementConditionExpressionBuilder<T> expressionBuilder = lastBuilder != null ? lastBuilder.And : builder;

                // Set object
                var operatorBuilder = (explicitConverter?.As(property, expressionBuilder)) ?? SetObject(property, builder);

                // Set operator
                var valueBuilder = (explicitConverter?.CompareUsing(property, operatorBuilder)) ?? SetOperator(property, operatorBuilder);

                // Set value
                lastBuilder = (explicitConverter?.ValueAs(property, value, parameters, valueBuilder)) ?? SetValue(property, value, valueBuilder, parameters);
                _logger.Debug($"Converted property <{property.Name}> with value <{value}> to SQL");
            }

            return lastBuilder;
        }

        object GetAlias(PropertyInfo property)
        {
            Guard.IsNotNull(property);

            return _aliasSelector?.Invoke(property) ?? property.ReflectedType;
        }

        IStatementConditionOperatorExpressionBuilder<T> SetObject(PropertyInfo property, IStatementConditionExpressionBuilder<T> builder)
        {
            Guard.IsNotNull(property);
            Guard.IsNotNull(builder);

            var name = _nameSelector?.Invoke(property);
            if (!name.HasValue()) name = property.Name;

            return builder.Column(GetAlias(property), name);
        }

        IStatementConditionRightExpressionBuilder<T> SetOperator(PropertyInfo property, IStatementConditionOperatorExpressionBuilder<T> builder)
        {
            Guard.IsNotNull(property);
            Guard.IsNotNull(builder);

            var rightExpressionBuilder = _operatorSelector?.Invoke(property, builder);

            if (rightExpressionBuilder != null) return rightExpressionBuilder;

            if (property.PropertyType.IsString())
            {
                return builder.Like;
            }
            else if (property.PropertyType.IsContainer())
            {
                return builder.In;
            }
            else
            {
                return builder.EqualTo;
            }
        }

        IChainedBuilder<T, IStatementConditionExpressionBuilder<T>> SetValue(PropertyInfo property, object? value, IStatementConditionRightExpressionBuilder<T> builder, DynamicParameters? parameters)
        {
            Guard.IsNotNull(property);
            Guard.IsNotNull(builder);

            var chainedBuilder = _valueSelector?.Invoke(property, value, parameters, builder);
            if (chainedBuilder != null) return chainedBuilder;

            if(property.PropertyType.IsContainer() && !property.PropertyType.IsString())
            {
                if (value == null) return builder.Values(Array.Empty<object>());

                return builder.Parameters(value.CastTo<IEnumerable>().Enumerate().Select((int i, object x) => {
                    var name = $"{property.Name}[{i}]";
                    parameters?.Add(name, x);
                    return name;
                }));
            }
            else
            {
                if (value == null) return builder.Value(property.PropertyType.IsNullable() ? string.Empty : property.PropertyType.GetDefaultValue());
                parameters?.Add(property.Name, value);
                return builder.Parameter(property.Name);
            }
        }

        #region Config
        /// <inheritdoc/>
        public ISearchCriteriaConverterBuilder<T, TSearchCriteria> Excluding(string name, params string[] additionalNames)
        {
            Guard.IsNotNullOrWhitespace(name);

            _excludedProperties.AddRange(Helper.Collection.Enumerate(name, additionalNames).Where(x => x.HasValue()));
            return this;
        }
        /// <inheritdoc/>
        public ISearchCriteriaConverterPropertyBuilder<T, TSearchCriteria, TValue> With<TValue>(System.Linq.Expressions.Expression<Func<TSearchCriteria, TValue>> property, bool allowNull = false)
        {
            Guard.IsNotNull(property);

            var converter = new ExplicitConverter<TValue>(this, property.ExtractProperty(nameof(property)), allowNull);
            _explicitConverters.Add(converter);
            return converter;
        }
        /// <inheritdoc/>
        public ISearchCriteriaConverterBuilder<T, TSearchCriteria> WithDefaultAlias(Func<PropertyInfo, object?> aliasSelector)
        {
            _aliasSelector = Guard.IsNotNull(aliasSelector);
            return this;
        }
        /// <inheritdoc/>
        public ISearchCriteriaConverterBuilder<T, TSearchCriteria> WithDefaultColumnName(Func<PropertyInfo, string?> nameSelector)
        {
            _nameSelector = Guard.IsNotNull(nameSelector);
            return this;
        }
        /// <inheritdoc/>
        public ISearchCriteriaConverterBuilder<T, TSearchCriteria> WithDefaultOperator(Func<PropertyInfo, IStatementConditionOperatorExpressionBuilder<T>, IStatementConditionRightExpressionBuilder<T>?> operatorSelector)
        {
            _operatorSelector = Guard.IsNotNull(operatorSelector);
            return this;
        }
        /// <inheritdoc/>
        public ISearchCriteriaConverterBuilder<T, TSearchCriteria> WithDefaultValue(Func<PropertyInfo, object, DynamicParameters?, IStatementConditionRightExpressionBuilder<T>, IChainedBuilder<T, IStatementConditionExpressionBuilder<T>>?> valueSelector)
        {
            _valueSelector = Guard.IsNotNull(valueSelector);
            return this;
        }
        #endregion

        #region Classes
        private class ExplicitConverter : ISearchCriteriaConverterBuilder<T, TSearchCriteria>
        {
            // Fields
            private readonly ISearchCriteriaConverterBuilder<T, TSearchCriteria> _parent;
            protected Func<PropertyInfo, IStatementConditionExpressionBuilder<T>, IStatementConditionOperatorExpressionBuilder<T>>? _objectSelector;
            protected Func<PropertyInfo, IStatementConditionOperatorExpressionBuilder<T>, IStatementConditionRightExpressionBuilder<T>>? _operatorSelector;
            protected Func<PropertyInfo, object, DynamicParameters?, IStatementConditionRightExpressionBuilder<T>, IChainedBuilder<T, IStatementConditionExpressionBuilder<T>>> _valueSelector;

            // Properties
            public PropertyInfo Property { get; }
            public bool AllowNull { get; }

            public ExplicitConverter(ISearchCriteriaConverterBuilder<T, TSearchCriteria> parent, PropertyInfo property, bool allowNull)
            {
                _parent = Guard.IsNotNull(parent);
                Property = Guard.IsNotNull(property);
                AllowNull = allowNull;
            }

            #region Config
            /// <inheritdoc/>
            public ISearchCriteriaConverterBuilder<T, TSearchCriteria> Excluding(string name, params string[] additionalNames)
            {
                return _parent.Excluding(name, additionalNames);
            }
            /// <inheritdoc/>
            public ISearchCriteriaConverterPropertyBuilder<T, TSearchCriteria, TValue> With<TValue>(System.Linq.Expressions.Expression<Func<TSearchCriteria, TValue>> property, bool allowNull = false)
            {
                return _parent.With<TValue>(property, allowNull);
            }
            /// <inheritdoc/>
            public ISearchCriteriaConverterBuilder<T, TSearchCriteria> WithDefaultAlias(Func<PropertyInfo, object?> aliasSelector)
            {
                return _parent.WithDefaultAlias(aliasSelector);
            }
            /// <inheritdoc/>
            public ISearchCriteriaConverterBuilder<T, TSearchCriteria> WithDefaultColumnName(Func<PropertyInfo, string?> nameSelector)
            {
                return _parent.WithDefaultColumnName(nameSelector);
            }
            /// <inheritdoc/>
            public ISearchCriteriaConverterBuilder<T, TSearchCriteria> WithDefaultOperator(Func<PropertyInfo, IStatementConditionOperatorExpressionBuilder<T>, IStatementConditionRightExpressionBuilder<T>?> operatorSelector)
            {
                return _parent.WithDefaultOperator(operatorSelector);
            }
            /// <inheritdoc/>
            public ISearchCriteriaConverterBuilder<T, TSearchCriteria> WithDefaultValue(Func<PropertyInfo, object, DynamicParameters?, IStatementConditionRightExpressionBuilder<T>, IChainedBuilder<T, IStatementConditionExpressionBuilder<T>>?> valueSelector)
            {
                return _parent.WithDefaultValue(valueSelector);
            }
            #endregion

            public IStatementConditionOperatorExpressionBuilder<T>? As(PropertyInfo property, IStatementConditionExpressionBuilder<T> builder)
            {
                Guard.IsNotNull(property);
                Guard.IsNotNull(builder);

                return _objectSelector?.Invoke(property, builder);
            }

            public IStatementConditionRightExpressionBuilder<T>? CompareUsing(PropertyInfo property, IStatementConditionOperatorExpressionBuilder<T> builder)
            {
                Guard.IsNotNull(property);
                Guard.IsNotNull(builder);

                return _operatorSelector?.Invoke(property, builder);
            }

            public IChainedBuilder<T, IStatementConditionExpressionBuilder<T>>? ValueAs(PropertyInfo property, object? value, DynamicParameters? parameters, IStatementConditionRightExpressionBuilder<T> builder)
            {
                Guard.IsNotNull(property);
                Guard.IsNotNull(builder);

                return _valueSelector?.Invoke(property, value, parameters, builder);
            }
        }

        private class ExplicitConverter<TValue> : ExplicitConverter, ISearchCriteriaConverterPropertyBuilder<T, TSearchCriteria, TValue>
        {
            public ExplicitConverter(ISearchCriteriaConverterBuilder<T, TSearchCriteria> parent, PropertyInfo property, bool allowNull) : base(parent, property, allowNull)
            {
            }

            /// <inheritdoc/>
            public ISearchCriteriaConverterPropertyBuilder<T, TSearchCriteria, TValue> As(Func<PropertyInfo, IStatementConditionExpressionBuilder<T>, IStatementConditionOperatorExpressionBuilder<T>> objectSelector)
            {
                _objectSelector = Guard.IsNotNull(objectSelector);
                return this;
            }
            /// <inheritdoc/>
            public ISearchCriteriaConverterPropertyBuilder<T, TSearchCriteria, TValue> CompareUsing(Func<PropertyInfo, IStatementConditionOperatorExpressionBuilder<T>, IStatementConditionRightExpressionBuilder<T>> operatorSelector)
            {
                _operatorSelector = Guard.IsNotNull(operatorSelector);
                return this;
            }
            /// <inheritdoc/>
            public ISearchCriteriaConverterPropertyBuilder<T, TSearchCriteria, TValue> ValueAs(Func<PropertyInfo, TValue, DynamicParameters?, IStatementConditionRightExpressionBuilder<T>, IChainedBuilder<T, IStatementConditionExpressionBuilder<T>>> valueSelector)
            {
                Guard.IsNotNull(valueSelector);

                _valueSelector = (p, v, param, b) => valueSelector?.Invoke(p, v.CastToOrDefault<TValue>(), param, b);
                return this;
            }
        }
        #endregion
    }
}
