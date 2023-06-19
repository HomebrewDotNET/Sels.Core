using Sels.SQL.QueryBuilder.Builder.Expressions;
using System;
using System.Text;
using SqlConstantExpression = Sels.SQL.QueryBuilder.Builder.Expressions.ConstantExpression;
using SqlParameterExpression = Sels.SQL.QueryBuilder.Builder.Expressions.ParameterExpression;
using Sels.Core.Extensions;
using System.Collections.Generic;
using Sels.Core;
using System.Linq;
using System.Collections;
using Sels.Core.Extensions.Conversion;

namespace Sels.SQL.QueryBuilder.Builder.Statement
{
    /// <summary>
    /// Builder for adding conditions to a sql query.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to create the query for</typeparam>
    /// <typeparam name="TDerived">The type of the builder to create the conditions for</typeparam>
    public interface IStatementConditionBuilder<TEntity, out TDerived>
    {
        /// <summary>
        /// Adds conditions to the current builder using <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">Builder for adding conditions</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Where(Func<IStatementConditionExpressionBuilder<TEntity>, IChainedBuilder<TEntity, IStatementConditionExpressionBuilder<TEntity>>> builder);
    }

    /// <summary>
    /// Builder for creating condition expressions.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to build the query for</typeparam>
    public interface IStatementConditionExpressionBuilder<TEntity> : ISharedExpressionBuilder<TEntity, IStatementConditionOperatorExpressionBuilder<TEntity>>
    {
        /// <summary>
        /// Inverts the result of the next condition by using the NOT keyword.
        /// </summary>
        /// <returns>Current selector for method chaining</returns>
        IStatementConditionExpressionBuilder<TEntity> Not();
        /// <summary>
        /// Any conditions created using <paramref name="builder"/> will be wrapped between ().
        /// </summary>
        /// <param name="builder">The builder to create the conditions within the codition group</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedBuilder<TEntity, IStatementConditionExpressionBuilder<TEntity>> WhereGroup(Func<IStatementConditionExpressionBuilder<TEntity>, IChainedBuilder<TEntity, IStatementConditionExpressionBuilder<TEntity>>> builder);

        #region FullExpression
        /// <summary>
        /// Adds a full sql condition expression.
        /// </summary>
        /// <param name="expression">The sql condition expression to add</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedBuilder<TEntity, IStatementConditionExpressionBuilder<TEntity>> FullExpression(IConditionExpression expression);
        /// <summary>
        /// Adds a full sql condition expression.
        /// </summary>
        /// <param name="expression">The sql expression to add</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedBuilder<TEntity, IStatementConditionExpressionBuilder<TEntity>> FullExpression(IExpression expression) => FullExpression(new FullConditionExpression(expression.ValidateArgument(nameof(expression))));
        /// <summary>
        /// Adds a full sql condition expression.
        /// </summary>
        /// <param name="expression">String containing the sql expression</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedBuilder<TEntity, IStatementConditionExpressionBuilder<TEntity>> FullExpression(string expression) => FullExpression(new RawExpression(expression.ValidateArgumentNotNullOrWhitespace(nameof(expression))));
        /// <summary>
        /// Adds a full sql condition expression.
        /// </summary>
        /// <param name="expression">String containing the sql expression</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedBuilder<TEntity, IStatementConditionExpressionBuilder<TEntity>> FullExpression(Action<StringBuilder, ExpressionCompileOptions> expression) => FullExpression(new DelegateExpression(expression.ValidateArgument(nameof(expression))));
        #endregion

        #region Exists
        /// <summary>
        /// Condition is true when any rows are returned by the sub query.
        /// </summary>
        /// <param name="query">Delegate that adds the query to the supplied builder</param>
        /// <returns>Builder for creating the sub query expression</returns>
        IChainedBuilder<TEntity, IStatementConditionExpressionBuilder<TEntity>> ExistsIn(Action<StringBuilder, ExpressionCompileOptions> query) => Expression(NullExpression.Value).CompareTo(new EnumExpression<Operators>(Operators.Exists)).Query(query.ValidateArgument(nameof(query)));
        /// <summary>
        /// Condition is true when any rows are returned by the sub query.
        /// </summary>
        /// <param name="query">The query string</param>
        /// <returns>Builder for creating the sub query expression</returns>
        IChainedBuilder<TEntity, IStatementConditionExpressionBuilder<TEntity>> ExistsIn(string query) => Expression(NullExpression.Value).CompareTo(new EnumExpression<Operators>(Operators.Exists)).Query(query.ValidateArgumentNotNullOrWhitespace(nameof(query)));
        /// <summary>
        /// Condition is true when any rows are returned by the sub query.
        /// </summary>
        /// <param name="builder">Builder for creating the sub query</param>
        /// <returns>Builder for creating the sub query expression</returns>
        IChainedBuilder<TEntity, IStatementConditionExpressionBuilder<TEntity>> ExistsIn(IQueryBuilder builder) => Expression(NullExpression.Value).CompareTo(new EnumExpression<Operators>(Operators.Exists)).Query(builder.ValidateArgument(nameof(builder)));
        #endregion
    }

    /// <summary>
    /// Builder for creating the expressionon the right-hand side of a condition.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to build the query for</typeparam>
    public interface IStatementConditionRightExpressionBuilder<TEntity> : ISharedExpressionBuilder<TEntity, IChainedBuilder<TEntity, IStatementConditionExpressionBuilder<TEntity>>>
    {
        #region List
        /// <summary>
        /// Compares an expression to a list of values.
        /// </summary>
        /// <param name="values">List of values that the expression should be compared to</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedBuilder<TEntity, IStatementConditionExpressionBuilder<TEntity>> Values(IEnumerable<object> values) => Expression(new ListExpression(values.ValidateArgument(nameof(values)).Select(x => new SqlConstantExpression(x))));
        /// <summary>
        /// Compares an expression to a list of values.
        /// </summary>
        /// <param name="values">Any additional values to compare to</param>
        /// <typeparam name="T">The type of the elements</typeparam>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedBuilder<TEntity, IStatementConditionExpressionBuilder<TEntity>> Values<T>(T[] values) => Values(values.ValidateArgument(nameof(values)).Enumerate());
        /// <summary>
        /// Compares an expression to a list of values.
        /// </summary>
        /// <param name="value">The first value in the list of values to compare to</param>
        /// <param name="values">Any additional values to compare to</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedBuilder<TEntity, IStatementConditionExpressionBuilder<TEntity>> Values(object value, params object[] values) => Values(Helper.Collection.Enumerate(value, values));
        /// <summary>
        /// Compares an expression to a list of sql parameters.
        /// </summary>
        /// <param name="parameter">The first parameter in the list of values to compare to</param>
        /// <param name="parameters">Any additional parameters in the list of values to conpare to</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedBuilder<TEntity, IStatementConditionExpressionBuilder<TEntity>> Parameters(string parameter, params string[] parameters) => Parameters(Helper.Collection.Enumerate(parameter, parameters));
        /// <summary>
        /// Compares an expression to a list of sql parameters.
        /// </summary>
        /// <param name="parameters">The list of sql parameters to compare to</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedBuilder<TEntity, IStatementConditionExpressionBuilder<TEntity>> Parameters(IEnumerable<string> parameters) => Values(parameters.ValidateArgument(nameof(parameters)).Select(x => new SqlParameterExpression(x)));
        #endregion
    }

    /// <summary>
    /// Builder for configuring how to compare 2 sql expressions.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to build the query for</typeparam>
    public interface IStatementConditionOperatorExpressionBuilder<TEntity> : IComparisonExpressionBuilder<TEntity, IStatementConditionRightExpressionBuilder<TEntity>>
    {       
        #region Operators
        /// <summary>
        /// First expression should be greater than <paramref name="lower"/> and lesser than <paramref name="top"/>.
        /// </summary>
        /// <param name="lower">The lowest value in the range</param>
        /// <param name="top">The highest value in the range</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedBuilder<TEntity, IStatementConditionExpressionBuilder<TEntity>> Between(object lower, object top) => CompareTo(new EnumExpression<Operators>(Operators.Between)).Expression(new BetweenValuesExpression(new SqlConstantExpression(lower.ValidateArgument(nameof(lower))), new SqlConstantExpression(top.ValidateArgument(nameof(top)))));
        /// <summary>
        /// First expression should be not greater than <paramref name="lower"/> and not lesser than <paramref name="top"/>.
        /// </summary>
        /// <param name="lower">The lowest value in the range</param>
        /// <param name="top">The highest value in the range</param>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedBuilder<TEntity, IStatementConditionExpressionBuilder<TEntity>> NotBetween(object lower, object top) => CompareTo(new EnumExpression<Operators>(Operators.NotBetween)).Expression(new BetweenValuesExpression(new SqlConstantExpression(lower.ValidateArgument(nameof(lower))), new SqlConstantExpression(top.ValidateArgument(nameof(top)))));
        /// <summary>
        /// Checks if first expression is equal to NULL.
        /// </summary>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedBuilder<TEntity, IStatementConditionExpressionBuilder<TEntity>> IsNull => CompareTo(new EnumExpression<Operators>(Operators.Is)).Value(DBNull.Value);
        /// <summary>
        /// Checks if first expression is not equal to NULL.
        /// </summary>
        /// <returns>Current builder for creating more conditions</returns>
        IChainedBuilder<TEntity, IStatementConditionExpressionBuilder<TEntity>> IsNotNull => CompareTo(new EnumExpression<Operators>(Operators.IsNot)).Value(DBNull.Value);
        #endregion       
    }
}
