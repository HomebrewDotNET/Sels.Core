using Sels.Core.Extensions;
using Sels.SQL.QueryBuilder.Builder.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.SQL.QueryBuilder.Expressions
{
    /// <summary>
    /// Expression that modifies a date based on an amount and interval.
    /// </summary>
    public class ModifyDateExpression : CustomCompilerExpression
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

        /// <inheritdoc cref="ModifyDateExpression"/>
        /// <param name="date"><inheritdoc cref="Date"/></param>
        /// <param name="amount"><inheritdoc cref="Amount"/></param>
        /// <param name="interval"><inheritdoc cref="Interval"/></param>
        public ModifyDateExpression(IExpression date, IExpression amount, DateInterval interval)
        {
            Date = date.ValidateArgument(nameof(date));
            Amount = amount.ValidateArgument(nameof(amount));
            Interval = interval;
        }
    }

    /// <summary>
    /// Determines the interval to add/substract to/from a date.
    /// </summary>
    public enum DateInterval
    {
        /// <summary>
        /// Interval is in milliseconds.
        /// </summary>
        Millisecond = 0,
        /// <summary>
        /// Interval is in seconds.
        /// </summary>
        Second = 1,
        /// <summary>
        /// Interval is in minutes.
        /// </summary>
        Minute = 2,
        /// <summary>
        /// Intergval is in hours.
        /// </summary>
        Hour = 3,
        /// <summary>
        /// Interval is in days.
        /// </summary>
        Day = 4,
        /// <summary>
        /// Interval is in weeks.
        /// </summary>
        Week = 5,
        /// <summary>
        /// Interval is in months.
        /// </summary>
        Month = 6,
        /// <summary>
        /// Interval is in years.
        /// </summary>
        Year = 7
    }
}
