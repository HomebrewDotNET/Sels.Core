using Sels.SQL.QueryBuilder.Builder.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.SQL.QueryBuilder.Expressions
{
    /// <summary>
    /// Expression that returns the current date.
    /// </summary>
    public class CurrentDateExpression : CustomCompilerExpression
    {
        // Properties
        /// <summary>
        /// Determines which current date to use.
        /// </summary>
        public DateType Type { get; } = DateType.Utc;

        /// <inheritdoc cref="CurrentDateExpression"/>
        /// <param name="type"><inheritdoc cref="Type"/></param>
        public CurrentDateExpression(DateType type = DateType.Utc)
        {
            Type = type;
        }
    }

    /// <summary>
    /// Determines the type of date.
    /// </summary>
    public enum DateType
    {
        /// <summary>
        /// Client date is used.
        /// </summary>
        Local = 0,
        /// <summary>
        /// Utc date is used.
        /// </summary>
        Utc = 1,
        /// <summary>
        /// Server (database) date is used.
        /// </summary>
        Server = 2
    }
}
