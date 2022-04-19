using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Data.SQL.Query
{
    /// <summary>
    /// Defines where in a select query an expression is placed.
    /// </summary>
    public enum SelectExpressionPositions
    {
        /// <summary>
        /// Expression should be positioned before the select columns.
        /// </summary>
        Before,
        /// <summary>
        /// Expression should be located with the columns.
        /// </summary>
        Column,
        /// <summary>
        /// Expression should be located after the select columns.
        /// </summary>
        AfterColumn,
        /// <summary>
        /// Expression should be located where the from statement is located.
        /// </summary>
        From,
        /// <summary>
        /// Expression should be located after the from statement.
        /// </summary>
        AfterFrom,
        /// <summary>
        /// Expression should be located where the join statements are located.
        /// </summary>
        Join,
        /// <summary>
        /// Expression should be located after the join statements.
        /// </summary>
        AfterJoin,
        /// <summary>
        /// Expression should be located with the other conditions.
        /// </summary>
        Where,
        /// <summary>
        /// Expression should be located after the conditions.
        /// </summary>
        AfterWhere,
        /// <summary>
        /// Expression should be located with the order group by statements.
        /// </summary>
        GroupBy,
        /// <summary>
        /// Expression should be located after the group by statements.
        /// </summary>
        AfterGroupBy,
        /// <summary>
        /// Expression should be located with the order by statements.
        /// </summary>
        OrderBy,
        /// <summary>
        /// Expression should be located after all other expressions.
        /// </summary>
        After
    }
}
