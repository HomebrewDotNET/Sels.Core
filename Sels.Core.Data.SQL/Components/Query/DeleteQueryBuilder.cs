using Sels.Core.Data.SQL.Query.Compilation;
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
    /// Builds a delete query.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to create the query for</typeparam>
    public class DeleteQueryBuilder<TEntity> : BaseQueryBuilder<TEntity, DeleteExpressionPositions, IDeleteQueryBuilder<TEntity>>, IDeleteQueryBuilder<TEntity>
    {
        /// <inheritdoc cref="DeleteQueryBuilder{TEntity}"/>
        /// <param name="compiler">Compiler to create the query using the expressions defined in the current builder</param>
        public DeleteQueryBuilder(IQueryCompiler<DeleteExpressionPositions> compiler) : base(compiler)
        {
        }

        /// <inheritdoc/>
        protected override IDeleteQueryBuilder<TEntity> Instance => this;

        /// <inheritdoc/>
        protected override DeleteExpressionPositions GetPositionForConditionExpression(ConditionGroupExpression<TEntity> conditionExpression)
        {
            return DeleteExpressionPositions.Where;
        }
        /// <inheritdoc/>
        protected override DeleteExpressionPositions GetPositionForJoinExpression(JoinExpression<TEntity> joinExpression)
        {
            return DeleteExpressionPositions.Join;
        }
    }
}
