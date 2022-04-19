using Sels.Core.Data.SQL.Query.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Data.SQL.Query
{
    /// <summary>
    /// Exposes methods for building a sql select query.
    /// </summary>
    /// <typeparam name="TDerived">The type to return for the fluent syntax</typeparam>
    /// <typeparam name="TEntity">The main entity to insert</typeparam>
    public interface ISelectQueryBuilder<TEntity, out TDerived> : IQueryBuilder<TEntity, SelectExpressionPositions, TDerived>, IConditionBuilder<TEntity, TDerived>, IQueryJoinBuilder<TEntity, TDerived>
    {
        #region Select
        #region All
        /// <summary>
        /// Selects all columns.
        /// </summary>
        /// <param name="dataset">Optional dataset to select from. Is the alias assigned to tables / sub queries</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived All(object? dataset = null) => Expression(new AllColumnsExpression(dataset), SelectExpressionPositions.Column);
        /// <summary>
        /// Select all columns from the dataset alias defined by <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to get the dataset alias from</typeparam>
        /// <param name="dataset">Overwrites the default dataset name defined for type <typeparamref name="T"/>. If a type is used the alias defined for the type is taken. Set to an empty string to omit the dataset alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived AllOf<T>(object? dataset = null) => All(dataset ?? typeof(T));
        /// <summary>
        /// Select all columns from the dataset alias defined by <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="dataset">Overwrites the default dataset name defined for type <typeparamref name="TEntity"/>. If a type is used the alias defined for the type is taken. Set to an empty string to omit the dataset alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived AllOf(object? dataset = null) => AllOf<TEntity>(dataset);
        #endregion
        #region Column
        /// <summary>
        /// Specifies a column to select.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select <paramref name="column"/> from</param>
        /// <param name="column">The name of the column to select</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Column(object? dataset, string column, string? columnAlias = null) => Expression(new ColumnExpression(dataset, column.ValidateArgumentNotNullOrWhitespace(nameof(column)), columnAlias), SelectExpressionPositions.Column);
        /// <summary>
        /// Specifies a column to select by using the name of the property selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to select the property from</typeparam>
        /// <param name="dataset">Overwrites the default dataset name defined for type <typeparamref name="T"/>. If a type is used the alias defined for the type is taken. Set to an empty string to omit the dataset alias</param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="columnAlias">Overwrites the default alias of <typeparamref name="T"/></param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Column<T>(object? dataset, Expression<Func<T, object?>> property, string? columnAlias = null) => Column(dataset, property.ValidateArgument(nameof(property)).ExtractProperty(nameof(property)).Name, columnAlias);
        /// <summary>
        /// Specifies a column to select.
        /// </summary>
        /// <param name="column">The name of the column to select</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Column(string column, string? columnAlias = null) => Column(null, column, columnAlias);
        /// <summary>
        /// Specifies a column to select by using the name of the property selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to select the property from</typeparam>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Column<T>(Expression<Func<T, object?>> property, string? columnAlias = null) => Column<T>(typeof(T), property, columnAlias);
        /// <summary>
        /// Specifies a column to select by using the name of the property selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Column(Expression<Func<TEntity, object?>> property, string? columnAlias = null) => Column<TEntity>(property, columnAlias);
        /// <summary>
        /// Specifies a column to select by using the name of the property selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="dataset">Overwrites the default dataset name defined for type <typeparamref name="TEntity"/></param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="columnAlias">Overwrites the default alias of <typeparamref name="TEntity"/></param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Column(object? dataset, Expression<Func<TEntity, object?>> property, string? columnAlias = null) => Column<TEntity>(dataset, property, columnAlias);
        #endregion
        #region Columns
        /// <summary>
        /// Specifies the columns to select.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select <paramref name="columns"/> from</param>
        /// <param name="columns">The columns to select</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Columns(object? dataset, IEnumerable<string> columns);
        /// <summary>
        /// Specifies the columns to select.
        /// </summary>
        /// <param name="column">The primary column to select</param>
        /// <param name="columns">Additional columns to select</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Columns(string column, params string[] columns) => Columns(Helper.Collection.Enumerate(column, columns));
        /// <summary>
        /// Specifies the columns to select.
        /// </summary>
        /// <param name="columns">The columns to select</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Columns(IEnumerable<string> columns) => Columns(null, columns);
        /// <summary>
        /// Specifies the columns to select.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select <paramref name="column"/> and <paramref name="columns"/> from</param>
        /// <param name="column">The primary column to select</param>
        /// <param name="columns">Additional columns to select</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Columns(object? dataset, string column, params string[] columns) => Columns(dataset, Helper.Collection.Enumerate(column, columns));
        #endregion
        #region ColumnsOf
        /// <summary>
        /// Specifies the columns to select by selecting the names of all public properties on <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to select the properties from</typeparam>
        /// <param name="dataset">Overwrites the default dataset name defined for type <typeparamref name="T"/>. If a type is used the alias defined for the type is taken. Set to an empty string to omit the dataset alias</param>
        /// <param name="excludedProperties">Optional names of properties to exclude</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived ColumnsOf<T>(object? dataset, params string[] excludedProperties);
        /// <summary>
        /// Specifies the columns to select by selecting the names of all public properties on <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="excludedProperties">Optional names of properties to exclude</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived ColumnsOf(params string[] excludedProperties) => ColumnsOf<TEntity>(excludedProperties);
        /// <summary>
        /// Specifies the columns to select by selecting the names of all public properties on <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to select the properties from</typeparam>
        /// <param name="excludedProperties">Optional names of properties to exclude</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived ColumnsOf<T>(params string[] excludedProperties) => ColumnsOf<T>(typeof(T), excludedProperties);
        /// <summary>
        /// Specifies the columns to select by selecting the names of all public properties on <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="dataset">Overwrites the default dataset name defined for type <typeparamref name="TEntity"/>. If a type is used the alias defined for the type is taken. Set to an empty string to omit the dataset alias</param>
        /// <param name="excludedProperties">Optional names of properties to exclude</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived ColumnsOf(object? dataset, params string[] excludedProperties) => ColumnsOf<TEntity>(dataset, excludedProperties);
        #endregion
        #endregion

        #region From
        /// <summary>
        /// Defines the table to select from.
        /// </summary>
        /// <param name="table">The table to select from</param>
        /// <param name="datasetAlias">Optional alias for the dataset. If a type is used the alias defined for the type is taken</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived From(string table, object? datasetAlias = null) => Expression(new TableExpression(datasetAlias, table.ValidateArgumentNotNullOrWhitespace(nameof(table))), SelectExpressionPositions.From);
        /// <summary>
        /// Defines the table to select from by using the name of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to get the table name from</typeparam>
        /// <param name="datasetAlias">Optional alias for the dataset. If a type is used the alias defined for the type is taken</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived From<T>(object? datasetAlias = null) => From(typeof(T).Name, datasetAlias ?? typeof(T));
        /// <summary>
        /// Defines the table to select from by using the name of <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="datasetAlias">Optional alias for the dataset. If a type is used the alias defined for the type is taken</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived From(object? datasetAlias = null) => From<TEntity>(datasetAlias);
        /// <summary>
        /// Defines the sub query to select from.
        /// </summary>
        /// <param name="query">Delegate that returns the sub query</param>
        /// <param name="datasetAlias">Alias for the sub query dataset. If a type is used the alias defined for the type is taken</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived FromQuery(Func<QueryBuilderOptions, string> query, object datasetAlias) => Expression(new SubQueryExpression(datasetAlias.ValidateArgument(nameof(datasetAlias)), query.ValidateArgument(nameof(query))), SelectExpressionPositions.From);
        /// <summary>
        /// Defines the sub query to select from.
        /// </summary>
        /// <param name="query">The sub query</param>
        /// <param name="datasetAlias">Alias for the sub query dataset. If a type is used the alias defined for the type is taken</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived FromQuery(string query, object datasetAlias) => FromQuery(x => query, datasetAlias);
        /// <summary>
        /// Defines the sub query to select from.
        /// </summary>
        /// <param name="builder">Builder that creates the sub query</param>
        /// <param name="datasetAlias">Alias for the sub query dataset. If a type is used the alias defined for the type is taken</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived FromQuery(IQueryBuilder builder, object datasetAlias) => FromQuery(x => builder.ValidateArgument(nameof(builder)).Build(x), datasetAlias);
        /// <summary>
        /// Defines the sub query to select from.
        /// </summary>
        /// <typeparam name="T">The type to get the dataset alias from</typeparam>
        /// <param name="query">Delegate that returns the sub query</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived FromQuery<T>(Func<QueryBuilderOptions, string> query) => FromQuery(query, typeof(T));
        /// <summary>
        /// Defines the sub query to select from.
        /// </summary>
        /// <typeparam name="T">The type to get the dataset alias from</typeparam>
        /// <param name="query">The sub query</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived FromQuery<T>(string query) => FromQuery(query, typeof(T));
        /// <summary>
        /// Defines the sub query to select from.
        /// </summary>
        /// <typeparam name="T">The type to get the dataset alias from</typeparam>
        /// <param name="builder">Builder that creates the sub query</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived FromQuery<T>(IQueryBuilder builder) => FromQuery(builder, typeof(T));
        #endregion

        #region Functions
        #region Count
        /// <summary>
        /// Counts the total amount of rows returned.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select column from</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived CountAll(object? dataset, string? columnAlias = null) => Expression(new FunctionExpression(Functions.Count, new ColumnExpression(dataset, Sql.All.ToString(), columnAlias)), SelectExpressionPositions.Column);
        /// <summary>
        /// Counts the total amount of rows where <paramref name="column"/> is not null.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select column from</param>
        /// <param name="column">The column to count</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Count(object? dataset, string column, string? columnAlias = null) => Expression(new FunctionExpression(Functions.Count, new ColumnExpression(dataset, column.ValidateArgumentNotNullOrWhitespace(nameof(column)), columnAlias)), SelectExpressionPositions.Column);
        /// <summary>
        /// Counts the total amount of rows where column selected by <paramref name="property"/> from <typeparamref name="T"/> is not null.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select column from</param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Count<T>(object? dataset, Expression<Func<T, object?>> property, string? columnAlias = null) => Count(dataset, property.ValidateArgument(nameof(property)).ExtractProperty(nameof(property)).Name, columnAlias);
        /// <summary>
        /// Counts the total amount of rows where column selected by <paramref name="property"/> from <typeparamref name="TEntity"/> is not null.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select column from</param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Count(object? dataset, Expression<Func<TEntity, object?>> property, string? columnAlias = null) => Count<TEntity>(dataset, property, columnAlias);
        /// <summary>
        /// Counts the total amount of rows returned.
        /// </summary>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived CountAll(string? columnAlias = null) => CountAll(null, columnAlias);
        /// <summary>
        /// Counts the total amount of rows where <paramref name="column"/> is not null.
        /// </summary>
        /// <param name="column">The column to count</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Count(string column, string? columnAlias = null) => Count(null, column, columnAlias);
        /// <summary>
        /// Counts the total amount of rows where column selected by <paramref name="property"/> from <typeparamref name="T"/> is not null.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Count<T>(Expression<Func<T, object?>> property, string? columnAlias = null) => Count<T>(typeof(T) , property, columnAlias);
        /// <summary>
        /// Counts the total amount of rows where column selected by <paramref name="property"/> from <typeparamref name="TEntity"/> is not null.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Count(Expression<Func<TEntity, object?>> property, string? columnAlias = null) => Count<TEntity>(property, columnAlias);

        #endregion
        #region Avg
        /// <summary>
        /// Calculates the average of <paramref name="column"/>.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select column from</param>
        /// <param name="column">The column to get the average from</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Average(object? dataset, string column, string? columnAlias = null) => Expression(new FunctionExpression(Functions.Avg, new ColumnExpression(dataset, column.ValidateArgumentNotNullOrWhitespace(nameof(column)), columnAlias)), SelectExpressionPositions.Column);
        /// <summary>
        ///  Calculates the average of the column selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select column from</param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Average<T>(object? dataset, Expression<Func<T, object?>> property, string? columnAlias = null) => Average(dataset, property.ValidateArgument(nameof(property)).ExtractProperty(nameof(property)).Name, columnAlias);
        /// <summary>
        ///  Calculates the average of the column selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select column from</param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Average(object? dataset, Expression<Func<TEntity, object?>> property, string? columnAlias = null) => Average<TEntity>(dataset, property, columnAlias);
        /// <summary>
        ///  Calculates the average of the column selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Average(Expression<Func<TEntity, object?>> property, string? columnAlias = null) => Average<TEntity>(property, columnAlias);
        /// <summary>
        /// Calculates the average of <paramref name="column"/>.
        /// </summary>
        /// <param name="column">The column to get the average from</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Average(string column, string? columnAlias = null) => Average(null, column, columnAlias);
        /// <summary>
        ///  Calculates the average of the column selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Average<T>(Expression<Func<T, object?>> property, string? columnAlias = null) => Average<T>(typeof(T), property, columnAlias);
        #endregion
        #region Sum
        /// <summary>
        /// Calculates the sum of <paramref name="column"/>.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select column from</param>
        /// <param name="column">The column to get the average from</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Sum(object? dataset, string column, string? columnAlias = null) => Expression(new FunctionExpression(Functions.Sum, new ColumnExpression(dataset, column.ValidateArgumentNotNullOrWhitespace(nameof(column)), columnAlias)), SelectExpressionPositions.Column);
        /// <summary>
        ///  Calculates the sum of the column selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select column from</param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Sum<T>(object? dataset, Expression<Func<T, object?>> property, string? columnAlias = null) => Sum(dataset, property.ValidateArgument(nameof(property)).ExtractProperty(nameof(property)).Name, columnAlias);
        /// <summary>
        ///  Calculates the sum of the column selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select column from</param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Sum(object? dataset, Expression<Func<TEntity, object?>> property, string? columnAlias = null) => Sum<TEntity>(dataset, property, columnAlias);
        /// <summary>
        /// Calculates the sum of <paramref name="column"/>.
        /// </summary>
        /// <param name="column">The column to get the average from</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Sum(string column, string? columnAlias = null) => Sum(null, column, columnAlias);
        /// <summary>
        ///  Calculates the sum of the column selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Sum<T>(Expression<Func<T, object?>> property, string? columnAlias = null) => Sum<T>(typeof(T), property, columnAlias);
        /// <summary>
        ///  Calculates the sum of the column selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Sum(Expression<Func<TEntity, object?>> property, string? columnAlias = null) => Sum<TEntity>(property, columnAlias);
        #endregion
        #region Max
        /// <summary>
        /// Returns the largest value of <paramref name="column"/>.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select column from</param>
        /// <param name="column">The column to get the max from</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Max(object? dataset, string column, string? columnAlias = null) => Expression(new FunctionExpression(Functions.Max, new ColumnExpression(dataset, column.ValidateArgumentNotNullOrWhitespace(nameof(column)), columnAlias)), SelectExpressionPositions.Column);
        /// <summary>
        /// Returns the largest value of the column selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select column from</param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Max<T>(object? dataset, Expression<Func<T, object?>> property, string? columnAlias = null) => Max(dataset, property.ValidateArgument(nameof(property)).ExtractProperty(nameof(property)).Name, columnAlias);
        /// <summary>
        /// Returns the largest value of column selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select column from</param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Max(object? dataset, Expression<Func<TEntity, object?>> property, string? columnAlias = null) => Max<TEntity>(dataset, property, columnAlias);
        /// <summary>
        /// Returns the largest value of <paramref name="column"/>.
        /// </summary>
        /// <param name="column">The column to get the max from</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Max(string column, string? columnAlias = null) => Max(null, column, columnAlias);
        /// <summary>
        /// Returns the largest value of column selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Max<T>(Expression<Func<T, object?>> property, string? columnAlias = null) => Max<T>(typeof(T), property, columnAlias);
        /// <summary>
        /// Returns the largest value of column selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Max(Expression<Func<TEntity, object?>> property, string? columnAlias = null) => Max<TEntity>(property, columnAlias);
        #endregion
        #region Min
        /// <summary>
        /// Returns the smallest value of <paramref name="column"/>.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select column from</param>
        /// <param name="column">The column to get the max from</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Min(object? dataset, string column, string? columnAlias = null) => Expression(new FunctionExpression(Functions.Min, new ColumnExpression(dataset, column.ValidateArgumentNotNullOrWhitespace(nameof(column)), columnAlias)), SelectExpressionPositions.Column);
        /// <summary>
        /// Returns the smallest value of the column selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select column from</param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Min<T>(object? dataset, Expression<Func<T, object?>> property, string? columnAlias = null) => Min(dataset, property.ValidateArgument(nameof(property)).ExtractProperty(nameof(property)).Name, columnAlias);
        /// <summary>
        /// Returns the smallest value of column selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select column from</param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Min(object? dataset, Expression<Func<TEntity, object?>> property, string? columnAlias = null) => Min<TEntity>(dataset, property, columnAlias);
        /// <summary>
        /// Returns the smallest value of <paramref name="column"/>.
        /// </summary>
        /// <param name="column">The column to get the max from</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Min(string column, string? columnAlias = null) => Min(null, column, columnAlias);
        /// <summary>
        /// Returns the smallest value of column selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Min<T>(Expression<Func<T, object?>> property, string? columnAlias = null) => Min<T>(typeof(T), property, columnAlias);
        /// <summary>
        /// Returns the smallest value of column selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Min(Expression<Func<TEntity, object?>> property, string? columnAlias = null) => Min<TEntity>(property, columnAlias);
        #endregion
        #endregion

        #region OrderBy
        /// <summary>
        /// Orders query results by <paramref name="column"/>.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select <paramref name="column"/> from</param>
        /// <param name="column">The column to order by</param>
        /// <param name="sortOrder">In what order to sort</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived OrderBy(object? dataset, string column, SortOrders? sortOrder = null) => Expression(new OrderByExpression(new ColumnExpression(dataset, column.ValidateArgumentNotNullOrWhitespace(nameof(column))), sortOrder), SelectExpressionPositions.OrderBy);
        /// <summary>
        /// Orders query results by column where the name of the property selected by <paramref name="property"/> from <typeparamref name="T"/> is used as the column name.
        /// </summary>
        /// <typeparam name="T">The type to select the property from</typeparam>
        /// <param name="dataset">Overwrites the default dataset name defined for type <typeparamref name="T"/></param>
        ///<param name="property">The expression that points to the property to use</param>
        /// <param name="sortOrder">In what order to sort</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived OrderBy<T>(object? dataset, Expression<Func<T, object?>> property, SortOrders? sortOrder = null) => OrderBy(dataset, property.ValidateArgument(nameof(property)).ExtractProperty(nameof(property)).Name, sortOrder);
        /// <summary>
        /// Orders query results by <paramref name="column"/>.
        /// </summary>
        /// <param name="column">The column to order by</param>
        /// <param name="sortOrder">In what order to sort</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived OrderBy(string column, SortOrders? sortOrder = null) => OrderBy(null, column, sortOrder);
        /// <summary>
        /// Orders query results by column where the name of the property selected by <paramref name="property"/> from <typeparamref name="T"/> is used as the column name.
        /// </summary>
        /// <typeparam name="T">The type to select the property from</typeparam>
        ///<param name="property">The expression that points to the property to use</param>
        /// <param name="sortOrder">In what order to sort</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived OrderBy<T>(Expression<Func<T, object?>> property, SortOrders? sortOrder = null) => OrderBy<T>(typeof(T), property, sortOrder);
        /// <summary>
        /// Orders query results by column where the name of the property selected by <paramref name="property"/> from <typeparamref name="TEntity"/> is used as the column name.
        /// </summary>
        /// <param name="dataset">Overwrites the default dataset name defined for type <typeparamref name="TEntity"/></param>
        ///<param name="property">The expression that points to the property to use</param>
        /// <param name="sortOrder">In what order to sort</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived OrderBy(object? dataset, Expression<Func<TEntity, object?>> property, SortOrders? sortOrder = null) => OrderBy<TEntity>(dataset, property, sortOrder);
        /// <summary>
        /// Orders query results by column where the name of the property selected by <paramref name="property"/> from <typeparamref name="TEntity"/> is used as the column name.
        /// </summary>
        ///<param name="property">The expression that points to the property to use</param>
        /// <param name="sortOrder">In what order to sort</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived OrderBy(Expression<Func<TEntity, object?>> property, SortOrders? sortOrder = null) => OrderBy<TEntity>(typeof(TEntity), property, sortOrder);
        #endregion

        #region GroupBy
        /// <summary>
        /// Groups query results by <paramref name="column"/>.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select <paramref name="column"/> from</param>
        /// <param name="column">The column to group by</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived GroupBy(object? dataset, string column) => Expression(new ColumnExpression(dataset, column.ValidateArgumentNotNullOrWhitespace(nameof(column))), SelectExpressionPositions.GroupBy);
        /// <summary>
        /// Groups query results by column where the name of the property selected by <paramref name="property"/> from <typeparamref name="T"/> is used as the column name.
        /// </summary>
        /// <typeparam name="T">The type to select the property from</typeparam>
        /// <param name="dataset">Overwrites the default dataset name defined for type <typeparamref name="T"/></param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived GroupBy<T>(object? dataset, Expression<Func<T, object?>> property) => GroupBy(dataset, property.ValidateArgument(nameof(property)).ExtractProperty(nameof(property)).Name);
        /// <summary>
        /// Groups query results by <paramref name="column"/>.
        /// </summary>
        /// <param name="column">The column to group by</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived GroupBy(string column) => GroupBy(null, column);
        /// <summary>
        /// Groups query results by column where the name of the property selected by <paramref name="property"/> from <typeparamref name="T"/> is used as the column name.
        /// </summary>
        /// <typeparam name="T">The type to select the property from</typeparam>
        ///<param name="property">The expression that points to the property to use</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived GroupBy<T>(Expression<Func<T, object?>> property) => GroupBy<T>(typeof(T), property);
        /// <summary>
        /// Groups query results by column where the name of the property selected by <paramref name="property"/> from <typeparamref name="TEntity"/> is used as the column name.
        /// </summary>
        /// <param name="dataset">Overwrites the default dataset name defined for type <typeparamref name="TEntity"/></param>
        ///<param name="property">The expression that points to the property to use</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived GroupBy(object? dataset, Expression<Func<TEntity, object?>> property) => GroupBy<TEntity>(dataset, property);
        /// <summary>
        /// Groups query results by column where the name of the property selected by <paramref name="property"/> from <typeparamref name="TEntity"/> is used as the column name.
        /// </summary>
        ///<param name="property">The expression that points to the property to use</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived GroupBy(Expression<Func<TEntity, object?>> property) => GroupBy<TEntity>(typeof(TEntity), property);
        #endregion
    }
    /// <inheritdoc cref="ISelectQueryBuilder{TEntity, TDerived}"/>
    public interface ISelectQueryBuilder<TEntity> : ISelectQueryBuilder<TEntity, ISelectQueryBuilder<TEntity>>
    {

    }
}
