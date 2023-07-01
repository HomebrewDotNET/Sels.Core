using Microsoft.Extensions.Logging;
using Sels.Core.Data.Contracts.Repository;
using Sels.Core.Data.MySQL.Models;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.DistributedLocking.SQL;
using Sels.DistributedLocking.SQL.Templates;
using Sels.SQL.QueryBuilder;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Sels.Core.Extensions.Logging.Advanced;
using Sels.SQL.QueryBuilder.Builder;
using Sels.SQL.QueryBuilder.MySQL;
using Dapper;
using Sels.Core.Data.SQL.Extensions.Dapper;
using Sels.DistributedLocking.MySQL.Options;
using Sels.Core.Data.FluentMigrationTool;
using Microsoft.Extensions.Options;
using FluentMigrator.Runner;
using Sels.DistributedLocking.MySQL.Migrations;
using System.Collections;
using System.Linq;
using Sels.SQL.QueryBuilder.Expressions;
using Sels.Core.Extensions.Logging;
using Sels.SQL.QueryBuilder.Builder.Statement;
using Sels.Core.Extensions.Fluent;
using Sels.Core.Conversion.Extensions;

namespace Sels.DistributedLocking.MySQL.Repository
{
    /// <summary>
    /// Manages lock state using queries that work with MySql/MariaDB databases.
    /// </summary>
    public class MySqlLockRepository : BaseSqlLockRepository
    {
        // Statis
        private static readonly object _deploymentLock = new object();

        // Fields
        private readonly string _connectionString;
        private readonly IMigrationToolFactory _migrationToolFactory;

        /// <inheritdoc cref="MySqlLockRepository"/>
        /// <param name="connectionString">The connetion string to use</param>
        /// <param name="deploymentOptions">Contains the options for the deployment of the database schema</param>
        /// <param name="queryProvider"><inheritdoc cref="BaseSqlLockRepository.QueryProvider"/></param>
        /// <param name="migrationToolFactory">Factory used to create the migration tool to deploy the database schema</param>
        /// <param name="logger">O<inheritdoc cref="BaseSqlLockRepository._logger"/></param>
        protected MySqlLockRepository(string connectionString, IOptions<MySqlLockRepositoryDeploymentOptions> deploymentOptions, ICachedSqlQueryProvider queryProvider, IMigrationToolFactory migrationToolFactory, ILogger logger = null) : base(queryProvider, logger)
        {
            var typedConnectionString = ConnectionString.Parse(connectionString.ValidateArgument(nameof(connectionString)));
            typedConnectionString.AllowUserVariables = true;
            _connectionString = typedConnectionString.ToString();
            _migrationToolFactory = migrationToolFactory.ValidateArgument(nameof(migrationToolFactory));
            deploymentOptions.ValidateArgument(nameof(deploymentOptions));

            _queryOptions |= ExpressionCompileOptions.AppendSeparator;

            // Set table names
            SqlLockTableName = deploymentOptions.Value.LockTableName;
            SqlLockRequestTableName = deploymentOptions.Value.LockRequestTableName;

            // Deploy schema if enabled
            DeployDatabaseSchema(deploymentOptions.Value);
        }

        /// <inheritdoc cref="MySqlLockRepository"/>
        /// <param name="connectionString">The connetion string to use</param>
        /// <param name="deploymentOptions">Contains the options for the deployment of the database schema</param>
        /// <param name="migrationToolFactory">Factory used to create the migration tool to deploy the database schema</param>
        /// <param name="queryProvider"><inheritdoc cref="BaseSqlLockRepository.QueryProvider"/></param>
        /// <param name="logger">O<inheritdoc cref="BaseSqlLockRepository._logger"/></param>
        public MySqlLockRepository(string connectionString, IOptions<MySqlLockRepositoryDeploymentOptions> deploymentOptions, ICachedSqlQueryProvider queryProvider, IMigrationToolFactory migrationToolFactory, ILogger<MySqlLockRepository> logger = null) : this(connectionString, deploymentOptions, queryProvider, migrationToolFactory, logger.CastToOrDefault<ILogger>())
        {

        }

        /// <inheritdoc/>
        public override async Task<IRepositoryTransaction> CreateTransactionAsync(CancellationToken token)
        {
            var connection = new MySqlRepositoryTransaction(_connectionString);
            await connection.OpenAsync(token).ConfigureAwait(false);
            return connection;
        }
        /// <inheritdoc/>
        protected override (IDbConnection Connection, IDbTransaction Transaction) GetRepositoryTransactionInfo(IRepositoryTransaction transaction)
        {
            if (transaction is MySqlRepositoryTransaction mySqlTransaction)
            {
                return (mySqlTransaction.Connection, mySqlTransaction.Transaction);
            }

            throw new InvalidOperationException($"Expected transaction of type <{typeof(MySqlRepositoryTransaction)}> but got <{transaction}>");
        }

        /// <inheritdoc/>
        public override async Task<SqlLockRequest> CreateRequestAsync(IRepositoryTransaction transaction, SqlLockRequest request, CancellationToken token)
        {
            request.ValidateArgument(nameof(request));
            var (dbConnection, dbTransaction) = GetTransactionInfo(transaction);

            _logger.Log($"Inserting new lock request on resource <{request.Resource}> for requester <{request.Requester}>");
            var query = QueryProvider.GetQuery(_queryNameFormat.FormatString(nameof(CreateRequestAsync)), p =>
            {
                var multiBuilder = p.New();

                // Insert record
                multiBuilder.Append(p.Insert<SqlLockRequest>().ColumnsOf(nameof(SqlLockRequest.Id))
                                    .ParametersFrom(excludedProperties: nameof(SqlLockRequest.Id)));

                // Return insert id
                multiBuilder.Append(p.Select().LastInsertedId());

                return multiBuilder;
            });

            _logger.Trace($"Inserting new lock request on resource <{request.Resource}> for requester <{request.Requester}> using query <{query}>");
            request.SetToUtc();
            var parameters = new DynamicParameters()
                                .AddParametersUsing(request, nameof(SqlLockRequest.Id));

            var id = await dbConnection.ExecuteScalarAsync<long>(new CommandDefinition(query, parameters, transaction: dbTransaction, cancellationToken: token)).ConfigureAwait(false);

            _logger.Log($"Inserted lock request <{id}> on resource <{request.Resource}> for requester <{request.Requester}>");
            request.Id = id;
            return request.SetToLocal();
        }
        /// <inheritdoc/>
        public override async Task<SqlLock> GetLockByResourceAsync(IRepositoryTransaction transaction, string resource, bool countRequests, bool forUpdate, CancellationToken token)
        {
            resource.ValidateArgument(nameof(resource));
            var (dbConnection, dbTransaction) = GetTransactionInfo(transaction);

            _logger.Log($"Fetching lock on resource <{resource}>");
            // Generate query
            var queryBuilder = QueryProvider.Select<SqlLock>().AllOf()
                                                .Where(w => w.Column(c => c.Resource).EqualTo.Parameter(nameof(resource)));
            if (countRequests)
            {
                _logger.Debug($"Counting pending requests for lock on resource <{resource}>");
                queryBuilder.Expression(b => b.Query(QueryProvider.Select<SqlLockRequest>().CountAll().Where(w => w.Column(c => c.Resource).EqualTo.Parameter(nameof(resource)))), nameof(SqlLock.PendingRequests));
            }

            if (forUpdate)
            {
                _logger.Debug($"Locking lock on resource <{resource}> during transaction");
                queryBuilder.ForUpdate();
            }

            var query = queryBuilder.Build(_queryOptions);
            var parameters = new DynamicParameters()
                                 .AddParameter($"@{nameof(resource)}", resource);
            _logger.Trace($"Fetching lock on resource <{resource}> using <{query}>");

            // Execute query
            var sqlLock = await dbConnection.QuerySingleOrDefaultAsync<SqlLock>(new CommandDefinition(query, parameters, transaction: dbTransaction, cancellationToken: token)).ConfigureAwait(false);

            // Query always returns even though there's no row so check resource too
            if (sqlLock == null || sqlLock.Resource == null)
            {
                _logger.Warning($"Lock on resource <{resource}> does not exist");
                return null;
            }
            else
            {
                _logger.Log($"Fetched lock on resource <{resource}>");
            }

            return sqlLock?.SetToLocal();
        }
        /// <inheritdoc/>
        public override async Task<(SqlLock[] Results, int TotalMatching)> SearchAsync(IRepositoryTransaction transaction, SqlQuerySearchCriteria searchCriteria, CancellationToken token = default)
        {
            searchCriteria.ValidateArgument(nameof(searchCriteria));
            var (dbConnection, dbTransaction) = GetTransactionInfo(transaction);
            _logger.Log($"Querying locks");

            const string CteName = "CTE";
            const string TotalColumnName = "Total";

            // Generate query
            var parameters = new DynamicParameters();
            var cteQuery = QueryProvider.Select<SqlLock>().AllOf();

            // Count pending requests
            if (searchCriteria.IncludePendingRequests)
            {
                cteQuery.Expression(e => e.Query(QueryProvider.Select<SqlLockRequest>().CountAll()
                                                     .Where(w => w.Column(c => c.Resource).EqualTo.Column<SqlLock>(c => c.Resource)))
                                       , nameof(SqlLock.PendingRequests));
                _logger.Debug($"Including pending requests in search query");
            }
            else
            {
                _logger.Debug($"Not including pending requests in search query. Defaulting to 0");
                cteQuery.Value(0, nameof(SqlLock.PendingRequests));
            }


            // Conditions
            int parameterCounter = 0;
            if (searchCriteria.Filters.HasValue())
            {
                cteQuery.Where(w =>
                {
                    // Add filters on columns
                    foreach (var filterGroup in searchCriteria.Filters.GroupBy(x => x.Column))
                    {
                        if (w.LastBuilder != null) w = w.LastBuilder.And;
                        w.WhereGroup(g =>
                        {
                            foreach(var (column, filter, isFullMatch) in filterGroup)
                            {
                                if (g.LastBuilder != null) g = g.LastBuilder.Or;
                                if (filter == null && isFullMatch)
                                {
                                    _logger.Debug($"Filtering search query where column <{column}> is NULL");
                                    _ = g.Column(SqlLockAlias, column).IsNull;
                                }
                                else
                                {
                                    _logger.Debug($"Filtering search query where column <{column}> is {(isFullMatch ? "equal to" : "like")} <{filter}>");
                                    var parameterName = $"Filter{parameterCounter++}";
                                    parameters.Add(parameterName, filter);
                                    g.Column(SqlLockAlias, column)
                                     .When(isFullMatch, x => x.EqualTo.Parameter(parameterName),
                                                        x => x.LikeParameter(parameterName));
                                }
                                
                            }
                            return g.LastBuilder;
                        });
                    }

                    // Filter on expiry date
                    if (searchCriteria.ShowExpiredOnly.HasValue)
                    {
                        if (w.LastBuilder != null) w = w.LastBuilder.And;

                        _logger.Debug($"Filtering search query on {(searchCriteria.ShowExpiredOnly.Value ? "expired" : "non expired")} locks");
                        w.Column(x => x.ExpiryDate)
                         .When(searchCriteria.ShowExpiredOnly.Value, x => x.LesserThan, x => x.GreaterOrEqualTo)
                         .CurrentDate(DateType.Utc);
                    }

                    return w.LastBuilder;
                });
                
            }

            // Full select
            var selectQuery = QueryProvider.Select<SqlLock>()
                                           .AllOf()
                                           .Expression(x => x.Query(QueryProvider.Select().CountAll().From(CteName)), TotalColumnName)
                                           .From(CteName, datasetAlias: SqlLockAlias);

            // Filter on pending results
            if (searchCriteria.PendingRequestFilter.HasValue)
            {
                var parameterName = $"Filter{parameterCounter++}";
                parameters.Add(parameterName, searchCriteria.PendingRequestFilter.Value);
                selectQuery.Where(w => w.Column(c => c.PendingRequests).GreaterThan.Parameter(parameterName));
            }

            // Pagination
            if (searchCriteria.Pagination.HasValue)
            {
                var (page, pageSize) = searchCriteria.Pagination.Value;
                _logger.Debug($"Applying pagination to search query using pages of size <{pageSize}> returning page <{page}>");
                selectQuery.Limit((page - 1) * pageSize, pageSize);
            }

            // Sorting
            if (searchCriteria.SortColumns.HasValue())
            {
                foreach(var (column, sortDescending) in searchCriteria.SortColumns)
                {
                    _logger.Debug($"Sorting query by column <{column}> {(sortDescending ? "descending" : "ascending")}");
                    selectQuery.OrderBy(SqlLockAlias, column, sortDescending ? SortOrders.Descending : SortOrders.Ascending);
                }
            }

            // Build query

            var query = QueryProvider.With().Cte(CteName)
                                            .As(cteQuery)
                                     .Execute(selectQuery)
                                     .Build(_queryOptions);

            _logger.Trace($"Querying locks using query <{query}>");

            int total = 0;
            var results = (await dbConnection.QueryAsync<SqlLock, long, SqlLock>(new CommandDefinition(query, parameters, transaction: dbTransaction, cancellationToken: token), (s, t) => {
                if(total == 0) total = t.ConvertTo<int>();
                return s;
            }, TotalColumnName).ConfigureAwait(false)).ToArray();

            _logger.Log($"Search query returned <{results.Length}> results out of the total <{total}>");
            return (results, total);
        }
        /// <inheritdoc/>
        public override async Task<SqlLock> TryAssignLockToAsync(IRepositoryTransaction transaction, string resource, string requester, DateTime? expiryDate, CancellationToken token)
        {
            resource.ValidateArgumentNotNullOrWhitespace(nameof(resource));
            requester.ValidateArgumentNotNullOrWhitespace(nameof(requester));
            var (dbConnection, dbTransaction) = GetTransactionInfo(transaction);

            _logger.Log($"Trying to assign lock <{resource}> to <{requester}>");

            // Generate query
            var query = QueryProvider.GetQuery(_queryNameFormat.FormatString(nameof(TryAssignLockToAsync)), q =>
            {
                const string ExistsVariable = "Exists";
                const string IsLockedVariable = "IsLocked";

                // Get query that checks if the lock exists and if it exists if it is locked
                var existsQuery = q.Select<SqlLock>().ForUpdate()
                                    .Expression(e => e.AssignVariable(ExistsVariable, 1))
                                    .Expression(e => e.AssignVariable(IsLockedVariable, b => b.Case(ca => ca.When(w => w.Column(c => c.Resource).EqualTo.Parameter(nameof(resource))
                                                                                                                       .And.WhereGroup(g => g.Column(c => c.LockedBy).IsNull.
                                                                                                                                            Or.WhereGroup(g => g.Column(c => c.LockedBy).EqualTo.Parameter(nameof(requester)).
                                                                                                                                                               Or.Column(c => c.ExpiryDate).LesserThan.CurrentDate()))                                                                                                                       
                                                                                                                  )
                                                                                                                  .Then.Value(0)
                                                                                                             .Else.Value(1)
                                                                                                   )
                                    ))
                                    .Where(x => x.Column(c => c.Resource).EqualTo.Parameter(nameof(resource)));

                // Query that selects the latest lock state
                var getLockQuery = q.Select<SqlLock>().Where(w => w.Column(c => c.Resource).EqualTo.Parameter(nameof(resource)));

                var lockQuery = q.If().Condition(w => w.Variable(ExistsVariable).EqualTo.Value(1))
                             // Record exists so we check if it is locked   
                             .Then(q.If().Condition(w => w.Variable(IsLockedVariable).EqualTo.Value(0))
                                         // Record is not locked so we check if there are pending requests
                                         .Then(q.If().Condition(w => w.ExistsIn(q.Select<SqlLockRequest>().Value(1).Where(sw => sw.Column(c => c.Resource).EqualTo.Parameter(nameof(resource))).Limit(1)))
                                                     // There are pending requests so assign the oldest non-timedout request
                                                     .Then(q.Update<SqlLock>()
                                                                      .Set.Column(c => c.LockedBy).To.Column<SqlLockRequest>(c => c.Requester)
                                                                      .Set.Column(c => c.ExpiryDate).To.Case(cw => cw.When(w => w.Column<SqlLockRequest>(c => c.ExpiryTime).IsNotNull)
                                                                                                                          .Then.ModifyDate(d => d.CurrentDate(), d => d.Column<SqlLockRequest>(c => c.ExpiryTime), DateInterval.Second)
                                                                                                                     .Else
                                                                                                                          .Null()
                                                                                                            )
                                                                      .Set.Column(c => c.LockedAt).To.CurrentDate()
                                                                      .Set.Column(c => c.LastLockDate).To.CurrentDate()
                                                                      .InnerJoin().SubQuery(q.With().Cte("Request")
                                                                                                    .As(q.Select<SqlLockRequest>().ForUpdate()
                                                                                                            .Where(w => w.Column(c => c.Resource).EqualTo.Parameter(nameof(resource)).And.Column(c => c.Timeout).IsNull.Or.Column(c => c.Timeout).GreaterThan.CurrentDate())
                                                                                                            .OrderBy(c => c.CreatedAt, SortOrders.Ascending)
                                                                                                            .Limit(1))
                                                                                             .Execute(q.Select().All().From("Request")), datasetAlias: SqlLockRequestAlias)
                                                                      .On(j => j.Column(c => c.Resource).EqualTo.Column<SqlLockRequest>(c => c.Resource))
                                                                      
                                                          )
                                                     // Delete assigned requests
                                                     .Then(q.Delete<SqlLockRequest>()
                                                            .InnerJoin().Table<SqlLock>().On(w => w.Column(c => c.Resource).EqualTo.Column<SqlLock>(c => c.Resource)
                                                                                                   .And.Column(c => c.Requester).EqualTo.Column<SqlLock>(c => c.LockedBy))
                                                            .Where(w => w.Column(c => c.Resource).EqualTo.Parameter(nameof(resource))))
                                                     // Get latest state
                                                     .Then(getLockQuery)
                                                .Else
                                                     // Assign lock
                                                     .Then(q.Update<SqlLock>()
                                                            .Set.Column(c => c.LockedBy).To.Parameter(nameof(requester))
                                                            .Set.Column(c => c.ExpiryDate).To.Parameter(nameof(expiryDate))
                                                            .Set.Column(c => c.LockedAt).To.CurrentDate()
                                                            .Set.Column(c => c.LastLockDate).To.CurrentDate()
                                                            .Where(w => w.Column(c => c.Resource).EqualTo.Parameter(nameof(resource)))
                                                          )
                                                     // Get latest state
                                                     .Then(getLockQuery)
                                              )
                                    .Else
                                         // Already locked so return latest state
                                         .Then(getLockQuery)
                                  )
                        .Else
                             // Record doesn't exist so we insert
                             .Then(q.Insert<SqlLock>().Columns(x => x.Resource, x => x.LockedBy, x => x.ExpiryDate, x => x.LockedAt, x => x.LastLockDate)
                                    .Values(e => e.Parameter(nameof(resource)), e => e.Parameter(nameof(requester)), e => e.Parameter(nameof(expiryDate)), e => e.CurrentDate(), e => e.CurrentDate()))
                             // Get latest state
                             .Then(getLockQuery);

                return q.New()
                        .Append(existsQuery)
                        .Append(lockQuery);
            });

            _logger.Trace($"Trying to assign lock <{resource}> to <{requester}> using query <{query}>");
            var parameters = new DynamicParameters()
                                 .AddParameter(nameof(resource), resource)
                                 .AddParameter(nameof(requester), requester)
                                 .AddParameter(nameof(expiryDate), expiryDate.HasValue && expiryDate.Value.Kind != DateTimeKind.Utc ? expiryDate.Value.ToUniversalTime() : expiryDate);

            // Execute query
            SqlLock sqlLock = null;
            using (var reader = await dbConnection.QueryMultipleAsync(new CommandDefinition(query, parameters, transaction: dbTransaction, cancellationToken: token)).ConfigureAwait(false))
            {
                while (!reader.IsConsumed)
                {
                    var @lock = await reader.ReadSingleOrDefaultAsync<SqlLock>().ConfigureAwait(false);
                    if (@lock != null) sqlLock = @lock;
                }
            }

            if (sqlLock == null) throw new InvalidOperationException($"Expected to receive latest sql state");

            _logger.Log($"Lock <{sqlLock.Resource}> is now assigned to <{sqlLock.LockedBy}>");
            return sqlLock.SetToLocal();
        }

        private void DeployDatabaseSchema(MySqlLockRepositoryDeploymentOptions options)
        {
            using (_logger.TraceMethod(this))
            {
                if (options.DeploySchema)
                {
                    lock (_deploymentLock)
                    {
                        _logger.Log($"Deploying database schema");
                        _logger.Debug($"Creating migration tool");
                        var migrationTool = _migrationToolFactory.Create(true)
                                                .ConfigureRunner(x => x.AddMySql5().WithGlobalConnectionString(_connectionString))
                                                .AddMigrationsFrom<VersionOneCreateLockTablesMigration>()
                                                .UseVersionTableMetaData(() => new SchemaVersionTableInfo(options.VersionTableName));
                        _logger.Log($"Setting migration state");
                        MigrationState.LockTableName = options.LockTableName;
                        MigrationState.LockRequestTableName = options.LockRequestTableName;

                        try
                        {
                            migrationTool.Deploy();
                        }
                        catch (Exception ex)
                        {
                            if (options.IgnoreMigrationExceptions)
                            {
                                _logger.LogException(LogLevel.Warning, $"Exception occured while deploying database schema. Might be caused by concurrent deployments. Ignoring", ex);
                                return;
                            }
                            _logger.Log($"Exception occured while deploying database schema.", ex);
                            throw;
                        }
                    }
                }
                else
                {
                    _logger.Warning($"Deployment of database schema is disabled. Manual deployment is required");
                }
            }
        }
    }
}
