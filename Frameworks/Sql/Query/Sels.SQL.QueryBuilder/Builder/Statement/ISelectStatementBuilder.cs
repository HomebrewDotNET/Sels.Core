using Sels.SQL.QueryBuilder.Builder.Expressions;
using Sels.SQL.QueryBuilder.Expressions;
using System.Linq.Expressions;
using System.Text;
using SqlConstantExpression = Sels.SQL.QueryBuilder.Builder.Expressions.ConstantExpression;
using SqlParameterExpression = Sels.SQL.QueryBuilder.Builder.Expressions.ParameterExpression;
using Sels.Core.Extensions;
using System.Linq;
using System;
using Sels.Core.Extensions.Reflection;
using System.Collections.Generic;
using Sels.Core;
using Sels.Core.Models;

namespace Sels.SQL.QueryBuilder.Builder.Statement
{
    /// <summary>
    /// Exposes methods for building a sql select query.
    /// </summary>
    /// <typeparam name="TDerived">The type to return for the fluent syntax</typeparam>
    /// <typeparam name="TEntity">The main entity to insert</typeparam>
    public interface ISelectStatementBuilder<TEntity, out TDerived> : 
        IStatementQueryBuilder<TEntity, SelectExpressionPositions, TDerived>, 
        IStatementConditionBuilder<TEntity, TDerived>,
        IStatementHavingBuilder<TEntity, TDerived>,
        IStatementJoinBuilder<TEntity, TDerived>
    {
        #region Keywords
        /// <summary>
        /// Select only distinct rows.
        /// </summary>
        /// <returns>Current builder for method chaining</returns>
        TDerived Distinct() => InnerExpressions.Any(x => x is DistintExpression) ? Instance : Expression(DistintExpression.Instance, SelectExpressionPositions.Before);
        #endregion

        #region Select
        #region All
        /// <summary>
        /// Selects all columns.
        /// </summary>
        /// <param name="dataset">Optional dataset to select from. Is the alias assigned to tables / sub queries</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived All(object dataset = null) => Expression(new AllColumnsExpression(dataset), SelectExpressionPositions.Column);
        /// <summary>
        /// Select all columns from the dataset alias defined by <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to get the dataset alias from</typeparam>
        /// <param name="dataset">Overwrites the default dataset name defined for type <typeparamref name="T"/>. If a type is used the alias defined for the type is taken. Set to an empty string to omit the dataset alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived AllOf<T>(object dataset = null) => All(dataset ?? typeof(T));
        /// <summary>
        /// Select all columns from the dataset alias defined by <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="dataset">Overwrites the default dataset name defined for type <typeparamref name="TEntity"/>. If a type is used the alias defined for the type is taken. Set to an empty string to omit the dataset alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived AllOf(object dataset = null) => AllOf<TEntity>(dataset);
        #endregion
        #region Column
        /// <summary>
        /// Specifies a column to select.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select <paramref name="column"/> from</param>
        /// <param name="column">The name of the column to select</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Column(object dataset, string column, string columnAlias) => Expression(new ColumnExpression(dataset, column.ValidateArgumentNotNullOrWhitespace(nameof(column)), columnAlias), SelectExpressionPositions.Column);
        /// <summary>
        /// Specifies a column to select by using the name of the property selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to select the property from</typeparam>
        /// <param name="dataset">Overwrites the default dataset name defined for type <typeparamref name="T"/>. If a type is used the alias defined for the type is taken. Set to an empty string to omit the dataset alias</param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="columnAlias">Overwrites the default alias of <typeparamref name="T"/></param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Column<T>(object dataset, Expression<Func<T, object>> property, string columnAlias = null) => Column(dataset, property.ValidateArgument(nameof(property)).ExtractProperty(nameof(property)).Name, columnAlias);
        /// <summary>
        /// Specifies a column to select.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select <paramref name="column"/> from</param>
        /// <param name="column">The name of the column to select</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Column(object dataset, string column) => Column(dataset, column, null);
        /// <summary>
        /// Specifies a column to select.
        /// </summary>
        /// <param name="column">The name of the column to select</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Column(string column) => Column(null, column, null);
        /// <summary>
        /// Specifies a column to select by using the name of the property selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to select the property from</typeparam>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Column<T>(Expression<Func<T, object>> property, string columnAlias = null) => Column<T>(typeof(T), property, columnAlias);
        /// <summary>
        /// Specifies a column to select by using the name of the property selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Column(Expression<Func<TEntity, object>> property, string columnAlias = null) => Column<TEntity>(property, columnAlias);
        /// <summary>
        /// Specifies a column to select by using the name of the property selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="dataset">Overwrites the default dataset name defined for type <typeparamref name="TEntity"/></param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="columnAlias">Overwrites the default alias of <typeparamref name="TEntity"/></param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Column(object dataset, Expression<Func<TEntity, object>> property, string columnAlias = null) => Column<TEntity>(dataset, property, columnAlias);
        #endregion
        #region Columns
        /// <summary>
        /// Specifies the columns to select.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select <paramref name="columns"/> from</param>
        /// <param name="columns">The columns to select</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Columns(object dataset, IEnumerable<string> columns);
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
        TDerived Columns(object dataset, string column, params string[] columns) => Columns(dataset, Helper.Collection.Enumerate(column, columns));
        #endregion
        #region ColumnsOf
        /// <summary>
        /// Specifies the columns to select by selecting the names of all public properties on <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to select the properties from</typeparam>
        /// <param name="dataset">Overwrites the default dataset name defined for type <typeparamref name="T"/>. If a type is used the alias defined for the type is taken. Set to an empty string to omit the dataset alias</param>
        /// <param name="excludedProperties">Optional names of properties to exclude</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived ColumnsOf<T>(object dataset, params string[] excludedProperties);
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
        TDerived ColumnsOf(object dataset, params string[] excludedProperties) => ColumnsOf<TEntity>(dataset, excludedProperties);
        #endregion
        #region Value
        /// <summary>
        /// Selects <paramref name="value"/> as a constant sql value.
        /// </summary>
        /// <param name="value">The value to select</param>
        /// <param name="alias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Value(object value, string alias = null) => Expression(new AliasExpression(new SqlConstantExpression(value), alias), SelectExpressionPositions.Column);
        #endregion
        #region Expression
        /// <summary>
        /// Selects a raw sql expression defined by the <see cref="object.ToString()"/> on <paramref name="value"/>.
        /// </summary>
        /// <param name="value">Object containing the raw sql expression</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Expression(object value) => Expression(new RawExpression(value.ValidateArgument(nameof(value))), SelectExpressionPositions.Column);
        /// <summary>
        /// Defines a value to select using <paramref name="builder"/>.
        /// </summary>
        /// <typeparam name="T">The main entity to create the expression for</typeparam>
        /// <param name="builder">The builder to create the expression</param>
        /// <param name="alias">Optional alias for the selected value</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Expression<T>(Action<ISharedExpressionBuilder<T, Null>> builder, string alias = null) => Expression(new AliasExpression(new ExpressionBuilder<T>(builder), alias), SelectExpressionPositions.Column);
        /// <summary>
        /// Defines a value to select using <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The builder to create the expression</param>
        /// <param name="alias">Optional alias for the selected value</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Expression(Action<ISharedExpressionBuilder<TEntity, Null>> builder, string alias = null) => Expression(new AliasExpression(new ExpressionBuilder<TEntity>(builder), alias), SelectExpressionPositions.Column);
        #endregion
        #region Case
        /// <summary>
        /// Select a value using a case expression.
        /// </summary>
        /// <param name="caseBuilder">Delegate that configures the case expression</param>
        /// <param name="caseAlias">Optional alias for the value returned from the case expression</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Case(Action<ICaseExpressionRootBuilder<TEntity>> caseBuilder, string caseAlias = null) => Case<TEntity>(caseBuilder, caseAlias);
        /// <summary>
        /// Select a value using a case expression.
        /// </summary>
        /// <typeparam name="T">The main type to create the case expression with</typeparam>
        /// <param name="caseBuilder">Delegate that configures the case expression</param>
        /// <param name="caseAlias">Optional alias for the value returned from the case expression</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Case<T>(Action<ICaseExpressionRootBuilder<T>> caseBuilder, string caseAlias = null) => Expression(new AliasExpression(new WrappedExpression(new CaseExpression<T>(caseBuilder)), caseAlias), SelectExpressionPositions.Column);
        #endregion
        #region Parameter
        /// <summary>
        /// Select the value from an SQL parameter.
        /// </summary>
        /// <param name="parameter">The name of the sql parameter</param>
        /// <param name="alias">Optional alias for the selected value</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Parameter(string parameter, string alias = null) => Expression(new AliasExpression(new SqlParameterExpression(parameter.ValidateArgumentNotNullOrWhitespace(nameof(parameter))), alias), SelectExpressionPositions.Column);
        /// <summary>
        /// Select the value from an SQL parameter where the parameter name is taken from the property name selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to select the property from</typeparam>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="alias">Optional alias for the selected value</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Parameter<T>(Expression<Func<T, object>> property, string alias = null) => Parameter(property.ValidateArgument(nameof(property)).ExtractProperty(nameof(property)).Name, alias);
        /// <summary>
        /// Select the value from an SQL parameter where the parameter name is taken from the property name selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="alias">Optional alias for the selected value</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Parameter(Expression<Func<TEntity, object>> property, string alias = null) => Parameter<TEntity>(property, alias);
        #endregion
        #region Variable
        /// <summary>
        /// Select the value from an SQL variable.
        /// </summary>
        /// <param name="variable">The name of the sql variable</param>
        /// <param name="alias">Optional alias for the selected value</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Variable(string variable, string alias = null) => Expression(new AliasExpression(new VariableExpression(variable.ValidateArgumentNotNullOrWhitespace(nameof(variable))), alias), SelectExpressionPositions.Column);
        /// <summary>
        /// Select the value from an SQL parameter where the variable name is taken from the property name selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to select the property from</typeparam>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="alias">Optional alias for the selected value</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Variable<T>(Expression<Func<T, object>> property, string alias = null) => Variable(property.ValidateArgument(nameof(property)).ExtractProperty(nameof(property)).Name);
        /// <summary>
        /// Select the value from an SQL parameter where the variable name is taken from the property name selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="alias">Optional alias for the selected value</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Variable(Expression<Func<TEntity, object>> property, string alias = null) => Variable<TEntity>(property);
        #endregion
        #endregion

        #region From
        /// <summary>
        /// Defines the table to select from.
        /// </summary>
        /// <param name="table">The table to select from</param>
        /// <param name="datasetAlias">Optional alias for the dataset. If a type is used the alias defined for the type is taken</param>
        /// <param name="database">Optional database to select the table from</param>
        /// <param name="schema">Optional schema where the table is defined in</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived From(string table, object datasetAlias = null, string database = null, string schema = null) => Expression(new TableExpression(database, schema, table.ValidateArgumentNotNullOrWhitespace(nameof(table)), datasetAlias), SelectExpressionPositions.From);
        /// <summary>
        /// Defines the table to select from by using the name of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to get the table name from</typeparam>
        /// <param name="datasetAlias">Optional alias for the dataset. If a type is used the alias defined for the type is taken</param>
        /// <param name="database">Optional database to select the table from</param>
        /// <param name="schema">Optional schema where the table is defined in</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived From<T>(object datasetAlias = null, string database = null, string schema = null) => From(typeof(T).Name, datasetAlias ?? typeof(T), database, schema);
        /// <summary>
        /// Defines the table to select from by using the name of <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="datasetAlias">Optional alias for the dataset. If a type is used the alias defined for the type is taken</param>
        /// <param name="database">Optional database to select the table from</param>
        /// <param name="schema">Optional schema where the table is defined in</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived From(object datasetAlias = null, string database = null, string schema = null) => From<TEntity>(datasetAlias, database, schema);
        /// <summary>
        /// Defines the sub query to select from.
        /// </summary>
        /// <param name="query">Delegate that adds the query to the supplied builder</param>
        /// <param name="datasetAlias">Alias for the sub query dataset. If a type is used the alias defined for the type is taken</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived FromQuery(Action<StringBuilder, ExpressionCompileOptions> query, object datasetAlias) => Expression(new SubQueryExpression(datasetAlias.ValidateArgument(nameof(datasetAlias)), query.ValidateArgument(nameof(query))), SelectExpressionPositions.From);
        /// <summary>
        /// Defines the sub query to select from.
        /// </summary>
        /// <param name="query">The sub query</param>
        /// <param name="datasetAlias">Alias for the sub query dataset. If a type is used the alias defined for the type is taken</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived FromQuery(string query, object datasetAlias) => FromQuery((b, o) => b.Append(query.ValidateArgumentNotNullOrWhitespace(nameof(query))), datasetAlias);
        /// <summary>
        /// Defines the sub query to select from.
        /// </summary>
        /// <param name="builder">Builder that creates the sub query</param>
        /// <param name="datasetAlias">Alias for the sub query dataset. If a type is used the alias defined for the type is taken</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived FromQuery(IQueryBuilder builder, object datasetAlias) => Expression(new SubQueryExpression(datasetAlias.ValidateArgument(nameof(datasetAlias)), builder.ValidateArgument(nameof(builder))), SelectExpressionPositions.From);
        /// <summary>
        /// Defines the sub query to select from.
        /// </summary>
        /// <typeparam name="T">The type to get the dataset alias from</typeparam>
        /// <param name="query">Delegate that adds the query to the supplied builder</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived FromQuery<T>(Action<StringBuilder, ExpressionCompileOptions> query) => FromQuery(query, typeof(T));
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
        TDerived CountAll(object dataset, string columnAlias = null) => Expression(new FunctionExpression(Functions.Count, new ColumnExpression(dataset, Sql.All.ToString(), columnAlias)), SelectExpressionPositions.Column);
        /// <summary>
        /// Counts the total amount of rows where <paramref name="column"/> is not null.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select column from</param>
        /// <param name="column">The column to count</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Count(object dataset, string column, string columnAlias = null) => Expression(new FunctionExpression(Functions.Count, new ColumnExpression(dataset, column.ValidateArgumentNotNullOrWhitespace(nameof(column)), columnAlias)), SelectExpressionPositions.Column);
        /// <summary>
        /// Counts the total amount of rows where column selected by <paramref name="property"/> from <typeparamref name="T"/> is not null.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select column from</param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Count<T>(object dataset, Expression<Func<T, object>> property, string columnAlias = null) => Count(dataset, property.ValidateArgument(nameof(property)).ExtractProperty(nameof(property)).Name, columnAlias);
        /// <summary>
        /// Counts the total amount of rows where column selected by <paramref name="property"/> from <typeparamref name="TEntity"/> is not null.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select column from</param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Count(object dataset, Expression<Func<TEntity, object>> property, string columnAlias = null) => Count<TEntity>(dataset, property, columnAlias);
        /// <summary>
        /// Counts the total amount of rows returned.
        /// </summary>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived CountAll(string columnAlias = null) => CountAll(null, columnAlias);
        /// <summary>
        /// Counts the total amount of rows where <paramref name="column"/> is not null.
        /// </summary>
        /// <param name="column">The column to count</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Count(string column, string columnAlias = null) => Count(null, column, columnAlias);
        /// <summary>
        /// Counts the total amount of rows where column selected by <paramref name="property"/> from <typeparamref name="T"/> is not null.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Count<T>(Expression<Func<T, object>> property, string columnAlias = null) => Count<T>(typeof(T), property, columnAlias);
        /// <summary>
        /// Counts the total amount of rows where column selected by <paramref name="property"/> from <typeparamref name="TEntity"/> is not null.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Count(Expression<Func<TEntity, object>> property, string columnAlias = null) => Count<TEntity>(property, columnAlias);

        #endregion
        #region Avg
        /// <summary>
        /// Calculates the average of <paramref name="column"/>.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select column from</param>
        /// <param name="column">The column to get the average from</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Average(object dataset, string column, string columnAlias = null) => Expression(new FunctionExpression(Functions.Avg, new ColumnExpression(dataset, column.ValidateArgumentNotNullOrWhitespace(nameof(column)), columnAlias)), SelectExpressionPositions.Column);
        /// <summary>
        ///  Calculates the average of the column selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select column from</param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Average<T>(object dataset, Expression<Func<T, object>> property, string columnAlias = null) => Average(dataset, property.ValidateArgument(nameof(property)).ExtractProperty(nameof(property)).Name, columnAlias);
        /// <summary>
        ///  Calculates the average of the column selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select column from</param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Average(object dataset, Expression<Func<TEntity, object>> property, string columnAlias = null) => Average<TEntity>(dataset, property, columnAlias);
        /// <summary>
        ///  Calculates the average of the column selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Average(Expression<Func<TEntity, object>> property, string columnAlias = null) => Average<TEntity>(property, columnAlias);
        /// <summary>
        /// Calculates the average of <paramref name="column"/>.
        /// </summary>
        /// <param name="column">The column to get the average from</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Average(string column, string columnAlias = null) => Average(null, column, columnAlias);
        /// <summary>
        ///  Calculates the average of the column selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Average<T>(Expression<Func<T, object>> property, string columnAlias = null) => Average<T>(typeof(T), property, columnAlias);
        #endregion
        #region Sum
        /// <summary>
        /// Calculates the sum of <paramref name="column"/>.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select column from</param>
        /// <param name="column">The column to get the average from</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Sum(object dataset, string column, string columnAlias = null) => Expression(new FunctionExpression(Functions.Sum, new ColumnExpression(dataset, column.ValidateArgumentNotNullOrWhitespace(nameof(column)), columnAlias)), SelectExpressionPositions.Column);
        /// <summary>
        ///  Calculates the sum of the column selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select column from</param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Sum<T>(object dataset, Expression<Func<T, object>> property, string columnAlias = null) => Sum(dataset, property.ValidateArgument(nameof(property)).ExtractProperty(nameof(property)).Name, columnAlias);
        /// <summary>
        ///  Calculates the sum of the column selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select column from</param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Sum(object dataset, Expression<Func<TEntity, object>> property, string columnAlias = null) => Sum<TEntity>(dataset, property, columnAlias);
        /// <summary>
        /// Calculates the sum of <paramref name="column"/>.
        /// </summary>
        /// <param name="column">The column to get the average from</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Sum(string column, string columnAlias = null) => Sum(null, column, columnAlias);
        /// <summary>
        ///  Calculates the sum of the column selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Sum<T>(Expression<Func<T, object>> property, string columnAlias = null) => Sum<T>(typeof(T), property, columnAlias);
        /// <summary>
        ///  Calculates the sum of the column selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Sum(Expression<Func<TEntity, object>> property, string columnAlias = null) => Sum<TEntity>(property, columnAlias);
        #endregion
        #region Max
        /// <summary>
        /// Returns the largest value of <paramref name="column"/>.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select column from</param>
        /// <param name="column">The column to get the max from</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Max(object dataset, string column, string columnAlias = null) => Expression(new FunctionExpression(Functions.Max, new ColumnExpression(dataset, column.ValidateArgumentNotNullOrWhitespace(nameof(column)), columnAlias)), SelectExpressionPositions.Column);
        /// <summary>
        /// Returns the largest value of the column selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select column from</param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Max<T>(object dataset, Expression<Func<T, object>> property, string columnAlias = null) => Max(dataset, property.ValidateArgument(nameof(property)).ExtractProperty(nameof(property)).Name, columnAlias);
        /// <summary>
        /// Returns the largest value of column selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select column from</param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Max(object dataset, Expression<Func<TEntity, object>> property, string columnAlias = null) => Max<TEntity>(dataset, property, columnAlias);
        /// <summary>
        /// Returns the largest value of <paramref name="column"/>.
        /// </summary>
        /// <param name="column">The column to get the max from</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Max(string column, string columnAlias = null) => Max(null, column, columnAlias);
        /// <summary>
        /// Returns the largest value of column selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Max<T>(Expression<Func<T, object>> property, string columnAlias = null) => Max<T>(typeof(T), property, columnAlias);
        /// <summary>
        /// Returns the largest value of column selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Max(Expression<Func<TEntity, object>> property, string columnAlias = null) => Max<TEntity>(property, columnAlias);
        #endregion
        #region Min
        /// <summary>
        /// Returns the smallest value of <paramref name="column"/>.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select column from</param>
        /// <param name="column">The column to get the max from</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Min(object dataset, string column, string columnAlias = null) => Expression(new FunctionExpression(Functions.Min, new ColumnExpression(dataset, column.ValidateArgumentNotNullOrWhitespace(nameof(column)), columnAlias)), SelectExpressionPositions.Column);
        /// <summary>
        /// Returns the smallest value of the column selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select column from</param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Min<T>(object dataset, Expression<Func<T, object>> property, string columnAlias = null) => Min(dataset, property.ValidateArgument(nameof(property)).ExtractProperty(nameof(property)).Name, columnAlias);
        /// <summary>
        /// Returns the smallest value of column selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select column from</param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Min(object dataset, Expression<Func<TEntity, object>> property, string columnAlias = null) => Min<TEntity>(dataset, property, columnAlias);
        /// <summary>
        /// Returns the smallest value of <paramref name="column"/>.
        /// </summary>
        /// <param name="column">The column to get the max from</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Min(string column, string columnAlias = null) => Min(null, column, columnAlias);
        /// <summary>
        /// Returns the smallest value of column selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Min<T>(Expression<Func<T, object>> property, string columnAlias = null) => Min<T>(typeof(T), property, columnAlias);
        /// <summary>
        /// Returns the smallest value of column selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="columnAlias">Optional column alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Min(Expression<Func<TEntity, object>> property, string columnAlias = null) => Min<TEntity>(property, columnAlias);
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
        TDerived OrderBy(object dataset, string column, SortOrders? sortOrder = null) => Expression(new OrderByExpression(new ColumnExpression(dataset, column.ValidateArgumentNotNullOrWhitespace(nameof(column))), sortOrder), SelectExpressionPositions.OrderBy);
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
        TDerived OrderByCase<T>(Action<ICaseExpressionRootBuilder<T>> caseBuilder, SortOrders? sortOrder = null) => Expression(new OrderByExpression(new WrappedExpression(new CaseExpression<T>(caseBuilder)), sortOrder), SelectExpressionPositions.OrderBy);
        #endregion

        #region GroupBy
        /// <summary>
        /// Groups query results by <paramref name="column"/>.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select <paramref name="column"/> from</param>
        /// <param name="column">The column to group by</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived GroupBy(object dataset, string column) => Expression(new ColumnExpression(dataset, column.ValidateArgumentNotNullOrWhitespace(nameof(column))), SelectExpressionPositions.GroupBy);
        /// <summary>
        /// Groups query results by column where the name of the property selected by <paramref name="property"/> from <typeparamref name="T"/> is used as the column name.
        /// </summary>
        /// <typeparam name="T">The type to select the property from</typeparam>
        /// <param name="dataset">Overwrites the default dataset name defined for type <typeparamref name="T"/></param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived GroupBy<T>(object dataset, Expression<Func<T, object>> property) => GroupBy(dataset, property.ValidateArgument(nameof(property)).ExtractProperty(nameof(property)).Name);
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
        TDerived GroupBy<T>(Expression<Func<T, object>> property) => GroupBy<T>(typeof(T), property);
        /// <summary>
        /// Groups query results by column where the name of the property selected by <paramref name="property"/> from <typeparamref name="TEntity"/> is used as the column name.
        /// </summary>
        /// <param name="dataset">Overwrites the default dataset name defined for type <typeparamref name="TEntity"/></param>
        ///<param name="property">The expression that points to the property to use</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived GroupBy(object dataset, Expression<Func<TEntity, object>> property) => GroupBy<TEntity>(dataset, property);
        /// <summary>
        /// Groups query results by column where the name of the property selected by <paramref name="property"/> from <typeparamref name="TEntity"/> is used as the column name.
        /// </summary>
        ///<param name="property">The expression that points to the property to use</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived GroupBy(Expression<Func<TEntity, object>> property) => GroupBy<TEntity>(typeof(TEntity), property);
        #endregion

        #region Union
        /// <summary>
        /// Adds a sub query expression whose results will be added to the result set of the current query. Duplicate rows are excluded.
        /// </summary>
        /// <param name="query">Delegate that returns the query string</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Union(Func<ExpressionCompileOptions, string> query) => Expression(new UnionExpression(query.ValidateArgument(nameof(query))), SelectExpressionPositions.After);
        /// <summary>
        /// Adds a sub query expression whose results will be added to the result set of the current query. Duplicate rows are excluded.
        /// </summary>
        /// <param name="query">The query string</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Union(string query) => Union(x => query.ValidateArgumentNotNullOrWhitespace(nameof(query)));
        /// <summary>
        /// Adds a sub query expression whose results will be added to the result set of the current query. Duplicate rows are excluded.
        /// </summary>
        /// <param name="builder">Builder for creating the sub query</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Union(IQueryBuilder builder) => Union(x => builder.ValidateArgument(nameof(builder)).Build(x));
        /// <summary>
        /// Adds a sub query expression whose results will be added to the result set of the current query. Duplicate rows are included.
        /// </summary>
        /// <param name="query">Delegate that returns the query string</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived UnionAll(Func<ExpressionCompileOptions, string> query) => Expression(new UnionExpression(query.ValidateArgument(nameof(query)), false), SelectExpressionPositions.After);
        /// <summary>
        /// Adds a sub query expression whose results will be added to the result set of the current query. Duplicate rows are included.
        /// </summary>
        /// <param name="query">The query string</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived UnionAll(string query) => UnionAll(x => query.ValidateArgumentNotNullOrWhitespace(nameof(query)));
        /// <summary>
        /// Adds a sub query expression whose results will be added to the result set of the current query. Duplicate rows are included.
        /// </summary>
        /// <param name="builder">Builder for creating the sub query</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived UnionAll(IQueryBuilder builder) => UnionAll(x => builder.ValidateArgument(nameof(builder)).Build(x));
        #endregion
    }
    /// <inheritdoc cref="ISelectStatementBuilder{TEntity, TDerived}"/>
    public interface ISelectStatementBuilder<TEntity> : ISelectStatementBuilder<TEntity, ISelectStatementBuilder<TEntity>>
    {

    }
}
