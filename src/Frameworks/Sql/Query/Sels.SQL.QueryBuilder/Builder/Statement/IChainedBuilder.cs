namespace Sels.SQL.QueryBuilder.Builder.Statement
{
    /// <summary>
    /// Builder for chaining other builder using and/or.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to build the query for</typeparam>
    /// <typeparam name="TReturn">The type to return for the fluent syntax</typeparam>
    public interface IChainedBuilder<TEntity, out TReturn>
    {
        /// <summary>
        /// Sets the logic operator on how to join the current condition and the condition created after calling this method.
        /// </summary>
        /// <param name="logicOperator">The logic operator to use</param>
        /// <returns>Current builder for method chaining</returns>
        TReturn AndOr(LogicOperators logicOperator = LogicOperators.And);
        /// <summary>
        /// Current condition and the condition created after calling this method either need to result in true.
        /// </summary>
        /// <returns>Current builder for method chaining</returns>
        TReturn Or => AndOr(LogicOperators.Or);
        /// <summary>
        /// Current condition and the condition created after calling this method both need to result in true.
        /// </summary>
        /// <returns>Current builder for method chaining</returns>
        TReturn And => AndOr(LogicOperators.And);
    }
}
