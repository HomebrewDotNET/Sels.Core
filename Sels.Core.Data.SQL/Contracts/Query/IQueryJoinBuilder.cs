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
    /// Builder for creating joins in sql queries.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to build the query for</typeparam>
    /// <typeparam name="TDerived">The type to return for the fluent syntax</typeparam>
    public interface IQueryJoinBuilder<TEntity, out TDerived>
    {
        /// <summary>
        /// Defines what table to join.
        /// </summary>
        /// <param name="joinType">The type of the join to perform</param>
        /// <param name="table">The table to select from</param>
        /// <param name="datasetAlias">Optional alias for the column dataset. If a type is used the alias defined for the type is taken</param>
        /// <param name="builder">Builder for defining what columns to join on</param>
        /// <returns>The builder used to call this method</returns>
        TDerived Join(Joins joinType, string table, object? datasetAlias, Action<IOnJoinBuilder<TEntity>> builder);
        /// <summary>
        /// Defines what table to join.
        /// </summary>
        /// <param name="joinType">The type of the join to perform</param>
        /// <param name="table">The table to select from</param>
        /// <param name="builder">Builder for defining what columns to join on</param>
        /// <returns>The builder used to call this method</returns>
        TDerived Join(Joins joinType, string table, Action<IOnJoinBuilder<TEntity>> builder) => Join(joinType, table, null, builder);
        /// <summary>
        /// Defines what table to join by using the name of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to get the table name from</typeparam>
        /// <param name="joinType">The type of the join to perform</param>
        /// <param name="builder">Builder for defining what columns to join on</param>
        /// <returns>The builder used to call this method</returns>
        TDerived Join<T>(Joins joinType, Action<IOnJoinBuilder<TEntity>> builder) => Join(joinType, typeof(T).Name, typeof(T), builder);
        /// <summary>
        /// Defines what table to join by using the name of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to get the table name from</typeparam>
        /// <param name="joinType">The type of the join to perform</param>
        /// <param name="datasetAlias">Optional alias for the column dataset. If a type is used the alias defined for the type is taken</param>
        /// <param name="builder">Builder for defining what columns to join on</param>
        /// <returns>The builder used to call this method</returns>
        TDerived Join<T>(Joins joinType, object? datasetAlias, Action<IOnJoinBuilder<TEntity>> builder) => Join(joinType, typeof(T).Name, datasetAlias, builder);
    }
    /// <summary>
    /// Builder for selecting the first column to join on.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to build the query for</typeparam>
    public interface IOnJoinBuilder<TEntity>
    {
        /// <summary>
        /// Defines the expression to join from.
        /// </summary>
        /// <param name="sqlExpression">Expression containing what to join</param>
        /// <returns>Builder for selecting what to join on</returns>
        IToJoinBuilder<TEntity> OnExpression(IExpression sqlExpression);
        /// <summary>
        /// Defines the expression to join from.
        /// </summary>
        /// <param name="sqlExpression">String containing what to join from</param>
        /// <returns>Builder for selecting what to join to</returns>
        IToJoinBuilder<TEntity> OnExpression(string sqlExpression) => OnExpression(new RawExpression(sqlExpression.ValidateArgument(nameof(sqlExpression))));
        /// <summary>
        /// Defines the expression to join from.
        /// </summary>
        /// <param name="sqlExpression">Delegate that appends the sql to the provided builder</param>
        /// <returns>Builder for selecting what to join to</returns>
        IToJoinBuilder<TEntity> OnExpression(Action<StringBuilder> sqlExpression) => OnExpression(new DelegateExpression(sqlExpression.ValidateArgument(nameof(sqlExpression))));
        /// <summary>
        /// Defines the expression to join from.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select <paramref name="column"/> from</param>
        /// <param name="column">The column name</param>
        /// <returns>Builder for selecting what to join to</returns>
        IToJoinBuilder<TEntity> On(object? dataset, string column) => OnExpression(new ColumnExpression(dataset, column.ValidateArgumentNotNullOrWhitespace(nameof(column))));
        /// <summary>
        /// Defines the expression to join from where the name is taken from the property name selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to select the property from</typeparam>
        /// <param name="dataset">Overwrites the default dataset name defined for type <typeparamref name="T"/>. If a type is used the alias defined for the type is taken. Set to an empty string to omit the dataset alias</param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder for selecting what to join to</returns>
        IToJoinBuilder<TEntity> On<T>(object? dataset, Expression<Func<T, object?>> property) => On(dataset, property.ValidateArgument(nameof(property)).ExtractProperty(nameof(property)).Name);
        /// <summary>
        /// Defines the expression to join from.
        /// </summary>
        /// <param name="column">The column name</param>
        /// <returns>Builder for selecting what to join to</returns>
        IToJoinBuilder<TEntity> On(string column) => On(null, column);
        /// <summary>
        /// Defines the expression to join from where the name is taken from the property name selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder for selecting what to join to</returns>
        IToJoinBuilder<TEntity> On(Expression<Func<TEntity, object?>> property) => On<TEntity>(property);
        /// <summary>
        /// Defines the expression to join from where the name is taken from the property name selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to select the property from</typeparam>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder for selecting what to join to</returns>
        IToJoinBuilder<TEntity> On<T>(Expression<Func<T, object?>> property) => On(typeof(T), property);
        /// <summary>
        /// Defines the expression to join from where the name is taken from the property name selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="dataset">Overwrites the default dataset name defined for type <typeparamref name="TEntity"/>. If a type is used the alias defined for the type is taken. Set to an empty string to omit the dataset alias</param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder for selecting what to join to</returns>
        IToJoinBuilder<TEntity> On(object? dataset, Expression<Func<TEntity, object?>> property) => On<TEntity>(dataset, property);
    }
    /// <summary>
    /// Builder for selecting what to join to to join on.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to build the query for</typeparam>
    public interface IToJoinBuilder<TEntity>
    {
        /// <summary>
        /// Defines the expression to join to.
        /// </summary>
        /// <param name="sqlExpression">Expression containing what to join</param>
        /// <returns>Builder for selecting what to join on</returns>
        IChainedJoinBuilder<TEntity> ToExpression(IExpression sqlExpression);
        /// <summary>
        /// Defines the expression to join to.
        /// </summary>
        /// <param name="sqlExpression">String containing what to join to</param>
        /// <returns>Builder for selecting what to join to</returns>
        IChainedJoinBuilder<TEntity> ToExpression(string sqlExpression) => ToExpression(new RawExpression(sqlExpression.ValidateArgument(nameof(sqlExpression))));
        /// <summary>
        /// Defines the expression to join to.
        /// </summary>
        /// <param name="sqlExpression">Delegate that appends the sql to the provided builder</param>
        /// <returns>Builder for selecting what to join to</returns>
        IChainedJoinBuilder<TEntity> ToExpression(Action<StringBuilder> sqlExpression) => ToExpression(new DelegateExpression(sqlExpression.ValidateArgument(nameof(sqlExpression))));
        /// <summary>
        /// Defines the expression to join to.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select <paramref name="column"/> from</param>
        /// <param name="column">The column name</param>
        /// <returns>Builder for creating more columns to join on or exit the current builder</returns>
        IChainedJoinBuilder<TEntity> To(object? dataset, string column) => ToExpression(new ColumnExpression(dataset, column.ValidateArgumentNotNullOrWhitespace(nameof(column))));
        /// <summary>
        /// Defines the expression to join to where the name is taken from the property name selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to select the property from</typeparam>
        /// <param name="dataset">Overwrites the default dataset name defined for type <typeparamref name="T"/>. If a type is used the alias defined for the type is taken. Set to an empty string to omit the dataset alias</param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder for creating more columns to join on or exit the current builder</returns>
        IChainedJoinBuilder<TEntity> To<T>(object? dataset, Expression<Func<T, object?>> property) => To(dataset, property.ValidateArgument(nameof(property)).ExtractProperty(nameof(property)).Name);
        /// <summary>
        /// Defines the expression to join to.
        /// </summary>
        /// <param name="column">The column name</param>
        /// <returns>Builder for creating more columns to join on or exit the current builder</returns>
        IChainedJoinBuilder<TEntity> To(string column) => To(null, column);
        /// <summary>
        /// Defines the expression to join to where the name is taken from the property name selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder for creating more columns to join on or exit the current builder</returns>
        IChainedJoinBuilder<TEntity> To(Expression<Func<TEntity, object?>> property) => To<TEntity>(property);
        /// <summary>
        /// Defines the expression to join to where the name is taken from the property name selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to select the property from</typeparam>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder for creating more columns to join on or exit the current builder</returns>
        IChainedJoinBuilder<TEntity> To<T>(Expression<Func<T, object?>> property) => To<T>(typeof(T), property);
        /// <summary>
        /// Defines the expression to join to where the name is taken from the property name selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="dataset">Overwrites the default dataset name defined for type <typeparamref name="TEntity"/>. If a type is used the alias defined for the type is taken. Set to an empty string to omit the dataset alias</param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder for creating more columns to join on or exit the current builder</returns>
        IChainedJoinBuilder<TEntity> To(object? dataset, Expression<Func<TEntity, object?>> property) => To<TEntity>(dataset, property);
    }
    /// <summary>
    /// Builder for defining more columns to join or creating more joins.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to build the query for</typeparam>
    public interface IChainedJoinBuilder<TEntity>
    {
        /// <summary>
        /// Returns current builder for defining more columns to join on.
        /// </summary>
        /// <returns>Current builder for method chaining</returns>
        IOnJoinBuilder<TEntity> And();
    }
}
