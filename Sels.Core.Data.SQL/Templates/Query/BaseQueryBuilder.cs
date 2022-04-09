using Sels.Core.Data.SQL.Query.Expressions;
using Sels.Core.Data.SQL.Query.Expressions.Condition;
using Sels.Core.Data.SQL.Query.Expressions.Join;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Data.SQL.Query
{
    /// <summary>
    /// Template for creating a <see cref="IQueryBuilder"/>.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to create the query for</typeparam>
    /// <typeparam name="TDerived">The type to return for the fluent syntax</typeparam>
    public abstract class BaseQueryBuilder<TEntity, TDerived> : IQueryBuilder<TEntity, TDerived>, 
        IQueryJoinBuilder<TEntity, TDerived>, 
        IConditionBuilder<TEntity, TDerived>
    {
        // Fields
        private readonly Dictionary<Type, string> _aliases = new Dictionary<Type, string>();

        // Properties
        /// <inheritdoc/>
        public IReadOnlyDictionary<Type, string> Aliases => _aliases;

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

            AddJoinExpression(new JoinExpression<TEntity>(joinType, new TableExpression(datasetAlias, table), builder));

            return Instance;
        }
        #endregion

        #region Condition
        /// <inheritdoc/>
        public TDerived Where(Action<IConditionExpressionBuilder<TEntity>> builder)
        {
            builder.ValidateArgument(nameof(builder));

            AddConditionExpression(new ConditionGroupExpression<TEntity>(builder, false));
            return Instance;
        }
        #endregion

        // Abstractions
        /// <inheritdoc/>
        public abstract IExpression[] InnerExpressions { get; }
        /// <inheritdoc/>
        public abstract string Build(QueryBuilderOptions options = QueryBuilderOptions.None);
        /// <inheritdoc/>
        public abstract void Build(StringBuilder builder, QueryBuilderOptions options = QueryBuilderOptions.None);

        /// <summary>
        /// The instance of the derived class inheriting from the current class.
        /// </summary>
        protected abstract TDerived Instance { get; }
        /// <summary>
        /// Adds a join expression to the current builder.
        /// </summary>
        /// <param name="joinExpression">The expression to add</param>
        protected abstract void AddJoinExpression(JoinExpression<TEntity> joinExpression);
        /// <summary>
        /// Adds a condition expression to the current builder.
        /// </summary>
        /// <param name="conditionExpression">The expression to add</param>
        protected abstract void AddConditionExpression(ConditionGroupExpression<TEntity> conditionExpression);
    }
}
