using Sels.Core.Data.SQL.Query.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Data.SQL.Query
{
    /// <summary>
    /// Exposes methods for building sql queries.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to create the query for</typeparam>
    /// <typeparam name="TDerived">The type to return for the fluent syntax</typeparam>
    public interface IQueryBuilder<TEntity, TDerived> : IQueryBuilder
    {
        /// <summary>
        /// Creates an alias for <typeparamref name="TEntity"/> when it is used in the current builder. A default alias is created by default for each new type.
        /// </summary>
        /// <param name="tableAlias">The alias for the type</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived AliasFor(string tableAlias) => AliasFor<TEntity>(tableAlias);
        /// <summary>
        /// Creates an alias for <typeparamref name="T"/> when it is used in the current builder. An alias is created by default for each new type.
        /// </summary>
        /// <typeparam name="T">The type to create the alias for</typeparam>
        /// <param name="tableAlias">The alias for the type</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived AliasFor<T>(string tableAlias);
        /// <summary>
        /// Gets the table alias for <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to get the alias from</typeparam>
        /// <param name="tableAlias">The defined table alias for type <paramref name="tableAlias"/></param>
        /// <returns>Current builder for method chaining</returns>
        TDerived OutAlias<T>(out string tableAlias);
    }
    /// <summary>
    /// Exposes methods for building sql queries.
    /// </summary>
    public interface IQueryBuilder
    {
        /// <summary>
        /// Array of currently defined expressions sorted in the order they would appear in the query.
        /// </summary>
        IExpression[] InnerExpressions { get; }
        /// <summary>
        /// Dictionary with any defined dataset aliases for the types used in the current builder.
        /// </summary>
        IReadOnlyDictionary<Type, string> Aliases { get; }
        /// <summary>
        /// Gets the table alias for <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to get the alias from</typeparam>
        /// <returns>Current builder for method chaining</returns>
        string GetAlias<T>();
        /// <summary>
        /// Gets the table alias for <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type to get the alias from</param>
        /// <returns>Current builder for method chaining</returns>
        string GetAlias(Type type);
        /// <summary>
        /// Builds the query string using the current builder.
        /// </summary>
        /// <param name="options">Optional settings for building the query</param>
        /// <returns>The query string</returns>
        string Build(QueryBuilderOptions options = QueryBuilderOptions.None);
        /// <summary>
        /// Builds the query string using the current builder and appends it to <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The builder to append to</param>
        /// <param name="options">Optional settings for building the query</param>
        void Build(StringBuilder builder, QueryBuilderOptions options = QueryBuilderOptions.None);
    }

    /// <summary>
    /// Exposes extra settings when building an sql query
    /// </summary>
    public enum QueryBuilderOptions
    {
        /// <summary>
        /// No selected options.
        /// </summary>
        None = 0,
        /// <summary>
        /// Formats the query to a more human readable format.
        /// </summary>
        Format = 1,
        /// <summary>
        /// Converts enums to string instead of int.
        /// </summary>
        EnumAsString = 2
    }
}
