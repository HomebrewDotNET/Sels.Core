using Sels.Core.Data.SQL.Query.Expressions;
using System.Text;

namespace Sels.Core.Data.SQL.Query.Statement
{
    /// <summary>
    /// Exposes methods for building sql statement queries.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to create the query for</typeparam>
    /// <typeparam name="TPosition">Type that defines where in a query expressions should be located</typeparam>
    /// <typeparam name="TDerived">The type to return for the fluent syntax</typeparam>
    public interface IStatementQueryBuilder<TEntity, TPosition, out TDerived> : IQueryBuilder<TPosition>
    {
        /// <summary>
        /// The instance that implemented this interface. Can be used by extensions methods.
        /// </summary>
        public TDerived Instance { get; }

        #region Alias
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
        #endregion

        #region Expression
        /// <summary>
        /// Adds a sql expression to the current builder.
        /// </summary>
        /// <param name="sqlExpression">The sql expression to add</param>
        /// <param name="position">Where in the query the expression should be placed</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Expression(IExpression sqlExpression, TPosition position);
        /// <summary>
        /// Adds a raw sql expression to the current builder.
        /// </summary>
        /// <param name="sqlExpression">String containing the sql expression</param>
        /// <param name="position">Where in the query the expression should be placed</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Expression(string sqlExpression, TPosition position) => Expression(new RawExpression(sqlExpression.ValidateArgumentNotNullOrWhitespace(nameof(sqlExpression))), position);
        /// <summary>
        /// Adds a sql expression to the current builder.
        /// </summary>
        /// <param name="sqlExpression">Delegate that adds the sql expression to the provided string builder</param>
        /// <param name="position">Where in the query the expression should be placed</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Expression(Action<StringBuilder> sqlExpression, TPosition position) => Expression(new DelegateExpression(sqlExpression.ValidateArgument(nameof(sqlExpression))), position);
        #endregion

        /// <summary>
        /// Clones the current builder to a new instance containing the same expressions.
        /// </summary>
        /// <returns>A query builder with the same expressions as the current builder</returns>
        TDerived Clone();
    }
}