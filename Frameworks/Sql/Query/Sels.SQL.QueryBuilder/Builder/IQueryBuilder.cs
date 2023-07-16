using Sels.SQL.QueryBuilder.Builder.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.SQL.QueryBuilder.Builder
{
    /// <summary>
    /// Exposes methods for building sql queries where the expressions are located within the query based on <typeparamref name="TPosition"/>.
    /// </summary>
    /// <typeparam name="TPosition">Type that defines where in a query expressions should be located</typeparam>
    public interface IQueryBuilder<TPosition> : IQueryBuilder
    {
        /// <summary>
        /// Dictionary of the currently defined expressions grouped by the position where they would appear in the query.
        /// </summary>
        public IReadOnlyDictionary<TPosition, OrderedExpression[]> Expressions { get; }

        /// <summary>
        /// Executes <paramref name="action"/> each time an expression is added to the current builder.
        /// </summary>
        /// <param name="action">The delegate called when an expression is added</param>
        /// <returns>Current builder for method chaining</returns>
        IQueryBuilder<TPosition> OnExpressionAdded(Action<OrderedExpression, TPosition> action);
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
        /// Executes <paramref name="action"/> each time an expression is added to the current builder.
        /// </summary>
        /// <param name="action">The delegate called when an expression is added</param>
        /// <returns>Current builder for method chaining</returns>
        IQueryBuilder OnExpressionAdded(Action<IExpression> action);

        /// <summary>
        /// Executes <paramref name="action"/> each time an expression in the current builder is compiled. Also includes sub expressions.
        /// </summary>
        /// <param name="action">The delegate called when an expression is compiled</param>
        /// <returns>Current builder for method chaining</returns>
        IQueryBuilder OnCompiling(Action<IExpression> action);

        /// <summary>
        /// Builds the query string using the current builder.
        /// </summary>
        /// <param name="options">Optional settings for building the query</param>
        /// <returns>The query string</returns>
        string Build(ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            var builder = new StringBuilder();
            Build(builder, options);
            return builder.ToString();
        }
        /// <summary>
        /// Builds the query string using the current builder and appends it to <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The builder to append to</param>
        /// <param name="options">Optional settings for building the query</param>
        /// <returns><paramref name="builder"/> for method chaining</returns>
        StringBuilder Build(StringBuilder builder, ExpressionCompileOptions options = ExpressionCompileOptions.None);
    }
}
