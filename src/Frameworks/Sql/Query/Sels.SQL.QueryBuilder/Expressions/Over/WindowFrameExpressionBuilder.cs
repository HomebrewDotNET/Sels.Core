using Sels.Core.Extensions;
using Sels.SQL.QueryBuilder.Builder;
using Sels.SQL.QueryBuilder.Builder.Expressions;
using System;
using System.Collections.Generic;
using System.Text;
using Sels.Core.Extensions.Text;
using Sels.SQL.QueryBuilder.Builder.Statement;

namespace Sels.SQL.QueryBuilder.Expressions.Over
{
    /// <summary>
    /// Expression that defines how to limit the window frame of a partition.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to create the expression for</typeparam>
    public class WindowFrameExpressionBuilder<TEntity> : BaseExpressionContainer, 
        ISelectStatementOverFrameBorderBuilder<TEntity>,
        ISelectStatementOverSharedFrameBorderBuilder<TEntity, ISelectStatementOverIntermediateFrameBorderBuilder<TEntity>>,
        ISelectStatementOverIntermediateFrameBorderBuilder<TEntity>,
        ISelectStatementOverSharedFrameBorderBuilder<TEntity, ISelectStatementOverBuilder<TEntity>>
    {
        // Fields
        private readonly ISelectStatementOverBuilder<TEntity> _parent;

        // Properties
        /// <summary>
        /// Expression that contains how to limit a window frame.
        /// </summary>
        public IExpression Limit { get; set; }
        /// <summary>
        /// Expression that contains the frame border.
        /// </summary>
        public IExpression FrameBorderExpression { get; set; }
        /// <summary>
        /// Expression that contains the frame upper bound border if one is defined.
        /// </summary>
        public IExpression UpperFrameBorderExpression { get; set; }

        /// <inheritdoc cref="WindowFrameExpressionBuilder{TEntity}"/>
        /// <param name="parent">The parent builder that created the current expression</param>
        /// <param name="limit"><inheritdoc cref="FrameBorderExpression"/></param>
        public WindowFrameExpressionBuilder(ISelectStatementOverBuilder<TEntity> parent, IExpression limit)
        {
            _parent = parent.ValidateArgument(nameof(parent));
            Limit = limit.ValidateArgument(nameof(limit));
        }

        /// <inheritdoc/>
        public ISelectStatementOverSharedFrameBorderBuilder<TEntity, ISelectStatementOverIntermediateFrameBorderBuilder<TEntity>> Between => this;
        /// <inheritdoc/>
        public ISelectStatementOverSharedFrameBorderBuilder<TEntity, ISelectStatementOverBuilder<TEntity>> And => this;

        /// <inheritdoc/>
        public ISelectStatementOverBuilder<TEntity> Expression(IExpression expression)
        {
            expression.ValidateArgument(nameof(expression));
            if (FrameBorderExpression == null) FrameBorderExpression = expression;
            else UpperFrameBorderExpression = expression;

            return _parent;
        }
        /// <inheritdoc/>
        ISelectStatementOverIntermediateFrameBorderBuilder<TEntity> ISelectStatementOverSharedFrameBorderBuilder<TEntity, ISelectStatementOverIntermediateFrameBorderBuilder<TEntity>>.Expression(IExpression expression)
        {
            FrameBorderExpression = expression.ValidateArgument(nameof(expression));
            return this;
        }

        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, Action<StringBuilder, IExpression> subBuilder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            subBuilder.ValidateArgument(nameof(subBuilder));
            Limit.ValidateArgument(nameof(Limit));
            FrameBorderExpression.ValidateArgument(nameof(FrameBorderExpression));

            subBuilder(builder, Limit);
            builder.AppendSpace();

            if (UpperFrameBorderExpression != null)
            {
                builder.Append(Sql.Over.Between).AppendSpace();
                subBuilder(builder, FrameBorderExpression);
                builder.AppendSpace().Append(Sql.LogicOperators.And).AppendSpace();
                subBuilder(builder, UpperFrameBorderExpression);
            }
            else
            {
                subBuilder(builder, FrameBorderExpression);
            }
        }
    }
}
