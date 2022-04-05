using Sels.Core.Data.SQL.Query.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Data.SQL.Query.Expressions
{
    /// <summary>
    /// Template for creating new <see cref="IColumnExpression"/> expressions.
    /// </summary>
    public abstract class BaseColumnExpression : BaseObjectExpression, IColumnExpression
    {
        /// <inheritdoc/>
        public string? Alias { get; }

        /// <inheritdoc cref="BaseColumnExpression"/>
        /// <param name="dataset"><inheritdoc cref="IDataSetExpression.DataSet"/></param>
        /// <param name="objectName"><inheritdoc cref="IObjectExpression.Object"/></param>
        /// <param name="alias"><inheritdoc cref="Alias"/></param>
        public BaseColumnExpression(object? dataset, string objectName, string? alias = null) : base(dataset, objectName)
        {
            Alias = alias;
        }
        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, Func<object, string?> datasetConverterer, Func<string, string>? objectConverter, QueryBuilderOptions options = QueryBuilderOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            datasetConverterer.ValidateArgument(nameof(datasetConverterer));
            objectConverter.ValidateArgument(nameof(objectConverter));

            ToSql(builder, datasetConverterer, objectConverter, true, options);
        }

        // Abstractions
        /// <inheritdoc/>
        public abstract void ToSql(StringBuilder builder, Func<object, string?> datasetConverterer, Func<string, string>? objectConverter, bool includeAlias = true, QueryBuilderOptions options = QueryBuilderOptions.None);
    }
}
