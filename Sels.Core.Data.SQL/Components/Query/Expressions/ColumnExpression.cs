using System.Text;

namespace Sels.Core.Data.SQL.Query.Expressions
{
    /// <summary>
    /// Expression that represents an sql column.
    /// </summary>
    public class ColumnExpression : BaseColumnExpression
    {
        /// <inheritdoc cref="ColumnExpression"/>
        /// <param name="dataSet"><inheritdoc cref="IDataSetExpression.DataSet"/></param>
        /// <param name="column"><inheritdoc cref="IObjectExpression.Object"/></param>
        /// <param name="alias"><inheritdoc cref="IColumnExpression.Alias"/></param>
        public ColumnExpression(object? dataSet, string column, string? alias = null) : base(dataSet, column, alias)
        {
        }

        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, Func<object, string?> datasetConverterer, Func<string, string>? columnConverter, bool includeAlias = true, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            datasetConverterer.ValidateArgument(nameof(datasetConverterer));

            var dataset = DataSet != null ? datasetConverterer(DataSet) : null;
            var column = columnConverter != null ? columnConverter(Object) ?? throw new InvalidOperationException($"{nameof(columnConverter)} returned null") : Object;

            if (dataset.HasValue()) builder.Append(dataset).Append('.');
            builder.Append(column);
            if(includeAlias && Alias != null) builder.AppendSpace().Append(Sql.As).AppendSpace().Append(Alias);
        }
    }
}
