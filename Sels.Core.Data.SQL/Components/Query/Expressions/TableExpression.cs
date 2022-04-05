using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Data.SQL.Query.Expressions
{
    /// <summary>
    /// Expression that represents a table to perform a query on.
    /// </summary>
    public class TableExpression : BaseObjectExpression
    {
        /// <inheritdoc cref="TableExpression"/>
        /// <param name="dataset"><inheritdoc cref="IDataSetExpression.DataSet"/></param>
        /// <param name="table"><inheritdoc cref="IObjectExpression.Object"/></param>
        public TableExpression(object? dataset, string table) : base(dataset, table)
        {
        }

        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, Func<object, string?> datasetConverterer, Func<string, string>? objectConverter, QueryBuilderOptions options = QueryBuilderOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            datasetConverterer.ValidateArgument(nameof(datasetConverterer));
            objectConverter.ValidateArgument(nameof(objectConverter));

            var dataset = DataSet != null ? datasetConverterer(DataSet) : null;
            var tableName = objectConverter(Object) ?? throw new InvalidOperationException($"{nameof(objectConverter)} returned null");

            builder.Append(tableName);
            if (dataset.HasValue()) builder.AppendSpace().Append(dataset);
        }
    }
}
