namespace Sels.SQL.QueryBuilder.Builder.Statement
{
    /// <summary>
    /// Defines where in a update query an expression is placed.
    /// </summary>
    public enum UpdateExpressionPositions
    {
        /// <summary>
        /// Expression should be positioned before the tables to update.
        /// </summary>
        Before,
        /// <summary>
        /// Expression should be located where the tables to update are located.
        /// </summary>
        Table,
        /// <summary>
        /// Expression should be located after the tables to update.
        /// </summary>
        AfterTable,
        /// <summary>
        /// Expression should be located where the join statements are located.
        /// </summary>
        Join,
        /// <summary>
        /// Expression should be located after the join statements.
        /// </summary>
        AfterJoin,
        /// <summary>
        /// Expression should be located where the set statements are located.
        /// </summary>
        Set,
        /// <summary>
        /// Expression should be located after the set statements.
        /// </summary>
        AfterSet,
        /// <summary>
        /// Expression should be located with the other conditions.
        /// </summary>
        Where,
        /// <summary>
        /// Expression should be located after the conditions.
        /// </summary>
        AfterWhere,
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
