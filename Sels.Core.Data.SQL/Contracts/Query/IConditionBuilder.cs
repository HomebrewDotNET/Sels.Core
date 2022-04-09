using Sels.Core.Data.SQL.Query.Expressions;
using Sels.Core.Data.SQL.Query.Expressions.Condition;
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
    /// Builder for adding conditions to a sql query.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to create the query for</typeparam>
    /// <typeparam name="TDerived">The type of the builder to create the conditions for</typeparam>
    public interface IConditionBuilder<TEntity, TDerived>
    {
        /// <summary>
        /// Adds conditions to the current builder using <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">Builder for adding conditions</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Where(Action<IConditionExpressionBuilder<TEntity>> builder);
    }

    /// <summary>
    /// Builder for creating condition expressions.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to build the query for</typeparam>
    public interface IConditionExpressionBuilder<TEntity> : IConditionSharedExpressionBuilder<TEntity, IConditionOperatorExpressionBuilder<TEntity>>
    {
        /// <summary>
        /// Inverts the result of the next condition by using the NOT keyword.
        /// </summary>
        /// <returns>Current selector for method chaining</returns>
        IConditionExpressionBuilder<TEntity> Not();
        /// <summary>
        /// Any conditions created using <paramref name="builder"/> will be wrapped between ().
        /// </summary>
        /// <param name="builder">The builder to create the conditions within the codition group</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedConditionBuilder<TEntity> WhereGroup(Action<IConditionExpressionBuilder<TEntity>> builder);

        #region FullExpression
        /// <summary>
        /// Adds a full sql condition expression.
        /// </summary>
        /// <param name="expression">The sql condition expression to add</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedConditionBuilder<TEntity> FullExpression(IConditionExpression expression);
        /// <summary>
        /// Adds a full sql condition expression.
        /// </summary>
        /// <param name="expression">The sql expression to add</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedConditionBuilder<TEntity> FullExpression(IExpression expression) => FullExpression(new FullConditionExpression(expression.ValidateArgument(nameof(expression))));
        /// <summary>
        /// Adds a full sql condition expression.
        /// </summary>
        /// <param name="expression">String containing the sql expression</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedConditionBuilder<TEntity> FullExpression(string expression) => FullExpression(new RawExpression(expression.ValidateArgumentNotNullOrWhitespace(nameof(expression))));
        /// <summary>
        /// Adds a full sql condition expression.
        /// </summary>
        /// <param name="expression">String containing the sql expression</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedConditionBuilder<TEntity> FullExpression(Action<StringBuilder> expression) => FullExpression(new DelegateExpression(expression.ValidateArgument(nameof(expression))));
        #endregion

        #region Exists
        /// <summary>
        /// Condition is true when any rows are returned by the sub query.
        /// </summary>
        /// <param name="query">Delegate that returns the query string</param>
        /// <returns>Builder for creating the sub query expression</returns>
        IChainedConditionBuilder<TEntity> ExistsIn(Func<QueryBuilderOptions, string> query) => Expression(NullExpression.Value).CompareTo(new EnumExpression<Operators>(Operators.Exists)).Query(query.ValidateArgument(nameof(query)));
        /// <summary>
        /// Condition is true when any rows are returned by the sub query.
        /// </summary>
        /// <param name="query">The query string</param>
        /// <returns>Builder for creating the sub query expression</returns>
        IChainedConditionBuilder<TEntity> ExistsIn(string query) => Expression(NullExpression.Value).CompareTo(new EnumExpression<Operators>(Operators.Exists)).Query(query.ValidateArgumentNotNullOrWhitespace(nameof(query)));
        /// <summary>
        /// Condition is true when any rows are returned by the sub query.
        /// </summary>
        /// <param name="builder">Builder for creating the sub query</param>
        /// <returns>Builder for creating the sub query expression</returns>
        IChainedConditionBuilder<TEntity> ExistsIn(IQueryBuilder builder) => Expression(NullExpression.Value).CompareTo(new EnumExpression<Operators>(Operators.Exists)).Query(builder.ValidateArgument(nameof(builder)));
        #endregion
    }

    /// <summary>
    /// Builder for creating the expressionon the right-hand side of a condition.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to build the query for</typeparam>
    public interface IConditionRightExpressionBuilder<TEntity> : IConditionSharedExpressionBuilder<TEntity, IChainedConditionBuilder<TEntity>>
    {
        #region List
        /// <summary>
        /// Compares an expression to a list of values.
        /// </summary>
        /// <param name="values">List of values that the expression should be compared to</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedConditionBuilder<TEntity> Values(IEnumerable<object> values) => Expression(new ListExpression(values.ValidateArgument(nameof(values)).Select(x => new SqlConstantExpression(x))));
        /// <summary>
        /// Compares an expression to a list of values.
        /// </summary>
        /// <param name="value">The first value in the list of values to compare to</param>
        /// <param name="values">Any additional values to conpare to</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedConditionBuilder<TEntity> Values(object value, params object[] values) => Values(Helper.Collection.EnumerateAll(value.AsArrayOrDefault(), values));
        /// <summary>
        /// Compares an expression to a list of sql parameters.
        /// </summary>
        /// <param name="parameter">The first parameter in the list of values to compare to</param>
        /// <param name="parameters">Any additional parameters in the list of values to conpare to</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedConditionBuilder<TEntity> Parameters(string parameter, params string[] parameters) => Parameters(Helper.Collection.EnumerateAll(parameter.AsArrayOrDefault(), parameters));
        /// <summary>
        /// Compares an expression to a list of sql parameters.
        /// </summary>
        /// <param name="parameters">The list of sql parameters to compare to</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedConditionBuilder<TEntity> Parameters(IEnumerable<string> parameters) => Values(parameters.ValidateArgument(nameof(parameters)).Select(x => new SqlParameterExpression(x)));
        #endregion
    }

    /// <summary>
    /// Builder for configuring how to compare 2 sql expressions.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to build the query for</typeparam>
    public interface IConditionOperatorExpressionBuilder<TEntity>
    {
        #region Expression
        /// <summary>
        /// Compares 2 expressions using the operator defined in <paramref name="sqlExpression"/>.
        /// </summary>
        /// <param name="sqlExpression">The sql expression containing the operator</param>
        /// <returns>Builder for selecting what to compare to the first expression</returns>
        IConditionRightExpressionBuilder<TEntity> CompareTo(IExpression sqlExpression);
        /// <summary>
        /// Compares 2 expressions using the operator defined in <paramref name="sqlExpression"/>.
        /// </summary>
        /// <param name="sqlExpression">String containing the sql operator</param>
        /// <returns>Builder for selecting what to compare to the first expression</returns>
        IConditionRightExpressionBuilder<TEntity> CompareTo(string sqlExpression) => CompareTo(new RawExpression(sqlExpression.ValidateArgumentNotNullOrEmpty(nameof(sqlExpression))));
        /// <summary>
        /// Compares 2 expressions using the operator defined in <paramref name="sqlExpression"/>.
        /// </summary>
        /// <param name="sqlExpression">Delegate that adds the sql operator to compare to the provided string builder</param>
        /// <returns>Builder for selecting what to compare to the first expression</returns>
        IConditionRightExpressionBuilder<TEntity> CompareTo(Action<StringBuilder> sqlExpression) => CompareTo(new DelegateExpression(sqlExpression.ValidateArgument(nameof(sqlExpression))));
        #endregion

        #region Operators
        /// <summary>
        /// The expressions should be equal.
        /// </summary>
        /// <returns>Builder for selecting what to compare to the first expression</returns>
        IConditionRightExpressionBuilder<TEntity> EqualTo() => CompareTo(new EnumExpression<Operators>(Operators.Equal));
        /// <summary>
        /// The expressions should not be equal.
        /// </summary>
        /// <returns>Builder for selecting what to compare to the first expression</returns>
        IConditionRightExpressionBuilder<TEntity> NotEqualTo() => CompareTo(new EnumExpression<Operators>(Operators.NotEqual));
        /// <summary>
        /// First expression should be greater than second expression.
        /// </summary>
        /// <returns>Builder for selecting what to compare to the first expression</returns>
        IConditionRightExpressionBuilder<TEntity> GreaterThan() => CompareTo(new EnumExpression<Operators>(Operators.Greater));
        /// <summary>
        /// First expression should be lesser than second expression.
        /// </summary>
        /// <returns>Builder for selecting what to compare to the first expression</returns>
        IConditionRightExpressionBuilder<TEntity> LesserThan() => CompareTo(new EnumExpression<Operators>(Operators.Less));
        /// <summary>
        /// First expression should be greater or equal to second expression.
        /// </summary>
        /// <returns>Builder for selecting what to compare to the first expression</returns>
        IConditionRightExpressionBuilder<TEntity> GreaterOrEqualTo() => CompareTo(new EnumExpression<Operators>(Operators.GreaterOrEqual));
        /// <summary>
        /// First expression should be lesser or equal to second expression.
        /// </summary>
        /// <returns>Builder for selecting what to compare to the first expression</returns>
        IConditionRightExpressionBuilder<TEntity> LesserOrEqualTo() => CompareTo(new EnumExpression<Operators>(Operators.LessOrEqual));
        /// <summary>
        /// First expression should be like the pattern defined in the second expression.
        /// </summary>
        /// <returns>Builder for selecting what to compare to the first expression</returns>
        IConditionRightExpressionBuilder<TEntity> Like() => CompareTo(new EnumExpression<Operators>(Operators.Like));
        /// <summary>
        /// First expression should not be like the pattern defined in the second expression.
        /// </summary>
        /// <returns>Builder for selecting what to compare to the first expression</returns>
        IConditionRightExpressionBuilder<TEntity> NotLike() => CompareTo(new EnumExpression<Operators>(Operators.NotLike));
        /// <summary>
        /// First expression should exist in a list of values defined by the second expression.
        /// </summary>
        /// <returns>Builder for selecting what to compare to the first expression</returns>
        IConditionRightExpressionBuilder<TEntity> In() => CompareTo(new EnumExpression<Operators>(Operators.In));
        /// <summary>
        /// First expression should not exist in a list of values defined by the second expression.
        /// </summary>
        /// <returns>Builder for selecting what to compare to the first expression</returns>
        IConditionRightExpressionBuilder<TEntity> NotIn() => CompareTo(new EnumExpression<Operators>(Operators.NotIn));
        /// <summary>
        /// First expression should be greater than <paramref name="lower"/> and lesser than <paramref name="top"/>.
        /// </summary>
        /// <param name="lower">The lowest value in the range</param>
        /// <param name="top">The highest value in the range</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedConditionBuilder<TEntity> Between(object lower, object top) => CompareTo(new EnumExpression<Operators>(Operators.Between)).Expression(new BetweenValuesExpression(new SqlConstantExpression(lower.ValidateArgument(nameof(lower))), new SqlConstantExpression(top.ValidateArgument(nameof(top)))));
        /// <summary>
        /// First expression should be not greater than <paramref name="lower"/> and not lesser than <paramref name="top"/>.
        /// </summary>
        /// <param name="lower">The lowest value in the range</param>
        /// <param name="top">The highest value in the range</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedConditionBuilder<TEntity> NotBetween(object lower, object top) => CompareTo(new EnumExpression<Operators>(Operators.NotBetween)).Expression(new BetweenValuesExpression(new SqlConstantExpression(lower.ValidateArgument(nameof(lower))), new SqlConstantExpression(top.ValidateArgument(nameof(top)))));
        /// <summary>
        /// Checks if first expression is equal to NULL.
        /// </summary>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedConditionBuilder<TEntity> IsNull() => CompareTo(new EnumExpression<Operators>(Operators.Is)).Value(DBNull.Value);
        /// <summary>
        /// Checks if first expression is not equal to NULL.
        /// </summary>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedConditionBuilder<TEntity> IsNotNull() => CompareTo(new EnumExpression<Operators>(Operators.IsNot)).Value(DBNull.Value);
        #endregion       

        #region Overloads
        /// <summary>
        /// First expression should be equal to <paramref name="constantValue"/>.
        /// </summary>
        /// <param name="constantValue">Object containing the constant sql value</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedConditionBuilder<TEntity> EqualTo(object constantValue) => EqualTo().Value(constantValue);
        /// <summary>
        /// First expression should not be equal to <paramref name="constantValue"/>.
        /// </summary>
        /// <param name="constantValue">Object containing the constant sql value</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedConditionBuilder<TEntity> NotEqualTo(object constantValue) => NotEqualTo().Value(constantValue);
        /// <summary>
        /// First expression should be greater than <paramref name="constantValue"/>.
        /// </summary>
        /// <param name="constantValue">Object containing the constant sql value</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedConditionBuilder<TEntity> GreaterThan(object constantValue) => GreaterThan().Value(constantValue);
        /// <summary>
        /// First expression should be lesser than <paramref name="constantValue"/>.
        /// </summary>
        /// <param name="constantValue">Object containing the constant sql value</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedConditionBuilder<TEntity> LesserThan(object constantValue) => LesserThan().Value(constantValue);
        /// <summary>
        /// First expression should be greater or equal to <paramref name="constantValue"/>.
        /// </summary>
        /// <param name="constantValue">Object containing the constant sql value</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedConditionBuilder<TEntity> GreaterOrEqualTo(object constantValue) => GreaterOrEqualTo().Value(constantValue);
        /// <summary>
        /// First expression should be lesser or equal to <paramref name="constantValue"/>.
        /// </summary>
        /// <param name="constantValue">Object containing the constant sql value</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedConditionBuilder<TEntity> LesserOrEqualTo(object constantValue) => LesserOrEqualTo().Value(constantValue);
        /// <summary>
        /// First expression should be like <paramref name="pattern"/>.
        /// </summary>
        /// <param name="pattern">The pattern that the first expression should be like</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedConditionBuilder<TEntity> Like(string pattern) => Like().Value(pattern);
        /// <summary>
        /// First expression should not be like <paramref name="pattern"/>.
        /// </summary>
        /// <param name="pattern">The pattern that the first expression should not be like</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedConditionBuilder<TEntity> NotLike(string pattern) => NotLike().Value(pattern);
        /// <summary>
        /// First expression should exist in a list of values.
        /// </summary>
        /// <param name="values">List of values that the expression should be compared to</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedConditionBuilder<TEntity> In(IEnumerable<object> values) => In().Values(values);
        /// <summary>
        /// First expression should exist in a list of values.
        /// </summary>
        /// <param name="value">The first value in the list of values to compare to</param>
        /// <param name="values">Any additional values to conpare to</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedConditionBuilder<TEntity> In(object value, params object[] values) => In().Values(value, values);
        /// <summary>
        /// First expression should not exist in a list of values.
        /// </summary>
        /// <param name="values">List of values that the expression should be compared to</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedConditionBuilder<TEntity> NotIn(IEnumerable<object> values) => NotIn().Values(values);
        /// <summary>
        /// First expression should not exist in a list of values.
        /// </summary>
        /// <param name="value">The first value in the list of values to compare to</param>
        /// <param name="values">Any additional values to conpare to</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedConditionBuilder<TEntity> NotIn(object value, params object[] values) => NotIn().Values(value, values);
        #endregion
    }

    /// <summary>
    /// Builder for creating expressions in a condition that can appear both on the left and right side of the condition.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to build the query for</typeparam>
    /// <typeparam name="TReturn">The type to return for the fluent syntax</typeparam>
    public interface IConditionSharedExpressionBuilder<TEntity, TReturn>
    {
        #region Expression
        /// <summary>
        /// Adds a sql expression to the condition.
        /// </summary>
        /// <param name="expression">The sql expression to add</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn Expression(IExpression expression);
        /// <summary>
        /// Adds a raw sql expression to the condition.
        /// </summary>
        /// <param name="sqlExpression">String containing the sql expression</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn Expression(string sqlExpression) => Expression(new RawExpression(sqlExpression.ValidateArgumentNotNullOrEmpty(sqlExpression)));
        /// <summary>
        /// Adds a sql expression to the condition.
        /// </summary>
        /// <param name="sqlExpression">Delegate that adds the sql expression to the provided string builder</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn Expression(Action<StringBuilder> sqlExpression) => Expression(new DelegateExpression(sqlExpression.ValidateArgument(nameof(sqlExpression))));
        #endregion

        #region Column
        /// <summary>
        /// Adds a column expression.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select <paramref name="column"/> from</param>
        /// <param name="column">The column to create the condition for</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn Column(object? dataset, string column) => Expression(new ColumnExpression(dataset, column.ValidateArgumentNotNullOrWhitespace(nameof(column))));
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
        /// <param name="dataset">Overwrites the default dataset name defined for type <typeparamref name="T"/>. If a type is used the alias defined for the type is taken</param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn Column<T>(object? dataset, Expression<Func<T, object?>> property) => Column(dataset, property.ValidateArgument(nameof(property)).ExtractProperty(nameof(property)).Name);
        /// <summary>
        /// Adds a column expression where the column name is taken from the property name selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to select the property from</typeparam>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn Column<T>(Expression<Func<T, object?>> property) => Column<T>(typeof(T), property);
        /// <summary>
        /// Adds a column expression where the column name is taken from the property name selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn Column(Expression<Func<TEntity, object?>> property) => Column<TEntity>(property);
        /// <summary>
        /// Adds a column expression where the column name is taken from the property name selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="dataset">Overwrites the default dataset name defined for type <typeparamref name="TEntity"/>. If a type is used the alias defined for the type is taken</param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn Column(object? dataset, Expression<Func<TEntity, object?>> property) => Column<TEntity>(dataset, property);
        #endregion

        #region Value
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
        TReturn Parameter<T>(Expression<Func<T, object?>> property) => Parameter(property.ValidateArgument(nameof(property)).ExtractProperty(nameof(property)).Name);
        /// <summary>
        /// Adds a sql parameter expression where the parameter name is taken from the property name selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn Parameter(Expression<Func<TEntity, object?>> property) => Parameter<TEntity>(property);
        #endregion
        #region Query
        /// <summary>
        /// Adds a sub query expression.
        /// </summary>
        /// <param name="query">Delegate that returns the query string</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn Query(Func<QueryBuilderOptions, string> query) => Expression(new SubQueryExpression(null, query.ValidateArgument(nameof(query))));
        /// <summary>
        /// Adds a sub query expression.
        /// </summary>
        /// <param name="query">The query string</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn Query(string query) => Query(x => query.ValidateArgumentNotNullOrWhitespace(nameof(query)));
        /// <summary>
        /// Adds a sub query expression.
        /// </summary>
        /// <param name="builder">Builder for creating the sub query</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn Query(IQueryBuilder builder) => Query(x => builder.ValidateArgument(nameof(builder)).Build(x));
        #endregion
    }

    /// <summary>
    /// Builder for creating conditions for sql queries chained after previously created expressions.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to build the query for</typeparam>
    public interface IChainedConditionBuilder<TEntity> : IConditionExpressionBuilder<TEntity>
    {
        /// <summary>
        /// Sets the logic operator on how to join the current condition and the condition created after calling this method.
        /// </summary>
        /// <param name="logicOperator">The logic operator to use</param>
        /// <returns>Current builder for method chaining</returns>
        IConditionExpressionBuilder<TEntity> And(LogicOperators logicOperator = LogicOperators.And);
        /// <summary>
        /// Current condition and the condition created after calling this method either need to result in true.
        /// </summary>
        /// <returns>Current builder for method chaining</returns>
        IConditionExpressionBuilder<TEntity> Or() => And(LogicOperators.Or);
    }
}
