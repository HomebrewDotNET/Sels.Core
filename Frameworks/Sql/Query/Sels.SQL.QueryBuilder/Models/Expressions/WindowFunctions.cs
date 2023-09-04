using Sels.Core.Attributes.Enumeration.Value;

namespace Sels.SQL.QueryBuilder.Builder.Expressions
{
    /// <summary>
    /// Contains the sql window functions.
    /// </summary>
    public enum WindowFunctions
    {
        /// <summary>
        /// Function that assigns an incrementing number for each row in the same window frame.
        /// </summary>
        [StringEnumValue(Sql.WindowFunctions.RowNumber)]
        RowNumber,
        /// <summary>
        /// Function that assigns an incrementing number for each row in the same window frame. 
        /// If 2 rows share the same value they will have the same number.
        /// Number is still increased even though if the value isn't assigned.
        /// </summary>
        [StringEnumValue(Sql.WindowFunctions.Dense)]
        Dense,
        /// <summary>
        /// Function that assigns an incrementing number for each row in the same window frame. 
        /// If 2 rows share the same value they will have the same number.
        /// </summary>
        [StringEnumValue(Sql.WindowFunctions.DenseRank)]
        DenseRank,
        /// <summary>
        /// Function that returns the percentile that the row falls into based on a sub division.
        /// </summary>
        [StringEnumValue(Sql.WindowFunctions.Ntile)]
        Ntile,
        /// <summary>
        /// Function that pulls the value from a preceding row.
        /// </summary>
        [StringEnumValue(Sql.WindowFunctions.Lag)]
        Lag,
        /// <summary>
        /// Function that pulls the value from a following row.
        /// </summary>
        [StringEnumValue(Sql.WindowFunctions.Lead)]
        Lead
    }
}
