using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sels.Core.Extensions.Reflection;

namespace Sels.Core.Data.SQL.Query.Expressions
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
        public override void ToSql(StringBuilder builder, QueryBuilderOptions options = QueryBuilderOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            var type = Value?.GetType();

            if (type == null || type.Is<DBNull>())
            {
                builder.Append(Sql.Null);
            }
            else if (type.IsAssignableTo<Enum>())
            {
                builder.Append(options.HasFlag(QueryBuilderOptions.EnumAsString) ? Value : Value.ChangeType<int>());
            }
            else if (type.Is<DateTime>())
            {
                builder.Append('\'').Append(Value.Cast<DateTime>().ToString("yyyy-MM-dd HH:mm:ss")).Append('\'');
            }
            else if (type.Is<double>())
            {
                builder.Append(Value.Cast<double>().ToString(CultureInfo.InvariantCulture));
            }
            else if (type.Is<decimal>())
            {
                builder.Append(Value.Cast<decimal>().ToString(CultureInfo.InvariantCulture));
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
