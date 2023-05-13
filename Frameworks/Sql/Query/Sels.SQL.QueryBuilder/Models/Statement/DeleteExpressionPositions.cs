namespace Sels.SQL.QueryBuilder.Builder.Statement
{
    /// <summary>
    /// Defines where in a delete query an expression is placed.
    /// </summary>
    public enum DeleteExpressionPositions
    {
        /// <summary>
        /// Expression should be positioned before the tables to delete from.
        /// </summary>
        Before,
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
        /// Expression should be located after all other expressions.
        /// </summary>
        After
    }
}
