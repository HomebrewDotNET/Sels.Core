using Sels.Core.Attributes.Enumeration.Value;

namespace Sels.SQL.QueryBuilder.Builder.Expressions
{
    /// <summary>
    /// Defines how 2 sql expressions should be compared.
    /// </summary>
    public enum Operators
    {
        /// <inheritdoc cref="Sql.ConditionOperators.Equal"/>
        [StringEnumValue(Sql.ConditionOperators.Equal)]
        Equal,
        /// <inheritdoc cref="Sql.ConditionOperators.NotEqual"/>
        [StringEnumValue(Sql.ConditionOperators.NotEqual)]
        NotEqual,
        /// <inheritdoc cref="Sql.ConditionOperators.Greater"/>
        [StringEnumValue(Sql.ConditionOperators.Greater)]
        Greater,
        /// <inheritdoc cref="Sql.ConditionOperators.Less"/>
        [StringEnumValue(Sql.ConditionOperators.Less)]
        Less,
        /// <inheritdoc cref="Sql.ConditionOperators.GreaterOrEqual"/>
        [StringEnumValue(Sql.ConditionOperators.GreaterOrEqual)]
        GreaterOrEqual,
        /// <inheritdoc cref="Sql.ConditionOperators.LessOrEqual"/>
        [StringEnumValue(Sql.ConditionOperators.LessOrEqual)]
        LessOrEqual,
        /// <inheritdoc cref="Sql.ConditionOperators.In"/>
        [StringEnumValue(Sql.ConditionOperators.In)]
        In,
        /// <inheritdoc cref="Sql.ConditionOperators.NotIn"/>
        [StringEnumValue(Sql.ConditionOperators.NotIn)]
        NotIn,
        /// <inheritdoc cref="Sql.ConditionOperators.Like"/>
        [StringEnumValue(Sql.ConditionOperators.Like)]
        Like,
        /// <inheritdoc cref="Sql.ConditionOperators.NotLike"/>
        [StringEnumValue(Sql.ConditionOperators.NotLike)]
        NotLike,
        /// <inheritdoc cref="Sql.ConditionOperators.Exists"/>
        [StringEnumValue(Sql.ConditionOperators.Exists)]
        Exists,
        /// <inheritdoc cref="Sql.ConditionOperators.NotExists"/>
        [StringEnumValue(Sql.ConditionOperators.NotExists)]
        NotExists,
        /// <inheritdoc cref="Sql.ConditionOperators.Between"/>
        [StringEnumValue(Sql.ConditionOperators.Between)]
        Between,
        /// <inheritdoc cref="Sql.ConditionOperators.NotBetween"/>
        [StringEnumValue(Sql.ConditionOperators.NotBetween)]
        NotBetween,
        /// <inheritdoc cref="Sql.ConditionOperators.Is"/>
        [StringEnumValue(Sql.ConditionOperators.Is)]
        Is,
        /// <inheritdoc cref="Sql.ConditionOperators.IsNot"/>
        [StringEnumValue(Sql.ConditionOperators.IsNot)]
        IsNot
    }
}
