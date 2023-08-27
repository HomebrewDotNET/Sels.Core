using Sels.SQL.QueryBuilder.Builder.Expressions;
using Sels.SQL.QueryBuilder.Builder.Statement;
using Sels.SQL.QueryBuilder.Templates.Query.Expressions.Proxy;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.SQL.QueryBuilder.Expressions.Select
{
    /// <summary>
    /// Represents an aggregated value returned from a select statement.
    /// </summary>
    /// <typeparam name="TDerived">The type to return for the fluent syntax</typeparam>
    /// <typeparam name="TEntity">The main entity to select from</typeparam>
    public class AggregateSelectedValueExpression<TEntity, TDerived> : SelectedValueExpression<TEntity, TDerived>, ISelectStatementAggregatedValueBuilder<TEntity, TDerived>
    {
        /// <inheritdoc cref="AggregateSelectedValueExpression{TEntity, TDerived}"/>
        /// <param name="expression">Expression that contains the selected value</param>
        /// <param name="target">The builder the selected value is attached to</param>
        public AggregateSelectedValueExpression(IExpression expression, ISelectStatementBuilder<TEntity, TDerived> target) : base(expression, target)
        {
            
        }
    }
}
