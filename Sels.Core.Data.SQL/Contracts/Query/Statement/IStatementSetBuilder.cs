using Sels.Core.Data.SQL.Query.Expressions;
using System.Linq.Expressions;
using System.Text;

namespace Sels.Core.Data.SQL.Query.Statement
{
    /// <summary>
    /// Builder for setting sql objects to a new value.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to create the query for</typeparam>
    /// <typeparam name="TReturn">The builder to return to set the value</typeparam>
    public interface IStatementSetBuilder<TEntity, out TReturn>
    {
        #region Expression
        /// <summary>
        /// Adds an sql expression to update.
        /// </summary>
        /// <param name="sqlExpression">The sql expression to add</param>
        /// <returns>Builder to select what to set the expression to</returns>
        IStatementSetToBuilder<TEntity, TReturn> SetExpression(IExpression sqlExpression);
        /// <summary>
        /// Adds a raw sql expression to update.
        /// </summary>
        /// <param name="sqlExpression">String containing the sql expression</param>
        /// <returns>Builder to select what to set the expression to</returns>
        IStatementSetToBuilder<TEntity, TReturn> SetExpression(string sqlExpression) => SetExpression(new RawExpression(sqlExpression.ValidateArgumentNotNullOrWhitespace(nameof(sqlExpression))));
        /// <summary>
        /// Adds a sql expression to update.
        /// </summary>
        /// <param name="sqlExpression">Delegate that adds the sql expression to the provided string builder</param>
        /// <returns>Builder to select what to set the expression to</returns>
        IStatementSetToBuilder<TEntity, TReturn> SetExpression(Action<StringBuilder> sqlExpression) => SetExpression(new DelegateExpression(sqlExpression.ValidateArgument(nameof(sqlExpression))));
        #endregion
        #region Column
        /// <summary>
        /// Specifies a column to update.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to update <paramref name="column"/> from</param>
        /// <param name="column">The name of the column to update</param>
        /// <returns>Builder to select what to set the expression to</returns>
        IStatementSetToBuilder<TEntity, TReturn> Set(object? dataset, string column) => SetExpression(new ColumnExpression(dataset, column.ValidateArgumentNotNullOrWhitespace(nameof(column)), null));
        /// <summary>
        /// Specifies a column to update by using the name of the property selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to select the property from</typeparam>
        /// <param name="dataset">Overwrites the default dataset name defined for type <typeparamref name="T"/>. If a type is used the alias defined for the type is taken. Set to an empty string to omit the dataset alias</param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder to select what to set the expression to</returns>
        IStatementSetToBuilder<TEntity, TReturn> Set<T>(object? dataset, Expression<Func<T, object?>> property) => Set(dataset, property.ValidateArgument(nameof(property)).ExtractProperty(nameof(property)).Name);
        /// <summary>
        /// Specifies a column to update.
        /// </summary>
        /// <param name="column">The name of the column to update</param>
        /// <returns>Builder to select what to set the expression to</returns>
        IStatementSetToBuilder<TEntity, TReturn> Set(string column) => Set(null, column);
        /// <summary>
        /// Specifies a column to update by using the name of the property selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to select the property from</typeparam>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder to select what to set the expression to</returns>
        IStatementSetToBuilder<TEntity, TReturn> Set<T>(Expression<Func<T, object?>> property) => Set<T>(typeof(T), property);
        /// <summary>
        /// Specifies a column to update by using the name of the property selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder to select what to set the expression to</returns>
        IStatementSetToBuilder<TEntity, TReturn> Set(Expression<Func<TEntity, object?>> property) => Set<TEntity>(property);
        /// <summary>
        /// Specifies a column to update by using the name of the property selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="dataset">Overwrites the default dataset name defined for type <typeparamref name="TEntity"/></param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder to select what to set the expression to</returns>
        IStatementSetToBuilder<TEntity, TReturn> Set(object? dataset, Expression<Func<TEntity, object?>> property) => Set<TEntity>(dataset, property);
        #endregion
    }
    /// <summary>
    /// Builder for returning the builder to set the new value.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to create the query for</typeparam>
    /// <typeparam name="TReturn">The builder to return to set the value</typeparam>
    public interface IStatementSetToBuilder<TEntity, out TReturn>
    {
        /// <summary>
        /// Returns the builder to select the value to set.
        /// </summary>
        public TReturn To { get; }
    }
}
