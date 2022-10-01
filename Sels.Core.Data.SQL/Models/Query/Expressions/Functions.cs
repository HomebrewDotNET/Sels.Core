using Sels.Core.Attributes.Enumeration.Value;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Data.SQL.Query.Expressions
{
    /// <summary>
    /// Contains the sql functions.
    /// </summary>
    public enum Functions
    {
        /// <summary>
        /// Function that returns the largest value of a column.
        /// </summary>
        [StringEnumValue(Sql.Functions.Max)]
        Max,
        /// <summary>
        /// Function that returns the smallest value of a column.
        /// </summary>
        [StringEnumValue(Sql.Functions.Min)]
        Min,
        /// <summary>
        /// Function that returns the averga of a column.
        /// </summary>
        [StringEnumValue(Sql.Functions.Avg)]
        Avg,
        /// <summary>
        /// Function that returns the total sum of a column.
        /// </summary>
        [StringEnumValue(Sql.Functions.Sum)]
        Sum,
        /// <summary>
        /// Function that counts how many columns aren't null or how many rows are returned.
        /// </summary>
        [StringEnumValue(Sql.Functions.Count)]
        Count
    }
}
