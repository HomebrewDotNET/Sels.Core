using Sels.Core.Attributes.Enumeration.Value;

namespace Sels.SQL.QueryBuilder.Builder
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
