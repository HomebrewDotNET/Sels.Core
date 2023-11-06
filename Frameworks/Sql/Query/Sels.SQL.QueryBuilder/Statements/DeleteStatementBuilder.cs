using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.SQL.QueryBuilder.Builder.Compilation;
using Sels.SQL.QueryBuilder.Builder.Expressions;
using System.Collections.Generic;
using System.Text;

namespace Sels.SQL.QueryBuilder.Builder.Statement
{
    /// <summary>
    /// Builds a delete query.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to create the query for</typeparam>
    public class DeleteStatementBuilder<TEntity> : BaseStatementBuilder<TEntity, DeleteExpressionPositions, IDeleteStatementBuilder<TEntity>>, IDeleteStatementBuilder<TEntity>
    {
        /// <inheritdoc cref="DeleteStatementBuilder{TEntity}"/>
        /// <param name="compiler">Compiler to create the query using the expressions defined in the current builder</param>
        public DeleteStatementBuilder(IQueryCompiler<DeleteExpressionPositions> compiler) : base(compiler)
        {
        }

        /// <inheritdoc cref="DeleteStatementBuilder{TEntity}"/>
        /// <param name="other">The builder to copy settings from</param>
        protected DeleteStatementBuilder(DeleteStatementBuilder<TEntity> other) : base(other)
        {
        }

        /// <inheritdoc/>
        public override IDeleteStatementBuilder<TEntity> Instance => this;

        /// <inheritdoc/>
        public IDeleteStatementBuilder<TEntity> OrderBy(IExpression expression)
        {
            expression.ValidateArgument(nameof(expression));
            return Expression(expression, DeleteExpressionPositions.OrderBy);
        }

        /// <inheritdoc/>
        public override IDeleteStatementBuilder<TEntity> Clone()
        {
            return new DeleteStatementBuilder<TEntity>(this);
        }

        /// <inheritdoc/>
        protected override DeleteExpressionPositions GetPositionForConditionExpression(ConditionGroupExpression<TEntity> conditionExpression)
        {
            return DeleteExpressionPositions.Where;
        }
        /// <inheritdoc/>
        protected override DeleteExpressionPositions GetPositionForJoinExpression(JoinExpression<TEntity, IDeleteStatementBuilder<TEntity>> joinExpression)
        {
            return DeleteExpressionPositions.Join;
        }

        /// <inheritdoc/>
        public override StringBuilder Build(StringBuilder builder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            // Auto assume no implicit 
            if (typeof(TEntity) == typeof(object)) options |= ExpressionCompileOptions.NoImplitExpressions;

            // Add implicit expressions
            if (!options.HasFlag(ExpressionCompileOptions.NoImplitExpressions) && (!Expressions.ContainsKey(DeleteExpressionPositions.From) || !Expressions[DeleteExpressionPositions.From].HasValue())) this.CastTo<IDeleteStatementBuilder<TEntity>>().From();

            return base.Build(builder, options);
        }
    }
}
