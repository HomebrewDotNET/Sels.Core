using Sels.Core.Data.SQL.Query.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Data.SQL.Query
{
    /// <summary>
    /// Exposes methods for building a sql delete query.
    /// </summary>
    /// <typeparam name="TDerived">The type to return for the fluent syntax</typeparam>
    /// <typeparam name="TEntity">The main entity to delete</typeparam>
    public interface IDeleteQueryBuilder<TEntity, TDerived> : IQueryBuilder<TEntity, DeleteExpressionPositions, TDerived>, IConditionBuilder<TEntity, TDerived>, IQueryJoinBuilder<TEntity, TDerived>
    {
        #region Expressions
        /// <summary>
        /// Adds a sql expression to the current builder.
        /// </summary>
        /// <param name="sqlExpression">The sql expression to add</param>
        /// <param name="location">Where in the query the expression should be placed</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Expression(IExpression sqlExpression, DeleteExpressionPositions location);
        /// <summary>
        /// Adds a raw sql expression to the current builder.
        /// </summary>
        /// <param name="sqlExpression">String containing the sql expression</param>
        /// <param name="location">Where in the query the expression should be placed</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Expression(string sqlExpression, DeleteExpressionPositions location) => Expression(new RawExpression(sqlExpression.ValidateArgumentNotNullOrWhitespace(nameof(sqlExpression))), location);
        /// <summary>
        /// Adds a sql expression to the current builder.
        /// </summary>
        /// <param name="sqlExpression">Delegate that adds the sql expression to the provided string builder</param>
        /// <param name="location">Where in the query the expression should be placed</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Expression(Action<StringBuilder> sqlExpression, DeleteExpressionPositions location) => Expression(new DelegateExpression(sqlExpression.ValidateArgument(nameof(sqlExpression))), location);
        #endregion

        #region From
        /// <summary>
        /// Defines the table to delete from.
        /// </summary>
        /// <param name="table">The table to select from</param>
        /// <param name="datasetAlias">Optional alias for the dataset. If a type is used the alias defined for the type is taken</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived From(string table, object? datasetAlias = null) => Expression(new TableExpression(datasetAlias, table.ValidateArgumentNotNullOrWhitespace(nameof(table))), DeleteExpressionPositions.From);
        /// <summary>
        /// Defines the table to delete from by using the name of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to get the table name from</typeparam>
        /// <param name="datasetAlias">Optional alias for the dataset. If a type is used the alias defined for the type is taken</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived From<T>(object? datasetAlias = null) => From(typeof(T).Name, datasetAlias ?? typeof(T));
        /// <summary>
        /// Defines the table to delete from by using the name of <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="datasetAlias">Optional alias for the dataset. If a type is used the alias defined for the type is taken</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived From(object? datasetAlias = null) => From<TEntity>(datasetAlias);
        #endregion
    }
    /// <inheritdoc cref="IDeleteQueryBuilder{TEntity, TDerived}"/>
    public interface IDeleteQueryBuilder<TEntity> : IDeleteQueryBuilder<TEntity, IDeleteQueryBuilder<TEntity>>
    {

    }
}
