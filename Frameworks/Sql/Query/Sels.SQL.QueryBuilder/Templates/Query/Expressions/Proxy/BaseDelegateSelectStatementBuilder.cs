using Sels.Core.Extensions;
using Sels.SQL.QueryBuilder.Builder;
using Sels.SQL.QueryBuilder.Builder.Expressions;
using Sels.SQL.QueryBuilder.Builder.Statement;
using Sels.SQL.QueryBuilder.Expressions.Select;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.SQL.QueryBuilder.Templates.Query.Expressions.Proxy
{
    /// <summary>
    /// Delegates calls from <see cref="ISelectStatementBuilder{TEntity, TDerived}"/> to an instance.
    /// </summary>
    /// <typeparam name="TDerived">The type to return for the fluent syntax</typeparam>
    /// <typeparam name="TEntity">The main entity to select from</typeparam>
    public abstract class BaseDelegateSelectStatementBuilder<TEntity, TDerived> : BaseDelegateStatementQueryBuilder<TEntity, SelectExpressionPositions, TDerived>, ISelectStatementBuilder<TEntity, TDerived>
    {
        // Fields
        private readonly ISelectStatementBuilder<TEntity, TDerived> _target;

        /// <inheritdoc cref="BaseDelegateSelectStatementBuilder{TEntity, TDerived}"/>
        /// <param name="target">The instance to delegate the calls to</param>
        public BaseDelegateSelectStatementBuilder(ISelectStatementBuilder<TEntity, TDerived> target) : base(target)
        {
            _target = target.ValidateArgument(nameof(target));
        }

        /// <inheritdoc/>
        public ISelectStatementSelectedValueBuilder<TEntity, TDerived> ColumnExpression(IExpression expression, int order = 0)
        {
            return _target.ColumnExpression(expression, order);
        }
        /// <inheritdoc/>
        public TDerived Columns(object dataset, IEnumerable<string> columns)
        {
            return _target.Columns(dataset, columns);
        }
        /// <inheritdoc/>
        public TDerived ColumnsOf<T>(object dataset, params string[] excludedProperties)
        {
            return _target.ColumnsOf<T>(dataset, excludedProperties);
        }
        /// <inheritdoc/>
        public TDerived Having(Func<IStatementConditionExpressionBuilder<TEntity>, IChainedBuilder<TEntity, IStatementConditionExpressionBuilder<TEntity>>> builder)
        {
            return _target.Having(builder);
        }
        /// <inheritdoc/>
        public IStatementJoinResultSetBuilder<TEntity, TDerived> Join(Joins joinType)
        {
            return _target.Join(joinType);
        }
        /// <inheritdoc/>
        public TDerived OrderBy(IExpression expression)
        {
            return _target.OrderBy(expression);
        }

        /// <inheritdoc/>
        public TDerived Where(Func<IStatementConditionExpressionBuilder<TEntity>, IChainedBuilder<TEntity, IStatementConditionExpressionBuilder<TEntity>>> builder)
        {
            return _target.Where(builder);
        }
    }
}
