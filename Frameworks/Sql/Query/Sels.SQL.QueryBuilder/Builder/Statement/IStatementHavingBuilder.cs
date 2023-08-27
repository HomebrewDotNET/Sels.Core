using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.SQL.QueryBuilder.Builder.Statement
{
    /// <summary>
    /// Builder for adding a HAVING clause to a sql qeury.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to create the query for</typeparam>
    /// <typeparam name="TDerived">The type of the builder to create the conditions for</typeparam>
    public interface IStatementHavingBuilder<TEntity, out TDerived>
    {
        /// <summary>
        /// Adds a HAVING clause to the current builder using <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">Builder for adding the contition to the HAVING clause</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Having(Func<IStatementConditionExpressionBuilder<TEntity>, IChainedBuilder<TEntity, IStatementConditionExpressionBuilder<TEntity>>> builder);
    }
}
