using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Data.SQL.Query
{
    /// <summary>
    /// Exposes extra settings when compiling expressions or query builders into sql.
    /// </summary>
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
        AppendSeparator = 3
    }
}
