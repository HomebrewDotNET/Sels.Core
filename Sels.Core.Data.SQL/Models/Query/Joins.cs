using Sels.Core.Attributes.Enumeration.Value;

namespace Sels.Core.Data.SQL.Query
{
    /// <summary>
    /// Contains the different types of sql joins.
    /// </summary>
    public enum Joins
    {
        /// <summary>
        /// Represents an Sql inner join.
        /// </summary>
        [StringEnumValue(Sql.Joins.InnerJoin)]
        Inner,
        /// <summary>
        /// Represents an Sql left (outer) join.
        /// </summary>
        [StringEnumValue(Sql.Joins.LeftJoin)]
        Left,
        /// <summary>
        /// Represents an Sql right (outer) join.
        /// </summary>
        [StringEnumValue(Sql.Joins.RightJoin)]
        Right,
        /// <summary>
        /// Represents an Sql full (outer) join.
        /// </summary>
        [StringEnumValue(Sql.Joins.FullJoin)]
        Full
    }
}
