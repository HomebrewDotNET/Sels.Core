using Sels.Core.Models;
using Sels.SQL.QueryBuilder.Builder.Statement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.SQL.QueryBuilder.Builder.Expressions
{
    /// <summary>
    /// Builder for creating a CASE WHEN sql expression.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to create the query for</typeparam>
    public interface ICaseExpressionRootBuilder<TEntity>
    {
        /// <summary>
        /// Creates a WHEN expression using <paramref name="whenBuilder"/>.
        /// </summary>
        /// <param name="whenBuilder">Builder for creating the condition</param>
        /// <returns>Builder for creating the THEN expression</returns>
        ICaseExpressionThenBuilder<TEntity> When(Action<ICaseExpressionConditionBuilder<TEntity>> whenBuilder);
        /// <summary>
        /// Defines the expression for the WHEN expression.
        /// </summary>
        /// <param name="expression">The expression to use</param>
        /// <returns>Builder for creating the THEN expression</returns>
        ICaseExpressionThenBuilder<TEntity> When(IExpression expression);
        /// <summary>
        /// Defines the expression for the WHEN expression using raw sql.
        /// </summary>
        /// <param name="rawSql">String containing the sql expression</param>
        /// <returns>Builder for creating the THEN expression</returns>
        ICaseExpressionThenBuilder<TEntity> When(string rawSql) => When(new RawExpression(rawSql));
    }

    /// <summary>
    /// Builder for creating a CASE WHEN sql expression. Option to terminate builder using ELSE expression.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to create the query for</typeparam>
    public interface ICaseExpressionBuilder<TEntity> : ICaseExpressionRootBuilder<TEntity>
    {
        /// <summary>
        /// Returns a builder for creating the expression for the final ELSE expression of a CASE expression.
        /// </summary>
        ISharedExpressionBuilder<TEntity, Null> Else { get; }
    }

    /// <summary>
    /// Builder that returns a builder for creating the expression that returns the value in a CASE WHEN THEN expression.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to create the query for</typeparam>
    public interface ICaseExpressionThenBuilder<TEntity>
    {
        /// <summary>
        /// Returns a builder for creating a THEN expression in a CASE WHEN expression.
        /// </summary>
        ISharedExpressionBuilder<TEntity, ICaseExpressionBuilder<TEntity>> Then { get; }
    }

    /// <summary>
    /// Builder for creating the condition in a CASE WHEN expression.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to build the query for</typeparam>
    public interface ICaseExpressionConditionBuilder<TEntity> : ISharedExpressionBuilder<TEntity, IComparisonExpressionBuilder<TEntity, ICaseExpressionFinalConditionBuilder<TEntity>>>
    {

    }
    /// <summary>
    /// Builder for selecting the expression on the right side of a CASE WHEN condition.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to build the query for</typeparam>
    public interface ICaseExpressionFinalConditionBuilder<TEntity> : ISharedExpressionBuilder<TEntity, IChainedBuilder<TEntity, ICaseExpressionConditionBuilder<TEntity>>>
    {

    }
}
