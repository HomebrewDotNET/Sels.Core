using Sels.Core.Data.SQL.Query.Expressions;
using System.Linq.Expressions;
using System.Text;

namespace Sels.Core.Data.SQL.Query.Statement
{
    /// <summary>
    /// Builder for building one or more common table expressions that can be used to execute a select query.
    /// </summary>
    public interface ICteStatementBuilder
    {
        /// <summary>
        /// Adds the cte defined in <paramref name="expression"/> to the current builder.
        /// </summary>
        /// <param name="expression">The expression containing the cte</param>
        /// <returns>Current builder for defining more cte's or selecting the query to execute with the cte's</returns>
        ICteOrSelectStatementBuilder Expression(IExpression expression);
        /// <summary>
        /// Adds the cte defined in <paramref name="expression"/> to the current builder.
        /// </summary>
        /// <param name="expression">Raw sql string containing the cte</param>
        /// <returns>Current builder for defining more cte's or selecting the query to execute with the cte's</returns>
        ICteOrSelectStatementBuilder Expression(string expression) => Expression(new RawExpression(expression.ValidateArgumentNotNullOrWhitespace(nameof(expression))));
        /// <summary>
        /// Adds the cte built by <paramref name="expressionBuilder"/> to the current builder.
        /// </summary>
        /// <param name="expressionBuilder">Delegate that adds the cte to the builder</param>
        /// <returns>Current builder for defining more cte's or selecting the query to execute with the cte's</returns>
        ICteOrSelectStatementBuilder Expression(Action<StringBuilder> expressionBuilder) => Expression(new DelegateExpression(expressionBuilder.ValidateArgument(nameof(expressionBuilder))));

        #region Cte
        /// <summary>
        /// Starts to build a cte expression for the current statement.
        /// </summary>
        /// <typeparam name="T">The main entity to map to the cte columns</typeparam>
        /// <param name="name">The name of the cte</param>
        /// <returns>Builder for creating the cte expression</returns>
        ICteExpressionBuilder<T> Cte<T>(string name);
        /// <summary>
        /// Starts to build a cte expression for the current statement.
        /// </summary>
        /// <typeparam name="T">The main entity to map to the cte columns</typeparam>
        /// <returns>Builder for creating the cte expression</returns>
        ICteExpressionBuilder<T> Cte<T>() => Cte<T>(typeof(T).Name);
        /// <summary>
        /// Starts to build a cte expression for the current statement.
        /// </summary>
        /// <param name="name">The name of the cte</param>
        /// <returns>Builder for creating the cte expression</returns>
        ICteExpressionBuilder<object> Cte(string name) => Cte<object>(name);
        #endregion
    }
    /// <summary>
    ///  Builder for building one or more common table expressions that can be used to execute a select query.
    /// </summary>
    public interface ICteOrSelectStatementBuilder : ICteStatementBuilder
    {
        /// <summary>
        /// Defines the select query to execute with the common table expressions.
        /// </summary>
        /// <param name="query">Delegate that returns the sub query</param>
        /// <returns>Query builder for converting the current builder into a query</returns>
        IQueryBuilder Execute(Func<ExpressionCompileOptions, string> query);
        /// <summary>
        /// Defines the select query to execute with the common table expressions.
        /// </summary>
        /// <param name="query">The sub query</param>
        /// <returns>Query builder for converting the current builder into a query</returns>
        IQueryBuilder Execute(string query) => Execute(x => query);
        /// <summary>
        /// Defines the select query to execute with the common table expressions.
        /// </summary>
        /// <param name="builder">Builder that creates the sub query</param>
        /// <returns>Query builder for converting the current builder into a query</returns>
        IQueryBuilder Execute(IQueryBuilder builder) => Execute(x => builder.ValidateArgument(nameof(builder)).Build(x));
    }
    /// <summary>
    /// Builder for creating a cte expression.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to map to the cte columns</typeparam>
    public interface ICteExpressionBuilder<TEntity>
    {
        #region Columns
        /// <summary>
        /// Defines a column for the current cte.
        /// </summary>
        /// <param name="column">The column to create the condition for</param>
        /// <returns>Current builder for method chaining</returns>
        ICteExpressionBuilder<TEntity> Column(string column);
        /// <summary>
        ///Defines a column for the current cte where the column name is taken from the property name selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to select the property from</typeparam>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Current builder for method chaining</returns>
        ICteExpressionBuilder<TEntity> Column<T>(Expression<Func<T, object?>> property) => Column(property.ValidateArgument(nameof(property)).ExtractProperty(nameof(property)).Name);
        /// <summary>
        /// Defines a column for the current cte where the column name is taken from the property name selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Current builder for method chaining</returns>
        ICteExpressionBuilder<TEntity> Column(Expression<Func<TEntity, object?>> property) => Column<TEntity>(property);
        #endregion

        #region Using
        /// <summary>
        /// Defines the query for the cte.
        /// </summary>
        /// <param name="query">Delegate that returns the sub query</param>
        /// <returns>Current builder for defining more cte's or selecting the query to execute with the cte's</returns>
        ICteOrSelectStatementBuilder Using(Func<ExpressionCompileOptions, string> query);
        /// <summary>
        /// Defines the query for the cte.
        /// </summary>
        /// <param name="query">The sub query</param>
        /// <returns>Current builder for defining more cte's or selecting the query to execute with the cte's</returns>
        ICteOrSelectStatementBuilder Using(string query) => Using(x => query);
        /// <summary>
        /// Defines the query for the cte.
        /// </summary>
        /// <param name="builder">Builder that creates the sub query</param>
        /// <returns>Current builder for defining more cte's or selecting the query to execute with the cte's</returns>
        ICteOrSelectStatementBuilder Using(IQueryBuilder builder) => Using(x => builder.ValidateArgument(nameof(builder)).Build(x));
        #endregion
    }
}
