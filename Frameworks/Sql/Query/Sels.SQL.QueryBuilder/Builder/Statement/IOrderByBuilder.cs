using Sels.Core.Extensions;
using Sels.SQL.QueryBuilder.Builder.Expressions;
using Sels.SQL.QueryBuilder.Expressions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Sels.Core.Extensions.Reflection;

namespace Sels.SQL.QueryBuilder.Builder.Statement
{
    /// <summary>
    /// Builder for adding ORDER BY expressions.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to build the query for</typeparam>
    /// <typeparam name="TDerived">The type inheriting the interface</typeparam>
    public interface IOrderByBuilder<TEntity, out TDerived>
    {
        /// <summary>
        /// Adds <paramref name="expression"/> that contains the value to order by.
        /// </summary>
        /// <param name="expression">The expression that contains the value to order by</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived OrderBy(IExpression expression);
        /// <summary>
        /// Orders query results by <paramref name="column"/>.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select <paramref name="column"/> from</param>
        /// <param name="column">The column to order by</param>
        /// <param name="sortOrder">In what order to sort</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived OrderBy(object dataset, string column, SortOrders? sortOrder = null) => OrderBy(new OrderByExpression(new ColumnExpression(dataset, column.ValidateArgumentNotNullOrWhitespace(nameof(column))), sortOrder));
        /// <summary>
        /// Orders query results by column where the name of the property selected by <paramref name="property"/> from <typeparamref name="T"/> is used as the column name.
        /// </summary>
        /// <typeparam name="T">The type to select the property from</typeparam>
        /// <param name="dataset">Overwrites the default dataset name defined for type <typeparamref name="T"/></param>
        ///<param name="property">The expression that points to the property to use</param>
        /// <param name="sortOrder">In what order to sort</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived OrderBy<T>(object dataset, Expression<Func<T, object>> property, SortOrders? sortOrder = null) => OrderBy(dataset, property.ValidateArgument(nameof(property)).ExtractProperty(nameof(property)).Name, sortOrder);
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
        TDerived OrderBy<T>(Expression<Func<T, object>> property, SortOrders? sortOrder = null) => OrderBy<T>(typeof(T), property, sortOrder);
        /// <summary>
        /// Orders query results by column where the name of the property selected by <paramref name="property"/> from <typeparamref name="TEntity"/> is used as the column name.
        /// </summary>
        /// <param name="dataset">Overwrites the default dataset name defined for type <typeparamref name="TEntity"/></param>
        ///<param name="property">The expression that points to the property to use</param>
        /// <param name="sortOrder">In what order to sort</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived OrderBy(object dataset, Expression<Func<TEntity, object>> property, SortOrders? sortOrder = null) => OrderBy<TEntity>(dataset, property, sortOrder);
        /// <summary>
        /// Orders query results by column where the name of the property selected by <paramref name="property"/> from <typeparamref name="TEntity"/> is used as the column name.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="sortOrder">In what order to sort</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived OrderBy(Expression<Func<TEntity, object>> property, SortOrders? sortOrder = null) => OrderBy<TEntity>(typeof(TEntity), property, sortOrder);
        /// <summary>
        /// Order query results using a CASE WHEN expression.
        /// </summary>
        /// <param name="caseBuilder">Delegate that configures the case expression</param>
        /// <param name="sortOrder">In what order to sort</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived OrderByCase(Action<ICaseExpressionRootBuilder<TEntity>> caseBuilder, SortOrders? sortOrder = null) => OrderByCase<TEntity>(caseBuilder, sortOrder);
        /// <summary>
        /// Order query results using a CASE WHEN expression.
        /// </summary>
        /// <typeparam name="T">The main type to create the case expression with</typeparam>
        /// <param name="caseBuilder">Delegate that configures the case expression</param>
        /// <param name="sortOrder">In what order to sort</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived OrderByCase<T>(Action<ICaseExpressionRootBuilder<T>> caseBuilder, SortOrders? sortOrder = null) => OrderBy(new OrderByExpression(new WrappedExpression(new CaseExpression<T>(caseBuilder)), sortOrder));
    }
}
