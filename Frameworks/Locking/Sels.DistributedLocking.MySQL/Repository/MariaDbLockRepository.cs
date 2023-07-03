﻿using Microsoft.Extensions.Logging;
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
using Sels.SQL.QueryBuilder.MySQL.MariaDb;

namespace Sels.DistributedLocking.MySQL.Repository
{
    /// <summary>
    /// Manages lock state using queries that work with MariaDB databases.
    /// Slightly more efficiënt than <see cref="MySqlLockRepository"/> due to the RETURNING keyword.
    /// </summary>
    public class MariaDbLockRepository : MySqlLockRepository
    {
        /// <inheritdoc cref="MariaDbLockRepository"/>
        /// <param name="connectionString">The connetion string to use</param>
        /// <param name="deploymentOptions">Contains the options for the deployment of the database schema</param>
        /// <param name="migrationToolFactory">Factory used to create the migration tool to deploy the database schema</param>
        /// <param name="queryProvider"><inheritdoc cref="BaseSqlLockRepository.QueryProvider"/></param>
        /// <param name="logger">O<inheritdoc cref="BaseSqlLockRepository._logger"/></param>
        public MariaDbLockRepository(string connectionString, IOptions<MySqlLockRepositoryDeploymentOptions> deploymentOptions, ICachedSqlQueryProvider queryProvider, IMigrationToolFactory migrationToolFactory, ILogger<MariaDbLockRepository> logger = null) : base(connectionString, deploymentOptions, queryProvider, migrationToolFactory, logger.CastToOrDefault<ILogger>())
        {

        }

        /// <inheritdoc/>
        public override async Task<SqlLockRequest> CreateRequestAsync(IRepositoryTransaction transaction, SqlLockRequest request, CancellationToken token)
        {
            request.ValidateArgument(nameof(request));
            var (dbConnection, dbTransaction) = GetTransactionInfo(transaction);

            _logger.Log($"Inserting new lock request on resource <{request.Resource}> for requester <{request.Requester}>");
            var query = QueryProvider.GetQuery(_queryNameFormat.FormatString(nameof(CreateRequestAsync)), p => p.Insert<SqlLockRequest>().ColumnsOf(nameof(SqlLockRequest.Id))
                                                                                                                .ParametersFrom(excludedProperties: nameof(SqlLockRequest.Id))
                                                                                                                .Returning(x => x.All()));

            _logger.Trace($"Inserting new lock request on resource <{request.Resource}> for requester <{request.Requester}> using query <{query}>");
            request.SetToUtc();
            var parameters = new DynamicParameters()
                                .AddParametersUsing(request, nameof(SqlLockRequest.Id));

            request = await dbConnection.QuerySingleAsync<SqlLockRequest>(new CommandDefinition(query, parameters, transaction: dbTransaction, cancellationToken: token)).ConfigureAwait(false);

            _logger.Log($"Inserted lock request <{request.Id}> on resource <{request.Resource}> for requester <{request.Requester}>");
            return request.SetToLocal();
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
                                    .Values(e => e.Parameter(nameof(resource)), e => e.Parameter(nameof(requester)), e => e.Parameter(nameof(expiryDate)), e => e.CurrentDate(), e => e.CurrentDate())
                                    .Returning(x => x.All()));

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
    }
}