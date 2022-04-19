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
    /// Exposes methods for building a sql update query.
    /// </summary>
    /// <typeparam name="TDerived">The type to return for the fluent syntax</typeparam>
    /// <typeparam name="TEntity">The main entity to update</typeparam>
    public interface IUpdateQueryBuilder<TEntity, out TDerived> : IQueryBuilder<TEntity, UpdateExpressionPositions, TDerived>, IConditionBuilder<TEntity, TDerived>, IQueryJoinBuilder<TEntity, TDerived>
    {
        #region Table
        /// <summary>
        /// Defines the table to delete from.
        /// </summary>
        /// <param name="table">The table to select from</param>
        /// <param name="datasetAlias">Optional alias for the dataset. If a type is used the alias defined for the type is taken</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Table(string table, object? datasetAlias = null) => Expression(new TableExpression(datasetAlias, table.ValidateArgumentNotNullOrWhitespace(nameof(table))), UpdateExpressionPositions.Table);
        /// <summary>
        /// Defines the table to delete from by using the name of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to get the table name from</typeparam>
        /// <param name="datasetAlias">Optional alias for the dataset. If a type is used the alias defined for the type is taken</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Table<T>(object? datasetAlias = null) => Table(typeof(T).Name, datasetAlias ?? typeof(T));
        /// <summary>
        /// Defines the table to delete from by using the name of <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="datasetAlias">Optional alias for the dataset. If a type is used the alias defined for the type is taken</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Table(object? datasetAlias = null) => Table<TEntity>(datasetAlias);
        #endregion

        #region Set
        #region Expression
        /// <summary>
        /// Adds an sql expression to update.
        /// </summary>
        /// <param name="sqlExpression">The sql expression to add</param>
        /// <returns>Builder to select what to set the expression to</returns>
        ISharedExpressionBuilder<TEntity, TDerived> SetExpressionTo(IExpression sqlExpression);
        /// <summary>
        /// Adds a raw sql expression to update.
        /// </summary>
        /// <param name="sqlExpression">String containing the sql expression</param>
        /// <returns>Builder to select what to set the expression to</returns>
        ISharedExpressionBuilder<TEntity, TDerived> SetExpressionTo(string sqlExpression) => SetExpressionTo(new RawExpression(sqlExpression.ValidateArgumentNotNullOrWhitespace(nameof(sqlExpression))));
        /// <summary>
        /// Adds a sql expression to update.
        /// </summary>
        /// <param name="sqlExpression">Delegate that adds the sql expression to the provided string builder</param>
        /// <returns>Builder to select what to set the expression to</returns>
        ISharedExpressionBuilder<TEntity, TDerived> SetExpressionTo(Action<StringBuilder> sqlExpression) => SetExpressionTo(new DelegateExpression(sqlExpression.ValidateArgument(nameof(sqlExpression))));
        #endregion
        #region Column
        /// <summary>
        /// Specifies a column to update.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to update <paramref name="column"/> from</param>
        /// <param name="column">The name of the column to update</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Builder to select what to set the expression to</returns>
        ISharedExpressionBuilder<TEntity, TDerived> SetTo(object? dataset, string column) => SetExpressionTo(new ColumnExpression(dataset, column.ValidateArgumentNotNullOrWhitespace(nameof(column)), null));
        /// <summary>
        /// Specifies a column to update by using the name of the property selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to select the property from</typeparam>
        /// <param name="dataset">Overwrites the default dataset name defined for type <typeparamref name="T"/>. If a type is used the alias defined for the type is taken. Set to an empty string to omit the dataset alias</param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder to select what to set the expression to</returns>
        ISharedExpressionBuilder<TEntity, TDerived> SetTo<T>(object? dataset, Expression<Func<T, object?>> property) => SetTo(dataset, property.ValidateArgument(nameof(property)).ExtractProperty(nameof(property)).Name);
        /// <summary>
        /// Specifies a column to update.
        /// </summary>
        /// <param name="column">The name of the column to update</param>
        /// <returns>Builder to select what to set the expression to</returns>
        ISharedExpressionBuilder<TEntity, TDerived> SetTo(string column) => SetTo(null, column);
        /// <summary>
        /// Specifies a column to update by using the name of the property selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to select the property from</typeparam>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder to select what to set the expression to</returns>
        ISharedExpressionBuilder<TEntity, TDerived> SetTo<T>(Expression<Func<T, object?>> property) => SetTo<T>(typeof(T), property);
        /// <summary>
        /// Specifies a column to update by using the name of the property selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder to select what to set the expression to</returns>
        ISharedExpressionBuilder<TEntity, TDerived> SetTo(Expression<Func<TEntity, object?>> property) => SetTo<TEntity>(property);
        /// <summary>
        /// Specifies a column to update by using the name of the property selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="dataset">Overwrites the default dataset name defined for type <typeparamref name="TEntity"/></param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder to select what to set the expression to</returns>
        ISharedExpressionBuilder<TEntity, TDerived> SetTo(object? dataset, Expression<Func<TEntity, object?>> property) => SetTo<TEntity>(dataset, property);
        #endregion
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

    /// <inheritdoc cref="IUpdateQueryBuilder{TEntity, TDerived}"/>
    public interface IUpdateQueryBuilder<TEntity> : IUpdateQueryBuilder<TEntity, IUpdateQueryBuilder<TEntity>>
    {

    }
}
