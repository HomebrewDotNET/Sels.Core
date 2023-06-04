using System;

namespace Sels.SQL.QueryBuilder.Builder.Statement
{
    /// <summary>
    /// Builder for creating joins in sql queries.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to build the query for</typeparam>
    /// <typeparam name="TDerived">The type to return for the fluent syntax</typeparam>
    public interface IStatementJoinBuilder<TEntity, out TDerived>
    {
        /// <summary>
        /// Creates a builder for creating a join to another table.
        /// </summary>
        /// <param name="joinType">The type of join to perform</param>
        /// <returns>Builder for selecting the table to join</returns>
        IStatementJoinTableBuilder<TEntity, TDerived> Join(Joins joinType);
        /// <summary>
        /// Creates a builder for creating an inner join to another table.
        /// </summary>
        /// <returns>Builder for selecting the table to join</returns>
        IStatementJoinTableBuilder<TEntity, TDerived> InnerJoin() => Join(Joins.Inner);
        /// <summary>
        /// Creates a builder for creating a left join to another table.
        /// </summary>
        /// <returns>Builder for selecting the table to join</returns>
        IStatementJoinTableBuilder<TEntity, TDerived> LeftJoin() => Join(Joins.Left);
        /// <summary>
        /// Creates a builder for creating a right join to another table.
        /// </summary>
        /// <returns>Builder for selecting the table to join</returns>
        IStatementJoinTableBuilder<TEntity, TDerived> RightJoin() => Join(Joins.Right);
        /// <summary>
        /// Creates a builder for creating a full join to another table.
        /// </summary>
        /// <returns>Builder for selecting the table to join</returns>
        IStatementJoinTableBuilder<TEntity, TDerived> FullJoin() => Join(Joins.Full);
    }
    /// <summary>
    /// Builder for selecting the table to join.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to build the query for</typeparam>
    /// <typeparam name="TDerived">The type to return for the fluent syntax</typeparam>
    public interface IStatementJoinTableBuilder<TEntity, out TDerived>
    {
        /// <summary>
        /// Defines the table to join.
        /// </summary>
        /// <param name="table">The table to join from</param>
        /// <param name="datasetAlias">Optional alias for the dataset. If a type is used the alias defined for the type is taken</param>
        /// <param name="database">Optional database to select the table from</param>
        /// <param name="schema">Optional schema where the table is defined in</param>
        /// <returns>Current builder for method chaining</returns>
        IStatementJoinOnBuilder<TEntity, TDerived> Table(string table, object? datasetAlias = null, string? database = null, string? schema = null);
        /// <summary>
        /// Defines the table to join by using the name of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to get the table name from</typeparam>
        /// <param name="datasetAlias">Optional alias for the dataset. If a type is used the alias defined for the type is taken</param>
        /// <param name="database">Optional database to select the table from</param>
        /// <param name="schema">Optional schema where the table is defined in</param>
        /// <returns>Current builder for method chaining</returns>
        IStatementJoinOnBuilder<TEntity, TDerived> Table<T>(object? datasetAlias = null, string? database = null, string? schema = null) => Table(typeof(T).Name, datasetAlias ?? typeof(T), database, schema);
        /// <summary>
        /// Defines the table to join by using the name of <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="datasetAlias">Optional alias for the dataset. If a type is used the alias defined for the type is taken</param>
        /// <param name="database">Optional database to select the table from</param>
        /// <param name="schema">Optional schema where the table is defined in</param>
        /// <returns>Current builder for method chaining</returns>
        IStatementJoinOnBuilder<TEntity, TDerived> Table(object? datasetAlias = null, string? database = null, string? schema = null) => Table<TEntity>(datasetAlias, database, schema);
    }
    /// <summary>
    /// Builder for defining the join condition.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to build the query for</typeparam>
    /// <typeparam name="TDerived">The type to return for the fluent syntax</typeparam>
    public interface IStatementJoinOnBuilder<TEntity, out TDerived>
    {
        /// <summary>
        /// Defines what to join on using <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">Builder for adding conditions</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived On(Action<IStatementJoinConditionBuilder<TEntity>> builder);
    }
    /// <summary>
    /// Builder for selecting the expression on the left side of a join condition.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to build the query for</typeparam>
    public interface IStatementJoinConditionBuilder<TEntity> : ISharedExpressionBuilder<TEntity, IComparisonExpressionBuilder<TEntity, IStatementJoinFinalConditionBuilder<TEntity>>>
    {

    }
    /// <summary>
    /// Builder for selecting the expression on the right side of a join condition.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to build the query for</typeparam>
    public interface IStatementJoinFinalConditionBuilder<TEntity> : ISharedExpressionBuilder<TEntity, IChainedBuilder<TEntity, IStatementJoinConditionBuilder<TEntity>>>
    {

    }
}
