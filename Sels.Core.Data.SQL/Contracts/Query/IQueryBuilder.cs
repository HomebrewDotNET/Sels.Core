﻿using Sels.Core.Data.SQL.Query.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Data.SQL.Query
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
        public IReadOnlyDictionary<TPosition, IExpression[]> Expressions { get; }
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
        /// Builds the query string using the current builder.
        /// </summary>
        /// <param name="options">Optional settings for building the query</param>
        /// <returns>The query string</returns>
        string Build(ExpressionCompileOptions options = ExpressionCompileOptions.None);
        /// <summary>
        /// Builds the query string using the current builder and appends it to <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The builder to append to</param>
        /// <param name="options">Optional settings for building the query</param>
        /// <returns><paramref name="builder"/> for method chaining</returns>
        StringBuilder Build(StringBuilder builder, ExpressionCompileOptions options = ExpressionCompileOptions.None);
    }
}
