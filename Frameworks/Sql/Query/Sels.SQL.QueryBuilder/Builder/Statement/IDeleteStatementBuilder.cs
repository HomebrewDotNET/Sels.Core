using Sels.SQL.QueryBuilder.Builder.Expressions;
using Sels.Core.Extensions;

namespace Sels.SQL.QueryBuilder.Builder.Statement
{
    /// <summary>
    /// Exposes methods for building a sql delete query.
    /// </summary>
    /// <typeparam name="TDerived">The type to return for the fluent syntax</typeparam>
    /// <typeparam name="TEntity">The main entity to delete</typeparam>
    public interface IDeleteStatementBuilder<TEntity, out TDerived> : IStatementQueryBuilder<TEntity, DeleteExpressionPositions, TDerived>, IStatementConditionBuilder<TEntity, TDerived>, IStatementJoinBuilder<TEntity, TDerived>
    {
        #region From
        /// <summary>
        /// Defines the table to delete from.
        /// </summary>
        /// <param name="database">Optional database to select the table from</param>
        /// <param name="schema">Optional schema where the table is defined in</param>
        /// <param name="table">The table to select from</param>
        /// <param name="datasetAlias">Optional alias for the dataset. If a type is used the alias defined for the type is taken</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived From(string table, object datasetAlias = null, string database = null, string schema = null) => Expression(new TableExpression(database, schema, table.ValidateArgumentNotNullOrWhitespace(nameof(table)), datasetAlias), DeleteExpressionPositions.From);
        /// <summary>
        /// Defines the table to delete from by using the name of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to get the table name from</typeparam>
        /// <param name="database">Optional database to select the table from</param>
        /// <param name="schema">Optional schema where the table is defined in</param>
        /// <param name="datasetAlias">Optional alias for the dataset. If a type is used the alias defined for the type is taken</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived From<T>(object datasetAlias = null, string database = null, string schema = null) => From(typeof(T).Name, datasetAlias ?? typeof(T), database, schema);
        /// <summary>
        /// Defines the table to delete from by using the name of <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="database">Optional database to select the table from</param>
        /// <param name="schema">Optional schema where the table is defined in</param>
        /// <param name="datasetAlias">Optional alias for the dataset. If a type is used the alias defined for the type is taken</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived From(object datasetAlias = null, string database = null, string schema = null) => From<TEntity>(datasetAlias, database, schema);
        #endregion
    }
    /// <inheritdoc cref="IDeleteStatementBuilder{TEntity, TDerived}"/>
    public interface IDeleteStatementBuilder<TEntity> : IDeleteStatementBuilder<TEntity, IDeleteStatementBuilder<TEntity>>
    {

    }
}
