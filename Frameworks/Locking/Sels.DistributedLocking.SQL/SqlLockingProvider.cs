using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Sels.Core;
using Sels.Core.Data.Contracts.Repository;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Logging.Advanced;
using Sels.DistributedLocking.Provider;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Sels.Core.Extensions.Conversion;
using System.Collections.Concurrent;
using System.Linq;
using Sels.DistributedLocking.Abstractions.Extensions;
using System.Reflection;
using Sels.Core.Extensions.Reflection;
using System.Runtime.CompilerServices;
using Sels.DistributedLocking.Abstractions.Models;
using Sels.Core.Dispose;
using Sels.Core.Components.Scope.Actions;

namespace Sels.DistributedLocking.SQL
{
    /// <summary>
    /// Provides distributed locks by relying on Sql locking for concurrency.
    /// </summary>
    public partial class SqlLockingProvider : ILockingProvider, IAsyncExposedDisposable
    {
        // Fields
        private readonly HashSet<RequestManager> _requestManagers = new HashSet<RequestManager>();
        private readonly ISqlLockRepository _lockRepository;
        private readonly ILogger _logger;
        private readonly CancellationTokenSource _maintenanceTokenSource = new CancellationTokenSource();
        private Task _maintenanceTask;

        // Properties
        /// <summary>
        /// Contains configuration for the currnt instance.
        /// </summary>
        public IOptionsMonitor<SqlLockingProviderOptions> OptionsMonitor { get; }
        /// <inheritdoc/>
        public bool? IsDisposed { get; private set; }

        /// <inheritdoc cref="SqlLockingProvider"/>
        /// <param name="lockRepository">Used to manage lock state</param>
        /// <param name="options">The configuration for this instance</param>
        /// <param name="logger">Optional logger for tracing</param>
        public SqlLockingProvider(ISqlLockRepository lockRepository, IOptionsMonitor<SqlLockingProviderOptions> options, ILogger<SqlLockingProvider> logger = null)
        {
            _lockRepository = lockRepository.ValidateArgument(nameof(lockRepository));
            _logger = logger;
            OptionsMonitor = options.ValidateArgument(nameof(options));

            _maintenanceTask = Task.Run(async () => await RunCleanupDuringLifetime(_maintenanceTokenSource.Token).ConfigureAwait(false));
        }

        /// <summary>
        /// Proxy constructor.
        /// </summary>
        public SqlLockingProvider()
        {

        }

        /// <inheritdoc/>
        public async virtual Task<ILockInfo> GetAsync(string resource, CancellationToken token = default)
        {
            resource.ValidateArgument(nameof(resource));

            _logger.Log($"Fetching lock state on resource <{resource}>");
            SqlLock sqlLock;
            await using (var transaction = await _lockRepository.CreateTransactionAsync(token).ConfigureAwait(false))
            {
                sqlLock = await _lockRepository.GetLockByResourceAsync(transaction, resource, true, false, token).ConfigureAwait(false);
            }

            if (sqlLock == null)
            {
                _logger.Debug($"No lock is placed on resource <{resource}>");
                return new SqlLock()
                {
                    Resource = resource,
                    PendingRequests = 0
                };
            }

            return sqlLock;
        }
        /// <inheritdoc/>
        public async virtual Task<ILockRequest[]> GetPendingRequestsAsync(string resource, CancellationToken token = default)
        {
            resource.ValidateArgumentNotNullOrWhitespace(nameof(resource));

            _logger.Log($"Fetching all pending requests for resource <{resource}>");

            await using (var transaction = await _lockRepository.CreateTransactionAsync(token).ConfigureAwait(false))
            {
                return (await _lockRepository.GetAllLockRequestsByResourceAsync(transaction, resource, token).ConfigureAwait(false)) ?? Array.Empty<SqlLockRequest>();
            }
        }
        /// <inheritdoc/>
        public async virtual Task<ILockResult> TryLockAsync(string resource, string requester, TimeSpan? expiryTime = null, bool keepAlive = false, CancellationToken token = default)
        {
            resource.ValidateArgumentNotNullOrWhitespace(nameof(resource));
            requester.ValidateArgumentNotNullOrWhitespace(nameof(requester));

            _logger.Log($"Attempting to lock resource <{resource}> for <{requester}>");

            // Try lock
            SqlLock sqlLock;
            await using (var transaction = await _lockRepository.CreateTransactionAsync(token).ConfigureAwait(false))
            {
                sqlLock = await _lockRepository.TryAssignLockToAsync(transaction, resource, requester, expiryTime.HasValue ? DateTime.Now.Add(expiryTime.Value) : (DateTime?)null, token).ConfigureAwait(false);
                await transaction.CommitAsync(token).ConfigureAwait(false);
            }

            // Check if lock was placed by requester
            if (sqlLock == null) throw new InvalidOperationException($"{nameof(ISqlLockRepository.TryAssignLockToAsync)} returned null");
            if (sqlLock.LockedBy.Equals(requester, StringComparison.OrdinalIgnoreCase))
            {
                _logger.Log($"Resource <{resource}> now locked by <{requester}>");
                return new LockResult(new AcquiredSqlLock(sqlLock, this, keepAlive, expiryTime.HasValue ? expiryTime.Value : TimeSpan.Zero, _logger));
            }
            else
            {
                _logger.Log($"Could not assign lock on resource <{resource}> to <{requester}> because it is currently held by <{sqlLock.LockedBy}>");
                return new LockResult(sqlLock);
            }
        }
        /// <inheritdoc/>
        public async virtual Task<ILock> LockAsync(string resource, string requester, TimeSpan? expiryTime = null, bool keepAlive = false, TimeSpan? timeout = null, CancellationToken token = default)
        {
            resource.ValidateArgumentNotNullOrWhitespace(nameof(resource));
            requester.ValidateArgumentNotNullOrWhitespace(nameof(requester));

            _logger.Log($"Attempting to lock resource <{resource}> for <{requester}>");

            Task<ILock> requestTask;
            await using (var transaction = await _lockRepository.CreateTransactionAsync(token).ConfigureAwait(false))
            {
                var sqlLock = await _lockRepository.TryAssignLockToAsync(transaction, resource, requester, expiryTime.HasValue ? DateTime.Now.Add(expiryTime.Value) : (DateTime?)null, token).ConfigureAwait(false);

                // Check if lock was placed by requester
                if (sqlLock == null) throw new InvalidOperationException($"{nameof(ISqlLockRepository.TryAssignLockToAsync)} returned null");
                if (sqlLock.LockedBy.Equals(requester, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.Log($"Resource <{resource}> now locked by <{requester}>");
                    await transaction.CommitAsync(token).ConfigureAwait(false);
                    return new AcquiredSqlLock(sqlLock, this, keepAlive, expiryTime.HasValue ? expiryTime.Value : TimeSpan.Zero, _logger);
                }
                else
                {
                    _logger.Log($"Could not assign lock on resource <{resource}> to <{requester}> because it is currently held by <{sqlLock.LockedBy}>. Creating lock request");
                    var sqlLockRequest = await _lockRepository.CreateRequestAsync(transaction, new SqlLockRequest() { Requester = requester, Resource = resource, ExpiryTime = expiryTime?.TotalSeconds, KeepAlive = keepAlive, Timeout = timeout.HasValue ? DateTime.Now.Add(timeout.Value) : (DateTime?)null, CreatedAt = DateTime.Now }, token).ConfigureAwait(false);

                    await transaction.CommitAsync(token).ConfigureAwait(false);

                    _logger.Log($"Created lock request <{sqlLockRequest.Id}> for <{sqlLockRequest.Requester}> on resource <{sqlLockRequest.Resource}>");
                    var manager = GetRequestManager(sqlLockRequest.Resource);
                    requestTask = manager.TrackRequest(sqlLockRequest, timeout, token);
                }
            }

            return await requestTask.ConfigureAwait(false);
        }
        /// <inheritdoc/>
        public async virtual Task<ILockQueryResult> QueryAsync(Action<ILockQueryCriteria> searchCriteria, CancellationToken token = default)
        {
            _logger.Log($"Querying locks");

            searchCriteria.ValidateArgument(nameof(searchCriteria));
            var sqlSearchCriteria = new SqlQuerySearchCriteria(searchCriteria);

            await using (var transaction = await _lockRepository.CreateTransactionAsync(token).ConfigureAwait(false))
            {
                var (results, total) = await _lockRepository.SearchAsync(transaction, sqlSearchCriteria, token).ConfigureAwait(false);

                return sqlSearchCriteria.Pagination.HasValue ? new LockQueryResult(results, sqlSearchCriteria.Pagination.Value.PageSize, total) : new LockQueryResult(results);
            }
        }
        /// <inheritdoc/>
        public async virtual Task ForceUnlockAsync(string resource, bool removePendingRequests = false, CancellationToken token = default)
        {
            resource.ValidateArgument(nameof(resource));

            _logger.Warning($"Forcefully removing lock on resource <{resource}>{(removePendingRequests ? " and any pending requests" : string.Empty)}");

            await using (var transaction = await _lockRepository.CreateTransactionAsync(token).ConfigureAwait(false))
            {
                await _lockRepository.ForceUnlockAsync(transaction, resource, removePendingRequests, token).ConfigureAwait(false);
                await transaction.CommitAsync(token).ConfigureAwait(false);
            }

            _logger.Warning($"Forcefully removed lock on resource <{resource}>{(removePendingRequests ? " and any pending requests" : string.Empty)}");
        }

        internal async Task<bool> HasLockAsync(string resource, string requester, CancellationToken token)
        {
            SqlLock sqlLock;
            await using (var transaction = await _lockRepository.CreateTransactionAsync(token).ConfigureAwait(false))
            {
                sqlLock = await _lockRepository.GetLockByResourceAsync(transaction, resource, false, false, token).ConfigureAwait(false);
            }

            return requester.Equals(sqlLock?.LockedBy, StringComparison.OrdinalIgnoreCase);
        }

        internal async virtual Task<SqlLock> ExtendExpiryAsync(string resource, string requester, TimeSpan extendTime, CancellationToken token = default)
        {
            resource.ValidateArgumentNotNullOrWhitespace(nameof(resource));
            requester.ValidateArgumentNotNullOrWhitespace(nameof(requester));

            _logger.Log($"Trying to extend expiry date on lock <{resource}> for <{requester}> by <{extendTime}>");

            // Try extend
            SqlLock sqlLock;
            await using (var transaction = await _lockRepository.CreateTransactionAsync(token).ConfigureAwait(false))
            {
                sqlLock = await _lockRepository.TryUpdateExpiryDateAsync(transaction, resource, requester, extendTime, token).ConfigureAwait(false);
                await transaction.CommitAsync(token).ConfigureAwait(false);
            }

            if (!ThrowOnStaleLock(sqlLock, requester, resource))
            {
                _logger.Warning($"Lock on resource <{resource}> for <{requester}> is stale. Can't extend expiry date");
            }
            else
            {
                _logger.Log($"Expiry date on lock <{resource}> extended to <{sqlLock.ExpiryDate}> for <{requester}>");
            }

            return sqlLock;
        }

        internal async virtual Task<bool> UnlockAsync(string resource, string requester, CancellationToken token = default)
        {
            resource.ValidateArgumentNotNullOrWhitespace(nameof(resource));
            requester.ValidateArgumentNotNullOrWhitespace(nameof(requester));

            _logger.Log($"Trying to unlock <{resource}> held by <{requester}>");

            // Try unlock
            SqlLock sqlLock;
            bool wasUnlocked;
            await using (var transaction = await _lockRepository.CreateTransactionAsync(token).ConfigureAwait(false))
            {
                (wasUnlocked, sqlLock) = await _lockRepository.TryUnlockAsync(transaction, resource, requester, token).ConfigureAwait(false);
                await transaction.CommitAsync(token).ConfigureAwait(false);
            }

            if (!wasUnlocked && !ThrowOnStaleLock(sqlLock, requester, resource))
            {
                _logger.Warning($"Lock on resource <{resource}> for <{requester}> is stale. Can't unlock");
                return false;
            }
            else
            {
                _logger.Log($"Resource <{resource}> unlocked by <{requester}>");
                return true;
            }
        }

        private bool ThrowOnStaleLock(SqlLock sqlLock, string requester, string resource)
        {
            requester.ValidateArgumentNotNullOrWhitespace(nameof(requester));
            var canThrow = OptionsMonitor.CurrentValue.ThrowOnStaleLock;

            // Null means lock doesn't exist anymore
            if (sqlLock == null)
            {
                if (canThrow) throw new StaleLockException(requester, new SqlLock() { Resource = resource, LockedBy = requester});
                return false;
            }

            // Not held anymore by requester
            if (!sqlLock.HasLock(requester))
            {
                if (canThrow) throw new ResourceAlreadyLockedException(requester, sqlLock);
                return false;
            }

            return true;
        }

        private RequestManager GetRequestManager(string resource)
        {
            resource.ValidateArgumentNotNullOrWhitespace(nameof(resource));

            lock (_requestManagers)
            {
                var requestManager = _requestManagers.FirstOrDefault(x => x.Resource.Equals(resource, StringComparison.OrdinalIgnoreCase));

                if (requestManager == null)
                {
                    _logger.Debug($"Creating request manager for resource <{resource}>");
                    requestManager = new RequestManager(resource, OptionsMonitor, (l, r) => new AcquiredSqlLock(l, this, r.KeepAlive, r.ExpiryTime.HasValue ? TimeSpan.FromSeconds(r.ExpiryTime.Value) : TimeSpan.Zero, _logger), _lockRepository, _logger);
                    _requestManagers.Add(requestManager);
                }

                return requestManager;
            }
        }

        private async Task RunCleanupDuringLifetime(CancellationToken token)
        {
            using (_logger.TraceMethod(this))
            {
                var sleepTime = 1000;
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        var options = OptionsMonitor.CurrentValue;
                        sleepTime = options.MaintenanceInterval.TotalMilliseconds.ChangeType<int>();
                        _logger.Debug($"Running maintenance in <{options.MaintenanceInterval}ms>");
                        await Helper.Async.Sleep(sleepTime, token).ConfigureAwait(false);
                        if (token.IsCancellationRequested)
                        {
                            _logger.Debug($"Maintenance task cancelled");
                            return;
                        }
                        // Refresh options
                        options = OptionsMonitor.CurrentValue;
                        _logger.Log($"Running maintenance on lock table");

                        // Cleanup managers with no pending requests
                        using (_logger.TraceAction(LogLevel.Debug, $"Lock cleanup"))
                        {
                            await using (var transaction = await _lockRepository.CreateTransactionAsync(token).ConfigureAwait(false))
                            {
                                // Check if cleanup needed
                                bool cleanupNeeded = true;
                                switch (options.CleanupMethod)
                                {
                                    case SqlLockCleanupMethod.Amount:
                                        var amountOfLocks = await _lockRepository.GetLockAmountAsync(transaction, token).ConfigureAwait(false);
                                        cleanupNeeded = amountOfLocks > options.CleanupAmount.Value;
                                        if (cleanupNeeded) _logger.Log($"Lock amount is higher than the configured amount of <{options.CleanupAmount}>. Current lock count is <{amountOfLocks}>. Cleaning table");
                                        break;
                                }

                                if (!cleanupNeeded)
                                {
                                    _logger.Log($"No cleanup of locks needed");
                                    continue;
                                }

                                // Get how old locks need to be before they can be removed
                                int? inactiveTime = null;
                                switch (options.CleanupMethod)
                                {
                                    case SqlLockCleanupMethod.Time:
                                        inactiveTime = options.CleanupAmount.Value;
                                        break;
                                }

                                if (token.IsCancellationRequested)
                                {
                                    _logger.Debug($"Cleanup task cancelled");
                                    return;
                                }

                                // Cleanup inaactive locks
                                if (inactiveTime.HasValue)
                                {
                                    _logger.Log($"Locks that have been inactive for more than <{inactiveTime}ms> will be removed");
                                }
                                else
                                {
                                    _logger.Log($"All inactive locks will be removed");
                                }

                                var deleted = await _lockRepository.DeleteInActiveLocksAsync(transaction, inactiveTime, token).ConfigureAwait(false);
                                await transaction.CommitAsync(token).ConfigureAwait(false);
                                _logger.Log($"Deleted <{deleted}> inactive locks");
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }
                    catch (Exception ex)
                    {
                        _logger.Log($"Error occured while running maintenance", ex);
                        await Helper.Async.Sleep(sleepTime, token).ConfigureAwait(false);
                    }
                }

                _logger.Debug($"Cleanup task cancelled");
            }
        }

        /// <inheritdoc/>
        public async virtual ValueTask DisposeAsync()
        {
            // Exit if already disposed
            if (IsDisposed.HasValue && IsDisposed.Value) return;

            using (new ExecutedAction(x => IsDisposed = x))
            {
                List<Exception> exceptions = new List<Exception>();
                _logger.Log($"Disposing");

                // Cancel maintenance task
                _maintenanceTokenSource?.Cancel();

                try
                {
                    // Wait for maintenance task
                    if (_maintenanceTask != null) await _maintenanceTask;
                }
                catch (Exception ex)
                {
                    _logger.Log($"Could not gracefully stop maintenance task", ex);
                    exceptions.Add(ex);
                }
                finally
                {
                    _maintenanceTokenSource?.Dispose();
                }

                // Dispose pending requests
                HashSet<RequestManager> managers = null;
                lock (_requestManagers)
                {
                    managers = _requestManagers.ToHashSet();
                    _requestManagers.Clear();
                }

                foreach (var manager in managers)
                {
                    try
                    {
                        await manager.DisposeAsync().ConfigureAwait(false);
                        _logger.Debug($"Disposed manager for resource <{manager.Resource}>");
                    }
                    catch (Exception ex)
                    {
                        _logger.Log($"Could not dispose manager for resource <{manager.Resource}>", ex);
                        exceptions.Add(ex);
                    }
                }

                if (exceptions.HasValue()) throw new AggregateException(exceptions); 
            }
        }
    }
}
