using Sels.SQL.QueryBuilder.Builder;
using Sels.SQL.QueryBuilder.Builder.Expressions;
using Sels.SQL.QueryBuilder.Builder.Statement;
using Sels.SQL.QueryBuilder.MySQL.Statements;
using System.Collections;
using System.Reflection;
using System.Text;

namespace Sels.SQL.QueryBuilder.MySQL
{
    /// <summary>
    /// Contains static helper methods and constant values for MySql.
    /// </summary>
    public static class MySql
    {
        #region Query Builder
        /// <summary>
        /// Returns a builder for creating a mysql insert query.
        /// </summary>
        /// <typeparam name="T">The main entity to query</typeparam>
        /// <param name="logger">Optional logger for tracing</param>
        /// <returns>A builder for creating a mysql query</returns>
        public static IInsertStatementBuilder<T> Insert<T>(ILogger? logger = null)
        {
            return new InsertStatementBuilder<T>(new MySqlCompiler(logger));
        }

        /// <summary>
        /// Returns a builder for creating a mysql insert query.
        /// </summary>
        /// <param name="logger">Optional logger for tracing</param>
        /// <returns>A builder for creating a mysql query</returns>
        public static IInsertStatementBuilder<object> Insert(ILogger? logger = null) => Insert<object>(logger);

        /// <summary>
        /// Returns a builder for creating a mysql select query.
        /// </summary>
        /// <typeparam name="T">The main entity to query</typeparam>
        /// <param name="logger">Optional logger for tracing</param>
        /// <returns>A builder for creating a mysql query</returns>
        public static ISelectStatementBuilder<T> Select<T>(ILogger? logger = null)
        {
            return new SelectStatementBuilder<T>(new MySqlCompiler(logger));
        }

        /// <summary>
        /// Returns a builder for creating a mysql select query.
        /// </summary>
        /// <param name="logger">Optional logger for tracing</param>
        /// <returns>A builder for creating a mysql query</returns>
        public static ISelectStatementBuilder<object> Select(ILogger? logger = null) => Select<object>(logger);

        /// <summary>
        /// Returns a builder for creating a select query using common table expressions.
        /// </summary>
        /// <param name="logger">Optional logger for tracing</param>
        /// <returns>A builder for creating a mysql query</returns>
        public static ICteStatementBuilder With(ILogger? logger = null)
        {
            return new CteStatementBuilder(new MySqlCompiler(logger));
        }

        /// <summary>
        /// Returns a builder for creating a mysql update query.
        /// </summary>
        /// <typeparam name="T">The main entity to query</typeparam>
        /// <param name="logger">Optional logger for tracing</param>
        /// <returns>A builder for creating a mysql query</returns>
        public static IUpdateStatementBuilder<T> Update<T>(ILogger? logger = null)
        {
            return new UpdateStatementBuilder<T>(new MySqlCompiler(logger));
        }

        /// <summary>
        /// Returns a builder for creating a mysql update query.
        /// </summary>
        /// <param name="logger">Optional logger for tracing</param>
        /// <returns>A builder for creating a mysql query</returns>
        public static IUpdateStatementBuilder<object> Update(ILogger? logger = null) => Update<object>(logger);

        /// <summary>
        /// Returns a builder for creating a mysql delete query.
        /// </summary>
        /// <typeparam name="T">The main entity to query</typeparam>
        /// <param name="logger">Optional logger for tracing</param>
        /// <returns>A builder for creating a mysql query</returns>
        public static IDeleteStatementBuilder<T> Delete<T>(ILogger? logger = null)
        {
            return new DeleteStatementBuilder<T>(new MySqlCompiler(logger));
        }

        /// <summary>
        /// Returns a builder for creating a mysql delete query.
        /// </summary>
        /// <param name="logger">Optional logger for tracing</param>
        /// <returns>A builder for creating a mysql query</returns>
        public static IDeleteStatementBuilder<object> Delete(ILogger? logger = null) => Delete<object>(logger);

        /// <summary>
        /// Returns a builder for creating queries consisting of multiple statements and/or expressions.
        /// </summary>
        /// <param name="logger">Optional logger for tracing</param>
        /// <returns></returns>
        public static IMultiStatementBuilder Build(ILogger? logger = null)
        {
            var compiler = new MySqlCompiler(logger);
            return new MySqlMultiStatementBuilder(compiler);
        }

        /// <summary>
        /// Compiles <paramref name="expression"/> into MySql.
        /// </summary>
        /// <param name="expression">The expression to compile</param>
        /// <param name="logger">Optional logger for tracing</param>
        /// <returns><paramref name="expression"/> compiled into MySql</returns>
        /// <param name="options">Optional settings for building the query</param>
        public static string Compile(IExpression expression, ExpressionCompileOptions options = ExpressionCompileOptions.None, ILogger? logger = null)
        {
            return new MySqlCompiler(logger).Compile(expression, null, options);
        }
        /// <summary>
        /// Compiles <paramref name="expression"/> into MySql and adds it to <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The builder to add the MySql string to</param>
        /// <param name="expression">The expression to compile</param>
        /// <param name="logger">Optional logger for tracing</param>
        /// <param name="options">Optional settings for building the query</param>
        /// <returns><paramref name="builder"/> for method chaining</returns>
        public static StringBuilder Compile(StringBuilder builder, IExpression expression, ExpressionCompileOptions options = ExpressionCompileOptions.None, ILogger? logger = null)
        {
            return new MySqlCompiler(logger).Compile(builder, expression, null, options);
        }
        #endregion

        /// <summary>
        /// Contains static values related to the schema of MySql databases.
        /// </summary>
        public static class Schema
        {
            /// <summary>
            /// Contains static values related to MySql indexes.
            /// </summary>
            public static class Indexes
            {
                /// <summary>
                /// The MySql name for the primary key index.
                /// </summary>
                public const string PrimaryKeyName = "PRIMARY";
            }
        }
        /// <summary>
        /// Contains static values related to MySql specific keywords.
        /// </summary>
        public static class Keywords
        {
            /// <summary>
            /// The keyword for a recursive cte.
            /// </summary>
            public const string Recursive = "RECURSIVE";
        }
    }
}
