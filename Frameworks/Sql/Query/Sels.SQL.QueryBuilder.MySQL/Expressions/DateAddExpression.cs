using Sels.Core.Extensions;
using Sels.SQL.QueryBuilder.Builder;
using Sels.SQL.QueryBuilder.Builder.Expressions;
using Sels.SQL.QueryBuilder.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.SQL.QueryBuilder.MySQL.Expressions
{
    /// <summary>
    /// MySQL expression that modifies a date based on an amount and interval.
    /// </summary>
    public class DateAddExpression : BaseExpressionContainer
    {
        // Properties
        /// <summary>
        /// Expression that contains the date to modify.
        /// </summary>
        public IExpression Date { get; }
        /// <summary>
        /// Expression that contains the amount to add/substract.
        /// </summary>
        public IExpression Amount { get; }
        /// <inheritdoc cref="DateInterval"/>
        public DateInterval Interval { get; }

        /// <inheritdoc cref="DateAddExpression"/>
        /// <param name="date"><inheritdoc cref="Date"/></param>
        /// <param name="amount"><inheritdoc cref="Amount"/></param>
        /// <param name="interval"><inheritdoc cref="Interval"/></param>
        public DateAddExpression(IExpression date, IExpression amount, DateInterval interval)
        {
            Date = date.ValidateArgument(nameof(date));
            Amount = amount.ValidateArgument(nameof(amount));
            Interval = interval;
        }

        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, Action<StringBuilder, IExpression> subBuilder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            subBuilder.ValidateArgument(nameof(subBuilder));

            builder.Append(MySql.Functions.DateAdd).Append('(');

            // Add date
            subBuilder(builder, Date);
            builder.Append(',').AppendSpace();

            // Append amount
            builder.Append("INTERVAL").AppendSpace();
            subBuilder(builder, Amount);
            builder.AppendSpace();


            // Append interval
            switch (Interval)
            {
                case DateInterval.Millisecond:
                    builder.Append("MICROSECOND");
                    break;
                default:
                    builder.Append(Interval.ToString().ToUpper());
                    break;
            }
            builder.Append(')');
        }
    }
}
