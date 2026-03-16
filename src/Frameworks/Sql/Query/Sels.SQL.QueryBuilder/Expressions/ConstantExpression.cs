using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Reflection;
using System;
using System.Globalization;
using System.Text;

namespace Sels.SQL.QueryBuilder.Builder.Expressions
{
    /// <summary>
    /// Expression that represents a constant value in a sql query.
    /// </summary>
    public class ConstantExpression : BaseExpression, IExpression
    {
        /// <summary>
        /// The value of the constant expression.
        /// </summary>
        public object Value { get; }

        /// <inheritdoc cref="ConstantExpression"/>
        /// <param name="value">The constant value</param>
        public ConstantExpression(object value)
        {
            Value = value;
        }

        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            var type = Value?.GetType();

            if (type == null || type.Is<DBNull>())
            {
                builder.Append(Sql.Null);
            }
            else if (type.Is<bool>())
            {
                var boolean = Value.CastTo<bool>();
                builder.Append(boolean ? 1 : 0);
            }
            else if (type.IsAssignableTo<Enum>())
            {
                builder.Append(options.HasFlag(ExpressionCompileOptions.EnumAsString) ? Value : Value.ChangeType<int>());
            }
            else if (type.Is<DateTimeOffset>())
            {
                var castedDate = Value.CastTo<DateTimeOffset>();
                if (options.HasFlag(ExpressionCompileOptions.DateAsUtc)) castedDate = castedDate.UtcDateTime;

                builder.Append('\'').Append(castedDate.ToString("yyyy-MM-dd HH:mm:ss")).Append('\'');
            }
            else if (type.Is<DateTime>())
            {
                var castedDate = Value.CastTo<DateTime>();
                if(options.HasFlag(ExpressionCompileOptions.DateAsUtc)) castedDate = castedDate.ToUniversalTime();

                builder.Append('\'').Append(castedDate.ToString("yyyy-MM-dd HH:mm:ss")).Append('\'');
            }
            else if (type.Is<double>())
            {
                builder.Append(Value.CastTo<double>().ToString(CultureInfo.InvariantCulture));
            }
            else if (type.Is<decimal>())
            {
                builder.Append(Value.CastTo<decimal>().ToString(CultureInfo.InvariantCulture));
            }
            else if (type.Is<float>())
            {
                builder.Append(Value.CastTo<float>().ToString(CultureInfo.InvariantCulture));
            }
            else if (type.IsNumeric())
            {
                builder.Append(Value);
            }
            else if (type is IExpression expression)
            {
                expression.ToSql(builder);
            }
            else
            {
                builder.Append('\'').Append(Value).Append('\'');
            }
        }
    }
}
