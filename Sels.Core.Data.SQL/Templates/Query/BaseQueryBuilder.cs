using Sels.Core.Data.SQL.Query.Compilation;
using Sels.Core.Data.SQL.Query.Expressions;
using Sels.Core.Data.SQL.Query.Expressions.Condition;
using Sels.Core.Data.SQL.Query.Expressions.Join;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Data.SQL.Query
{
    /// <summary>
    /// Template for creating a <see cref="IQueryBuilder"/>.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to create the query for</typeparam>
    /// <typeparam name="TPosition">Type that defines where in a query expressions should be located</typeparam>
    /// <typeparam name="TDerived">The type to return for the fluent syntax</typeparam>
    public abstract class BaseQueryBuilder<TEntity, TPosition, TDerived> : IQueryBuilder<TEntity, TPosition, TDerived>, 
        IQueryJoinBuilder<TEntity, TDerived>, 
        IConditionBuilder<TEntity, TDerived>
        where TPosition : notnull
    {
        // Fields
        private readonly Dictionary<Type, string> _aliases = new Dictionary<Type, string>();
        private readonly Dictionary<TPosition, List<IExpression>> _expressions = new();
        private readonly IQueryCompiler<TPosition> _compiler;

        // Properties
        /// <inheritdoc/>
        public IReadOnlyDictionary<TPosition, IExpression[]> Expressions => _expressions.ToDictionary(x => x.Key, x => x.Value.ToArray());
        /// <inheritdoc/>
        public IExpression[] InnerExpressions => _expressions.OrderBy(x => x.Key).SelectMany(x => x.Value).ToArray();
        /// <inheritdoc/>
        public IReadOnlyDictionary<Type, string> Aliases => _aliases;

        /// <inheritdoc cref="BaseQueryBuilder{TEntity, TPosition, TDerived}"/>
        /// <param name="compiler">Compiler to create the query using the expressions defined in the current builder</param>
        public BaseQueryBuilder(IQueryCompiler<TPosition> compiler)
        {
            _compiler = compiler.ValidateArgument(nameof(compiler));
        }

        #region Alias
        /// <inheritdoc/>
        public TDerived AliasFor<T>(string tableAlias)
        {
            tableAlias.ValidateArgumentNotNullOrWhitespace(nameof(tableAlias));

            _aliases.AddOrUpdate(typeof(T), tableAlias);
            return Instance;
        }
        /// <inheritdoc/>
        public string GetAlias<T>()
        {
            return GetAlias(typeof(T));
        }
        /// <inheritdoc/>
        public string GetAlias(Type type)
        {
            type.ValidateArgument(nameof(type));

            return GetOrSetAlias(type);
        }
        /// <inheritdoc/>
        public TDerived OutAlias<T>(out string tableAlias)
        {
            tableAlias = GetAlias<T>();
            return Instance;
        }

        private string GetOrSetAlias(Type type)
        {
            type.ValidateArgument(nameof(type));

            if (_aliases.ContainsKey(type))
            {
                return _aliases[type];
            }
            else
            {
                var alias = type.Name[0].ToString();

                var counter = 1;
                while(_aliases.Any(x => x.Value.Equals(alias, StringComparison.OrdinalIgnoreCase)))
                {
                    alias = alias + counter;
                    counter++;
                }

                _aliases.Add(type, alias);
                return alias;
            }
        }
        #endregion

        #region Join
        /// <inheritdoc/>
        public TDerived Join(Joins joinType, string table, object? datasetAlias, Action<IOnJoinBuilder<TEntity>> builder)
        {
            table.ValidateArgument(nameof(table));
            builder.ValidateArgument(nameof(builder));

            var expression = new JoinExpression<TEntity>(joinType, new TableExpression(datasetAlias, table), builder);
            Expression(expression, GetPositionForJoinExpression(expression));

            return Instance;
        }
        #endregion

        #region Condition
        /// <inheritdoc/>
        public TDerived Where(Action<IConditionExpressionBuilder<TEntity>> builder)
        {
            builder.ValidateArgument(nameof(builder));

            var expression = new ConditionGroupExpression<TEntity>(builder, false);
            Expression(expression, GetPositionForConditionExpression(expression));
            return Instance;
        }
        #endregion

        #region Expression
        /// <inheritdoc/>
        public TDerived Expression(IExpression sqlExpression, TPosition position)
        {
            sqlExpression.ValidateArgument(nameof(sqlExpression));
            position.ValidateArgument(nameof(position));    

            _expressions.AddValueToList(position, sqlExpression);
            return Instance;
        }
        #endregion

        #region Build
        /// <inheritdoc/>
        public string Build(QueryBuilderOptions options = QueryBuilderOptions.None)
        {
            var builder = new StringBuilder();
            Build(builder, options);
            return builder.ToString();
        }
        /// <inheritdoc/>
        public void Build(StringBuilder builder, QueryBuilderOptions options = QueryBuilderOptions.None)
        {
            builder.ValidateArgument(nameof(builder));

            _compiler.CompileTo(builder, this, Expressions, options);
        }
        #endregion

        /// <summary>
        /// Returns all properties on type <typeparamref name="T"/> that can be used as object names/values for a query.
        /// </summary>
        /// <typeparam name="T">The type to get the properties from</typeparam>
        /// <param name="excludedProperties">Optional names of properties to exclude</param>
        /// <returns>All usable properties on <typeparamref name="T"/></returns>
        protected IEnumerable<PropertyInfo> GetColumnPropertiesFrom<T>(string[]? excludedProperties)
        {
            return typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(x => x.GetIndexParameters()?.Length == 0 && (!excludedProperties.HasValue() || !excludedProperties.Contains(x.Name, StringComparer.OrdinalIgnoreCase)));
        }

        // Abstractions
        /// <inheritdoc/>
        public abstract TDerived Instance { get; }

        /// <summary>
        /// Gets the position for <paramref name="joinExpression"/>.
        /// </summary>
        /// <param name="joinExpression">The expression to get the position for</param>
        protected abstract TPosition GetPositionForJoinExpression(JoinExpression<TEntity> joinExpression);
        /// <summary>
        /// Gets the position for <paramref name="conditionExpression"/>.
        /// </summary>
        /// <param name="conditionExpression">The expression to get the position for</param>
        protected abstract TPosition GetPositionForConditionExpression(ConditionGroupExpression<TEntity> conditionExpression);
    }
}
