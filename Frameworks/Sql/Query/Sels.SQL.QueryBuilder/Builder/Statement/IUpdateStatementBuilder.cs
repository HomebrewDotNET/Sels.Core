using Sels.SQL.QueryBuilder.Builder.Expressions;

namespace Sels.SQL.QueryBuilder.Builder.Statement
{
    /// <summary>
    /// Exposes methods for building a sql update query.
    /// </summary>
    /// <typeparam name="TDerived">The type to return for the fluent syntax</typeparam>
    /// <typeparam name="TEntity">The main entity to update</typeparam>
    public interface IUpdateStatementBuilder<TEntity, out TDerived> : IStatementQueryBuilder<TEntity, UpdateExpressionPositions, TDerived>, IStatementSetBuilder<TEntity, ISharedExpressionBuilder<TEntity, TDerived>>, IStatementConditionBuilder<TEntity, TDerived>, IStatementJoinBuilder<TEntity, TDerived>
    {
        #region Table
        /// <summary>
        /// Defines the table to update.
        /// </summary>
        /// <param name="table">The table to select from</param>
        /// <param name="datasetAlias">Optional alias for the dataset. If a type is used the alias defined for the type is taken</param>
        /// <param name="database">Optional database to select the table from</param>
        /// <param name="schema">Optional schema where the table is defined in</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Table(string table, object? datasetAlias = null, string? database = null, string? schema = null) => Expression(new TableExpression(database, schema, table.ValidateArgumentNotNullOrWhitespace(nameof(table)), datasetAlias), UpdateExpressionPositions.Table);
        /// <summary>
        /// Defines the table to update by using the name of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to get the table name from</typeparam>
        /// <param name="datasetAlias">Optional alias for the dataset. If a type is used the alias defined for the type is taken</param>
        /// <param name="database">Optional database to select the table from</param>
        /// <param name="schema">Optional schema where the table is defined in</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Table<T>(object? datasetAlias = null, string? database = null, string? schema = null) => Table(typeof(T).Name, datasetAlias ?? typeof(T), database, schema);
        /// <summary>
        /// Defines the table to update by using the name of <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="datasetAlias">Optional alias for the dataset. If a type is used the alias defined for the type is taken</param>
        /// <param name="database">Optional database to select the table from</param>
        /// <param name="schema">Optional schema where the table is defined in</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Table(object? datasetAlias = null, string? database = null, string? schema = null) => Table<TEntity>(datasetAlias, database, schema);
        #endregion

        #region Set        
        #region From
        /// <summary>
        /// Sets the columns to update using the public properties on <typeparamref name="T"/> as column names and using the values from <paramref name="valueObject"/>.
        /// </summary>
        /// <typeparam name="T">The type to get the properties from</typeparam>
        /// <param name="valueObject">Object containing the values to update with</param>
        /// <param name="dataset">Overwrites the default dataset name defined for type <typeparamref name="T"/>. If a type is used the alias defined for the type is taken. Set to an empty string to omit the dataset alias</param>
        /// <param name="excludedProperties">Optional names of properties to exclude</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived SetUsing<T>(T valueObject, object? dataset = null, params string[] excludedProperties);
        /// <summary>
        /// Sets the columns to update using the public properties on <typeparamref name="T"/> as column names and parameter names.
        /// </summary>
        /// <typeparam name="T">The type to get the properties from</typeparam>
        /// <param name="dataset">Overwrites the default dataset name defined for type <typeparamref name="T"/>. If a type is used the alias defined for the type is taken. Set to an empty string to omit the dataset alias</param>
        /// <param name="excludedProperties">Optional names of properties to exclude</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived SetFrom<T>(object? dataset = null, params string[] excludedProperties);
        /// <summary>
        /// Sets the columns to update using the public properties on <typeparamref name="TEntity"/> as column names and parameter names.
        /// </summary>
        /// <param name="dataset">Overwrites the default dataset name defined for type <typeparamref name="TEntity"/>. If a type is used the alias defined for the type is taken. Set to an empty string to omit the dataset alias</param>
        /// <param name="excludedProperties">Optional names of properties to exclude</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived SetFrom(object? dataset = null, params string[] excludedProperties) => SetFrom<TEntity>(dataset, excludedProperties);
        #endregion
        #endregion
    }

    /// <inheritdoc cref="IUpdateStatementBuilder{TEntity, TDerived}"/>
    public interface IUpdateStatementBuilder<TEntity> : IUpdateStatementBuilder<TEntity, IUpdateStatementBuilder<TEntity>>
    {

    }
}
