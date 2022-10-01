using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Data.SQL.Query.Expressions
{
    /// <summary>
    /// Expression that represents an alias given to a dataset (e.g. Tables, columns, sub queries, common table expressions, ...)
    /// </summary>
    public class DataSetExpression : BaseDataSetExpression
    {
        /// <inheritdoc cref="DataSetExpression"/>
        /// <param name="dataset"><inheritdoc cref="IDataSetExpression.DataSet"/></param>
        public DataSetExpression(object dataset) : base(dataset)
        {
        }

        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, Func<object, string?> datasetConverterer, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            datasetConverterer.ValidateArgument(nameof(datasetConverterer));

            var dataSet = DataSet != null ? datasetConverterer(DataSet) : null;

            if (dataSet != null) builder.Append(dataSet);
        }
    }
}
