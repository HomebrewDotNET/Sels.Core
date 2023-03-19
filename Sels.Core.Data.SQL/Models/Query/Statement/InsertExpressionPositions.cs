namespace Sels.Core.Data.SQL.Query.Statement
{
    /// <summary>
    /// Defines where in an insert query an expression is placed.
    /// </summary>
    public enum InsertExpressionPositions
    {
        /// <summary>
        /// Expression should be positioned before the table to insert into.
        /// </summary>
        Before,
        /// <summary>
        /// Expression should be located where the into keyword is located.
        /// </summary>
        Into,
        /// <summary>
        /// Expression should be located after the into statement.
        /// </summary>
        AfterInto,
        /// <summary>
        /// Expression should be located where the columns to insert into are located.
        /// </summary>
        Columns,
        /// <summary>
        /// Expression should be located after the insert columns.
        /// </summary>
        AfterColumns,
        /// <summary>
        /// Expression should be located where the values to insert are located.
        /// </summary>
        Values,
        /// <summary>
        /// Expression should be located after all other expressions.
        /// </summary>
        After
    }
}
