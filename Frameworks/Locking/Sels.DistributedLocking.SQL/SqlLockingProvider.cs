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
using Sels.Core.Extensions.Linq;
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

        #region Classes
        /// <summary>
        /// A sql lock that was placed on a resource.
        /// </summary>
        private class AcquiredSqlLock : ILock
        {
            // Fields
            private bool _unlockCalled = false;
            private readonly SqlLockingProvider _provider;
            private readonly object _threadLock = new object();
            private TimeSpan _extendTime;
            private ILogger _logger;
            private readonly bool _keepAlive;
            private CancellationTokenSource _extendCancelSource;
            private Task _extendTask;

            // Properties
            /// <inheritdoc/>
            public string Resource { get; private set; }
            /// <inheritdoc/>
            public string LockedBy { get; private set; }
            /// <inheritdoc/>
            public DateTime? LockedAt { get; private set; }
            /// <inheritdoc/>
            public DateTime? LastLockDate { get; private set; }
            /// <inheritdoc/>
            public DateTime? ExpiryDate { get; private set; }
            /// <inheritdoc/>
            public int PendingRequests { get; private set; }

            /// <inheritdoc cref="AcquiredSqlLock"/>
            /// <param name="sqlLock">The current state of the sql lock</param>
            /// <param name="provider">The provider that created this instance</param>
            /// <param name="keepAlive">If the lock should be kept alive if the expiry date is set</param>
            /// <param name="extendTime">By how much to extend the expiry date by if <paramref name="keepAlive"/> is set to true</param>
            /// <param name="logger">Optional logger for tracing</param>
            public AcquiredSqlLock(SqlLock sqlLock, SqlLockingProvider provider, bool keepAlive, TimeSpan extendTime, ILogger logger)
            {
                sqlLock.ValidateArgument(nameof(sqlLock));
                _provider = provider.ValidateArgument(nameof(sqlLock));
                _keepAlive = keepAlive;
                _extendTime = extendTime;
                _logger = logger;

                // Copy over state
                Resource = sqlLock.Resource;
                LockedBy = sqlLock.LockedBy;
                LockedAt = sqlLock.LockedAt;
                LastLockDate = sqlLock.LastLockDate;
                ExpiryDate = sqlLock.ExpiryDate;
                PendingRequests = sqlLock.PendingRequests;

                TryStartExpiryTask();
            }

            /// <inheritdoc/>
            public Task<bool> HasLockAsync(CancellationToken token = default) => _provider.HasLockAsync(Resource, LockedBy, token);
            /// <inheritdoc/>
            public async Task<bool> ExtendAsync(TimeSpan extendTime, CancellationToken token = default)
            {
                var currentExpiry = ExpiryDate;
                var sqlLock = await _provider.ExtendExpiryAsync(Resource, LockedBy, extendTime, token).ConfigureAwait(false);

                if (sqlLock != null && sqlLock.HasLock(LockedBy) && (!currentExpiry.HasValue || currentExpiry.Value < sqlLock.ExpiryDate.Value))
                {
                    ExpiryDate = sqlLock.ExpiryDate;
                    TryStartExpiryTask();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            /// <inheritdoc/>
            public async Task<bool> UnlockAsync(CancellationToken token = default)
            {
                _unlockCalled = true;
                return await _provider.UnlockAsync(Resource, LockedBy, token).ConfigureAwait(false);
            }

            private void TryStartExpiryTask()
            {
                using (_logger.TraceMethod(this))
                {
                    lock (_threadLock)
                    {
                        if (!ExpiryDate.HasValue)
                        {
                            _logger.Debug($"No expiry set on lock <{Resource}> held by <{LockedBy}> so no need to start extend task");
                            return;
                        }

                        if (_keepAlive && (_extendTask == null || _extendTask.IsCompleted))
                        {
                            _extendCancelSource = _extendCancelSource == null ? new CancellationTokenSource() : _extendCancelSource;
                            _logger.Debug($"Keep alive is enabled on lock <{Resource}> held by <{LockedBy}> with an expiry date set at <{ExpiryDate}>. Starting extend task");

                            _extendTask = ExtendLockDuringLifetime(_extendCancelSource.Token);
                        }
                    }
                }
            }

            private async Task ExtendLockDuringLifetime(CancellationToken token)
            {
                using (_logger.TraceMethod(this))
                {
                    while (!token.IsCancellationRequested)
                    {
                        try
                        {
                            // No need to extend
                            if (!ExpiryDate.HasValue)
                            {
                                _logger.Warning($"Keep alive task for lock <{Resource}> held by <{LockedBy}> is running but no expiry date is set. Stopping task");
                                return;
                            }
                            // We're past the expiry date so check if we still have the lock
                            else if (ExpiryDate.Value < DateTime.Now && !await HasLockAsync(token).ConfigureAwait(false))
                            {
                                _logger.Warning($"Could not extend lock <{Resource}> held by <{LockedBy}> in time. Expired at <{ExpiryDate.Value}>, time now is <{DateTime.Now}>. Lock is stale");
                                if (_provider.OptionsMonitor.CurrentValue.ThrowOnStaleLock) throw new StaleLockException(LockedBy, this);
                                return;
                            }

                            // Sleep until we can extend the lock
                            var sleepTime = (ExpiryDate.Value - LockedAt.Value).Add(TimeSpan.FromMilliseconds(-_provider.OptionsMonitor.CurrentValue.ExpiryOffset));
                            _logger.Debug($"Extending expiry date for lock <{Resource}> held by <{LockedBy}> in <{sleepTime}ms> before it expires at <{ExpiryDate.Value}>");
                            await Helper.Async.Sleep(sleepTime, token).ConfigureAwait(false);
                            if (token.IsCancellationRequested) return;

                            // Extend lock
                            _logger.Debug($"Extending expiry date for lock <{Resource}> held by <{LockedBy}> by <{_extendTime}>");
                            if(!await ExtendAsync(_extendTime, token).ConfigureAwait(false))
                            {
                                _logger.Warning($"Could not extend expiry date on lock <{Resource}> for <{LockedBy}>. Lock is stale so stopping expiry task");
                                return;
                            }
                        }
                        catch (StaleLockException)
                        {
                            throw;
                        }
                        catch (ResourceAlreadyLockedException)
                        {
                            throw;
                        }
                        catch (ObjectDisposedException)
                        {
                            throw;
                        }
                        catch (OperationCanceledException)
                        {
                            return;
                        }
                        catch (Exception ex)
                        {
                            _logger.Log($"Something when wrong while trying to extend the expiry for lock <{Resource}> held by <{LockedBy}>.", ex);

                            // Wait before retrying
                            await Helper.Async.Sleep(TimeSpan.FromMilliseconds(_provider.OptionsMonitor.CurrentValue.ExpiryOffset / 3)).ConfigureAwait(false);
                        }
                    }
                }
            }

            /// <inheritdoc/>
            public async ValueTask DisposeAsync()
            {
                using (_logger.TraceMethod(this))
                {
                    _logger.Log($"Disposing lock <{Resource}> held by <{LockedBy}>");
                    try
                    {
                        // Cancel extend task if it's running
                        _extendCancelSource?.Cancel();
                        if (_extendTask != null) await _extendTask.ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        _logger.Log(ex);
                        throw;
                    }
                    finally
                    {
                        _extendCancelSource?.Dispose();

                        if (!_unlockCalled)
                        {
                            await _provider.UnlockAsync(Resource, LockedBy, Helper.App.ApplicationToken).ConfigureAwait(false);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Manages pending requests on a resource.
        /// </summary>
        private class RequestManager : IAsyncDisposable
        {
            // Fields
            private readonly CancellationTokenSource _watcherTokenSource = new CancellationTokenSource();
            private Task _watcherTask;
            private readonly List<PendingLockRequest> _pendingRequests = new List<PendingLockRequest>();
            private readonly object _threadLock = new object();
            private readonly IOptionsMonitor<SqlLockingProviderOptions> _options;
            private readonly Func<SqlLock, SqlLockRequest, AcquiredSqlLock> _requestCompleter;
            private readonly ISqlLockRepository _repository;
            private readonly ILogger _logger;

            // Properties
            /// <summary>
            /// The resource the requests are pending for.
            /// </summary>
            public string Resource { get; }
            /// <summary>
            /// How many requests the manager has pending.
            /// </summary>
            public int Pending
            {
                get
                {
                    lock (_threadLock)
                    {
                        return _pendingRequests.Count;
                    }
                }
            }

            /// <inheritdoc cref="RequestManager"/>
            /// <param name="resource">The resource to watch</param>
            /// <param name="requestCompleter">Delegate used to create <see cref="AcquiredSqlLock"/> for completed requests</param>
            /// <param name="repository">The repository used to manage lock state</param>
            /// <param name="options">The configured options</param>
            /// <param name="logger">Optional logger for tracing</param>
            public RequestManager(string resource, IOptionsMonitor<SqlLockingProviderOptions> options, Func<SqlLock, SqlLockRequest, AcquiredSqlLock> requestCompleter, ISqlLockRepository repository, ILogger logger)
            {
                Resource = resource.ValidateArgumentNotNullOrWhitespace(nameof(resource));
                _repository = repository.ValidateArgument(nameof(repository));
                _requestCompleter = requestCompleter.ValidateArgument(nameof(requestCompleter));
                _options = options.ValidateArgument(nameof(options));
                _logger = logger;
            }

            /// <summary>
            /// Let's the manager track the assignment of <paramref name="sqlLockRequest"/>.
            /// </summary>
            /// <param name="sqlLockRequest">The lock requests to watch</param>
            /// <param name="timeout">The timeout for the request if one is set</param>
            /// <param name="cancellationToken">Cancellation token that can be cancelled by the creator of the request</param>
            /// <returns>Task that can be awaited by the </returns>
            public Task<ILock> TrackRequest(SqlLockRequest sqlLockRequest, TimeSpan? timeout, CancellationToken cancellationToken)
            {
                sqlLockRequest.ValidateArgument(nameof(sqlLockRequest));

                lock (_threadLock)
                {
                    _watcherTokenSource.Token.ThrowIfCancellationRequested();
                    var pendingRequest = new PendingLockRequest(sqlLockRequest, timeout, cancellationToken);
                    _pendingRequests.Add(pendingRequest);

                    // Restart watcher if needed
                    if (_watcherTask == null || _watcherTask.IsCompleted) _watcherTask = Task.Run(WatchRequests);

                    return pendingRequest.WaitTask;
                }
            }

            private async Task WatchRequests()
            {
                var token = _watcherTokenSource.Token;
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        var sleepTime = _options.CurrentValue.RequestPollingRate;
                        await Helper.Async.Sleep(sleepTime, token).ConfigureAwait(false);
                        if (token.IsCancellationRequested) return;

                        _logger.Log($"Checking if requests have been assigned for resource <{Resource}>");
                        List<PendingLockRequest> requestsToDelete = new List<PendingLockRequest>();
                        PendingLockRequest[] orderedRequests;

                        // Check if we have pending requests to manage
                        lock (_threadLock)
                        {
                            orderedRequests = _pendingRequests.OrderBy(x => x.Request.CreatedAt).ToArray();
                            if (!orderedRequests.HasValue())
                            {
                                _logger.Log($"No more pending requests for resource <{Resource}> stopping watcher");
                                return;
                            }
                        }

                        // Try assign pending requests
                        await using (var transaction = await _repository.CreateTransactionAsync(token).ConfigureAwait(false))
                        {
                            // First check if lock has been assigned
                            var sqlLock = await _repository.GetLockByResourceAsync(transaction, Resource, false, true, token).ConfigureAwait(false);

                            var matchingRequests = orderedRequests.Where(x => x.Request.Requester.Equals(sqlLock?.LockedBy, StringComparison.OrdinalIgnoreCase)).ToArray();

                            // No matching requests so we attempt to lock
                            if (!matchingRequests.HasValue())
                            {
                                var request = orderedRequests.First();
                                if (sqlLock.CanLock(request.Request.Requester))
                                {
                                    _logger.Log($"Lock on resource <{Resource}> can be acquired by {request.Request.Requester}. Attempting to lock");

                                    sqlLock = await _repository.TryAssignLockToAsync(transaction, request.Request.Resource, request.Request.Requester, request.Request.ExpiryTime.HasValue ? DateTime.Now.AddSeconds(request.Request.ExpiryTime.Value) : (DateTime?)null, token).ConfigureAwait(false);

                                    matchingRequests = orderedRequests.Where(x => x.Request.Requester.Equals(sqlLock?.LockedBy, StringComparison.OrdinalIgnoreCase)).ToArray();
                                }

                            }

                            requestsToDelete.AddRange(matchingRequests);

                            // Check if requests have been removed
                            var requestIds = orderedRequests.Select(x => x.Request.Id).ToArray();
                            var deletedIds = await _repository.GetDeletedRequestIds(transaction, requestIds, token).ConfigureAwait(false);
                            var deletedRequests = orderedRequests.Where(x => !matchingRequests.Contains(x) && deletedIds.Contains(x.Request.Id)).ToArray();

                            // Get requests that have timed out
                            var timedoutRequests = orderedRequests.Where(x => !matchingRequests.Contains(x) && x.Request.Timeout.HasValue && DateTime.Now > x.Request.Timeout.Value).ToArray();
                            requestsToDelete.AddRange(timedoutRequests);

                            // Get cancelled requests
                            var cancelledRequests = orderedRequests.Where(x => !requestsToDelete.Contains(x) && x.CancellationToken.IsCancellationRequested).ToArray();
                            requestsToDelete.AddRange(cancelledRequests);

                            // Remove completed requests 
                            if (requestsToDelete.HasValue())
                            {
                                _logger.Log($"Removing requests <{requestsToDelete.Select(x => x.Request.Id).JoinString(", ")}>");

                                await _repository.DeleteAllRequestsById(transaction, requestsToDelete.Select(x => x.Request.Id).ToArray(), token).ConfigureAwait(false);
                            }

                            // Commit
                            await transaction.CommitAsync(token).ConfigureAwait(false);

                            // Complete if we have assigned requests
                            matchingRequests.Execute(x =>
                            {
                                var @lock = _requestCompleter(sqlLock, x.Request);
                                x.Set(@lock);
                                lock (_threadLock)
                                {
                                    _pendingRequests.Remove(x);
                                }
                                _logger.Log($"Completed request <{x.Request.Id}> placed by <{x.Request.Requester}> on resource <{x.Request.Resource}>");
                            });

                            // Complete timed out requests
                            timedoutRequests.Execute(x =>
                            {
                                x.Set(new LockTimeoutException(x.Request.Requester, sqlLock, x.Timeout ?? TimeSpan.Zero));
                                lock (_threadLock)
                                {
                                    _pendingRequests.Remove(x);
                                }
                                _logger.Warning($"Timed out request <{x.Request.Id}> placed by <{x.Request.Requester}> on resource <{x.Request.Resource}>");
                            });

                            // Complete cancelled requests
                            cancelledRequests.Execute(x =>
                            {
                                x.Set(new OperationCanceledException($"Request <{x.Request.Id}> placed by <{x.Request.Requester}> on resource <{x.Request.Resource}> was cancelled by caller"));
                                lock (_threadLock)
                                {
                                    _pendingRequests.Remove(x);
                                }
                                _logger.Warning($"Cancelled request <{x.Request.Id}> placed by <{x.Request.Requester}> on resource <{x.Request.Resource}>");
                            });

                            // Completed deleted requests
                            deletedRequests.Execute(x =>
                            {
                                x.Set(new OperationCanceledException($"Request <{x.Request.Id}> placed by <{x.Request.Requester}> on resource <{x.Request.Resource}> was removed"));
                                lock (_threadLock)
                                {
                                    _pendingRequests.Remove(x);
                                }
                                _logger.Warning($"Request <{x.Request.Id}> placed by <{x.Request.Requester}> on resource <{x.Request.Resource}> was removed");
                            });
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.Warning($"Watcher was cancelled while checking pending requests on <{Resource}>. Stopping");
                        return;
                    }
                    catch (ObjectDisposedException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        _logger.Log($"Something went wrong when checking pending lock requests for resource <{Resource}>", ex);
                    }
                }
            }

            /// <inheritdoc/>
            public async ValueTask DisposeAsync()
            {
                List<Exception> exceptions = new List<Exception>();
                _logger.Log($"Cancelling any pending requests on resource <{Resource}>");

                // Cancel watcher
                _watcherTokenSource.Cancel();

                // Wait for watcher to cancel
                try
                {
                    if (_watcherTask != null) await _watcherTask.ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.Log(ex);
                    exceptions.Add(ex);
                }
                finally
                {
                    // Dispose token source
                    _watcherTokenSource?.Dispose();
                }

                // Get requests to cancel
                PendingLockRequest[] requestsToCancel = null;
                lock (_threadLock)
                {
                    requestsToCancel = _pendingRequests.ToArray();
                    _pendingRequests.Clear();
                }

                // Cancel requests so callers can return
                requestsToCancel.Execute(x =>
                {
                    var message = $"Cancelled request <{x.Request.Id}> placed by <{x.Request.Requester}> on resource <{x.Request.Resource}>";
                    x.Set(new OperationCanceledException(message));
                    _logger.Log(message);
                });

                try
                {
                    if (requestsToCancel.HasValue())
                    {
                        // Delete requests
                        await using (var transaction = await _repository.CreateTransactionAsync(default).ConfigureAwait(false))
                        {
                            await _repository.DeleteAllRequestsById(transaction, requestsToCancel.Select(x => x.Request.Id).ToArray(), default).ConfigureAwait(false);
                            await transaction.CommitAsync().ConfigureAwait(false);
                        }
                    }       
                }
                catch(Exception ex)
                {
                    _logger.Log(ex);
                    exceptions.Add(ex);
                }

                if (exceptions.HasValue()) throw new AggregateException(exceptions);
            }
            private class PendingLockRequest : IDisposable
            {
                // Fields
                private readonly TaskCompletionSource<ILock> _taskSource = new TaskCompletionSource<ILock>();

                // Properties
                public SqlLockRequest Request { get; }
                public TimeSpan? Timeout { get; }
                public CancellationToken CancellationToken { get; }

                public PendingLockRequest(SqlLockRequest sqlLockRequest, TimeSpan? timeout, CancellationToken cancellationToken)
                {
                    Request = sqlLockRequest.ValidateArgument(nameof(sqlLockRequest));
                    Timeout = timeout;
                    CancellationToken = cancellationToken;
                }

                /// <summary>
                /// Task that is awaited by the caller when waiting on a lock to be placed.
                /// </summary>
                public Task<ILock> WaitTask => _taskSource.Task;

                public void Set(ILock @lock)
                {
                    lock (this)
                    {
                        if (!_taskSource.Task.IsCompleted) _taskSource.SetResult(@lock);
                    }
                }

                public void Set(Exception exception)
                {
                    lock (this)
                    {
                        if (!_taskSource.Task.IsCompleted) _taskSource.SetException(exception);
                    }
                }

                /// <inheritdoc/>
                public void Dispose()
                {
                    lock (this)
                    {
                        if (!_taskSource.Task.IsCompleted) _taskSource.SetException(new OperationCanceledException($"Lock request <{Request?.Id}> was cancelled"));
                    }
                }
            }
        }
        #endregion
    }
}
