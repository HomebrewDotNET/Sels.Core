using Sels.Core.Data.SQL.Query.Statement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Data.SQL.Query.Expressions
{
    /// <summary>
    /// Expression that represents a union query.
    /// </summary>
    public class UnionExpression : BaseExpression
    {
        // Fields
        private readonly Func<ExpressionCompileOptions, string> _subQueryBuilder;

        // Properties
        /// <summary>
        /// True if duplicates are removed when concatenating, otherwise false if diplicates are included.
        /// </summary>
        public bool IsDistinct { get; }

        /// <inheritdoc cref="UnionExpression"/>
        /// <param name="subQueryBuilder">Delegate that creates the query string</param>
        /// <param name="isDistinct"><inheritdoc cref="IsDistinct"/></param>
        public UnionExpression(Func<ExpressionCompileOptions, string> subQueryBuilder, bool isDistinct = true)
        {
            _subQueryBuilder = subQueryBuilder.ValidateArgument(nameof(subQueryBuilder));
            IsDistinct = isDistinct;
        }
        /// <inheritdoc cref="SubQueryExpression"/>
        /// <param name="queryBuilder">Builder that creates the query string</param>
        /// <param name="isDistinct"><inheritdoc cref="IsDistinct"/></param>
        public UnionExpression(IQueryBuilder queryBuilder, bool isDistinct = true) : this(x => queryBuilder.Build(x), isDistinct)
        {
            queryBuilder.ValidateArgument(nameof(queryBuilder));
        }

        /// <summary>
        /// Gets the query string of the result set to concatenate.
        /// </summary>
        /// <param name="options">Optional settings for building the query</param>
        /// <returns>The query string</returns>
        public string GetQuery(ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            return _subQueryBuilder(options);
        }

        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));

            var query = GetQuery(options);
            if (!query.HasValue()) throw new InvalidOperationException("Query builder delegate returned empty string");

            builder.Append(IsDistinct ? Sql.Clauses.Union : Sql.Clauses.UnionAll);
            if (options.HasFlag(ExpressionCompileOptions.Format)) builder.AppendLine(); else builder.AppendSpace();
            builder.Append(query);
        }
    }
}
