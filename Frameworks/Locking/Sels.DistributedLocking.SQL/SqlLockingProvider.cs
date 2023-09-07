using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Sels.Core;
using Sels.Core.Data.Contracts.Repository;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Logging;
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
using Sels.Core.Scope.Actions;
using Castle.Core.Resource;
using Sels.Core.Mediator;
using Sels.Core.Mediator.Event;
using Sels.DistributedLocking.SQL.Event;
using Sels.Core.Async.TaskManagement;
using Sels.Core.Extensions.Threading;
using Sels.Core.Extensions.Linq;
using Sels.Core.Conversion.Attributes.Serialization;

namespace Sels.DistributedLocking.SQL
{
    /// <summary>
    /// Provides distributed locks by relying on Sql locking for concurrency.
    /// </summary>
    public partial class SqlLockingProvider : ILockingProvider, IAsyncExposedDisposable
    {
        // Constants
        /// <summary>
        /// The worker that performs maintenance related tasks.
        /// </summary>
        private const string MaintenanceWorkerName = nameof(SqlLockingProvider) + ".MaintenanceWorker";
        /// <summary>
        /// The worker that assigns pending requests to resources that can be locked.
        /// </summary>
        private const string AssignmentWorkerName = nameof(SqlLockingProvider) + ".AssignmentWorker";
        /// <summary>
        /// The worker that checks the state of pending requests and completes the requests based on the state.
        /// </summary>
        private const string CompletionWorkerName = nameof(SqlLockingProvider) + ".CompletionWorker";

        // Fields
        private readonly HashSet<PendingLockRequest> _pendingRequests = new HashSet<PendingLockRequest>();
        private readonly ISqlLockRepository _lockRepository;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;
        private ITaskManager _taskManager;
        private readonly INotifier _notifier;
        private readonly IEventSubscriber _eventSubscriber;

        // Properties
        /// <summary>
        /// Contains configuration for the currnt instance.
        /// </summary>
        public IOptionsMonitor<SqlLockingProviderOptions> OptionsMonitor { get; }
        /// <inheritdoc/>
        public bool? IsDisposed { get; private set; }

        /// <inheritdoc cref="SqlLockingProvider"/>
        /// <param name="notifier">Used to raised events</param>
        /// <param name="eventSubscriber">Used to subscribe to events</param>
        /// <param name="taskManager">Used to manage workers</param>
        /// <param name="lockRepository">Used to manage lock state</param>
        /// <param name="options">The configuration for this instance</param>
        /// <param name="loggerFactory">Optional factory used to create loggers for child instances</param>
        /// <param name="logger">Optional logger for tracing</param>
        public SqlLockingProvider(INotifier notifier, IEventSubscriber eventSubscriber, ITaskManager taskManager, ISqlLockRepository lockRepository, IOptionsMonitor<SqlLockingProviderOptions> options, ILoggerFactory loggerFactory = null, ILogger<SqlLockingProvider> logger = null)
        {
            _notifier = notifier.ValidateArgument(nameof(notifier));
            _eventSubscriber = eventSubscriber.ValidateArgument(nameof(eventSubscriber));
            _taskManager = taskManager.ValidateArgument(nameof(taskManager));
            _lockRepository = lockRepository.ValidateArgument(nameof(lockRepository));
            _logger = logger;
            _loggerFactory = loggerFactory;
            OptionsMonitor = options.ValidateArgument(nameof(options));
            // Triggers validation
            _ = OptionsMonitor.CurrentValue;

            // Start workers
            taskManager.TrySchedule(this, MaintenanceWorkerName, RunMaintenanceAsync, x => x.ExecuteFirst(() => _logger.Debug($"Running maintenance in <{OptionsMonitor.CurrentValue.MaintenanceInterval}>"))
                                                                                            .DelayStartBy(() => OptionsMonitor.CurrentValue.MaintenanceInterval)
                                                                                            .WithManagedOptions(ManagedTaskOptions.KeepRunning));
            taskManager.TrySchedule(this, AssignmentWorkerName, AssignPendingRequestsAsync, x => x.ExecuteFirst(() => _logger.Debug($"Assigning pending requests in <{OptionsMonitor.CurrentValue.RequestAssignmentInterval}>"))
                                                                                                  .DelayStartBy(() => OptionsMonitor.CurrentValue.RequestAssignmentInterval)
                                                                                                  .WithManagedOptions(ManagedTaskOptions.KeepRunning)
                                                                                                  .ContinueWith((m, t, o, c) => m.TrySchedule(this, CompletionWorkerName, CompletePendingRequestsAsync))
                                                                                                  );
        }

        /// <summary>
        /// Proxy constructor.
        /// </summary>
        protected SqlLockingProvider()
        {

        }

        #region Provider
        /// <inheritdoc/>
        public async virtual Task<ILockInfo> GetAsync(string resource, CancellationToken token = default)
        {
            resource.ValidateArgument(nameof(resource));

            _logger.Log($"Fetching lock state on resource <{resource}>");
            SqlLock sqlLock;
            await using (var transaction = await _lockRepository.CreateTransactionAsync(token).ConfigureAwait(false))
            {
                sqlLock = await _lockRepository.GetLockByResourceAsync(transaction, resource, true, token).ConfigureAwait(false);
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
                sqlLock = await _lockRepository.TryLockAsync(transaction, resource, requester, expiryTime.HasValue ? DateTime.Now.Add(expiryTime.Value) : (DateTime?)null, token).ConfigureAwait(false);
                await transaction.CommitAsync(token).ConfigureAwait(false);
            }

            // Check if lock was placed by requester
            if (sqlLock == null) throw new InvalidOperationException($"{nameof(ISqlLockRepository.TryLockAsync)} returned null");
            if (requester.Equals(sqlLock.LockedBy, StringComparison.OrdinalIgnoreCase))
            {
                _logger.Log($"Resource <{resource}> now locked by <{requester}>");
                return new LockResult(new AcquiredSqlLock(sqlLock, this, keepAlive, expiryTime.HasValue ? expiryTime.Value : TimeSpan.Zero, _logger));
            }
            else
            {
                _logger.Log($"Could not assign lock on resource <{resource}> to <{requester}> because it is currently held by <{sqlLock.LockedBy}> and there are <{sqlLock.PendingRequests}> pending requests");
                return new LockResult(sqlLock);
            }
        }
        /// <inheritdoc/>
        public async virtual Task<IPendingLockRequest> LockAsync(string resource, string requester, TimeSpan? expiryTime = null, bool keepAlive = false, TimeSpan? timeout = null, CancellationToken token = default)
        {
            resource.ValidateArgumentNotNullOrWhitespace(nameof(resource));
            requester.ValidateArgumentNotNullOrWhitespace(nameof(requester));

            _logger.Log($"Attempting to lock resource <{resource}> for <{requester}>");

            IPendingLockRequest request;
            await using (var transaction = await _lockRepository.CreateTransactionAsync(token).ConfigureAwait(false))
            {
                var sqlLock = await _lockRepository.TryLockAsync(transaction, resource, requester, expiryTime.HasValue ? DateTime.Now.Add(expiryTime.Value) : (DateTime?)null, token).ConfigureAwait(false);

                // Check if lock was placed by requester
                if (sqlLock == null) throw new InvalidOperationException($"{nameof(ISqlLockRepository.TryLockAsync)} returned null");
                // Lock acquired
                if (requester.Equals(sqlLock.LockedBy, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.Log($"Resource <{resource}> now locked by <{requester}>");
                    await transaction.CommitAsync(token).ConfigureAwait(false);
                    var @lock = new AcquiredSqlLock(sqlLock, this, keepAlive, expiryTime.HasValue ? expiryTime.Value : TimeSpan.Zero, _loggerFactory?.CreateLogger<AcquiredSqlLock>());
                    request = new CompletedRequest(@lock) { Requester = requester, Resource = resource, ExpiryTime = expiryTime?.TotalSeconds, KeepAlive = keepAlive, Timeout = timeout.HasValue ? DateTime.Now.Add(timeout.Value) : (DateTime?)null, CreatedAt = DateTime.Now };
                }
                // Resource is not free or has pending requests. Create request
                else
                {
                    _logger.Log($"Could not assign lock on resource <{resource}> to <{requester}> because it is currently held by <{sqlLock.LockedBy}> and there are <{sqlLock.PendingRequests}> pending requests. Creating lock request");
                    var sqlLockRequest = await _lockRepository.CreateRequestAsync(transaction, new SqlLockRequest() { Requester = requester, Resource = resource, ExpiryTime = expiryTime?.TotalSeconds, KeepAlive = keepAlive, Timeout = timeout.HasValue ? DateTime.Now.Add(timeout.Value) : (DateTime?)null, CreatedAt = DateTime.Now }, token).ConfigureAwait(false);

                    _logger.Log($"Created lock request <{sqlLockRequest.Id}> for <{sqlLockRequest.Requester}> on resource <{sqlLockRequest.Resource}>");
                    await transaction.CommitAsync(token).ConfigureAwait(false);

                    var pendingRequest = new PendingLockRequest(sqlLockRequest, timeout, token);
                    request = pendingRequest;

                    // Track request
                    lock (_pendingRequests)
                    {
                        _pendingRequests.Add(pendingRequest);
                    }
                }
            }

            return request;
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
                sqlLock = await _lockRepository.GetLockByResourceAsync(transaction, resource, false, token).ConfigureAwait(false);
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

                // Raise event that lock was unlocked
                _ = await _notifier.RaiseEventAsync(this, new ResourceUnlockedEvent(resource), x => x.WithOptions(EventOptions.FireAndForget | EventOptions.AllowParallelExecution), token);

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
                if (canThrow) throw new StaleLockException(requester, new SqlLock() { Resource = resource, LockedBy = requester });
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
        #endregion

        #region Workers
        private async Task RunMaintenanceAsync(CancellationToken token)
        {
            using (_logger.TraceMethod(this))
            {
                try
                {
                    // Refresh options
                    var options = OptionsMonitor.CurrentValue;

                    // Cleanup old locks
                    if (!options.IsCleanupEnabled) return;
                    _logger.Log($"Running maintenance on lock table");
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
                                return;
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

                            // Cleanup inactive locks
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
                    return;
                }
            }
        }
        private async Task AssignPendingRequestsAsync(CancellationToken token)
        {
            using (Helper.Time.CaptureDuration(x => TraceTime("Assign pending requests", x)))
            {
                try
                {
                    _logger.Log($"Worker trying to assign pending requests");

                    lock (_pendingRequests)
                    {
                        if (!_pendingRequests.HasValue())
                        {
                            _logger.Log($"There are no pending requests to assign. Sleeping");
                            return;
                        }
                    }

                    await using (var transaction = await _lockRepository.CreateTransactionAsync(token).ConfigureAwait(false))
                    {
                        await _lockRepository.TryAssignLockRequestsAsync(transaction, token);

                        await transaction.CommitAsync(token).ConfigureAwait(false);
                    }
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    _logger.Log($"Error occured while assigning pending requests", ex);
                    return;
                }
            }
        }
        private async Task CompletePendingRequestsAsync(CancellationToken token)
        {
            using (Helper.Time.CaptureDuration(x => TraceTime("Completing pending requests", x)))
            {
                try
                {
                    _logger.Log($"Worker trying to complete pending requests");

                    var queue = new ConcurrentBag<PendingLockRequest>();
                    List<long> requestsToDelete = new List<long>();

                    // Get locks to check
                    lock (_pendingRequests)
                    {
                        if (!_pendingRequests.HasValue())
                        {
                            _logger.Log($"There are no pending requests to complete. Sleeping");
                            return;
                        }

                        foreach (var request in _pendingRequests)
                        {
                            if (request.Callback.IsCompleted)
                            {
                                _pendingRequests.Remove(request);
                                requestsToDelete.Add(request.Request.Id);
                            }
                            else queue.Add(request);
                        }
                    }

                    _logger.Log($"Got <{queue.Count}> pending requests to check");
                    while (queue.Count > 0)
                    {
                        // Take limited set of locks to check
                        HashSet<PendingLockRequest> pendingLockRequests = new HashSet<PendingLockRequest>();
                        while (pendingLockRequests.Count < OptionsMonitor.CurrentValue.RequestCheckLimit && queue.TryTake(out var item))
                        {
                            pendingLockRequests.Add(item);
                        }

                        _logger.Debug($"Checking the next <{pendingLockRequests.Count}> pending requests. Fetching their state");

                        // Gather the state of the locks
                        SqlLockRequest[] sqlLockRequests = null;
                        await using (var transaction = await _lockRepository.CreateTransactionAsync(token).ConfigureAwait(false))
                        {
                            sqlLockRequests = await _lockRepository.GetAllLockRequestsByIdsAsync(transaction, pendingLockRequests.Select(x => x.Request.Id).ToArray(), token);
                        }

                        _logger.Debug($"Comparing lock state to <{pendingLockRequests.Count}> pending requests");
                        // Check current state for each lock
                        foreach (var pendingLockRequest in pendingLockRequests)
                        {
                            _logger.Debug($"Comparing state for request <{pendingLockRequest.Request.Id}>");
                            var sqlLockRequest = sqlLockRequests.FirstOrDefault(x => x.Id.Equals(pendingLockRequest.Request.Id));

                            // Removed
                            if (sqlLockRequest == null)
                            {
                                pendingLockRequest.Set(new OperationCanceledException($"Request <{pendingLockRequest.Request.Id}> placed by <{pendingLockRequest.Request.Requester}> on resource <{pendingLockRequest.Request.Resource}> was removed"));
                                lock (_pendingRequests)
                                {
                                    _pendingRequests.Remove(pendingLockRequest);
                                }
                                _logger.Warning($"Request <{pendingLockRequest.Request.Id}> placed by <{pendingLockRequest.Request.Requester}> on resource <{pendingLockRequest.Request.Resource}> was removed");
                                pendingLockRequest.Dispose();
                            }
                            // Lock was assigned
                            else if (sqlLockRequest.IsAssigned)
                            {
                                var @lock = new AcquiredSqlLock(sqlLockRequest.Lock, this, pendingLockRequest.Request.KeepAlive, pendingLockRequest.Request.ExpiryTime.HasValue ? TimeSpan.FromSeconds(pendingLockRequest.Request.ExpiryTime.Value) : TimeSpan.Zero, _loggerFactory?.CreateLogger<AcquiredSqlLock>());
                                pendingLockRequest.Set(@lock);

                                requestsToDelete.Add(pendingLockRequest.Request.Id);
                                lock (_pendingRequests)
                                {
                                    _pendingRequests.Remove(pendingLockRequest);
                                }
                                _logger.Log($"Completed request <{pendingLockRequest.Request.Id}> placed by <{pendingLockRequest.Request.Requester}> on resource <{pendingLockRequest.Request.Resource}>");
                                pendingLockRequest.Dispose();
                            }
                        }

                        // Delete completed
                        if (requestsToDelete.HasValue())
                        {
                            _logger.Debug($"Removing the state of <{requestsToDelete.Count}> requests");
                            await using (var transaction = await _lockRepository.CreateTransactionAsync(token).ConfigureAwait(false))
                            {
                                await _lockRepository.DeleteAllRequestsById(transaction, requestsToDelete.ToArray(), token).ConfigureAwait(false);

                                await transaction.CommitAsync(token).ConfigureAwait(false);
                            }
                            requestsToDelete.Clear();
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    _logger.Log($"Error occured while assigning pending requests", ex);
                    return;
                }
            }
        }
        private void TraceTime(string action, TimeSpan duration)
        {
            var logLevel = LogLevel.Trace;
            if(duration > OptionsMonitor.CurrentValue.PerformanceErrorDurationThreshold)
            {
                logLevel = LogLevel.Error;
            }
            else if (duration > OptionsMonitor.CurrentValue.PerformanceWarningDurationThreshold)
            {
                logLevel = LogLevel.Warning;
            }

            _logger.Log(logLevel, $"Executed action <{action}> in <{duration}>");
        }
        #endregion

        /// <inheritdoc/>
        public async virtual ValueTask DisposeAsync()
        {
            // Exit if already disposed
            if (IsDisposed.HasValue && IsDisposed.Value) return;

            using (new ExecutedAction(x => IsDisposed = x))
            {
                List<Exception> exceptions = new List<Exception>();
                _logger.Log($"Disposing. Stopping workers");

                // Cancel workers
                try
                {
                    await _taskManager.StopAllForAsync(this);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }

                // Dispose pending requests
                while (_pendingRequests.HasValue())
                {
                    HashSet<PendingLockRequest> lockRequests = null;
                    lock (_pendingRequests)
                    {
                        lockRequests = _pendingRequests.ToHashSet();
                        _pendingRequests.Clear();
                    }

                    lockRequests.Execute(x => x.Dispose(), (x, e) => exceptions.Add(e));
                }

                if (exceptions.HasValue()) throw new AggregateException(exceptions);
            }
        }

        #region Classes
        private class CompletedRequest : SqlLockRequest, IPendingLockRequest
        {
            public CompletedRequest(AcquiredSqlLock @lock)
            {
                Callback = Task.FromResult<ILock>(@lock);
            }

            public Task<ILock> Callback { get; }
        }

        private class PendingLockRequest : IPendingLockRequest, IDisposable
        {
            // Fields
            private readonly TaskCompletionSource<ILock> _taskSource = new TaskCompletionSource<ILock>(TaskCreationOptions.RunContinuationsAsynchronously);
            private readonly CancellationTokenSource _timeoutTokenSource;
            private readonly CancellationTokenRegistration _tokenRegistration;

            // Properties
            public SqlLockRequest Request { get; }
            public TimeSpan? Timeout { get; }
            public CancellationToken CancellationToken { get; }

            /// <summary>
            /// Task that is awaited by the caller when waiting on a lock to be placed.
            /// </summary>
            public Task<ILock> Callback => _taskSource.Task;

            /// <inheritdoc/>
            string ILockRequest.Resource => Request.Resource;
            /// <inheritdoc/>
            string ILockRequest.Requester => Request.Requester;
            /// <inheritdoc/>
            TimeSpan? ILockRequest.ExpiryTime => Request.CastTo<ILockRequest>().ExpiryTime;
            /// <inheritdoc/>
            bool ILockRequest.KeepAlive => Request.KeepAlive;
            /// <inheritdoc/>
            DateTime? ILockRequest.Timeout => Request.Timeout;
            /// <inheritdoc/>
            DateTime ILockRequest.CreatedAt => Request.CreatedAt;

            public PendingLockRequest(SqlLockRequest sqlLockRequest, TimeSpan? timeout, CancellationToken cancellationToken)
            {
                Request = sqlLockRequest.ValidateArgument(nameof(sqlLockRequest));
                Timeout = timeout;
                if (timeout.HasValue)
                {
                    _timeoutTokenSource = new CancellationTokenSource();
                    Task.Run(async () =>
                    {
                        await Helper.Async.Sleep(timeout.Value, _timeoutTokenSource.Token);
                        if (_timeoutTokenSource.IsCancellationRequested) return;
                        Set(new LockTimeoutException(Request.Requester, Request.Resource, timeout.Value));
                    });
                }

                _tokenRegistration = cancellationToken.Register(() =>
                {
                    _timeoutTokenSource?.Cancel();
                    SetCancelled();
                });
                CancellationToken = cancellationToken;
                cancellationToken.ThrowIfCancellationRequested();
            }

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

            public void SetCancelled() => Set(new OperationCanceledException($"Request <{Request.Id}> placed by <{Request.Requester}> on resource <{Request.Resource}> was cancelled by caller"));

            /// <inheritdoc/>
            public void Dispose()
            {
                _tokenRegistration.Dispose();
                _timeoutTokenSource?.Dispose();

                SetCancelled();
            }
        }
        #endregion
    }
}
