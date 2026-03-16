using Sels.Core.Attributes.Enumeration.Value;

namespace Sels.SQL.QueryBuilder.Builder
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
