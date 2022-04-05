using Sels.Core.Data.SQL.Query.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using SqlConstantExpression = Sels.Core.Data.SQL.Query.Expressions.ConstantExpression;
using SqlParameterExpression = Sels.Core.Data.SQL.Query.Expressions.ParameterExpression;

namespace Sels.Core.Data.SQL.Query
{
    /// <summary>
    /// Builder for creating conditions for sql queries.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to build the query for</typeparam>
    /// <typeparam name="TDerived">The type to return for the fluent syntax</typeparam>
    public interface IConditionBuilder<TEntity, TDerived>
    {
        /// <summary>
        /// Inverts the result of the next condition by using the NOT keyword.
        /// </summary>
        /// <returns>Current selector for method chaining</returns>
        IConditionBuilder<TEntity, TDerived> Not();
        /// <summary>
        /// Any conditions created using <paramref name="builder"/> will be wrapped between ().
        /// </summary>
        /// <param name="builder">The builder to create the conditions within the codition group</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedConditionBuilder<TEntity, TDerived> WhereGroup(Action<IConditionBuilder<TEntity, TDerived>> builder);

        #region Column
        /// <summary>
        /// Creates a condition for <paramref name="column"/>.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select <paramref name="column"/> from</param>
        /// <param name="column">The column to create the condition for</param>
        /// <returns>Builder for selecting what to compare to <paramref name="column"/></returns>
        IConditionOperatorBuilder<TEntity, TDerived> WhereColumn(object? dataset, string column) =>  WhereExpression(new ColumnExpression(dataset, column.ValidateArgumentNotNullOrWhitespace(nameof(column))));
        /// <summary>
        /// Creates a condition for <paramref name="column"/>.
        /// </summary>
        /// <param name="column">The column to create the condition for</param>
        /// <returns>Builder for selecting what to compare to <paramref name="column"/></returns>
        IConditionOperatorBuilder<TEntity, TDerived> WhereColumn(string column) => WhereColumn(null, column);
        /// <summary>
        /// Creates a condition for column where the name is taken from the property name selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to select the property from</typeparam>
        /// <param name="dataset">Overwrites the default dataset name defined for type <typeparamref name="T"/>. If a type is used the alias defined for the type is taken</param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder for selecting what to compare to the column selected by <paramref name="property"/></returns>
        IConditionOperatorBuilder<TEntity, TDerived> WhereColumn<T>(object? dataset, Expression<Func<T, object?>> property) => WhereColumn(dataset, property.ValidateArgument(nameof(property)).ExtractProperty(nameof(property)).Name);
        /// <summary>
        /// Creates a condition for column where the name is taken from the property name selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to select the property from</typeparam>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder for selecting what to compare to the column selected by <paramref name="property"/></returns>
        IConditionOperatorBuilder<TEntity, TDerived> WhereColumn<T>(Expression<Func<T, object?>> property) => WhereColumn<T>(typeof(T), property);
        /// <summary>
        /// Creates a condition for column where the name is taken from the property name selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder for selecting what to compare to the column selected by <paramref name="property"/></returns>
        IConditionOperatorBuilder<TEntity, TDerived> WhereColumn(Expression<Func<TEntity, object?>> property) => WhereColumn<TEntity>(property);
        /// <summary>
        /// Creates a condition for column where the name is taken from the property name selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="dataset">Overwrites the default dataset name defined for type <typeparamref name="TEntity"/>. If a type is used the alias defined for the type is taken</param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder for selecting what to compare to the column selected by <paramref name="property"/></returns>
        IConditionOperatorBuilder<TEntity, TDerived> WhereColumn(object? dataset, Expression<Func<TEntity, object?>> property) => WhereColumn<TEntity>(dataset, property);
        #endregion

        #region Expression
        /// <summary>
        /// Adds a sql expression on the left side of the condition.
        /// </summary>
        /// <param name="expression">The sql expression to add</param>
        /// <returns>Builder for selecting what to compare to the expression</returns>
        IConditionOperatorBuilder<TEntity, TDerived> WhereExpression(IExpression expression);
        /// <summary>
        /// Adds a raw sql expression on the left side of the condition.
        /// </summary>
        /// <param name="sqlExpression">String containing the sql expression</param>
        /// <returns>Builder for selecting what to compare to the expression</returns>
        IConditionOperatorBuilder<TEntity, TDerived> WhereExpression(string sqlExpression) => WhereExpression(new RawExpression(sqlExpression.ValidateArgumentNotNullOrEmpty(sqlExpression)));
        /// <summary>
        /// Adds a raw sql expression on the left side of the condition.
        /// </summary>
        /// <param name="sqlExpression">Delegate that adds the sql expression to the provided string builder</param>
        /// <returns>Builder for selecting what to compare to the expression</returns>
        IConditionOperatorBuilder<TEntity, TDerived> WhereExpression(Action<StringBuilder> sqlExpression) => WhereExpression(new DelegateExpression(sqlExpression.ValidateArgument(nameof(sqlExpression))));

        #endregion
    }
    /// <summary>
    /// Builder for configuring how to compare 2 sql expressions.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to build the query for</typeparam>
    /// <typeparam name="TDerived">The type to return for the fluent syntax</typeparam>
    public interface IConditionOperatorBuilder<TEntity, TDerived>
    {
        #region Expression
        /// <summary>
        /// Compares 2 expressions using the operator defined in <paramref name="sqlExpression"/>.
        /// </summary>
        /// <param name="sqlExpression">The sql expression containing the operator</param>
        /// <returns>Builder for selecting what to compare to the first expression</returns>
        IConditionRightExpressionBuilder<TEntity, TDerived> CompareTo(IExpression sqlExpression);
        /// <summary>
        /// Compares 2 expressions using the operator defined in <paramref name="sqlExpression"/>.
        /// </summary>
        /// <param name="sqlExpression">String containing the sql operator</param>
        /// <returns>Builder for selecting what to compare to the first expression</returns>
        IConditionRightExpressionBuilder<TEntity, TDerived> CompareTo(string sqlExpression) => CompareTo(new RawExpression(sqlExpression.ValidateArgumentNotNullOrEmpty(nameof(sqlExpression))));
        /// <summary>
        /// Compares 2 expressions using the operator defined in <paramref name="sqlExpression"/>.
        /// </summary>
        /// <param name="sqlExpression">Delegate that adds the sql operator to compare to the provided string builder</param>
        /// <returns>Builder for selecting what to compare to the first expression</returns>
        IConditionRightExpressionBuilder<TEntity, TDerived> CompareTo(Action<StringBuilder> sqlExpression) => CompareTo(new DelegateExpression(sqlExpression.ValidateArgument(nameof(sqlExpression))));
        #endregion

        #region Operators
        /// <summary>
        /// The expressions should be equal.
        /// </summary>
        /// <returns>Builder for selecting what to compare to the first expression</returns>
        IConditionRightExpressionBuilder<TEntity, TDerived> EqualTo() => CompareTo(new EnumExpression<Operators>(Operators.Equal));
        /// <summary>
        /// The expressions should not be equal.
        /// </summary>
        /// <returns>Builder for selecting what to compare to the first expression</returns>
        IConditionRightExpressionBuilder<TEntity, TDerived> NotEqualTo() => CompareTo(new EnumExpression<Operators>(Operators.NotEqual));
        /// <summary>
        /// First expression should be greater than second expression.
        /// </summary>
        /// <returns>Builder for selecting what to compare to the first expression</returns>
        IConditionRightExpressionBuilder<TEntity, TDerived> GreaterThan() => CompareTo(new EnumExpression<Operators>(Operators.Greater));
        /// <summary>
        /// First expression should be lesser than second expression.
        /// </summary>
        /// <returns>Builder for selecting what to compare to the first expression</returns>
        IConditionRightExpressionBuilder<TEntity, TDerived> LesserThan() => CompareTo(new EnumExpression<Operators>(Operators.Less));
        /// <summary>
        /// First expression should be greater or equal to second expression.
        /// </summary>
        /// <returns>Builder for selecting what to compare to the first expression</returns>
        IConditionRightExpressionBuilder<TEntity, TDerived> GreaterOrEqualTo() => CompareTo(new EnumExpression<Operators>(Operators.GreaterOrEqual));
        /// <summary>
        /// First expression should be lesser or equal to second expression.
        /// </summary>
        /// <returns>Builder for selecting what to compare to the first expression</returns>
        IConditionRightExpressionBuilder<TEntity, TDerived> LesserOrEqualTo() => CompareTo(new EnumExpression<Operators>(Operators.LessOrEqual));
        /// <summary>
        /// First expression should be like the pattern defined in the second expression.
        /// </summary>
        /// <returns>Builder for selecting what to compare to the first expression</returns>
        IConditionRightExpressionBuilder<TEntity, TDerived> Like() => CompareTo(new EnumExpression<Operators>(Operators.Like));
        /// <summary>
        /// First expression should not be like the pattern defined in the second expression.
        /// </summary>
        /// <returns>Builder for selecting what to compare to the first expression</returns>
        IConditionRightExpressionBuilder<TEntity, TDerived> NotLike() => CompareTo(new EnumExpression<Operators>(Operators.NotLike));
        /// <summary>
        /// First expression should exist in a list of values defined by the second expression.
        /// </summary>
        /// <returns>Builder for selecting what to compare to the first expression</returns>
        IConditionRightExpressionBuilder<TEntity, TDerived> In() => CompareTo(new EnumExpression<Operators>(Operators.In));
        /// <summary>
        /// First expression should not exist in a list of values defined by the second expression.
        /// </summary>
        /// <returns>Builder for selecting what to compare to the first expression</returns>
        IConditionRightExpressionBuilder<TEntity, TDerived> NotIn() => CompareTo(new EnumExpression<Operators>(Operators.NotIn));
        /// <summary>
        /// First expression should exist in any of the rows returned by the sub query.
        /// </summary>
        /// <returns>Builder for selecting what to compare to the first expression</returns>
        IConditionRightExpressionBuilder<TEntity, TDerived> ExistsIn() => CompareTo(new EnumExpression<Operators>(Operators.Exists));
        /// <summary>
        /// First expression should not exist in any of the rows returned by the sub query.
        /// </summary>
        /// <returns>Builder for selecting what to compare to the first expression</returns>
        IConditionRightExpressionBuilder<TEntity, TDerived> NotExistsIn() => CompareTo(new EnumExpression<Operators>(Operators.NotExists));
        /// <summary>
        /// First expression should be greater than <paramref name="lower"/> and lesser than <paramref name="top"/>.
        /// </summary>
        /// <param name="lower">The lowest value in the range</param>
        /// <param name="top">The highest value in the range</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedConditionBuilder<TEntity, TDerived> Between(object lower, object top) => CompareTo(new EnumExpression<Operators>(Operators.Between)).Expression(new BetweenValuesExpression(new SqlConstantExpression(lower.ValidateArgument(nameof(lower))), new SqlConstantExpression(top.ValidateArgument(nameof(top)))));
        /// <summary>
        /// First expression should be not greater than <paramref name="lower"/> and not lesser than <paramref name="top"/>.
        /// </summary>
        /// <param name="lower">The lowest value in the range</param>
        /// <param name="top">The highest value in the range</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedConditionBuilder<TEntity, TDerived> NotBetween(object lower, object top) => CompareTo(new EnumExpression<Operators>(Operators.NotBetween)).Expression(new BetweenValuesExpression(new SqlConstantExpression(lower.ValidateArgument(nameof(lower))), new SqlConstantExpression(top.ValidateArgument(nameof(top)))));
        /// <summary>
        /// Checks if first expression is equal to NULL.
        /// </summary>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedConditionBuilder<TEntity, TDerived> IsNull() => CompareTo(new EnumExpression<Operators>(Operators.Is)).Value(DBNull.Value);
        /// <summary>
        /// Checks if first expression is not equal to NULL.
        /// </summary>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedConditionBuilder<TEntity, TDerived> IsNotNull() => CompareTo(new EnumExpression<Operators>(Operators.IsNot)).Value(DBNull.Value);
        #endregion       

        #region Overloads
        /// <summary>
        /// First expression should be equal to <paramref name="constantValue"/>.
        /// </summary>
        /// <param name="constantValue">Object containing the constant sql value</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedConditionBuilder<TEntity, TDerived> EqualTo(object constantValue) => EqualTo().Value(constantValue);
        /// <summary>
        /// First expression should not be equal to <paramref name="constantValue"/>.
        /// </summary>
        /// <param name="constantValue">Object containing the constant sql value</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedConditionBuilder<TEntity, TDerived> NotEqualTo(object constantValue) => NotEqualTo().Value(constantValue);
        /// <summary>
        /// First expression should be greater than <paramref name="constantValue"/>.
        /// </summary>
        /// <param name="constantValue">Object containing the constant sql value</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedConditionBuilder<TEntity, TDerived> GreaterThan(object constantValue) => GreaterThan().Value(constantValue);
        /// <summary>
        /// First expression should be lesser than <paramref name="constantValue"/>.
        /// </summary>
        /// <param name="constantValue">Object containing the constant sql value</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedConditionBuilder<TEntity, TDerived> LesserThan(object constantValue) => LesserThan().Value(constantValue);
        /// <summary>
        /// First expression should be greater or equal to <paramref name="constantValue"/>.
        /// </summary>
        /// <param name="constantValue">Object containing the constant sql value</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedConditionBuilder<TEntity, TDerived> GreaterOrEqualTo(object constantValue) => GreaterOrEqualTo().Value(constantValue);
        /// <summary>
        /// First expression should be lesser or equal to <paramref name="constantValue"/>.
        /// </summary>
        /// <param name="constantValue">Object containing the constant sql value</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedConditionBuilder<TEntity, TDerived> LesserOrEqualTo(object constantValue) => LesserOrEqualTo().Value(constantValue);
        /// <summary>
        /// First expression should be like <paramref name="pattern"/>.
        /// </summary>
        /// <param name="pattern">The pattern that the first expression should be like</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedConditionBuilder<TEntity, TDerived> Like(string pattern) => Like().Value(pattern);
        /// <summary>
        /// First expression should not be like <paramref name="pattern"/>.
        /// </summary>
        /// <param name="pattern">The pattern that the first expression should not be like</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedConditionBuilder<TEntity, TDerived> NotLike(string pattern) => NotLike().Value(pattern);
        /// <summary>
        /// First expression should exist in a list of values.
        /// </summary>
        /// <param name="values">List of values that the expression should be compared to</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedConditionBuilder<TEntity, TDerived> In(IEnumerable<object> values) => In().Values(values);
        /// <summary>
        /// First expression should exist in a list of values.
        /// </summary>
        /// <param name="value">The first value in the list of values to compare to</param>
        /// <param name="values">Any additional values to conpare to</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedConditionBuilder<TEntity, TDerived> In(object value, params object[] values) => In().Values(value, values);
        /// <summary>
        /// First expression should not exist in a list of values.
        /// </summary>
        /// <param name="values">List of values that the expression should be compared to</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedConditionBuilder<TEntity, TDerived> NotIn(IEnumerable<object> values) => NotIn().Values(values);
        /// <summary>
        /// First expression should not exist in a list of values.
        /// </summary>
        /// <param name="value">The first value in the list of values to compare to</param>
        /// <param name="values">Any additional values to conpare to</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedConditionBuilder<TEntity, TDerived> NotIn(object value, params object[] values) => NotIn().Values(value, values);
        #endregion
    }

    /// <summary>
    /// Builder for selecting what to compare to the first expression.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to build the query for</typeparam>
    /// <typeparam name="TDerived">The type to return for the fluent syntax</typeparam>
    public interface IConditionRightExpressionBuilder<TEntity, TDerived>
    {
        #region Column
        /// <summary>
        /// Compares the first expression to a column.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select <paramref name="column"/> from</param>
        /// <param name="column">The column to create the condition for</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedConditionBuilder<TEntity, TDerived> Column(object? dataset, string column) => Expression(new ColumnExpression(dataset, column.ValidateArgumentNotNullOrWhitespace(nameof(column))));
        /// <summary>
        /// Compares the first expression to a column where the name is taken from the property name selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to select the property from</typeparam>
        /// <param name="dataset">Overwrites the default dataset name defined for type <typeparamref name="T"/>. If a type is used the alias defined for the type is taken</param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedConditionBuilder<TEntity, TDerived> Column<T>(object? dataset, Expression<Func<T, object?>> property) => Column(dataset, property.ValidateArgument(nameof(property)).ExtractProperty(nameof(property)).Name);
        /// <summary>
        /// Compares the first expression to a column.
        /// </summary>
        /// <param name="column">The column to create the condition for</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedConditionBuilder<TEntity, TDerived> Column(string column) => Column(null, column);
        /// <summary>
        /// Compares the first expression to a column where the name is taken from the property name selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedConditionBuilder<TEntity, TDerived> Column(Expression<Func<TEntity, object?>> property) => Column<TEntity>(property);
        /// <summary>
        /// Compares the first expression to a column where the name is taken from the property name selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to select the property from</typeparam>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedConditionBuilder<TEntity, TDerived> Column<T>(Expression<Func<T, object?>> property) => Column<T>(typeof(T), property);
        /// <summary>
        /// Compares the first expression to a column where the name is taken from the property name selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="dataset">Overwrites the default dataset name defined for type <typeparamref name="TEntity"/>. If a type is used the alias defined for the type is taken</param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedConditionBuilder<TEntity, TDerived> Column(object? dataset, Expression<Func<TEntity, object?>> property) => Column<TEntity>(dataset, property);
        #endregion

        #region Value
        /// <summary>
        /// Compares an expression to a constant sql value.
        /// </summary>
        /// <param name="constantValue">Object containing the constant sql value to compare to</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedConditionBuilder<TEntity, TDerived> Value(object constantValue) => Expression(new SqlConstantExpression(constantValue.ValidateArgument(nameof(constantValue))));
        #endregion

        #region Parameter
        /// <summary>
        /// Compares the first expression to a parameter.
        /// </summary>
        /// <param name="parameter">The column to create the condition for</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedConditionBuilder<TEntity, TDerived> Parameter(string parameter) => Expression(new SqlParameterExpression(parameter.ValidateArgumentNotNullOrWhitespace(nameof(parameter))));
        /// <summary>
        /// Compares the first expression to a parameter where the name is taken from the property name selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to select the property from</typeparam>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedConditionBuilder<TEntity, TDerived> Parameter<T>(Expression<Func<T, object?>> property) => Parameter(property.ValidateArgument(nameof(property)).ExtractProperty(nameof(property)).Name);
        /// <summary>
        /// Compares the first expression to a parameter where the name is taken from the property name selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedConditionBuilder<TEntity, TDerived> Parameter(Expression<Func<TEntity, object?>> property) => Parameter<TEntity>(property);
        #endregion

        #region Expression
        /// <summary>
        /// Compares an expression to the sql expression defined in <paramref name="sqlExpression"/>.
        /// </summary>
        /// <param name="sqlExpression">The sql expression to compare to</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedConditionBuilder<TEntity, TDerived> Expression(IExpression sqlExpression);
        /// <summary>
        /// Compares an expression to the sql expression defined in <paramref name="sqlExpression"/>.
        /// </summary>
        /// <param name="sqlExpression">String containing the sql expression to compare to</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedConditionBuilder<TEntity, TDerived> Expression(string sqlExpression) => Expression(new RawExpression(sqlExpression.ValidateArgumentNotNullOrEmpty(nameof(sqlExpression))));
        /// <summary>
        /// Compares an expression to the sql expression defined in <paramref name="sqlExpression"/>.
        /// </summary>
        /// <param name="sqlExpression">Delegate that adds the sql expression to compare to the provided string builder</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedConditionBuilder<TEntity, TDerived> Expression(Action<StringBuilder> sqlExpression) => Expression(new DelegateExpression(sqlExpression.ValidateArgument(nameof(sqlExpression))));
        #endregion

        #region Query
        /// <summary>
        /// Compares an expression to a sub query.
        /// </summary>
        /// <param name="query">Delegate that returns the query string</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedConditionBuilder<TEntity, TDerived> Query(Func<QueryBuilderOptions, string> query) => Expression(new SubQueryExpression(null, query.ValidateArgument(nameof(query))));
        /// <summary>
        /// Compares an expression to a sub query.
        /// </summary>
        /// <param name="query">The query string</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedConditionBuilder<TEntity, TDerived> Query(string query) => Query(x => query.ValidateArgumentNotNullOrWhitespace(nameof(query)));
        /// <summary>
        /// Compares an expression to a sub query.
        /// </summary>
        /// <param name="builder">Builder for creating the sub query</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedConditionBuilder<TEntity, TDerived> Query(IQueryBuilder builder) => Query(x => builder.ValidateArgument(nameof(builder)).Build(x));
        #endregion

        #region List
        /// <summary>
        /// Compares an expression to a list of values.
        /// </summary>
        /// <param name="values">List of values that the expression should be compared to</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedConditionBuilder<TEntity, TDerived> Values(IEnumerable<object> values) => Expression(new ListExpression(values.ValidateArgument(nameof(values)).Select(x => new SqlConstantExpression(x))));
        /// <summary>
        /// Compares an expression to a list of values.
        /// </summary>
        /// <param name="value">The first value in the list of values to compare to</param>
        /// <param name="values">Any additional values to conpare to</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedConditionBuilder<TEntity, TDerived> Values(object value, params object[] values) => Values(Helper.Collection.EnumerateAll(value.AsArrayOrDefault(), values));
        /// <summary>
        /// Compares an expression to a list of sql parameters.
        /// </summary>
        /// <param name="parameter">The first parameter in the list of values to compare to</param>
        /// <param name="parameters">Any additional parameters in the list of values to conpare to</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedConditionBuilder<TEntity, TDerived> Parameters(string parameter, params string[] parameters) => Parameters(Helper.Collection.EnumerateAll(parameter.AsArrayOrDefault(), parameters));
        /// <summary>
        /// Compares an expression to a list of sql parameters.
        /// </summary>
        /// <param name="parameters">The list of sql parameters to compare to</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedConditionBuilder<TEntity, TDerived> Parameters(IEnumerable<string> parameters) => Values(parameters.ValidateArgument(nameof(parameters)).Select(x => new SqlParameterExpression(x)));
        #endregion
    }

    /// <summary>
    /// Builder for creating conditions for sql queries chained after previously created expressions.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to build the query for</typeparam>
    /// <typeparam name="TDerived">The type to return for the fluent syntax</typeparam>
    public interface IChainedConditionBuilder<TEntity, TDerived> : IConditionBuilder<TEntity, TDerived>
    {
        /// <summary>
        /// Sets the logic operator on how to join the current condition and the condition created after calling this method.
        /// </summary>
        /// <param name="logicOperator">The logic operator to use</param>
        /// <returns>Current builder for method chaining</returns>
        IConditionBuilder<TEntity, TDerived> And(LogicOperators logicOperator =  LogicOperators.And);
        /// <summary>
        /// Current condition and the condition created after calling this method either need to result in true.
        /// </summary>
        /// <returns>Current builder for method chaining</returns>
        IConditionBuilder<TEntity, TDerived> Or() => And(LogicOperators.Or);
        /// <summary>
        /// Exits the current condition builder.
        /// </summary>
        /// <returns>Builder of type <typeparamref name="TDerived"/> to continue method chaining</returns>
        TDerived Exit();
    }
}
