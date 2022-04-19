using Microsoft.Extensions.Logging;
using Sels.Core.Data.MySQL.Query.Compiling;
using Sels.Core.Data.SQL.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Data.MySQL
{
    /// <summary>
    /// Contains static helper methods and constant values for mysql.
    /// </summary>
    public static class MySql
    {
        /// <summary>
        /// Returns a builder for creating a mysql select query.
        /// </summary>
        /// <typeparam name="T">The main entity to query</typeparam>
        /// <param name="loggers">Optional loggers for tracing</param>
        /// <returns>A builder for creating a mysql query</returns>
        public static ISelectQueryBuilder<T> Select<T>(IEnumerable<ILogger>? loggers = null)
        {
            return new SelectQueryBuilder<T>(new MySqlCompiler(loggers));
        }

        /// <summary>
        /// Returns a builder for creating a mysql select query.
        /// </summary>
        /// <param name="loggers">Optional loggers for tracing</param>
        /// <returns>A builder for creating a mysql query</returns>
        public static ISelectQueryBuilder<object> Select(IEnumerable<ILogger>? loggers = null) => Select<object>(loggers);

        /// <summary>
        /// Returns a builder for creating a mysql delete query.
        /// </summary>
        /// <typeparam name="T">The main entity to query</typeparam>
        /// <param name="loggers">Optional loggers for tracing</param>
        /// <returns>A builder for creating a mysql query</returns>
        public static IDeleteQueryBuilder<T> Delete<T>(IEnumerable<ILogger>? loggers = null)
        {
            return new DeleteQueryBuilder<T>(new MySqlCompiler(loggers));
        }

        /// <summary>
        /// Returns a builder for creating a mysql delete query.
        /// </summary>
        /// <param name="loggers">Optional loggers for tracing</param>
        /// <returns>A builder for creating a mysql query</returns>
        public static IDeleteQueryBuilder<object> Delete(IEnumerable<ILogger>? loggers = null) => Delete<object>(loggers);

        /// <summary>
        /// Returns a builder for creating a mysql insert query.
        /// </summary>
        /// <typeparam name="T">The main entity to query</typeparam>
        /// <param name="loggers">Optional loggers for tracing</param>
        /// <returns>A builder for creating a mysql query</returns>
        public static IInsertQueryBuilder<T> Insert<T>(IEnumerable<ILogger>? loggers = null)
        {
            return new InsertQueryBuilder<T>(new MySqlCompiler(loggers));
        }

        /// <summary>
        /// Returns a builder for creating a mysql insert query.
        /// </summary>
        /// <param name="loggers">Optional loggers for tracing</param>
        /// <returns>A builder for creating a mysql query</returns>
        public static IInsertQueryBuilder<object> Insert(IEnumerable<ILogger>? loggers = null) => Insert<object>(loggers);
    }
}
