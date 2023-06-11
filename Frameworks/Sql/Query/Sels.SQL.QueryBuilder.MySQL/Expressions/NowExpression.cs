using Sels.Core.Extensions;
using Sels.SQL.QueryBuilder.Builder;
using Sels.SQL.QueryBuilder.Builder.Expressions;
using Sels.SQL.QueryBuilder.Expressions;
using System;
using System.Collections.Generic;
using System.Text;
using SqlConstantExpression = Sels.SQL.QueryBuilder.Builder.Expressions.ConstantExpression;

namespace Sels.SQL.QueryBuilder.MySQL.Expressions
{
    /// <summary>
    /// MySQL expression that returns the current date.
    /// </summary>
    public class NowExpression : BaseExpressionContainer
    {
        // Properties
        /// <summary>
        /// Determines which current date to use.
        /// </summary>
        public DateType Type { get; } = DateType.Utc;

        /// <inheritdoc cref="NowExpression"/>
        /// <param name="type"><inheritdoc cref="Type"/></param>
        public NowExpression(DateType type = DateType.Utc)
        {
            Type = type;
        }

        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, Action<StringBuilder, IExpression> subBuilder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            subBuilder.ValidateArgument(nameof(subBuilder));

            switch (Type)
            {
                case DateType.Server:
                    builder.Append(MySql.Functions.Now).Append("()");
                    break;
                case DateType.Utc:
                    builder.Append(MySql.Functions.UtcNow).Append("()");
                    break;
                case DateType.Local:
                    subBuilder(builder, new SqlConstantExpression(DateTimeOffset.Now));
                    break;
                default:
                    throw new NotSupportedException($"Date type <{Type}> is not supported");
            }
        }
    }
}
