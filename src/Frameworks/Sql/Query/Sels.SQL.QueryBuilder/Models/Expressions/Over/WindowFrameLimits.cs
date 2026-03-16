using Sels.Core.Attributes.Enumeration.Value;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.SQL.QueryBuilder.Models.Expressions.Over
{
    /// <summary>
    /// Contains how to limit a window frame.
    /// </summary>
    public enum WindowFrameLimits
    {
        /// <summary>
        /// Limits a windows frame using the ROWS clause.
        /// </summary>
        [StringEnumValue(Sql.Over.Rows)]
        Rows,
        /// <summary>
        /// Limits a windows frame using the RANGE clause.
        /// </summary>
        [StringEnumValue(Sql.Over.Range)]
        Range,
        /// <summary>
        /// Limits a windows frame using the GROUPS clause.
        /// </summary>
        [StringEnumValue(Sql.Over.Groups)]
        Groups
    }
}
