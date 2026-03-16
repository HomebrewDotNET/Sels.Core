using Sels.Core.Extensions;
using Sels.Core.Extensions.Linq;
using Sels.Core.Extensions.Text;
using Sels.SQL.QueryBuilder.Builder;
using Sels.SQL.QueryBuilder.Builder.Compilation;
using Sels.SQL.QueryBuilder.Builder.Expressions;
using Sels.SQL.QueryBuilder.Builder.Statement;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.SQL.QueryBuilder.Expressions.Over
{
    /// <summary>
    /// Expression that contains an OVER clause defined on an aggregated value.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to create the builder for</typeparam>
    public class OverClauseExpressionBuilder<TEntity> : BaseExpressionContainer, ISelectStatementOverBuilder<TEntity>
    {
        // Properties
        /// <summary>
        /// Optional expressions that defines the values to partition by.
        /// </summary>
        public List<IExpression> PartitionByExpressions { get; set; }
        /// <summary>
        /// Optional expression for ordering the partitions.
        /// </summary>
        public List<IExpression> OrderByExpressions { get; set; }
        /// <summary>
        /// Optional expression that limits the window frame of each partition.
        /// </summary>
        public IExpression WindowFrameExpression { get; set; }

        /// <inheritdoc cref="OverClauseExpressionBuilder{TEntity}"/>
        /// <param name="configurator">Delegate for configuring the current instance</param>
        public OverClauseExpressionBuilder(Func<ISelectStatementOverBuilder<TEntity>, ISelectStatementOverBuilder<TEntity>> configurator)
        {
            configurator.ValidateArgument(nameof(configurator));
            configurator(this);
        }
        /// <inheritdoc/>
        public ISelectStatementOverBuilder<TEntity> OrderBy(IExpression expression)
        {
            expression.ValidateArgument(nameof(expression));
            OrderByExpressions ??= new List<IExpression>();
            OrderByExpressions.Add(expression);
            return this;
        }
        /// <inheritdoc/>
        public ISelectStatementOverBuilder<TEntity> PartitionBy(IExpression expression)
        {
            expression.ValidateArgument(nameof(expression));
            PartitionByExpressions ??= new List<IExpression>();
            PartitionByExpressions.Add(expression);
            return this;
        }
        /// <inheritdoc/>
        public ISelectStatementOverBuilder<TEntity> WindowFrame(IExpression expression)
        {
            WindowFrameExpression = expression.ValidateArgument(nameof(expression));
            return this;
        }
        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, Action<StringBuilder, IExpression> subBuilder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            subBuilder.ValidateArgument(nameof(subBuilder));

            if(PartitionByExpressions.HasValue())
            {
                builder.Append(Sql.Over.PartitionBy).AppendSpace();
                PartitionByExpressions.Execute((i, e) =>
                {
                    subBuilder(builder, e);
                    if (i < PartitionByExpressions.Count - 1) builder.Append(", ");
                });
            }

            if (OrderByExpressions.HasValue())
            {
                if (PartitionByExpressions.HasValue()) builder.AppendSpace();
                builder.Append(Sql.Clauses.OrderBy).AppendSpace();
                OrderByExpressions.Execute((i, e) =>
                {
                    subBuilder(builder, e);
                    if (i < OrderByExpressions.Count - 1) builder.Append(", ");
                });
            }

            if(WindowFrameExpression != null)
            {
                if (OrderByExpressions.HasValue()) builder.AppendSpace();
                subBuilder(builder, WindowFrameExpression);
            }
        }
    }
}
