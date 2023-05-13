using System.Text;
using Sels.Core.Attributes.Enumeration.Value;

namespace Sels.SQL.QueryBuilder.Builder.Expressions
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
        public override void ToSql(StringBuilder builder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));

            builder.Append(Value.GetStringValue());
        }
    }
}
