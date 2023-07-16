﻿using Sels.SQL.QueryBuilder.Builder.Expressions;
using Sels.SQL.QueryBuilder.Expressions;
using System;
using System.Linq.Expressions;
using System.Text;
using SqlConstantExpression = Sels.SQL.QueryBuilder.Builder.Expressions.ConstantExpression;
using SqlParameterExpression = Sels.SQL.QueryBuilder.Builder.Expressions.ParameterExpression;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Reflection;
using Sels.Core.Models;
namespace Sels.SQL.QueryBuilder.Builder.Statement
{
    /// <summary>
    /// Builder that adds common sql expressions.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to build the query for</typeparam>
    /// <typeparam name="TReturn">The type to return for the fluent syntax</typeparam>
    public interface ISharedExpressionBuilder<TEntity, out TReturn>
    {
        #region Expression
        /// <summary>
        /// Adds a sql expression to the builder.
        /// </summary>
        /// <param name="expression">The sql expression to add</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn Expression(IExpression expression);
        /// <summary>
        /// Adds a raw sql expression to the builder.
        /// </summary>
        /// <param name="sqlExpression">String containing the sql expression</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn Expression(string sqlExpression) => Expression(new RawExpression(sqlExpression.ValidateArgumentNotNullOrEmpty(sqlExpression)));
        /// <summary>
        /// Adds a sql expression to the builder.
        /// </summary>
        /// <param name="sqlExpression">Delegate that adds the sql expression to the provided string builder</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn Expression(Action<StringBuilder, ExpressionCompileOptions> sqlExpression) => Expression(new DelegateExpression(sqlExpression.ValidateArgument(nameof(sqlExpression))));
        #endregion

        #region Column
        /// <summary>
        /// Adds a column expression.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select <paramref name="column"/> from</param>
        /// <param name="column">The column to create the condition for</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn Column(object dataset, string column) => Expression(new ColumnExpression(dataset, column.ValidateArgumentNotNullOrWhitespace(nameof(column))));
        /// <summary>
        /// Adds a column expression.
        /// </summary>
        /// <param name="column">The column to create the condition for</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn Column(string column) => Column(null, column);
        /// <summary>
        /// Adds a column expression where the column name is taken from the property name selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to select the property from</typeparam>
        /// <param name="dataset">Overwrites the default dataset name defined for type <typeparamref name="T"/>. If a type is used the alias defined for the type is taken. Set to an empty string to omit the dataset alias</param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn Column<T>(object dataset, Expression<Func<T, object>> property) => Column(dataset, property.ValidateArgument(nameof(property)).ExtractProperty(nameof(property)).Name);
        /// <summary>
        /// Adds a column expression where the column name is taken from the property name selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to select the property from</typeparam>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn Column<T>(Expression<Func<T, object>> property) => Column<T>(typeof(T), property);
        /// <summary>
        /// Adds a column expression where the column name is taken from the property name selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn Column(Expression<Func<TEntity, object>> property) => Column<TEntity>(property);
        /// <summary>
        /// Adds a column expression where the column name is taken from the property name selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="dataset">Overwrites the default dataset name defined for type <typeparamref name="TEntity"/>. If a type is used the alias defined for the type is taken. Set to an empty string to omit the dataset alias</param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn Column(object dataset, Expression<Func<TEntity, object>> property) => Column<TEntity>(dataset, property);
        #endregion

        #region Value
        /// <summary>
        /// Adds a SQL null value.
        /// </summary>
        /// <returns>Builder for creating more expressions</returns>
        TReturn Null() => Value(DBNull.Value);
        /// <summary>
        /// Adds a constant sql value expression.
        /// </summary>
        /// <param name="constantValue">Object containing the constant sql value to compare to</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn Value(object constantValue) => Expression(new SqlConstantExpression(constantValue.ValidateArgument(nameof(constantValue))));
        #endregion

        #region Parameter
        /// <summary>
        /// Adds a sql parameter expression.
        /// </summary>
        /// <param name="parameter">The name of the sql parameter</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn Parameter(string parameter) => Expression(new SqlParameterExpression(parameter.ValidateArgumentNotNullOrWhitespace(nameof(parameter))));
        /// <summary>
        /// Adds a sql parameter expression where the parameter name is taken from the property name selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to select the property from</typeparam>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn Parameter<T>(Expression<Func<T, object>> property) => Parameter(property.ValidateArgument(nameof(property)).ExtractProperty(nameof(property)).Name);
        /// <summary>
        /// Adds a sql parameter expression where the parameter name is taken from the property name selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn Parameter(Expression<Func<TEntity, object>> property) => Parameter<TEntity>(property);
        #endregion

        #region Variable
        /// <summary>
        /// Adds a sql variable expression.
        /// </summary>
        /// <param name="variable">The name of the sql variable</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn Variable(string variable) => Expression(new VariableExpression(variable.ValidateArgumentNotNullOrWhitespace(nameof(variable))));
        /// <summary>
        /// Adds a sql variable expression where the variable name is taken from the property name selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to select the property from</typeparam>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn Variable<T>(Expression<Func<T, object>> property) => Variable(property.ValidateArgument(nameof(property)).ExtractProperty(nameof(property)).Name);
        /// <summary>
        /// Adds a sql variable expression where the variable name is taken from the property name selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn Variable(Expression<Func<TEntity, object>> property) => Variable<TEntity>(property);
        #endregion

        #region VariableAssignment
        /// <summary>
        /// Assigns a new value to a SQL variable.
        /// </summary>
        /// <param name="variable">The name of the sql variable</param>
        /// <param name="constant">The constant value to assign to the variable</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn AssignVariable(string variable, object constant) => AssignVariable(variable, e => e.Value(constant));
        /// <summary>
        /// Assigns a new value to a SQL variable.
        /// </summary>
        /// <typeparam name="T">The main entity for build the query for</typeparam>
        /// <param name="variable">The name of the sql variable</param>
        /// <param name="builder">Builder to select the value to assign</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn AssignVariable<T>(string variable, Action<ISharedExpressionBuilder<T, Null>> builder) => Expression(new VariableInlineAssignmentExpression(new VariableExpression(variable.ValidateArgumentNotNullOrWhitespace(nameof(variable))), new ExpressionBuilder<T>(builder)));
        /// <summary>
        /// Assigns a new value to a SQL variable.
        /// </summary>
        /// <param name="variable">The name of the sql variable</param>
        /// <param name="builder">Builder to select the value to assign</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn AssignVariable(string variable, Action<ISharedExpressionBuilder<TEntity, Null>> builder) => AssignVariable<TEntity>(variable, builder);
        /// <summary>
        /// Assigns a new value to a SQL variable where the variable name is taken from the property name selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The main entity for build the query for</typeparam>
        /// <param name="property">The expression that points to the property to use</param>
        ///  <param name="builder">Builder to select the value to assign</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn AssignVariable<T>(Expression<Func<T, object>> property, Action<ISharedExpressionBuilder<T, Null>> builder) => AssignVariable(property.ValidateArgument(nameof(property)).ExtractProperty(nameof(property)).Name, builder);
        /// <summary>
        /// Assigns a new value to a SQL variable where the variable name is taken from the property name selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="builder">Builder to select the value to assign</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn AssignVariable(Expression<Func<TEntity, object>> property, Action<ISharedExpressionBuilder<TEntity, Null>> builder) => AssignVariable<TEntity>(property, builder);
        #endregion

        #region Query
        /// <summary>
        /// Adds a sub query expression.
        /// </summary>
        /// <param name="query">Delegate that adds the query to the supplied builder</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn Query(Action<StringBuilder, ExpressionCompileOptions> query) => Expression(new SubQueryExpression(null, query.ValidateArgument(nameof(query))));
        /// <summary>
        /// Adds a sub query expression.
        /// </summary>
        /// <param name="query">The query string</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn Query(string query) => Query((b, o) => b.Append(query.ValidateArgumentNotNullOrWhitespace(nameof(query))));
        /// <summary>
        /// Adds a sub query expression.
        /// </summary>
        /// <param name="builder">Builder for creating the sub query</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn Query(IQueryBuilder builder) => Expression(new SubQueryExpression(null, builder.ValidateArgument(nameof(builder))));
        #endregion

        #region Case
        /// <summary>
        /// Adds a case expression.
        /// </summary>
        /// <param name="caseBuilder">Delegate that configures the case expression</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn Case(Action<ICaseExpressionRootBuilder<TEntity>> caseBuilder) => Case<TEntity>(caseBuilder);
        /// <summary>
        /// Adds a case expression.
        /// </summary>
        /// <typeparam name="T">The main type to create the case expression with</typeparam>
        /// <param name="caseBuilder">Delegate that configures the case expression</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn Case<T>(Action<ICaseExpressionRootBuilder<T>> caseBuilder) => Expression(new WrappedExpression(new CaseExpression<T>(caseBuilder)));
        #endregion

        #region Date
        /// <summary>
        /// Adds an expression that returns the current date.
        /// </summary>
        /// <param name="type">Determines which date is returned</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn CurrentDate(DateType type = DateType.Utc) => Expression(new CurrentDateExpression(type));

        /// <summary>
        /// Adds an expression where a date is modified.
        /// </summary>
        /// <param name="date">String that contains the date to modify</param>
        /// <param name="amount">The amount to add/substract to/from the date</param>
        /// <param name="interval"><inheritdoc cref="DateInterval"/></param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn ModifyDate(string date, double amount, DateInterval interval = DateInterval.Millisecond) => Expression(new ModifyDateExpression(new SqlConstantExpression(date.ValidateArgumentNotNullOrWhitespace(nameof(date))), new SqlConstantExpression(amount), interval));
        /// <summary>
        /// Adds an expression where a date is modified.
        /// </summary>
        /// <param name="date">The date to modify</param>
        /// <param name="amount">The amount to add/substract to/from the date</param>
        /// <param name="interval"><inheritdoc cref="DateInterval"/></param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn ModifyDate(DateTimeOffset date, double amount, DateInterval interval = DateInterval.Millisecond) => Expression(new ModifyDateExpression(new SqlConstantExpression(date), new SqlConstantExpression(amount), interval));
        /// <summary>
        /// Adds an expression where a date is modified.
        /// </summary>
        /// <typeparam name="T">The main entity to build the expression for</typeparam>
        /// <param name="dateExpressionBuilder">Delegate that selects the date expression to modify</param>
        /// <param name="amount">The amount to add/substract to/from the date</param>
        /// <param name="interval"><inheritdoc cref="DateInterval"/></param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn ModifyDate<T>(Action<ISharedExpressionBuilder<T, Null>> dateExpressionBuilder, double amount, DateInterval interval = DateInterval.Millisecond) => Expression(new ModifyDateExpression(new ExpressionBuilder<T>(dateExpressionBuilder.ValidateArgument(nameof(dateExpressionBuilder))), new SqlConstantExpression(amount), interval));
        /// <summary>
        /// Adds an expression where a date is modified.
        /// </summary>
        /// <param name="dateExpressionBuilder">Delegate that selects the date expression to modify</param>
        /// <param name="amount">The amount to add/substract to/from the date</param>
        /// <param name="interval"><inheritdoc cref="DateInterval"/></param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn ModifyDate(Action<ISharedExpressionBuilder<TEntity, Null>> dateExpressionBuilder, double amount, DateInterval interval = DateInterval.Millisecond) => ModifyDate<TEntity>(dateExpressionBuilder, amount, interval);
        /// <summary>
        /// Adds an expression where a date is modified.
        /// </summary>
        /// <typeparam name="T">The main entity to build the expression for</typeparam>
        /// <param name="dateExpressionBuilder">Delegate that selects the date expression to modify</param>
        /// <param name="amountExpressionBuilder">Delegate that selects the value expression</param>
        /// <param name="interval"><inheritdoc cref="DateInterval"/></param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn ModifyDate<T>(Action<ISharedExpressionBuilder<T, Null>> dateExpressionBuilder, Action<ISharedExpressionBuilder<T, Null>> amountExpressionBuilder, DateInterval interval = DateInterval.Millisecond) => Expression(new ModifyDateExpression(new ExpressionBuilder<T>(dateExpressionBuilder.ValidateArgument(nameof(dateExpressionBuilder))), new ExpressionBuilder<T>(amountExpressionBuilder.ValidateArgument(nameof(amountExpressionBuilder))), interval));
        /// <summary>
        /// Adds an expression where a date is modified.
        /// </summary>
        /// <param name="dateExpressionBuilder">Delegate that selects the date expression to modify</param>
        /// <param name="amountExpressionBuilder">Delegate that selects the value expression</param>
        /// <param name="interval"><inheritdoc cref="DateInterval"/></param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn ModifyDate(Action<ISharedExpressionBuilder<TEntity, Null>> dateExpressionBuilder, Action<ISharedExpressionBuilder<TEntity, Null>> amountExpressionBuilder, DateInterval interval = DateInterval.Millisecond) => ModifyDate<TEntity>(dateExpressionBuilder, amountExpressionBuilder, interval);
        #endregion
    }
}
