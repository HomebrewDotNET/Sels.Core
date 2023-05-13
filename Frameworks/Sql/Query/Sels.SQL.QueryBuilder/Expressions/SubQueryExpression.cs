using System.Text;

namespace Sels.SQL.QueryBuilder.Builder.Expressions
{
    /// <summary>
    /// Expression that represents a sub query.
    /// </summary>
    public class SubQueryExpression : BaseDataSetExpression
    {
        // Fields
        private readonly Func<ExpressionCompileOptions, string> _subQueryBuilder;
        private readonly bool _wrap;

        /// <inheritdoc cref="SubQueryExpression"/>
        /// <param name="dataset"><inheritdoc cref="IDataSetExpression.DataSet"/></param>
        /// <param name="subQueryBuilder">Delegate that creates the query string</param>
        /// <param name="wrap">If the query needs to be wrapped with ()</param>
        public SubQueryExpression(object? dataset, Func<ExpressionCompileOptions, string> subQueryBuilder, bool wrap = true) : base(dataset)
        {
            _subQueryBuilder = subQueryBuilder.ValidateArgument(nameof(subQueryBuilder));
            _wrap = wrap;
        }
        /// <inheritdoc cref="SubQueryExpression"/>
        /// <param name="dataset"><inheritdoc cref="IDataSetExpression.DataSet"/></param>
        /// <param name="queryBuilder">Builder that creates the query string</param>
        /// <param name="wrap">If the query needs to be wrapped with ()</param>
        public SubQueryExpression(object? dataset, IQueryBuilder queryBuilder, bool wrap = true) : this(dataset, x => queryBuilder.Build(x), wrap)
        {
            queryBuilder.ValidateArgument(nameof(queryBuilder));
        }

        /// <summary>
        /// Gets the query string.
        /// </summary>
        /// <param name="options">Optional settings for building the query</param>
        /// <returns>The query string</returns>
        public string GetQuery(ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            return _subQueryBuilder(options);
        }

        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, Func<object, string?> datasetConverterer, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            datasetConverterer.ValidateArgument(nameof(datasetConverterer));

            var dataset = DataSet != null ? datasetConverterer(DataSet) : null;
            var query = GetQuery(options);
            if(!query.HasValue()) throw new InvalidOperationException("Query builder delegate returned empty string");

            builder.Append(_wrap ? "(" : String.Empty).Append(query).Append(_wrap ? ")" : String.Empty);
            if (dataset.HasValue()) builder.AppendSpace().Append(dataset);
        }
    }
}
