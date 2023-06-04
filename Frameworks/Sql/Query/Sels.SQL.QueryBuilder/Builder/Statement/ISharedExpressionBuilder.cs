using Sels.SQL.QueryBuilder.Builder.Expressions;
using Sels.SQL.QueryBuilder.Expressions;
using System;
using System.Linq.Expressions;
using System.Text;
using SqlConstantExpression = Sels.SQL.QueryBuilder.Builder.Expressions.ConstantExpression;
using SqlParameterExpression = Sels.SQL.QueryBuilder.Builder.Expressions.ParameterExpression;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Reflection;

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
        /// <param name="dataset">Overwrites the default dataset name defined for type <typeparamref name="T"/>. If a type is used the alias defined for the type is taken. Set to an empty string to omit the dataset alias</param>
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
        /// <param name="dataset">Overwrites the default dataset name defined for type <typeparamref name="TEntity"/>. If a type is used the alias defined for the type is taken. Set to an empty string to omit the dataset alias</param>
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
    }

    /// <summary>
    /// Builder that adds comparison operators.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to build the query for</typeparam>
    /// <typeparam name="TReturn">The type to return for the fluent syntax</typeparam>
    public interface IComparisonExpressionBuilder<TEntity, out TReturn>
    {
        #region Expression
        /// <summary>
        /// Compares 2 expressions using the operator defined in <paramref name="sqlExpression"/>.
        /// </summary>
        /// <param name="sqlExpression">The sql expression containing the operator</param>
        /// <returns>Builder for selecting what to compare to the first expression</returns>
        TReturn CompareTo(IExpression sqlExpression);
        /// <summary>
        /// Compares 2 expressions using the operator defined in <paramref name="sqlExpression"/>.
        /// </summary>
        /// <param name="sqlExpression">String containing the sql operator</param>
        /// <returns>Builder for selecting what to compare to the first expression</returns>
        TReturn CompareTo(string sqlExpression) => CompareTo(new RawExpression(sqlExpression.ValidateArgumentNotNullOrEmpty(nameof(sqlExpression))));
        /// <summary>
        /// Compares 2 expressions using the operator defined in <paramref name="sqlExpression"/>.
        /// </summary>
        /// <param name="sqlExpression">Delegate that adds the sql operator to compare to the provided string builder</param>
        /// <returns>Builder for selecting what to compare to the first expression</returns>
        TReturn CompareTo(Action<StringBuilder, ExpressionCompileOptions> sqlExpression) => CompareTo(new DelegateExpression(sqlExpression.ValidateArgument(nameof(sqlExpression))));
        #endregion

        #region Operators
        /// <summary>
        /// The expressions should be equal.
        /// </summary>
        /// <returns>Builder for selecting what to compare to the first expression</returns>
        TReturn EqualTo => CompareTo(new EnumExpression<Operators>(Operators.Equal));
        /// <summary>
        /// The expressions should not be equal.
        /// </summary>
        /// <returns>Builder for selecting what to compare to the first expression</returns>
        TReturn NotEqualTo => CompareTo(new EnumExpression<Operators>(Operators.NotEqual));
        /// <summary>
        /// First expression should be greater than second expression.
        /// </summary>
        /// <returns>Builder for selecting what to compare to the first expression</returns>
        TReturn GreaterThan => CompareTo(new EnumExpression<Operators>(Operators.Greater));
        /// <summary>
        /// First expression should be lesser than second expression.
        /// </summary>
        /// <returns>Builder for selecting what to compare to the first expression</returns>
        TReturn LesserThan => CompareTo(new EnumExpression<Operators>(Operators.Less));
        /// <summary>
        /// First expression should be greater or equal to second expression.
        /// </summary>
        /// <returns>Builder for selecting what to compare to the first expression</returns>
        TReturn GreaterOrEqualTo => CompareTo(new EnumExpression<Operators>(Operators.GreaterOrEqual));
        /// <summary>
        /// First expression should be lesser or equal to second expression.
        /// </summary>
        /// <returns>Builder for selecting what to compare to the first expression</returns>
        TReturn LesserOrEqualTo => CompareTo(new EnumExpression<Operators>(Operators.LessOrEqual));
        /// <summary>
        /// First expression should be like the pattern defined in the second expression.
        /// </summary>
        /// <returns>Builder for selecting what to compare to the first expression</returns>
        TReturn Like => CompareTo(new EnumExpression<Operators>(Operators.Like));
        /// <summary>
        /// First expression should not be like the pattern defined in the second expression.
        /// </summary>
        /// <returns>Builder for selecting what to compare to the first expression</returns>
        TReturn NotLike => CompareTo(new EnumExpression<Operators>(Operators.NotLike));
        /// <summary>
        /// First expression should exist in a list of values defined by the second expression.
        /// </summary>
        /// <returns>Builder for selecting what to compare to the first expression</returns>
        TReturn In => CompareTo(new EnumExpression<Operators>(Operators.In));
        /// <summary>
        /// First expression should not exist in a list of values defined by the second expression.
        /// </summary>
        /// <returns>Builder for selecting what to compare to the first expression</returns>
        TReturn NotIn => CompareTo(new EnumExpression<Operators>(Operators.NotIn));
        #endregion       
    }
    /// <summary>
    /// Builder for chaining other builder using and/or.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to build the query for</typeparam>
    /// <typeparam name="TReturn">The type to return for the fluent syntax</typeparam>
    public interface IChainedBuilder<TEntity, out TReturn>
    {
        /// <summary>
        /// Sets the logic operator on how to join the current condition and the condition created after calling this method.
        /// </summary>
        /// <param name="logicOperator">The logic operator to use</param>
        /// <returns>Current builder for method chaining</returns>
        TReturn AndOr(LogicOperators logicOperator = LogicOperators.And);
        /// <summary>
        /// Current condition and the condition created after calling this method either need to result in true.
        /// </summary>
        /// <returns>Current builder for method chaining</returns>
        TReturn Or => AndOr(LogicOperators.Or);
        /// <summary>
        /// Current condition and the condition created after calling this method both need to result in true.
        /// </summary>
        /// <returns>Current builder for method chaining</returns>
        TReturn And => AndOr(LogicOperators.And);
    }
}
