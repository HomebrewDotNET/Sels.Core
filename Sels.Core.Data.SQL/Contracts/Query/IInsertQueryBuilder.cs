using Sels.Core.Data.SQL.Query.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using SqlParameterExpression = Sels.Core.Data.SQL.Query.Expressions.ParameterExpression;

namespace Sels.Core.Data.SQL.Query
{
    /// <summary>
    /// Exposes methods for building a sql insert query.
    /// </summary>
    /// <typeparam name="TDerived">The type to return for the fluent syntax</typeparam>
    /// <typeparam name="TEntity">The main entity to delete</typeparam>
    public interface IInsertQueryBuilder<TEntity, out TDerived> : IQueryBuilder<TEntity, InsertExpressionPositions, TDerived>
    {
        #region Into
        /// <summary>
        /// Defines the table to insert into.
        /// </summary>
        /// <param name="table">The name of the table to insert into</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Into(string table) => Expression(new TableExpression(null, table.ValidateArgumentNotNullOrWhitespace(nameof(table))), InsertExpressionPositions.Into);
        /// <summary>
        /// Defines the table to insert into where the table name is taken from <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type whose name to use as table name</typeparam>
        /// <returns>Current builder for method chaining</returns>
        TDerived Into<T>() => Into(typeof(T).Name);
        /// <summary>
        /// Defines the table to insert into where the table name is taken from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <returns>Current builder for method chaining</returns>
        TDerived Into() => Into(typeof(TEntity).Name);
        #endregion

        #region Columns
        #region Column
        /// <summary>
        /// Specifies a column to insert into.
        /// </summary>
        /// <param name="column">The name of the column to insert into</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Column(string column) => Expression(new ColumnExpression(null, column.ValidateArgumentNotNullOrWhitespace(nameof(column))), InsertExpressionPositions.Columns);
        /// <summary>
        /// Specifies a column to insert into by using the name of the property selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to select the property from</typeparam>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Column<T>(Expression<Func<T, object?>> property) => Column(property.ExtractProperty(nameof(property)).Name);
        /// <summary>
        /// Specifies a column to insert into by using the name of the property selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Column(Expression<Func<TEntity, object?>> property) => Column<TEntity>(property);
        #endregion
        #region Columns
        /// <summary>
        /// Specifies the columns to insert into.
        /// </summary>
        /// <param name="columns">The columns to select</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Columns(IEnumerable<string> columns);
        /// <summary>
        /// Specifies the columns to insert into.
        /// </summary>
        /// <param name="column">The primary column to insert into</param>
        /// <param name="columns">Additional columns to insert into</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Columns(string column, params string[] columns) => Columns(Helper.Collection.Enumerate(column, columns));
        #endregion
        #region ColumnsOf
        /// <summary>
        /// Specifies the columns to insert into by selecting the names of all public properties on <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to select the properties from</typeparam>
        /// <param name="excludedProperties">Optional names of properties to exclude</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived ColumnsOf<T>(params string[] excludedProperties);
        /// <summary>
        /// Specifies the columns to insert into by selecting the names of all public properties on <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="excludedProperties">Optional names of properties to exclude</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived ColumnsOf(params string[] excludedProperties) => ColumnsOf<TEntity>(excludedProperties);
        #endregion
        #endregion

        #region Values
        #region Constants
        /// <summary>
        /// Defines the constant values to insert.
        /// </summary>
        /// <param name="values">List of values to insert</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Values(IEnumerable<object> values);
        /// <summary>
        /// Defines the constant values to insert.
        /// </summary>
        /// <param name="value">The first value to insert</param>
        /// <param name="values">Optional additional values to insert</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Values(object value, params object[] values) => Values(Helper.Collection.Enumerate(value, values));
        /// <summary>
        /// Defines the constant values to insert by taking the values from all public properties on <typeparamref name="T"/>.
        /// </summary>
        /// <param name="valueObject">The object to get the values from</param>
        /// <param name="excludedProperties">Optional names of properties to exclude</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived ValuesUsing<T>(T valueObject, params string[] excludedProperties);
        #endregion
        #region Parameters
        /// <summary>
        /// Defines the values to insert by defining sql parameters.
        /// </summary>
        /// <param name="parameters">The parameters to insert with</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Parameters(IEnumerable<string> parameters) => Values(parameters.ValidateArgumentNotNullOrEmpty(nameof(parameters)).Select(x => new SqlParameterExpression(x)));
        /// <summary>
        /// Defines the values to insert by defining sql parameters.
        /// </summary>
        /// <param name="parameter">The first parameter to insert with</param>
        /// <param name="parameters">Optional additional parameters to insert with</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Parameters(string parameter, params string[] parameters) => Parameters(Helper.Collection.Enumerate(parameter, parameters));
        /// <summary>
        /// Defines the values to insert by defining sql parameters where the parameters names are taken from all public properties on <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to get the properties from</typeparam>
        /// <param name="suffix">Optional suffix that needs to be appended to the parameters name. When set to null the suffix is omited</param>
        /// <param name="excludedProperties">Optional names of properties to exclude</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived ParametersFrom<T>(int? suffix = null, params string[] excludedProperties);
        /// <summary>
        /// Defines the values to insert by defining sql parameters where the parameters names are taken from all public properties on <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="suffix">Optional suffix that needs to be appended to the parameters name. When set to null the suffix is omited</param>
        /// <param name="excludedProperties">Optional names of properties to exclude</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived ParametersFrom(int? suffix = null, params string[] excludedProperties) => ParametersFrom<TEntity>(suffix, excludedProperties);
        #endregion
        #endregion
    }

    /// <inheritdoc cref="IInsertQueryBuilder{TEntity, TDerived}"/>
    public interface IInsertQueryBuilder<TEntity> : IInsertQueryBuilder<TEntity, IInsertQueryBuilder<TEntity>>
    {

    }
}
