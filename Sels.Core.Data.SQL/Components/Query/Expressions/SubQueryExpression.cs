using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Data.SQL.Query.Expressions
{
    /// <summary>
    /// Expression that represents a sub query
    /// </summary>
    public class SubQueryExpression : BaseDataSetExpression
    {
        // Fields
        private readonly Func<QueryBuilderOptions, string> _subQueryBuilder;

        /// <inheritdoc cref="SubQueryExpression"/>
        /// <param name="dataset"><inheritdoc cref="IDataSetExpression.DataSet"/></param>
        /// <param name="subQueryBuilder">Delegate that creates the query string</param>
        public SubQueryExpression(object? dataset, Func<QueryBuilderOptions, string> subQueryBuilder) : base(dataset)
        {
            _subQueryBuilder = subQueryBuilder.ValidateArgument(nameof(subQueryBuilder));
        }
        /// <inheritdoc cref="SubQueryExpression"/>
        /// <param name="dataset"><inheritdoc cref="IDataSetExpression.DataSet"/></param>
        /// <param name="queryBuilder">Builder that creates the query string</param>
        public SubQueryExpression(object? dataset, IQueryBuilder queryBuilder) : this(dataset, x => queryBuilder.Build(x))
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
        public override void ToSql(StringBuilder builder, Func<object, string?> datasetConverterer, QueryBuilderOptions options = QueryBuilderOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            datasetConverterer.ValidateArgument(nameof(datasetConverterer));

            var dataset = DataSet != null ? datasetConverterer(DataSet) : null;
            var query = GetQuery(options);
            if(!query.HasValue()) throw new InvalidOperationException("Query builder delegate returned empty string");

            builder.Append('(').Append(query).Append(')');
            if (dataset.HasValue()) builder.AppendSpace().Append(dataset);
        }
    }
}
