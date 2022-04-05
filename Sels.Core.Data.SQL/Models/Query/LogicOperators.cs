using Sels.Core.Attributes.Enumeration.Value;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Data.SQL.Query
{
    /// <summary>
    /// Defines how 2 conditions are compared.
    /// </summary>
    public enum LogicOperators
    {
        /// <inheritdoc cref="Sql.LogicOperators.And"/>
        [StringEnumValue(Sql.LogicOperators.And)]
        And,
        /// <inheritdoc cref="Sql.LogicOperators.Or"/>
        [StringEnumValue(Sql.LogicOperators.Or)]
        Or
    }
}
