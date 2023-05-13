using Sels.SQL.QueryBuilder.Builder;
using Sels.SQL.QueryBuilder.Builder.Expressions;
using Sels.SQL.QueryBuilder.Builder.Statement;
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
        /// <param name="loggers">Optional loggers for tracing</param>
        /// <returns>A builder for creating a mysql query</returns>
        public static IInsertStatementBuilder<T> Insert<T>(IEnumerable<ILogger>? loggers = null)
        {
            return new InsertStatementBuilder<T>(new MySqlCompiler(loggers));
        }

        /// <summary>
        /// Returns a builder for creating a mysql insert query.
        /// </summary>
        /// <param name="loggers">Optional loggers for tracing</param>
        /// <returns>A builder for creating a mysql query</returns>
        public static IInsertStatementBuilder<object> Insert(IEnumerable<ILogger>? loggers = null) => Insert<object>(loggers);

        /// <summary>
        /// Returns a builder for creating a mysql select query.
        /// </summary>
        /// <typeparam name="T">The main entity to query</typeparam>
        /// <param name="loggers">Optional loggers for tracing</param>
        /// <returns>A builder for creating a mysql query</returns>
        public static ISelectStatementBuilder<T> Select<T>(IEnumerable<ILogger>? loggers = null)
        {
            return new SelectStatementBuilder<T>(new MySqlCompiler(loggers));
        }

        /// <summary>
        /// Returns a builder for creating a mysql select query.
        /// </summary>
        /// <param name="loggers">Optional loggers for tracing</param>
        /// <returns>A builder for creating a mysql query</returns>
        public static ISelectStatementBuilder<object> Select(IEnumerable<ILogger>? loggers = null) => Select<object>(loggers);

        /// <summary>
        /// Returns a builder for creating a select query using common table expressions.
        /// </summary>
        /// <param name="loggers">Optional loggers for tracing</param>
        /// <returns>A builder for creating a mysql query</returns>
        public static ICteStatementBuilder With(IEnumerable<ILogger>? loggers = null)
        {
            return new CteStatementBuilder(new MySqlCompiler(loggers));
        }

        /// <summary>
        /// Returns a builder for creating a mysql update query.
        /// </summary>
        /// <typeparam name="T">The main entity to query</typeparam>
        /// <param name="loggers">Optional loggers for tracing</param>
        /// <returns>A builder for creating a mysql query</returns>
        public static IUpdateStatementBuilder<T> Update<T>(IEnumerable<ILogger>? loggers = null)
        {
            return new UpdateStatementBuilder<T>(new MySqlCompiler(loggers));
        }

        /// <summary>
        /// Returns a builder for creating a mysql update query.
        /// </summary>
        /// <param name="loggers">Optional loggers for tracing</param>
        /// <returns>A builder for creating a mysql query</returns>
        public static IUpdateStatementBuilder<object> Update(IEnumerable<ILogger>? loggers = null) => Update<object>(loggers);

        /// <summary>
        /// Returns a builder for creating a mysql delete query.
        /// </summary>
        /// <typeparam name="T">The main entity to query</typeparam>
        /// <param name="loggers">Optional loggers for tracing</param>
        /// <returns>A builder for creating a mysql query</returns>
        public static IDeleteStatementBuilder<T> Delete<T>(IEnumerable<ILogger>? loggers = null)
        {
            return new DeleteStatementBuilder<T>(new MySqlCompiler(loggers));
        }

        /// <summary>
        /// Returns a builder for creating a mysql delete query.
        /// </summary>
        /// <param name="loggers">Optional loggers for tracing</param>
        /// <returns>A builder for creating a mysql query</returns>
        public static IDeleteStatementBuilder<object> Delete(IEnumerable<ILogger>? loggers = null) => Delete<object>(loggers);

        /// <summary>
        /// Compiles <paramref name="expression"/> into MySql.
        /// </summary>
        /// <param name="expression">The expression to compile</param>
        /// <param name="loggers">Optional loggers for tracing</param>
        /// <returns><paramref name="expression"/> compiled into MySql</returns>
        public static string Compile(IExpression expression, IEnumerable<ILogger>? loggers = null)
        {
            return new MySqlCompiler(loggers).Compile(expression);
        }
        /// <summary>
        /// Compiles <paramref name="expression"/> into MySql and adds it to <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The builder to add the MySql string to</param>
        /// <param name="expression">The expression to compile</param>
        /// <param name="loggers">Optional loggers for tracing</param>
        /// <returns><paramref name="builder"/> for method chaining</returns>
        public static StringBuilder Compile(StringBuilder builder, IExpression expression, IEnumerable<ILogger>? loggers = null)
        {
            return new MySqlCompiler(loggers).Compile(builder, expression);
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
