using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sels.Core.Attributes.Enumeration.Value;

namespace Sels.Core.Data.SQL.Query.Expressions
{
    /// <summary>
    /// Expression where the string enum value is used as sql constant.
    /// </summary>
    public class EnumExpression<T> : BaseExpression, IExpression where T : Enum
    {
        /// <summary>
        /// The enum value of the expression.
        /// </summary>
        public T Value { get; }

        /// <inheritdoc cref="EnumExpression{T}"/>
        /// <param name="value"><inheritdoc cref="Value"/></param>
        public EnumExpression(T value)
        {
            Value = value.ValidateArgument(nameof(value));
        }

        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, QueryBuilderOptions options = QueryBuilderOptions.None)
        {
            builder.ValidateArgument(nameof(builder));

            builder.Append(Value.GetStringValue());
        }
    }
}
