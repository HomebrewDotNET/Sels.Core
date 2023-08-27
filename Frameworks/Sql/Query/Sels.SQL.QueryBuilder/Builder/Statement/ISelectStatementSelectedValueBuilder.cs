using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.SQL.QueryBuilder.Builder.Statement
{
    /// <summary>
    /// Builder for configuring a value returned by a select statement.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to select from</typeparam>
    /// <typeparam name="TDerived">The type inheriting the current interface</typeparam>
    public interface ISelectStatementSelectedValueBuilder<TEntity, out TDerived> : ISelectStatementBuilder<TEntity, TDerived>
    {
        /// <summary>
        /// Defines an alias for the selected value.
        /// </summary>
        /// <param name="alias">The alias for the selected value</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived As(string alias);
    }
}
