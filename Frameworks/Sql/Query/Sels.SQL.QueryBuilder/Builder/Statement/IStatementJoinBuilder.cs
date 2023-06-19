using Sels.Core.Extensions;
using Sels.SQL.QueryBuilder.Builder.Expressions;
using System;
using System.Text;

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
        /// <returns>Builder for selecting the result set to join</returns>
        IStatementJoinResultSetBuilder<TEntity, TDerived> Join(Joins joinType);
        /// <summary>
        /// Creates a builder for creating an inner join to another table.
        /// </summary>
        /// <returns>Builder for selecting the result set to join</returns>
        IStatementJoinResultSetBuilder<TEntity, TDerived> InnerJoin() => Join(Joins.Inner);
        /// <summary>
        /// Creates a builder for creating a left join to another table.
        /// </summary>
        /// <returns>Builder for selecting the result set to join</returns>
        IStatementJoinResultSetBuilder<TEntity, TDerived> LeftJoin() => Join(Joins.Left);
        /// <summary>
        /// Creates a builder for creating a right join to another table.
        /// </summary>
        /// <returns>Builder for selecting the result set to join</returns>
        IStatementJoinResultSetBuilder<TEntity, TDerived> RightJoin() => Join(Joins.Right);
        /// <summary>
        /// Creates a builder for creating a full join to another table.
        /// </summary>
        /// <returns>Builder for selecting the result set to join</returns>
        IStatementJoinResultSetBuilder<TEntity, TDerived> FullJoin() => Join(Joins.Full);
    }
    /// <summary>
    /// Builder for selecting the result set to join.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to build the query for</typeparam>
    /// <typeparam name="TDerived">The type to return for the fluent syntax</typeparam>
    public interface IStatementJoinResultSetBuilder<TEntity, out TDerived>
    {
        /// <summary>
        /// Defines the expression that contains the result set to join.
        /// </summary>
        /// <param name="expression">The expression that contains the result set to join</param>
        /// <returns>Current builder for method chaining</returns>
        IStatementJoinOnBuilder<TEntity, TDerived> Expression(IExpression expression);
        /// <summary>
        /// Defines the table to join.
        /// </summary>
        /// <param name="table">The table to join from</param>
        /// <param name="datasetAlias">Optional alias for the dataset. If a type is used the alias defined for the type is taken</param>
        /// <param name="database">Optional database to select the table from</param>
        /// <param name="schema">Optional schema where the table is defined in</param>
        /// <returns>Current builder for method chaining</returns>
        IStatementJoinOnBuilder<TEntity, TDerived> Table(string table, object datasetAlias = null, string database = null, string schema = null) => Expression(new TableExpression(database, schema, table.ValidateArgumentNotNullOrWhitespace(nameof(table)), datasetAlias));
        /// <summary>
        /// Defines the table to join by using the name of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to get the table name from</typeparam>
        /// <param name="datasetAlias">Optional alias for the dataset. If a type is used the alias defined for the type is taken</param>
        /// <param name="database">Optional database to select the table from</param>
        /// <param name="schema">Optional schema where the table is defined in</param>
        /// <returns>Current builder for method chaining</returns>
        IStatementJoinOnBuilder<TEntity, TDerived> Table<T>(object datasetAlias = null, string database = null, string schema = null) => Table(typeof(T).Name, datasetAlias ?? typeof(T), database, schema);
        /// <summary>
        /// Defines the table to join by using the name of <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="datasetAlias">Optional alias for the dataset. If a type is used the alias defined for the type is taken</param>
        /// <param name="database">Optional database to select the table from</param>
        /// <param name="schema">Optional schema where the table is defined in</param>
        /// <returns>Current builder for method chaining</returns>
        IStatementJoinOnBuilder<TEntity, TDerived> Table(object datasetAlias = null, string database = null, string schema = null) => Table<TEntity>(datasetAlias, database, schema);

        #region SubQuery
        /// <summary>
        /// Defines the sub query to join.
        /// </summary>
        /// <param name="query">Delegate that adds the query to the supplied builder</param>
        /// <param name="datasetAlias">Alias for the sub query dataset. If a type is used the alias defined for the type is taken</param>
        /// <returns>Current builder for method chaining</returns>
        IStatementJoinOnBuilder<TEntity, TDerived> SubQuery(Action<StringBuilder, ExpressionCompileOptions> query, object datasetAlias) => Expression(new SubQueryExpression(datasetAlias.ValidateArgument(nameof(datasetAlias)), query.ValidateArgument(nameof(query))));
        /// <summary>
        /// Defines the sub query to join.
        /// </summary>
        /// <param name="query">The sub query</param>
        /// <param name="datasetAlias">Alias for the sub query dataset. If a type is used the alias defined for the type is taken</param>
        /// <returns>Current builder for method chaining</returns>
        IStatementJoinOnBuilder<TEntity, TDerived> SubQuery(string query, object datasetAlias) => SubQuery((b, o) => b.Append(query.ValidateArgumentNotNullOrWhitespace(nameof(query))), datasetAlias.ValidateArgument(nameof(datasetAlias)));
        /// <summary>
        /// Defines the sub query to join.
        /// </summary>
        /// <param name="builder">Builder that creates the sub query</param>
        /// <param name="datasetAlias">Alias for the sub query dataset. If a type is used the alias defined for the type is taken</param>
        /// <returns>Current builder for method chaining</returns>
        IStatementJoinOnBuilder<TEntity, TDerived> SubQuery(IQueryBuilder builder, object datasetAlias) => Expression(new SubQueryExpression(datasetAlias.ValidateArgument(nameof(datasetAlias)), builder.ValidateArgument(nameof(builder))));
        /// <summary>
        /// Defines the sub query to join.
        /// </summary>
        /// <typeparam name="T">The type to get the dataset alias from</typeparam>
        /// <param name="query">Delegate that adds the query to the supplied builder</param>
        /// <returns>Current builder for method chaining</returns>
        IStatementJoinOnBuilder<TEntity, TDerived> SubQuery<T>(Action<StringBuilder, ExpressionCompileOptions> query) => SubQuery(query, typeof(T));
        /// <summary>
        /// Defines the sub query to join.
        /// </summary>
        /// <typeparam name="T">The type to get the dataset alias from</typeparam>
        /// <param name="query">The sub query</param>
        /// <returns>Current builder for method chaining</returns>
        IStatementJoinOnBuilder<TEntity, TDerived> SubQuery<T>(string query) => SubQuery(query, typeof(T));
        /// <summary>
        /// Defines the sub query to join.
        /// </summary>
        /// <typeparam name="T">The type to get the dataset alias from</typeparam>
        /// <param name="builder">Builder that creates the sub query</param>
        /// <returns>Current builder for method chaining</returns>
        IStatementJoinOnBuilder<TEntity, TDerived> SubQuery<T>(IQueryBuilder builder) => SubQuery(builder, typeof(T));
        #endregion
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
