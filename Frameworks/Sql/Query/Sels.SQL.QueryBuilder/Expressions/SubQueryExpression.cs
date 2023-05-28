﻿using System.Text;

namespace Sels.SQL.QueryBuilder.Builder.Expressions
{
    /// <summary>
    /// Expression that represents a sub query.
    /// </summary>
    public class SubQueryExpression : BaseDataSetExpression
    {
        // Fields
        private readonly Action<StringBuilder, ExpressionCompileOptions> _subQueryBuilder;
        private readonly bool _wrap;

        /// <inheritdoc cref="SubQueryExpression"/>
        /// <param name="dataset"><inheritdoc cref="IDataSetExpression.DataSet"/></param>
        /// <param name="subQueryBuilder">Delegate that adds the query to the supplied builder</param>
        /// <param name="wrap">If the query needs to be wrapped with ()</param>
        public SubQueryExpression(object? dataset, Action<StringBuilder, ExpressionCompileOptions> subQueryBuilder, bool wrap = true) : base(dataset)
        {
            _subQueryBuilder = subQueryBuilder.ValidateArgument(nameof(subQueryBuilder));
            _wrap = wrap;
        }
        /// <inheritdoc cref="SubQueryExpression"/>
        /// <param name="dataset"><inheritdoc cref="IDataSetExpression.DataSet"/></param>
        /// <param name="queryBuilder">Builder that creates the query string</param>
        /// <param name="wrap">If the query needs to be wrapped with ()</param>
        public SubQueryExpression(object? dataset, IQueryBuilder queryBuilder, bool wrap = true) : this(dataset, (b, o) => queryBuilder.Build(b, o), wrap)
        {
            queryBuilder.ValidateArgument(nameof(queryBuilder));
        }

        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, Func<object, string?> datasetConverterer, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            datasetConverterer.ValidateArgument(nameof(datasetConverterer));

            var dataset = DataSet != null ? datasetConverterer(DataSet) : null;

            if (_wrap) builder.Append('(');
            _subQueryBuilder(builder, options);
            if (_wrap) builder.Append(')');
            if (dataset.HasValue()) builder.AppendSpace().Append(dataset);
        }
    }
}