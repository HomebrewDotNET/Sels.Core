using System;
using System.Collections.Generic;

namespace Sels.SQL.QueryBuilder.Builder
{
    /// <summary>
    /// Query builder that works with table aliases for types.
    /// </summary>
    public interface IAliasQueryBuilder : IQueryBuilder
    {
        #region Alias
        /// <summary>
        /// Dictionary with any defined dataset aliases for the types used in the current builder.
        /// </summary>
        IReadOnlyDictionary<Type, string> Aliases { get; }
        /// <summary>
        /// Creates an alias for <paramref name="type"/> when it is used in the current builder. An alias is created by default for each new type.
        /// </summary>
        /// <param name="type">The type to create an alias for</param>
        /// <param name="alias">The alias for the type</param>
        void SetAlias(Type type, string alias);
        /// <summary>
        /// Creates an alias for <typeparamref name="T"/> when it is used in the current builder. An alias is created by default for each new type.
        /// </summary>
        /// <typeparam name="T">The type to create an alias for</typeparam>
        /// <param name="alias">The alias for the type</param>
        void SetAlias<T>(string alias) => SetAlias(typeof(T), alias);
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
        #endregion
    }
}