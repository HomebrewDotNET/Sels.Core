using Sels.Core.Extensions;
using Sels.SQL.QueryBuilder.Builder.Expressions;
using System;
using System.Linq.Expressions;
using System.Text;
using Sels.Core.Extensions.Reflection;

namespace Sels.SQL.QueryBuilder.Builder.Statement
{
    /// <summary>
    /// Builder for setting sql objects to a new value.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to create the query for</typeparam>
    /// <typeparam name="TReturn">The builder to return to set the value</typeparam>
    public interface IStatementSetBuilder<TEntity, out TReturn>
    {
        /// <summary>
        /// Returns the builder to select the value to set.
        /// </summary>
        public ISharedExpressionBuilder<TEntity, IStatementSetToBuilder<TEntity, TReturn>> Set { get; }
    }
    /// <summary>
    /// Builder for returning the builder to set the new value.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to create the query for</typeparam>
    /// <typeparam name="TReturn">The builder to return to set the value</typeparam>
    public interface IStatementSetToBuilder<TEntity, out TReturn>
    {
        /// <summary>
        /// Returns the builder to select the value to set.
        /// </summary>
        public TReturn To { get; }
    }
}
