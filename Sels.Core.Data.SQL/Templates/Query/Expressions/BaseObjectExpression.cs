using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Data.SQL.Query.Expressions
{
    /// <summary>
    /// Template for creating a <see cref="IObjectExpression"/>.
    /// </summary>
    public abstract class BaseObjectExpression : BaseDataSetExpression, IObjectExpression
    {
        /// <inheritdoc/>
        public string Object { get; }

        /// <inheritdoc cref="BaseObjectExpression"/>
        /// <param name="dataset"><inheritdoc cref="IDataSetExpression.DataSet"/></param>
        /// <param name="objectName"><inheritdoc cref="Object"/></param>
        public BaseObjectExpression(object? dataset, string objectName) : base(dataset)
        {
            Object = objectName.ValidateArgumentNotNullOrWhitespace(nameof(objectName));
        }
        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, Func<object, string?> datasetConverterer, QueryBuilderOptions options = QueryBuilderOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            datasetConverterer.ValidateArgument(nameof(datasetConverterer));

            ToSql(builder, datasetConverterer, x => x, options);
        }

        /// <inheritdoc/>
        public abstract void ToSql(StringBuilder builder, Func<object, string?> datasetConverterer, Func<string, string>? objectConverter, QueryBuilderOptions options = QueryBuilderOptions.None);
    }
}
