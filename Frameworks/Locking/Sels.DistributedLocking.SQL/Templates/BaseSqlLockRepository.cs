using Sels.Core.Data.Contracts.Repository;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Reflection;
using Sels.SQL.QueryBuilder;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Sels.Core.Extensions.Logging.Advanced;
using Microsoft.Extensions.Logging;
using Sels.SQL.QueryBuilder.Builder;
using Dapper;
using Sels.Core.Extensions.Linq;
using System.Linq;
using Sels.SQL.QueryBuilder.Extensions;
using Sels.Core.Extensions.Fluent;
using Sels.SQL.QueryBuilder.Expressions;
using System.Data.Common;
using Sels.Core.Conversion.Attributes.Serialization;

namespace Sels.DistributedLocking.SQL.Templates
{
    /// <summary>
    /// Base class for creating a <see cref="ISqlLockRepository"/> using a <see cref="ICachedSqlQueryProvider"/> and Dapper.
    /// </summary>
    public abstract class BaseSqlLockRepository : ISqlLockRepository
    {
        // Fields
        /// <summary>
        /// The compile options used to build queries.
        /// </summary>
        protected ExpressionCompileOptions _queryOptions = ExpressionCompileOptions.Format | ExpressionCompileOptions.DateAsUtc;
        /// <summary>
        /// The default format used to created named queries.
        /// </summary>
        protected readonly string _queryNameFormat;
        /// <summary>
        /// Service that builds database independant sql queries.
        /// </summary>
        protected readonly ICachedSqlQueryProvider _queryProvider;
        /// <summary>
        /// Optional logger for tracing.
        /// </summary>
        protected readonly ILogger _logger;

        /// <inheritdoc cref="BaseSqlLockRepository"/>
        /// <param name="queryProvider"><inheritdoc cref="_queryProvider"/></param>
        /// <param name="logger">O<inheritdoc cref="_logger"/></param>
        public BaseSqlLockRepository(ICachedSqlQueryProvider queryProvider, ILogger logger = null)
        {
            _queryProvider = queryProvider.ValidateArgument(nameof(queryProvider));
            _queryNameFormat = $"{GetType().GetDisplayName()}.{{0}}";

            _logger = logger;
        }

        /// <inheritdoc/>
        public virtual async Task DeleteAllRequestsById(IRepositoryTransaction transaction, long[] ids, CancellationToken token)
        {
            ids.ValidateArgumentNotNullOrEmpty(nameof(ids));
            var (dbConnection, dbTransaction) = GetTransactionInfo(transaction);

            _logger.Log($"Deleting <{ids.Length}> lock requests");
            var query = _queryProvider.Delete<SqlLockRequest>()
                                            .Where(w => w.Column(c => c.Id).In.Values(ids))
                                            .Build(_queryOptions);
            _logger.Trace($"Deleting <{ids.Length}> lock requests using query <{query}>");

            await dbConnection.ExecuteAsync(new CommandDefinition(query, transaction: dbTransaction, cancellationToken: token));
            _logger.Log($"Deleted <{ids.Length}> lock requests");
        }
        /// <inheritdoc/>
        public virtual async Task<int> DeleteInActiveLocksAsync(IRepositoryTransaction transaction, int? inactiveTime = null, CancellationToken token = default)
        {
            var (dbConnection, dbTransaction) = GetTransactionInfo(transaction);
            _logger.Log($"Deleting all {(inactiveTime.HasValue ? "inactive" : "free")} locks");
            // Delete all locks with no pending requests that aren't locked
            var query = _queryProvider.Delete<SqlLock>()
                                        .Where(w => w.Column(c => c.LockedBy).IsNull
                                                    // Add extra filter on last lock date when inactive is set
                                                    .When(inactiveTime.HasValue, b => b.And.WhereGroup(g => g.Column(c => c.LastLockDate).IsNull.Or.Column(c => c.LastLockDate).LesserThan.ModifyDate(b => b.CurrentDate(), -inactiveTime.Value, DateInterval.Millisecond)))
                                                    .And.Not().ExistsIn(
                                                                        _queryProvider.Select<SqlLockRequest>().Where(w => w.Column(c => c.Resource).EqualTo.Column<SqlLock>(c => c.Resource))
                                                                        )
                                                )
                                        .Build(_queryOptions);

            _logger.Trace($"Deleting all {(inactiveTime.HasValue ? "inactive" : "free")} locks using query {query}");
            var deleted = await dbConnection.ExecuteAsync(new CommandDefinition(query, transaction: dbTransaction, cancellationToken: token));
            _logger.Log($"Deleted <{deleted}> {(inactiveTime.HasValue ? "inactive" : "free")} locks");
            return deleted;
        }
        /// <inheritdoc/>
        public virtual async Task<SqlLockRequest[]> GetAllLockRequestsByResourceAsync(IRepositoryTransaction transaction, string resource, CancellationToken token)
        {
            resource.ValidateArgumentNotNullOrWhitespace(nameof(resource));
            var (dbConnection, dbTransaction) = GetTransactionInfo(transaction);

            _logger.Log($"Fetching all lock requests for resource <{resource}>");
            var query = _queryProvider.GetQuery(_queryNameFormat.FormatString(nameof(GetAllLockRequestsByResourceAsync)), p => p.Select<SqlLockRequest>().All()
                                                                                                                                .Where(w => w.Column(c => c.Resource).EqualTo.Parameter(nameof(resource)))
                                                                                                                                .OrderBy(c => c.CreatedAt, SortOrders.Ascending)
                                                                                                                                .Build(_queryOptions));
            _logger.Trace($"Fetching all lock requests for resource <{resource}> using <{query}>");
            var parameters = new DynamicParameters();
            parameters.Add($"@{nameof(resource)}", resource);

            var results = (await dbConnection.QueryAsync<SqlLockRequest>(new CommandDefinition(query, transaction: dbTransaction, cancellationToken: token))).ToArray();
            _logger.Log($"Fetched <{results.Length}> lock requests for resource <{resource}>");
            return results;
        }
        /// <inheritdoc/>
        public virtual async Task<int> GetLockAmountAsync(IRepositoryTransaction transaction, CancellationToken token = default)
        {
            var (dbConnection, dbTransaction) = GetTransactionInfo(transaction);
            _logger.Log($"Getting the total amount of locks");
            var query = _queryProvider.GetQuery(_queryNameFormat.FormatString(nameof(GetLockAmountAsync)), p => p.Select<SqlLock>().CountAll()
                                                                                                                 .Build(_queryOptions));

            _logger.Trace($"Getting the total amount of locks using query <{query}>");

            var count = await dbConnection.ExecuteScalarAsync<int>(new CommandDefinition(query, transaction: dbTransaction, cancellationToken: token));
            _logger.Log($"Total amount of locks is <{count}>");
            return count;
        }
        /// <inheritdoc/>
        public virtual async Task<(bool WasUnlocked, SqlLock CurrentState)> TryUnlockAsync(IRepositoryTransaction transaction, string resource, string requester, CancellationToken token)
        {
            resource.ValidateArgumentNotNullOrWhitespace(nameof(resource));
            requester.ValidateArgumentNotNullOrWhitespace(nameof(requester));
            var (dbConnection, dbTransaction) = GetTransactionInfo(transaction);

            _logger.Log($"Trying to unlock resource <{resource}> for <{requester}>");
            var query = _queryProvider.GetQuery(_queryNameFormat.FormatString(nameof(TryUnlockAsync)), p =>
            {
                var multiBuilder = p.Build();

                // Try update first
                multiBuilder.Append(p.Update<SqlLock>()
                                        .Set.Column(c => c.LockedBy).To.Null()
                                        .Where(w => w.Column(c => c.Resource).EqualTo.Parameter(nameof(resource))
                                                    .And.Column(c => c.LockedBy).EqualTo.Parameter(nameof(requester))));
                // Select latest state
                multiBuilder.Append(p.Select<SqlLock>()
                                     .Where(w => w.Column(c => c.Resource).EqualTo.Parameter(nameof(resource))));

                return multiBuilder.Build(_queryOptions);
            });

            _logger.Trace($"Trying to unlock resource <{resource}> for <{requester}> using query <{query}>");
            var parameters = new DynamicParameters();
            parameters.Add($"@{nameof(resource)}", resource);
            parameters.Add($"@{nameof(requester)}", requester);

            var multiQuery = await dbConnection.QueryMultipleAsync(new CommandDefinition(query, transaction: dbTransaction, cancellationToken: token));
            var wasUnlocked = (await multiQuery.ReadSingleAsync<int>()) >= 0;
            var currentLock = await multiQuery.ReadFirstAsync<SqlLock>();

            _logger.Log($"Resource <{resource}> was {(wasUnlocked ? "unlocked" : "not unlocked")} by <{requester}>");
            return (wasUnlocked, currentLock);
        }

        /// <inheritdoc/>
        public virtual async Task<SqlLock> TryUpdateExpiryDateAsync(IRepositoryTransaction transaction, string resource, string requester, TimeSpan extendTime, CancellationToken token)
        {
            resource.ValidateArgumentNotNullOrWhitespace(nameof(resource));
            requester.ValidateArgumentNotNullOrWhitespace(nameof(requester));
            var (dbConnection, dbTransaction) = GetTransactionInfo(transaction);

            _logger.Log($"Trying to update expiry time for lock on resource <{resource}> for <{requester}>");
            var query = _queryProvider.GetQuery(_queryNameFormat.FormatString(nameof(TryUpdateExpiryDateAsync)), p =>
            {
                var multiBuilder = p.Build();

                // Try update
                multiBuilder.Append(p.Update<SqlLock>()
                                        .Set.Column(c => c.ExpiryDate).To.Case(ca =>
                                                                            ca.When(w => w.Column(c => c.ExpiryDate).IsNull())
                                                                                .Then.ModifyDate(b => b.CurrentDate(DateType.Utc), b => b.Parameter(nameof(extendTime)), DateInterval.Millisecond)
                                                                              .Else.ModifyDate(b => b.Column(c => c.ExpiryDate), b => b.Parameter(nameof(extendTime)), DateInterval.Millisecond))
                                        .Where(w => w.Column(c => c.Resource).EqualTo.Parameter(nameof(resource))
                                                    .And.Column(c => c.LockedBy).EqualTo.Parameter(nameof(requester))));

                // Select latest state
                multiBuilder.Append(p.Select<SqlLock>()
                                     .Where(w => w.Column(c => c.Resource).EqualTo.Parameter(nameof(resource))));

                return multiBuilder.Build(_queryOptions);
            });

            _logger.Trace($"Trying to update expiry time for lock on resource <{resource}> for <{requester}> using query <{query}>");
            var parameters = new DynamicParameters();
            parameters.Add($"@{nameof(resource)}", resource);
            parameters.Add($"@{nameof(requester)}", requester);
            parameters.Add($"@{nameof(extendTime)}", extendTime.TotalMilliseconds);

            var multiQuery = await dbConnection.QueryMultipleAsync(new CommandDefinition(query, transaction: dbTransaction, cancellationToken: token));
            var wasUpdated = (await multiQuery.ReadSingleAsync<int>()) >= 0;
            var currentLock = await multiQuery.ReadFirstAsync<SqlLock>();

            if (wasUpdated) _logger.Log($"Expiry date on resource <{resource}> held by <{requester}> has been extended to <{currentLock.ExpiryDate}>");
            else _logger.Warning($"Resource <{resource}> is no longer held by <{requester}> so can't extend expiry date. Resource is currently held by <{currentLock.LockedBy}>");

            return currentLock;
        }

        /// <inheritdoc/>
        public abstract Task<SqlLock> GetLockByResourceAsync(IRepositoryTransaction transaction, string resource, bool countRequests, bool forUpdate, CancellationToken token);
        /// <inheritdoc/>
        public abstract Task<(SqlLock[] Results, int TotalMatching)> SearchAsync(IRepositoryTransaction transaction, string filter = null, int page = 0, int pageSize = 100, PropertyInfo sortColumn = null, bool sortDescending = false, CancellationToken token = default);
        /// <inheritdoc/>
        public abstract Task<SqlLock> TryAssignLockToAsync(IRepositoryTransaction transaction, string resource, string requester, DateTimeOffset? expiryDate, CancellationToken token);

        private (IDbConnection Connection, IDbTransaction Transaction) GetTransactionInfo(IRepositoryTransaction transaction)
        {
            transaction.ValidateArgument(nameof(transaction));

            var (connection, dbTransaction) = GetRepositoryTransactionInfo(transaction);
            if (connection == null) throw new InvalidOperationException($"{nameof(GetRepositoryTransactionInfo)} did not return a connection");
            if (dbTransaction == null) throw new InvalidOperationException($"{nameof(GetRepositoryTransactionInfo)} did not return a transaction");
            return (connection, dbTransaction);
        }

        // Abstractions
        /// <summary>
        /// Returns the connection and transaction from <paramref name="transaction"/>.
        /// </summary>
        /// <param name="transaction">The transaction to get the connection and transaction for</param>
        /// <returns>The connection and transaction included in <paramref name="transaction"/></returns>
        protected abstract (IDbConnection Connection, IDbTransaction Transaction) GetRepositoryTransactionInfo(IRepositoryTransaction transaction);

        /// <inheritdoc/>
        public abstract Task<SqlLockRequest> CreateRequestAsync(IRepositoryTransaction transaction, SqlLockRequest request, CancellationToken token);
        /// <inheritdoc/>
        public abstract Task<IRepositoryTransaction> CreateTransactionAsync(CancellationToken token);
    }
}

