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
        private readonly Func<QueryBuilderOptions, string> _subQueryBuilder;

        /// <inheritdoc cref="UnionExpression"/>
        /// <param name="subQueryBuilder">Delegate that creates the query string</param>
        public UnionExpression(Func<QueryBuilderOptions, string> subQueryBuilder)
        {
            _subQueryBuilder = subQueryBuilder.ValidateArgument(nameof(subQueryBuilder));
        }
        /// <inheritdoc cref="SubQueryExpression"/>
        /// <param name="queryBuilder">Builder that creates the query string</param>
        public UnionExpression(IQueryBuilder queryBuilder) : this(x => queryBuilder.Build(x))
        {
            queryBuilder.ValidateArgument(nameof(queryBuilder));
        }

        /// <summary>
        /// Gets the query string.
        /// </summary>
        /// <param name="options">Optional settings for building the query</param>
        /// <returns>The query string</returns>
        public string GetQuery(QueryBuilderOptions options = QueryBuilderOptions.None)
        {
            return _subQueryBuilder(options);
        }

        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, QueryBuilderOptions options = QueryBuilderOptions.None)
        {
            builder.ValidateArgument(nameof(builder));

            var query = GetQuery(options);
            if (!query.HasValue()) throw new InvalidOperationException("Query builder delegate returned empty string");

            builder.Append(Sql.Clauses.Union);
            if (options.HasFlag(QueryBuilderOptions.Format)) builder.AppendLine(); else builder.AppendSpace();
            builder.Append(query);
        }
    }
}
