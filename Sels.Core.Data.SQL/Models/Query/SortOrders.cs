using Sels.Core.Attributes.Enumeration.Value;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Data.SQL.Query
{
    /// <summary>
    /// Defines the sort order.
    /// </summary>
    public enum SortOrders
    {
        /// <summary>
        /// Order by smallest first.
        /// </summary>
        [StringEnumValue(Sql.SortOrders.Asc)]
        Ascending,
        /// <summary>
        /// Order by largest first.
        /// </summary>
        [StringEnumValue(Sql.SortOrders.Desc)]
        Descending
    }
}
