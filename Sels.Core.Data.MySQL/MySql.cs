using Dapper;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using Sels.Core.Components.Scope;
using Sels.Core.Data.Contracts.Repository;
using Sels.Core.Data.MySQL.Models;
using Sels.Core.Data.MySQL.Models.Repository;
using Sels.Core.Data.MySQL.Query.Compiling;
using Sels.Core.Data.SQL.Extensions.Dapper;
using Sels.Core.Data.SQL.Query;
using Sels.Core.Data.SQL.Query.Expressions;
using Sels.Core.Data.SQL.Query.Statement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Data.MySQL
{
    /// <summary>
    /// Contains static helper methods and constant values for MySql.
    /// </summary>
    public static class MySql
    {
        #region Database
        /// <summary>
        /// Contains static helper methods for working with a mysql based database
        /// </summary>
        public static class Database
        {
            /// <summary>
            /// Creates the database defined in <paramref name="connectionString"/> if it does not exist.
            /// </summary>
            /// <param name="connectionString">The connection string to make the connection with</param>
            /// <param name="cancellationToken">Token to cancel the request</param>
            /// <exception cref="ArgumentException"></exception>
            public async static Task EnsureDatabaseExistsAsync(string connectionString, CancellationToken cancellationToken = default)
            {
                var typedConnectionString = ConnectionString.Parse(connectionString);
                if (!typedConnectionString.Database.HasValue()) throw new ArgumentException($"{nameof(typedConnectionString.Database)} is not set in the connection string");
                var database = typedConnectionString.Database;
                // Remove database from connection string to avoid unknown database
                typedConnectionString.Database = null;

                using (var connection = await OpenConnectionAsync(typedConnectionString.ToString(), cancellationToken))
                {
                    _ = await connection.Connection.ExecuteAsync(new CommandDefinition($"CREATE DATABASE IF NOT EXISTS `{database}`", transaction: connection.Transaction, cancellationToken: cancellationToken)).ConfigureAwait(false);
                    await connection.CommitAsync(cancellationToken);
                }
            }

            /// <summary>
            /// Contains static helper methods for gaining exclusive locks on a database.
            /// </summary>
            public static class Locking
            {
                /// <summary>
                /// Creates an exclusive lock on database resource with name <paramref name="key"/>. Provides concurrency between all instances using this method to lock the target database.
                /// Uses a table called <see cref="DatabaseLock"/> and sql locking to handle the locks.
                /// </summary>
                /// <param name="connectionString">Connection string to connect to the database</param>
                /// <param name="key">The name of the resource to lock</param>
                /// <param name="requester">The name of the instance that is requesting the lock. The name should be unique for the different instances. Instances with the same name can lock and unlock the held resource</param>
                /// <param name="expireAfter">How many minutes to lock the resource for. After the allotted time other instances will be able to lock the resource</param>
                /// <param name="loggers">Optional logger for tracing</param>
                /// <param name="cancellationToken">Optional token to cancel the request</param>
                /// <returns>Object that can be disposed to unlock the resource with name <paramref name="key"/></returns>
                public async static Task<IAsyncDisposable> LockAsync(string connectionString, string key, string requester, int expireAfter = 30, IEnumerable<ILogger>? loggers = null, CancellationToken cancellationToken = default)
                {
                    using (loggers.TraceMethod(typeof(Database), nameof(LockAsync)))
                    {
                        connectionString.ValidateArgumentNotNullOrWhitespace(nameof(connectionString));
                        key.ValidateArgumentNotNullOrWhitespace(nameof(key));
                        requester.ValidateArgumentNotNullOrWhitespace(nameof(requester));
                        expireAfter.ValidateArgumentLargerOrEqual(nameof(expireAfter), 1);

                        var typedConnectionString = ConnectionString.Parse(connectionString);
                        // Force to true as we need variables
                        typedConnectionString.AllowUserVariables = true;
                        connectionString = typedConnectionString.ToString();
                       
                        using (var connection = await OpenConnectionAsync(connectionString, cancellationToken).ConfigureAwait(false))
                        {
                            // Create lock table if it doesn't exist yet
                            await CreateLockTableAsync(connection, loggers, cancellationToken).ConfigureAwait(false);
                            await connection.CommitAsync(cancellationToken).ConfigureAwait(false);
                            await connection.CreateTransactionAsync().ConfigureAwait(false);

                            // Try lock resource
                            using (var logger = loggers.CreateTimedLogger(LogLevel.Information, $"Locking resource <{key}> for <{requester}>", x => $"Locked resource <{key}> for <{requester}> for <{expireAfter}> minutes in <{x.PrintTotalMs()}>"))
                            {
                                var databaseLock = await TryLockAsync(connection, key, requester, expireAfter, loggers, cancellationToken).ConfigureAwait(false);
                                // Keep trying until we have a lock
                                while (!databaseLock.LockedBy.Equals(requester, StringComparison.OrdinalIgnoreCase))
                                {
                                    // Release update lock
                                    await connection.CommitAsync(cancellationToken).ConfigureAwait(false);

                                    logger.Log((t, l) => l.Debug($"Could not lock resource <{key}> for <{requester}> as it is locked by <{databaseLock.LockedBy}> until <{databaseLock.ExpiryDate}>. Waiting ({t.PrintTotalMs()})"));
                                    await Task.Delay(1000).ConfigureAwait(false);

                                    // Begin new transaction
                                    await connection.CreateTransactionAsync(token: cancellationToken).ConfigureAwait(false);
                                    databaseLock = await TryLockAsync(connection, key, requester, expireAfter, loggers, cancellationToken).ConfigureAwait(false);
                                }

                                await connection.CommitAsync(cancellationToken).ConfigureAwait(false);
                            }
                        }

                        return await new AsyncScopedAction(
                            () =>
                            {
                                return Task.CompletedTask;
                            },
                            async () =>
                            {
                                using (var connection = await OpenConnectionAsync(connectionString, cancellationToken))
                                {
                                    // Try release lock
                                    var databaseLock = await TryUnlockAsync(connection, key, requester, loggers, cancellationToken).ConfigureAwait(false);

                                    if (databaseLock.LockedBy != null)
                                    {
                                        loggers.Warning($"Could not unlock resource <{key}> for <{requester}> because it was already locked by <{databaseLock.LockedBy}> at <{databaseLock.LockedAt}>. This is possible if the lock expired");
                                    }
                                    else
                                    {
                                        loggers.Log($"Unlocked resource <{key}> that was locked by <{requester}>");
                                    }
                                    await connection.CommitAsync(cancellationToken).ConfigureAwait(false);
                                }                               
                            }).StartAsync();
                    }
                }

                #region Lock helpers
                private async static Task CreateLockTableAsync(IDataRepositoryConnection connection, IEnumerable<ILogger>? loggers, CancellationToken cancellationToken)
                {
                    using (loggers.TraceMethod(typeof(Database), nameof(CreateLockTableAsync)))
                    {
                        connection.ValidateArgument(nameof(connection));

                        loggers.Log($"Creating table {nameof(DatabaseLock)} if does not exist");
                        const string query = $@"
                            CREATE TABLE IF NOT EXISTS {nameof(DatabaseLock)} (
                                {nameof(DatabaseLock.Name)} VARCHAR(100) PRIMARY KEY,
                                {nameof(DatabaseLock.LockedBy)} VARCHAR(100) NULL,
                                {nameof(DatabaseLock.LockedAt)} DATETIME NOT NULL,
                                {nameof(DatabaseLock.ExpiryDate)} DATETIME NOT NULL
                            )";

                        loggers.Trace($"Creating table {nameof(DatabaseLock)} with query: {Environment.NewLine + query}");
                        _ = await connection.Connection.ExecuteAsync(new CommandDefinition(query, transaction: connection.Transaction, cancellationToken: cancellationToken)).ConfigureAwait(false);

                        loggers.Log($"Table {nameof(DatabaseLock)} available for locking");
                    }
                }

                private static Lazy<string> LockQuery { get; } = new Lazy<string>(() =>
                {
                    const string ExistsVariable = "@Exists";

                    // Build query
                    var queryBuilder = new StringBuilder();
                    // Check if record exists
                    MySql.Select<DatabaseLock>().Expression($"{ExistsVariable} := 1").From().ForUpdate()
                            .Where(x => x.Column(c => c.Name).EqualTo.Parameter(c => c.Name))
                            .Build(queryBuilder, ExpressionCompileOptions.Format | ExpressionCompileOptions.AppendSeparator);
                    queryBuilder.AppendLine($"IF {ExistsVariable} = 1 THEN");
                    // Update record
                    MySql.Update<DatabaseLock>().Table()
                            .SetFrom(excludedProperties: nameof(DatabaseLock.Name))
                            .Where(x =>
                                x.Column(c => c.Name).EqualTo.Parameter(c => c.Name)
                                .And.Column(c => c.LockedBy).IsNull
                                .Or.Column(c => c.LockedBy).EqualTo.Parameter(p => p.LockedBy)
                                .Or.Column(c => c.ExpiryDate).LesserThan.Expression("NOW()")
                            ).Build(queryBuilder, ExpressionCompileOptions.Format | ExpressionCompileOptions.AppendSeparator);
                    queryBuilder.AppendLine($"ELSE");
                    // Insert record
                    MySql.Insert<DatabaseLock>().Into().ParametersFrom()
                            .Build(queryBuilder, ExpressionCompileOptions.Format | ExpressionCompileOptions.AppendSeparator);
                    queryBuilder.AppendLine($"END IF;");
                    // Select record
                    MySql.Select<DatabaseLock>().All().From()
                            .Where(x => x.Column(c => c.Name).EqualTo.Parameter(c => c.Name))
                            .Build(queryBuilder, ExpressionCompileOptions.Format | ExpressionCompileOptions.AppendSeparator);

                    return queryBuilder.ToString();
                }, true);
                private async static Task<DatabaseLock> TryLockAsync(IDataRepositoryConnection connection, string key, string requester, int expireAfter, IEnumerable<ILogger>? loggers, CancellationToken cancellationToken)
                {
                    using (loggers.TraceMethod(typeof(Database), nameof(TryLockAsync)))
                    {
                        connection.ValidateArgument(nameof(connection));
                        key.ValidateArgumentNotNullOrWhitespace(nameof(key));
                        requester.ValidateArgumentNotNullOrWhitespace(nameof(requester));
                        expireAfter.ValidateArgumentLargerOrEqual(nameof(expireAfter), 1);

                        var query = LockQuery.Value;
                        loggers.Log($"Attempting to lock resource <{key}> for <{requester}>");
                        loggers.Trace($"Locking resource <{key}> using query: {Environment.NewLine + query}");

                        var databaseLock = new DatabaseLock()
                        {
                            Name = key,
                            LockedBy = requester,
                            LockedAt = DateTime.Now,
                            ExpiryDate = DateTime.Now.AddMinutes(expireAfter)
                        };

                        try
                        {
                            var parameters = new DynamicParameters().AddParametersUsing(databaseLock);

                            using (var result = await connection.Connection.QueryMultipleAsync(new CommandDefinition(query, parameters, connection.Transaction, cancellationToken: cancellationToken)).ConfigureAwait(false)) {
                                databaseLock = null;
                                // Keep reading until the last result which is the one we want
                                while (!result.IsConsumed)
                                {
                                    databaseLock = await result.ReadSingleOrDefaultAsync<DatabaseLock>().ConfigureAwait(false);
                                }
                                
                                // Get result from last select
                                return databaseLock ?? throw new InvalidOperationException($"Expected to receive database lock");
                            }
                        }
                        catch (InvalidOperationException)
                        {
                            throw;
                        }
                        catch (MySqlException mySqlEx) when (mySqlEx.ErrorCode == MySqlErrorCode.DuplicateKeyEntry)
                        {
                            // Try again when we get duplicate key. Running again should cause the update to trigger instead of the insert. Edge case if 2 or more processes run the query at the same time while the record doesn't exist causing multiple inserts to happen for the key.
                            return await TryLockAsync(connection, key, requester, expireAfter, loggers, cancellationToken);
                        }
                    }
                }

                private static Lazy<string> UnlockQuery { get; } = new Lazy<string>(() =>
                {
                    // Build query
                    var queryBuilder = new StringBuilder();
                    // Update record
                    MySql.Update<DatabaseLock>().Table()
                                        .Set(x => x.LockedBy).To.Null()
                                        .Where(x =>
                                            x.Column(c => c.Name).EqualTo.Parameter(p => p.Name).And
                                            .Column(c => c.LockedBy).EqualTo.Parameter(p => p.LockedBy)
                                        )
                                        .Build(queryBuilder, ExpressionCompileOptions.Format | ExpressionCompileOptions.AppendSeparator);
                    // Select record
                    MySql.Select<DatabaseLock>().All().From()
                                        .Where(x => x.Column(c => c.Name).EqualTo.Parameter(p => p.Name))
                                        .Build(queryBuilder, ExpressionCompileOptions.Format | ExpressionCompileOptions.AppendSeparator);

                    return queryBuilder.ToString();
                }, true);
                private async static Task<DatabaseLock> TryUnlockAsync(IDataRepositoryConnection connection, string key, string requester, IEnumerable<ILogger>? loggers, CancellationToken cancellationToken)
                {
                    using (loggers.TraceMethod(typeof(Database), nameof(TryUnlockAsync)))
                    {
                        connection.ValidateArgument(nameof(connection));
                        key.ValidateArgumentNotNullOrWhitespace(nameof(key));
                        requester.ValidateArgumentNotNullOrWhitespace(nameof(requester));

                        var query = UnlockQuery.Value;
                        loggers.Log($"Attempting to unlock resource <{key}> for <{requester}>");
                        loggers.Trace($"Unlocking resource <{key}> using query: {Environment.NewLine + query}");

                        // Create parameters
                        var parameters = new DynamicParameters()
                            .AddParameter(nameof(DatabaseLock.Name), key)
                            .AddParameter(nameof(DatabaseLock.LockedBy), requester);

                        DatabaseLock databaseLock = null;
                        using (var result = await connection.Connection.QueryMultipleAsync(new CommandDefinition(UnlockQuery.Value, parameters, connection.Transaction, cancellationToken: cancellationToken)).ConfigureAwait(false))
                        {
                            while (!result.IsConsumed)
                            {
                                databaseLock = await result.ReadSingleOrDefaultAsync<DatabaseLock>().ConfigureAwait(false);
                            }

                            // Get result from last select
                            return databaseLock ?? throw new InvalidOperationException($"Expected to receive database lock");
                        }
                    }
                }

                private class DatabaseLock
                {
                    public string Name { get; set; }
                    public string LockedBy { get; set; }
                    public DateTime LockedAt { get; set; }
                    public DateTime ExpiryDate { get; set; }
                }
                #endregion
            }

            private static async Task<IDataRepositoryConnection> OpenConnectionAsync(string connectionString, CancellationToken cancellationToken = default)
            {
                connectionString.ValidateArgumentNotNullOrWhitespace(nameof(connectionString));
                var connection = new MySqlDataRepositoryConnection(connectionString, true);
                await connection.OpenAsync(cancellationToken);
                return connection;
            }
        }
        #endregion

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

        /// <summary>
        /// Contains extra helper methods for building queries.
        /// </summary>
        public static class Builder
        {
            /// <summary>
            /// Builds a mysql condition clause using the public instance properties on <paramref name="conditions"/>. Condition will be based on the property type and name.
            /// </summary>
            /// <typeparam name="TEntity">Type of the main entity the query is built for</typeparam>
            /// <typeparam name="T">Type of the object containing the conditions</typeparam>
            /// <param name="builder">Builder to create the conditions</param>
            /// <param name="parameters">Object where the parameters for the condition will be added to</param>
            /// <param name="conditions">Object containing the conditions</param>
            /// <param name="dataSet">Optional dataset for the columns in the where condition, when set to null, <see cref="MemberInfo.ReflectedType"/> is taken as the dataset</param>
            /// <param name="excludedProperties">Names of properties on <typeparamref name="T"/> to exclude from the condition</param>
            /// <returns>Builder to chain more conditions or null if no conditions were created</returns>
            public static IChainedBuilder<TEntity, IStatementConditionExpressionBuilder<TEntity>> BuildConditionsFrom<TEntity, T>(IStatementConditionExpressionBuilder<TEntity> builder, DynamicParameters parameters, T conditions, object? dataSet = null, params string[] excludedProperties)
            {
                builder.ValidateArgument(nameof(builder));
                conditions.ValidateArgument(nameof(conditions));
                parameters.ValidateArgument(nameof(parameters));

                var properties = conditions.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(x => !excludedProperties.HasValue() || !excludedProperties.Contains(x.Name, StringComparer.OrdinalIgnoreCase)).ToArray();

                IChainedBuilder<TEntity, IStatementConditionExpressionBuilder<TEntity>> chainedBuilder = null;
                for (int i = 0; i < properties.Length; i++)
                {
                    var property = properties[i];
                    var value = property.GetValue(conditions);
                    if (value == null) continue;
                    if (value is string stringValue && !stringValue.HasValue()) continue;

                    if (chainedBuilder != null) builder = chainedBuilder.And;

                    chainedBuilder = BuildConditionFrom(builder, parameters, property, value, dataSet);
                }

                return chainedBuilder;
            }
            /// <summary>
            /// Builds a mysql condition for <paramref name="property"/>. Condition will be based on the property type and name.
            /// </summary>
            /// <typeparam name="TEntity">Type of the main entity the query is built for</typeparam>
            /// <param name="builder">Builder to create the conditions</param>
            /// <param name="parameters">Object where the parameters for the condition will be added to</param>
            /// <param name="property">The property to create the condition for</param>
            /// <param name="propertyValue">The value contained in <paramref name="property"/></param>
            /// <param name="dataSet">Optional dataset for the columns in the where condition, when set to null, <see cref="MemberInfo.ReflectedType"/> is taken as the dataset</param>
            /// <returns>Builder to chain more conditions or null if no conditions were created</returns>
            public static IChainedBuilder<TEntity, IStatementConditionExpressionBuilder<TEntity>> BuildConditionFrom<TEntity>(IStatementConditionExpressionBuilder<TEntity> builder, DynamicParameters parameters, PropertyInfo property, object propertyValue, object? dataSet = null)
            {
                builder.ValidateArgument(nameof(builder));
                parameters.ValidateArgument(nameof(parameters));
                property.ValidateArgument(nameof(property));
                if (propertyValue == null) return null;
                if (propertyValue is string stringValue && !stringValue.HasValue()) return null;
                dataSet ??= property.ReflectedType;

                // Select operator based on type
                if (property.PropertyType.Is<string>())
                {
                    parameters.AddParameter(property.Name, propertyValue);
                    return builder.Column(dataSet, property.Name).LikeParameter(property.Name);
                }
                else if (property.PropertyType.IsContainer())
                {
                    var values = propertyValue.Cast<IEnumerable>().Enumerate().ToArray();
                    for(int i = 0; i < values.Length; i++)
                    {
                        parameters.AddParameter($"{property.Name}[{i}]", values[i]);
                    }
                    return builder.Column(dataSet, property.Name.TrimEnd('s')).In.Values(LinqExtensions.Select(values, (i, x) => new ParameterExpression($"{property.Name}[{i}]")));
                }
                else
                {
                    parameters.AddParameter(property.Name, propertyValue);

                    if (property.Name.EndsWith("From"))
                    {
                        return builder.Column(dataSet, property.Name[..property.Name.IndexOf("From")]).GreaterOrEqualTo.Parameter(property.Name);
                    }
                    else if (property.Name.EndsWith("To"))
                    {
                        return builder.Column(dataSet, property.Name[..property.Name.IndexOf("To")]).LesserOrEqualTo.Parameter(property.Name);
                    }
                    else
                    {
                        return builder.Column(dataSet, property.Name).EqualTo.Parameter(property.Name);
                    }
                }
            }
        }
    }
}
