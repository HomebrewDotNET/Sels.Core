using System;

namespace Sels.SQL.QueryBuilder.Builder
{
    /// <summary>
    /// Exposes extra settings when compiling expressions or query builders into sql.
    /// </summary>
    [Flags]
    public enum ExpressionCompileOptions
    {
        /// <summary>
        /// No selected options.
        /// </summary>
        None = 0,
        /// <summary>
        /// Formats the query to a more human readable format.
        /// </summary>
        Format = 1,
        /// <summary>
        /// Converts enums to string instead of int.
        /// </summary>
        EnumAsString = 2,
        /// <summary>
        /// Appends a ; after each statement.
        /// </summary>
        AppendSeparator = 4,
        /// <summary>
        /// Disables any implicit expressions that get added by the builders themselves. Set automatically when TEntity is object.
        /// </summary>
        NoImplitExpressions = 8,
        /// <summary>
        /// Use the Utc time for dates.
        /// </summary>
        DateAsUtc = 16
    }
}
