using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Data.SQL.Query.Expressions
{
    /// <summary>
    /// Expression that represents the syntax for selecting everything (e.g. *) from a dataset.
    /// </summary>
    public class AllColumnsExpression : BaseExpression, IDataSetExpression
    {
        /// <summary>
        /// Object containing the dataset to select everything from.
        /// </summary>
        public object? DataSet { get; }

        /// <inheritdoc cref="AllColumnsExpression"/>
        /// <param name="dataset"><inheritdoc cref="DataSet"/></param>
        public AllColumnsExpression(object? dataset = null)
        {
            DataSet = dataset;
        }

        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            ToSql(builder, x => x.ToString(), options);
        }
        /// <inheritdoc/>
        public void ToSql(StringBuilder builder, Func<object, string?> datasetConverterer, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            datasetConverterer.ValidateArgument(nameof(datasetConverterer));

            var dataset = DataSet != null ? datasetConverterer(DataSet) : null;

            if (dataset.HasValue()) builder.Append(dataset).Append('.');
            builder.Append(Sql.All);
        }
    }
}
