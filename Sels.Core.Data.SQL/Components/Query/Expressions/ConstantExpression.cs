using System;
using System.Collections.Generic;
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
            Value = value.ValidateArgument(nameof(value));
        }

        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, QueryBuilderOptions options = QueryBuilderOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            var type = Value.GetType();

            if (type.Is<string>())
            {
                builder.Append('\'').Append(Value.ToString()).Append('\'');
            }
            else if (type.Is<DBNull>())
            {
                builder.Append(Sql.Null);
            }
            else if (type.IsAssignableTo<Enum>())
            {
                builder.Append(options.HasFlag(QueryBuilderOptions.EnumAsString) ? Value.ToString() : Value.ChangeType<int>());
            }
            else if (type is IExpression expression)
            {
                expression.ToSql(builder);
            }
            else
            {
                builder.Append(Value.ToString());
            }
        }
    }
}
