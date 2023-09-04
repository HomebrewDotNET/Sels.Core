using Sels.Core.Extensions;
using Sels.SQL.QueryBuilder.Builder;
using Sels.SQL.QueryBuilder.Builder.Expressions;
using Sels.SQL.QueryBuilder.Builder.Statement;
using Sels.SQL.QueryBuilder.Expressions.Over;
using Sels.SQL.QueryBuilder.Templates.Query.Expressions.Proxy;
using System;
using System.Collections.Generic;
using System.Text;
using Sels.Core.Extensions.Text;

namespace Sels.SQL.QueryBuilder.Expressions.Select
{
    /// <summary>
    /// Represents an aggregated value returned from a select statement.
    /// </summary>
    /// <typeparam name="TDerived">The type to return for the fluent syntax</typeparam>
    /// <typeparam name="TEntity">The main entity to select from</typeparam>
    public class AggregateSelectedValueExpression<TEntity, TDerived> : SelectedValueExpression<TEntity, TDerived>, ISelectStatementAggregatedValueBuilder<TEntity, TDerived>
    {
        // Properties
        /// <summary>
        /// Optional over clause for the current value.
        /// </summary>
        public IExpression OverClause { get; set; }

        /// <inheritdoc cref="AggregateSelectedValueExpression{TEntity, TDerived}"/>
        /// <param name="expression">Expression that contains the selected value</param>
        /// <param name="target">The builder the selected value is attached to</param>
        public AggregateSelectedValueExpression(IExpression expression, ISelectStatementBuilder<TEntity, TDerived> target) : base(expression, target)
        {
            
        }

        /// <inheritdoc/>
        public ISelectStatementAggregatedValueBuilder<TEntity, TDerived> Over(Func<ISelectStatementOverBuilder<TEntity>, ISelectStatementOverBuilder<TEntity>> builder)
        {
            OverClause = new OverClauseExpressionBuilder<TEntity>(builder.ValidateArgument(nameof(builder)));
            return this;
        }

        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, Action<StringBuilder, IExpression> subBuilder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            subBuilder.ValidateArgument(nameof(subBuilder));

            subBuilder(builder, ValueExpression);

            if(OverClause != null)
            {
                builder.AppendSpace().Append(Sql.Clauses.Over).Append('(');
                subBuilder(builder, OverClause);
                builder.Append(')');
            }

            if (Alias.HasValue())
            {
                builder.AppendSpace().Append(Sql.As).AppendSpace();
                subBuilder(builder, new ObjectExpression(null, Alias));
            }
        }
    }
}
