using Microsoft.Extensions.Logging;
using Sels.SQL.QueryBuilder.Builder;
using Sels.SQL.QueryBuilder.Builder.Expressions;
using Sels.SQL.QueryBuilder.Builder.Statement;
using Sels.SQL.QueryBuilder.MySQL.Expressions;
using Sels.SQL.QueryBuilder.Statements;
using System.Collections;
using System.Reflection;
using System.Text;

namespace Sels.SQL.QueryBuilder.MySQL
{
    /// <summary>
    /// Contains static helper methods for building queries and constant values related to MySql.
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
        public static IInsertStatementBuilder<T> Insert<T>(ILogger logger = null)
        {
            return new InsertStatementBuilder<T>(new MySqlCompiler(logger));
        }

        /// <summary>
        /// Returns a builder for creating a mysql insert query.
        /// </summary>
        /// <param name="logger">Optional logger for tracing</param>
        /// <returns>A builder for creating a mysql query</returns>
        public static IInsertStatementBuilder<object> Insert(ILogger logger = null) => Insert<object>(logger);

        /// <summary>
        /// Returns a builder for creating a mysql select query.
        /// </summary>
        /// <typeparam name="T">The main entity to query</typeparam>
        /// <param name="logger">Optional logger for tracing</param>
        /// <returns>A builder for creating a mysql query</returns>
        public static ISelectStatementBuilder<T> Select<T>(ILogger logger = null)
        {
            return new SelectStatementBuilder<T>(new MySqlCompiler(logger));
        }

        /// <summary>
        /// Returns a builder for creating a mysql select query.
        /// </summary>
        /// <param name="logger">Optional logger for tracing</param>
        /// <returns>A builder for creating a mysql query</returns>
        public static ISelectStatementBuilder<object> Select(ILogger logger = null) => Select<object>(logger);

        /// <summary>
        /// Returns a builder for creating a query using common table expressions.
        /// </summary>
        /// <param name="logger">Optional logger for tracing</param>
        /// <returns>A builder for creating a mysql query</returns>
        public static ICteStatementBuilder With(ILogger logger = null)
        {
            return new CteStatementBuilder(new MySqlCompiler(logger));
        }

        /// <summary>
        /// Returns a builder for creating a mysql update query.
        /// </summary>
        /// <typeparam name="T">The main entity to query</typeparam>
        /// <param name="logger">Optional logger for tracing</param>
        /// <returns>A builder for creating a mysql query</returns>
        public static IUpdateStatementBuilder<T> Update<T>(ILogger logger = null)
        {
            return new UpdateStatementBuilder<T>(new MySqlCompiler(logger));
        }

        /// <summary>
        /// Returns a builder for creating a mysql update query.
        /// </summary>
        /// <param name="logger">Optional logger for tracing</param>
        /// <returns>A builder for creating a mysql query</returns>
        public static IUpdateStatementBuilder<object> Update(ILogger logger = null) => Update<object>(logger);

        /// <summary>
        /// Returns a builder for creating a mysql delete query.
        /// </summary>
        /// <typeparam name="T">The main entity to query</typeparam>
        /// <param name="logger">Optional logger for tracing</param>
        /// <returns>A builder for creating a mysql query</returns>
        public static IDeleteStatementBuilder<T> Delete<T>(ILogger logger = null)
        {
            return new DeleteStatementBuilder<T>(new MySqlCompiler(logger));
        }

        /// <summary>
        /// Returns a builder for creating a mysql delete query.
        /// </summary>
        /// <param name="logger">Optional logger for tracing</param>
        /// <returns>A builder for creating a mysql query</returns>
        public static IDeleteStatementBuilder<object> Delete(ILogger logger = null) => Delete<object>(logger);

        /// <summary>
        /// Returns a builder for creating queries consisting of multiple statements and/or expressions.
        /// </summary>
        /// <param name="logger">Optional logger for tracing</param>
        /// <returns>A builder for creating a multi statement SQL query</returns>
        public static IMultiStatementBuilder New(ILogger logger = null)
        {
            var compiler = new MySqlCompiler(logger);
            return new MultiStatementBuilder(compiler);
        }

        /// <summary>
        /// Returns a builder for creating an IF ELSE statement.
        /// </summary>
        /// <param name="logger">Optional logger for tracing</param>
        /// <returns>A builder for creating a SQL condition using IF ELSE statements</returns>
        public static IIfConditionStatementBuilder If(ILogger logger = null)
        {
            var compiler = new MySqlCompiler(logger);
            return new IfStatementBuilder(compiler, compiler);
        }

        /// <summary>
        /// Returns a builder for creating a SET statement to assign a value to a variable.
        /// </summary>
        /// <param name="logger">Optional logger for tracing</param>
        /// <returns>A builder for creating a variable set statement</returns>
        public static IVariableSetterRootStatementBuilder Set(ILogger logger = null)
        {
            var compiler = new MySqlCompiler(logger);
            return new VariableSetterStatementBuilder(compiler);
        }

        /// <summary>
        /// Compiles <paramref name="expression"/> into MySql.
        /// </summary>
        /// <param name="expression">The expression to compile</param>
        /// <param name="logger">Optional logger for tracing</param>
        /// <returns><paramref name="expression"/> compiled into MySql</returns>
        /// <param name="options">Optional settings for building the query</param>
        public static string Compile(IExpression expression, ExpressionCompileOptions options = ExpressionCompileOptions.None, ILogger logger = null)
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
        public static StringBuilder Compile(StringBuilder builder, IExpression expression, ExpressionCompileOptions options = ExpressionCompileOptions.None, ILogger logger = null)
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
            /// <summary>
            /// The keyword placed between the condition and the statements to execute for <see cref="Sql.Statements.If"/> and <see cref="Sql.Statements.ElseIf"/>
            /// </summary>
            public const string Then = "THEN";
            /// <summary>
            /// THe keyword used to close a <see cref="Sql.Statements.If"/> statement.
            /// </summary>
            public const string EndIf = "END IF";
            /// <summary>
            /// The operator used to assign variables inline in MySql.
            /// </summary>
            public const string VariableAssignmentOperator = ":=";
        }
        /// <summary>
        /// Contains static values related to MySQL/MariaDB specific functions.
        /// </summary>
        public static class Functions
        {
            /// <summary>
            /// Function that returns the current database date.
            /// </summary>
            public const string Now = "NOW";
            /// <summary>
            /// Function that returns the current UTC date.
            /// </summary>
            public const string UtcNow = "UTC_TIMESTAMP";
            /// <summary>
            /// Function that modifies a date.
            /// </summary>
            public const string DateAdd = "DATE_ADD";
            /// <summary>
            /// Function that returns the last inserted id.
            /// </summary>
            public const string LastInsertId = "LAST_INSERT_ID";
            /// <summary>
            /// Function that returns how many rows were inserted, updated or deleted by the previous statement.
            /// </summary>
            public const string RowCount = "ROW_COUNT";
            /// <summary>
            /// Function that places an user level lock on an identifier.
            /// </summary>
            public const string GetLock = "GET_LOCK";
            /// <summary>
            /// Function that releases an user level lock from an identifier.
            /// </summary>
            public const string ReleaseLock = "RELEASE_LOCK";
        }

        /// <summary>
        /// Contains static values related to MySql specific statements.
        /// </summary>
        public static class Statements 
        {
            /// <summary>
            /// The MySql statement for raising errors.
            /// </summary>
            public static class Signal
            {
                /// <summary>
                /// The name of the signal statement.
                /// </summary>
                public const string Name = "SIGNAL";
                /// <summary>
                /// The expression for raising a custom sql state in an error.
                /// </summary>
                public const string SqlState = "SQLSTATE";

                /// <summary>
                /// The value for <see cref="SqlState"/> to return for a generic error.
                /// </summary>
                public const string UnhandledUserDefinedExceptionState = "45000";
            }
        }
    }
}
