using Sels.Core.Data.SQL.Query.Compilation;
using Sels.Core.Data.SQL.Query.Expressions;
using Sels.Core.Data.SQL.Query.Expressions.Condition;
using Sels.Core.Data.SQL.Query.Expressions.Join;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Data.SQL.Query.Statement
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
        /// <param name="compiler">Compiler to create the query using the expressions defined in the current builder</param>
        /// <param name="expressions">The expressions for the current query</param>
        public DeleteStatementBuilder(IQueryCompiler<DeleteExpressionPositions> compiler, Dictionary<DeleteExpressionPositions, List<IExpression>> expressions) : base(compiler, expressions)
        {
        }

        /// <inheritdoc/>
        public override IDeleteStatementBuilder<TEntity> Instance => this;

        /// <inheritdoc/>
        protected override IDeleteStatementBuilder<TEntity> Clone(IQueryCompiler<DeleteExpressionPositions> compiler, Dictionary<DeleteExpressionPositions, List<IExpression>> expressions)
        {
            return new DeleteStatementBuilder<TEntity>(compiler, expressions);
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
    }
}
