using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.SQL.QueryBuilder.Builder.Statement
{
    /// <summary>
    /// Builder for configuring an aggregated value returned by a aggregate/window function in select statement.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to select from</typeparam>
    /// <typeparam name="TDerived">The type inheriting the current interface</typeparam>
    public interface ISelectStatementAggregatedValueBuilder<TEntity, out TDerived> : ISelectStatementSelectedValueBuilder<TEntity, TDerived>  
    {
        /// <summary>
        /// Defines an OVER clause for the current aggregated value.
        /// </summary>
        /// <param name="builder">Builder used to create the over clause</param>
        /// <returns>Current builder for method chaining</returns>
        ISelectStatementAggregatedValueBuilder<TEntity, TDerived> Over(Func<ISelectStatementOverBuilder<TEntity>, ISelectStatementOverBuilder<TEntity>> builder);
    }
}
