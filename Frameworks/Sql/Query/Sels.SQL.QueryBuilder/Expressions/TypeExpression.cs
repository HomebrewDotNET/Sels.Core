using Sels.Core.Extensions;
using Sels.SQL.QueryBuilder.Builder;
using Sels.SQL.QueryBuilder.Builder.Expressions;
using Sels.SQL.QueryBuilder.Templates.Query.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.SQL.QueryBuilder.Expressions
{
    /// <summary>
    /// Expression that maps a .net type to an SQL type. (e.g. System.String to VARCHAR, ...)
    /// </summary>
    public class TypeExpression : BaseTypeExpression
    {
        // Properties
        /// <summary>
        /// The .net type to map to a SQL type.
        /// </summary>
        public Type Type { get; }
        /// <summary>
        /// Length for the target sql type if it supports a length. (e.g. VARCHAR(20), VARCHAR(MAX), ...)
        /// </summary>
        public int? Length { get; set; }

        /// <inheritdoc cref="TypeExpression"/>
        /// <param name="type"><inheritdoc cref="Type"/></param>
        /// <param name="length"><inheritdoc cref="Length"/></param>
        public TypeExpression(Type type, int? length = null)
        {
            Type = type.ValidateArgument(nameof(type));
            if(length.HasValue) Length = length.Value.ValidateArgumentLargerOrEqual(nameof(length), 0);
        }

        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, Action<StringBuilder, Type, int?> typeConverter, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            typeConverter.ValidateArgument(nameof(typeConverter));

            typeConverter(builder, Type, Length);
        }
    }
}
