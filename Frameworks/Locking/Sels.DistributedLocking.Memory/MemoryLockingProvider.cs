using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Sels.Core;
using Sels.Core.Components.Scope.Actions;
using Sels.Core.Dispose;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Linq;
using Sels.Core.Extensions.Logging.Advanced;
using Sels.Core.Extensions.Reflection;
using Sels.DistributedLocking.Abstractions.Extensions;
using Sels.DistributedLocking.Abstractions.Models;
using Sels.DistributedLocking.Provider;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sels.DistributedLocking.Memory
{
    /// <summary>
    /// Provides process wide distributed locks by storing the locks in memory and making use of thread locks to handle concurrency.
    /// </summary>
    public class MemoryLockingProvider : ILockingProvider, IAsyncExposedDisposable
    {
        // Fields
        private readonly Dictionary<string, MemoryLockInfo> _locks = new Dictionary<string, MemoryLockInfo>(StringComparer.OrdinalIgnoreCase);
        private readonly ILogger<MemoryLockingProvider> _logger;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _cleanupTask;
        private IDisposable _optionsMonitor;

        // Properties
        /// <summary>
        /// Contains configuration for the currnt instance.
        /// </summary>
        public IOptionsMonitor<MemoryLockingProviderOptions> OptionsMonitor { get; }

        /// <summary>
        /// Indicates that the current instance is currently running cleanup on inactive locks.
        /// </summary>
        public bool IsRunningCleanup { get; private set; }
        /// <inheritdoc/>
        public bool? IsDisposed { get; private set; }

        /// <inheritdoc cref="MemoryLockingProvider"/>
        /// <param name="options"><inheritdoc cref="Options"/></param>
        /// <param name="logger">Optional logger for tracing</param>
        public MemoryLockingProvider(IOptionsMonitor<MemoryLockingProviderOptions> options, ILogger<MemoryLockingProvider> logger = null)
        {
            OptionsMonitor = options.ValidateArgument(nameof(options));
            _logger = logger;

            // Start cleanup task
            _cancellationTokenSource = new CancellationTokenSource();
            if(OptionsMonitor.CurrentValue.IsCleanupEnabled) _cleanupTask = RunCleanupDuringLifetime(_cancellationTokenSource.Token);

            // Monitor options for changes
            _optionsMonitor = options.OnChange((o, n) =>
            {
                // Start if not running
                if((_cleanupTask == null || _cleanupTask.IsCompleted) && o.IsCleanupEnabled)
                {
                    _cleanupTask = RunCleanupDuringLifetime(_cancellationTokenSource.Token);
                    return;
                }

                // Stop if running
                if(_cleanupTask != null && !o.IsCleanupEnabled)
                {
                    _cancellationTokenSource.Cancel();
                    _cleanupTask.ConfigureAwait(false).GetAwaiter().GetResult();
                    _cancellationTokenSource.Dispose();
                    _cancellationTokenSource = new CancellationTokenSource();
                    return;
                }
            });
        }

        /// <inheritdoc/>
        public virtual Task<ILockResult> TryLockAsync(string resource, string requester, TimeSpan? expiryTime = null, bool keepAlive = false, CancellationToken token = default)
        {
            using (_logger.TraceMethod(this))
            {
                resource.ValidateArgumentNotNullOrWhitespace(nameof(resource));
                requester.ValidateArgumentNotNullOrWhitespace(nameof(requester));

                return Task.FromResult(TryLock(resource, requester, expiryTime, keepAlive, token));
            }
        }

        /// <inheritdoc/>
        public virtual ILockResult TryLock(string resource, string requester, TimeSpan? expiryTime = null, bool keepAlive = false, CancellationToken token = default)
        {
            using (_logger.TraceMethod(this))
            {
                resource.ValidateArgumentNotNullOrWhitespace(nameof(resource));
                requester.ValidateArgumentNotNullOrWhitespace(nameof(requester));

                _logger.Log($"Attempting to lock resource <{resource}> requested by <{requester}>");

                var memoryLock = GetLockOrSet(resource);
                lock (memoryLock.SyncRoot)
                {
                    // Check if we can lock
                    var canLock = false;
                    if (memoryLock.LockedBy == null)
                    {
                        _logger.Debug($"Resource <{resource}> is not locked by anyone. Lock can be acquired by <{requester}>");
                        canLock = true;
                    }
                    else if (memoryLock.ExpiryDate.HasValue && memoryLock.ExpiryDate.Value < DateTime.Now)
                    {
                        _logger.Debug($"Resource <{resource}> is currently held by <{memoryLock.LockedBy}> but expired at <{memoryLock.ExpiryDate}>. Lock will be transfered to <{requester}>");
                        canLock = true;
                    }

                    // Check if there are pending requests first as they have priority
                    if (canLock)
                    {
                        _logger.Debug($"Checking if there are pending requests on resource <{resource}>");
                        if (TryAssignPendingRequest(resource))
                        {
                            _logger.Log($"Resource was not locked but had pending requests. Lock is now assigned to <{memoryLock.LockedBy}>");
                            return new LockResult(memoryLock);
                        }
                    }
                    else if (memoryLock.LockedBy.Equals(requester, StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.Debug($"Resource <{resource}> is already locked by requester <{requester}>. Lock will be updated");
                        canLock = true;
                    }

                    // Create lock
                    if (canLock)
                    {
                        memoryLock.LockedBy = requester;
                        memoryLock.ExpiryDate = expiryTime.HasValue ? DateTime.Now.Add(expiryTime.Value) : (DateTime?)null;
                        memoryLock.LastLockDate = DateTime.Now;
                        memoryLock.LockedAt = DateTime.Now;
                        _logger.Log($"Resource <{resource}> is now held by <{requester}>");
                        var lockObject = new MemoryLock(this, memoryLock, keepAlive, expiryTime ?? TimeSpan.Zero, _logger);
                        return new LockResult(lockObject);
                    }

                    _logger.Log($"Lock on resource <{resource}> could not be acquired by <{requester}>. Currently held by <{memoryLock.LockedBy}>");
                    return new LockResult(memoryLock);
                }
            }
        }

        /// <inheritdoc/>
        public virtual Task<ILock> LockAsync(string resource, string requester, TimeSpan? expiryTime = null, bool keepAlive = false, TimeSpan? timeout = null, CancellationToken token = default)
        {
            using (_logger.TraceMethod(this))
            {
                resource.ValidateArgumentNotNullOrWhitespace(nameof(resource));
                requester.ValidateArgumentNotNullOrWhitespace(nameof(requester));

                _logger.Log($"Waiting for resource <{resource}> to be locked by <{requester}>");
                var memoryLock = GetLockOrSet(resource);
                Task<ILock> lockTask = null;
                lock (memoryLock.SyncRoot)
                {
                    var lockResult = TryLock(resource, requester, expiryTime, keepAlive);
                    if (!lockResult.Success)
                    {
                        _logger.Log($"Resource <{resource}> is already locked. Creating lock request for <{requester}>");

                        var existing = memoryLock.Requests.FirstOrDefault(x => x.Requester.Equals(requester));

                        if(existing == null)
                        {
                            // Create and store request
                            var request = new MemoryLockRequest(requester, memoryLock, timeout, _logger, token)
                            {
                                ExpiryTime = expiryTime,
                                KeepAlive = keepAlive
                            };
                            memoryLock.Requests.Enqueue(request);

                            _logger.Log($"Lock request created for requester <{requester}> on resource <{resource}>");
                            lockTask = request.CallbackTask;
                        }
                        else
                        {
                            // Request already exists so no need to create another one
                            _logger.Warning($"A request on resource <{resource}> was already placed by <{requester}>. Not creating a new request");
                            lockTask = existing.CallbackTask;
                        }
                    }
                    else
                    {
                        _logger.Log($"Resource <{resource}> was not locked and is now held by <{requester}>");
                        lockTask = Task.FromResult(lockResult.AcquiredLock);
                    }
                }
                return lockTask;
            }
        }
        /// <inheritdoc/>
        public virtual Task<ILockInfo> GetAsync(string resource, CancellationToken token = default)
        {
            using (_logger.TraceMethod(this))
            {
                _logger.Log($"Returning lock on <{resource}>");
                var memoryLock = GetLockOrSet(resource);
                lock (memoryLock.SyncRoot)
                {
                    return Task.FromResult(new MemoryLockInfo(memoryLock).CastTo<ILockInfo>());
                }
            }
        }
        /// <inheritdoc/>
        public virtual Task<ILockRequest[]> GetPendingRequestsAsync(string resource, CancellationToken token = default)
        {
            using (_logger.TraceMethod(this))
            {
                resource.ValidateArgumentNotNullOrWhitespace(nameof(resource));

                _logger.Log($"Returning pending requests on resource <{resource}>");

                var memoryLock = GetLockOrSet(resource);
                lock (memoryLock.SyncRoot)
                {
                    return Task.FromResult(memoryLock.Requests.OrderBy(x => x.CreatedAt).Cast<ILockRequest>().ToArray());
                }
            }
        }
        /// <inheritdoc/>
        public virtual Task<ILockQueryResult> QueryAsync(Action<ILockQueryCriteria> searchCriteria, CancellationToken token = default)
        {
            using (_logger.TraceMethod(this))
            {
                searchCriteria.ValidateArgument(nameof(searchCriteria));

                var querySearchCriteria = new MemoryQuerySearchCriteria(searchCriteria);
                _logger.Log($"Querying locks");
                Dictionary<string, MemoryLockInfo>.ValueCollection locks = null;

                lock (_locks)
                {
                    locks = _locks.Values;
                }

                // Apply filter
                var filtered = querySearchCriteria.ApplyFilter(locks).ToArray();

                var total = filtered.Length;
                // Apply sorting
                var enumerator = querySearchCriteria.ApplySorting(filtered);

                // Apply pagination
                if (querySearchCriteria.Pagination.HasValue)
                {
                    var (page, pageSize) = querySearchCriteria.Pagination.Value;
                    enumerator = enumerator.Skip((page - 1) * pageSize).Take(pageSize);
                }

                // Return result
                var result = enumerator.Select(x =>
                {
                    var memoryLock = x.CastTo<MemoryLockInfo>();
                    lock (memoryLock.SyncRoot)
                    {
                        return new MemoryLockInfo(memoryLock);
                    }
                }).Cast<ILockInfo>().ToArray();

                _logger.Log($"Query returned <{result.Length}> results");
                return Task.FromResult<ILockQueryResult>(querySearchCriteria.Pagination.HasValue ? new LockQueryResult(result, querySearchCriteria.Pagination.Value.PageSize, total) : new LockQueryResult(result));
            }
        }
        /// <inheritdoc/>
        public virtual Task ForceUnlockAsync(string resource, bool removePendingRequests = false, CancellationToken token = default)
        {
            using (_logger.TraceMethod(this))
            {
                resource.ValidateArgumentNotNullOrWhitespace(nameof(resource));

                _logger.Warning($"Forcefully unlocking resource <{resource}>");

                var memoryLock = GetLockOrSet(resource);
                lock (memoryLock.SyncRoot)
                {
                    token.ThrowIfCancellationRequested();
                    // Remove any pending locks
                    if (removePendingRequests)
                    {
                        _logger.Warning($"Cancelling all lock requests on resource <{resource}>");
                        while (memoryLock.Requests.TryDequeue(out var request))
                        {
                            using (request)
                            {
                                request.AbortRequest(new TaskCanceledException($"Forcefully cancelled"));
                            }
                        }
                    }

                    // Set temp lock user so we can reuse Unlock logic
                    memoryLock.LockedBy = "SYS";
                    if (!Unlock(resource, "SYS")) throw new InvalidOperationException($"Did not expect force unlock to fail");
                }

                _logger.Warning($"Forcefully unlocked resource <{resource}>");
                return Task.CompletedTask;
            }
        }

        /// <summary>
        /// Checks if lock <paramref name="resource"/> is still held by <paramref name="requester"/>.
        /// </summary>
        /// <param name="resource">The lock to check</param>
        /// <param name="requester">Who is supposed to have the lock</param>
        /// <returns>True if lock <paramref name="resource"/> is still held by <paramref name="requester"/></returns>
        public virtual bool HasLock(string resource, string requester)
        {
            using (_logger.TraceMethod(this))
            {
                resource.ValidateArgumentNotNullOrWhitespace(nameof(resource));
                requester.ValidateArgumentNotNullOrWhitespace(nameof(requester));

                var memoryLock = GetLockOrSet(resource);
                lock (memoryLock.SyncRoot)
                {
                    var hasLock = memoryLock.HasLock(requester);
                    _logger.Debug($"Lock <{resource}> is {(hasLock ? "locked" : "not locked")} by <{requester}>");
                    return hasLock;
                }
            }
        }
        /// <summary>
        /// Extend expiry date on lock <paramref name="resource"/> by <paramref name="extendTime"/>.
        /// </summary>
        /// <param name="resource">The lock to extend</param>
        /// <param name="requester">Who is requesting the lock to be extended</param>
        /// <param name="extendTime">How much time to extend the expiry date with</param>
        /// <param name="lockInfo">The current state of the lock</param>
        /// <returns>True if the expiry date was extended, otherwise false</returns>
        public virtual bool TryExtend(string resource, string requester, TimeSpan extendTime, out ILockInfo lockInfo)
        {
            using (_logger.TraceMethod(this))
            {
                resource.ValidateArgumentNotNullOrWhitespace(nameof(resource));
                requester.ValidateArgumentNotNullOrWhitespace(nameof(requester));

                _logger.Log($"Trying to extend expiry date on lock <{resource}> requested by <{requester}>");
                var memoryLock = GetLockOrSet(resource);
                lockInfo = memoryLock;
                lock (memoryLock.SyncRoot)
                {
                    // Check if lock is being held
                    if(!ValidateLock(requester, memoryLock))
                    {
                        _logger.Warning($"Lock <{memoryLock.Resource}> is not held anymore by <{requester}>. Can't extend lock");
                        return false;
                    }

                    // Set new expiry date
                    memoryLock.ExpiryDate = memoryLock.ExpiryDate.HasValue ? memoryLock.ExpiryDate.Value.Add(extendTime) : DateTime.Now.Add(extendTime);
                    _logger.Log($"Expiry date for lock <{resource}> was set to <{memoryLock.ExpiryDate}> by <{requester}>");

                    return true;
                }
            }
        }

        /// <summary>
        /// Tries to unlock resource <paramref name="resource"/> held by <paramref name="requester"/>.
        /// </summary>
        /// <param name="resource">The lock to release</param>
        /// <param name="requester">Who is supposed to hold the lock</param>
        public virtual bool Unlock(string resource, string requester)
        {
            using (_logger.TraceMethod(this))
            {
                resource.ValidateArgumentNotNullOrWhitespace(nameof(resource));
                requester.ValidateArgumentNotNullOrWhitespace(nameof(requester));

                _logger.Log($"Trying to unlock resource <{resource}> requested by <{requester}>");

                var wasUnlocked = false;
                var memoryLock = GetLockOrSet(resource);
                lock (memoryLock.SyncRoot)
                {
                    try
                    {
                        // Check if lock is being held
                        if (!ValidateLock(requester, memoryLock))
                        {
                            _logger.Warning($"Lock <{memoryLock.Resource}> is not held anymore by <{requester}>. Can't unlock");
                            wasUnlocked = false;
                        }
                        else
                        {
                            // Release the lock
                            memoryLock.LockedBy = null;
                            memoryLock.LockedAt = null;
                            memoryLock.ExpiryDate = null;

                            _logger.Log($"Lock <{resource}> was unlocked by <{requester}>");
                            wasUnlocked = true;
                        }
                    }
                    finally
                    {
                        TryAssignPendingRequest(resource);
                    }
                    
                    return wasUnlocked;
                }
            }
        }

        /// <summary>
        /// Tries to assign the next pending lock request on <paramref name="resource"/>.
        /// </summary>
        /// <param name="resource">The resource to check the pending requests for</param>
        public virtual bool TryAssignPendingRequest(string resource)
        {
            using (_logger.TraceMethod(this))
            {
                resource.ValidateArgumentNotNullOrWhitespace(nameof(resource));

                var memoryLock = GetLockOrSet(resource);
                var tasks = new List<Task>();

                lock (memoryLock.SyncRoot)
                {
                    if (!memoryLock.CanbeLocked()) {
                        _logger.Debug($"Can't assign requests on resource <{resource}> because lock can't be locked");
                        return false;
                    }

                    _logger.Debug($"Checking if there are pending lock requests on resource <{resource}> to assign");
                    var currentHolder = memoryLock.LockedBy;
                    var currentExpiryDate = memoryLock.ExpiryDate;
                    var currentLastLockDate = memoryLock.LastLockDate;
                    var currentLockDate = memoryLock.LastLockDate;

                    // Check for pending lock requests so we can assign to the next requester
                    while (memoryLock.Requests.TryDequeue(out var request))
                    {
                        using (request)
                        {
                            _logger.Debug($"Got pending lock request on resource <{resource}> requested by <{request.Requester}> created at <{request.CreatedAt}>");
                            // Skip finished requests
                            if (request.IsCompleted) {
                                _logger.Warning($"Pending lock request on resource <{resource}> requested by <{request.Requester}> created at <{request.CreatedAt}> already completed. Skipping");
                                continue;
                            }

                            memoryLock.LockedBy = request.Requester;
                            memoryLock.ExpiryDate = request.ExpiryTime.HasValue ? DateTime.Now.Add(request.ExpiryTime.Value) : (DateTime?)null;
                            memoryLock.LastLockDate = DateTime.Now;
                            memoryLock.LockedAt = DateTime.Now;
                            var @lock = new MemoryLock(this, memoryLock, request.KeepAlive, request.ExpiryTime ?? TimeSpan.Zero, _logger);
                            if (request.AssignLock(@lock))
                            {
                                _logger.Log($"Lock on resource <{resource}> was assigned to <{request.Requester}> through request created at <{request.CreatedAt}>");
                                return true;
                            }
                            else
                            {
                                _logger.Warning($"Could not assign lock <{resource}> to <{request.Requester}>. Request will be aborted");

                                // Reset old state on lock
                                memoryLock.LockedBy = currentHolder;
                                memoryLock.ExpiryDate = currentExpiryDate;
                                memoryLock.LastLockDate = currentLastLockDate;
                                memoryLock.LockedAt = currentLockDate;

                                // We don't really care for exceptions here
                                _ = @lock.DisposeAsync();
                            }
                        }
                    }
                }
                _logger.Debug($"No pending lock requests assigned for resource <{resource}>");
                return false;    
            }
        }

        /// <summary>
        /// Event handler called when lock <paramref name="resource"/> expires.
        /// </summary>
        /// <param name="resource">The resource that expired</param>
        public virtual void OnLockExpired(string resource)
        {
            using (_logger.TraceMethod(this))
            {
                resource.ValidateArgumentNotNullOrWhitespace(nameof(resource));
                _logger.Log($"Lock on resource <{resource}> expired. Trying to assign request if any are pending");

                var memoryLock = GetLockOrSet(resource);
                lock (memoryLock.SyncRoot)
                {
                    if (TryAssignPendingRequest(resource))
                    {
                        _logger.Log($"Request was assigned to expired lock on resource <{resource}>");
                    }
                }               
            }
        }

        private MemoryLockInfo GetLockOrSet(string resource, [CallerMemberName] string caller = null)
        {
            resource.ValidateArgumentNotNullOrWhitespace(nameof(resource));

            _logger.Debug($"Trying to get lock info for resource <{resource}> for caller <{caller}>");

            lock (_locks)
            {
                var lockInfo = _locks.TryGetOrSet(resource, () =>
                {
                    _logger.Debug($"Created new lock info for resource <{resource}> for caller <{caller}>");
                    return new MemoryLockInfo(resource);
                });

                _logger.Debug($"Got lock info for resource <{lockInfo.Resource}> with <{lockInfo.PendingRequests}> pending requests for caller <{caller}>");
                return lockInfo;
            }
        }

        private bool ValidateLock(string requester, MemoryLockInfo memoryLock)
        {
            requester.ValidateArgumentNotNullOrWhitespace(nameof(requester));
            memoryLock.ValidateArgument(nameof(memoryLock));

            lock (memoryLock.SyncRoot)
            {
                var hasLock = HasLock(memoryLock.Resource, requester);
                if (!hasLock && OptionsMonitor.CurrentValue.ThrowOnStaleLock)
                {
                    if (memoryLock.LockedBy.HasValue()) throw new ResourceAlreadyLockedException(requester, memoryLock);
                    throw new StaleLockException(requester, memoryLock);
                }
                return hasLock;
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
                        sleepTime = options.CleanupInterval.TotalMilliseconds.ChangeType<int>();
                        if (!options.IsCleanupEnabled)
                        {
                            _logger.Warning($"Cleanup task was started but cleanup is disabled. Stopping");
                            return;
                        }
                        _logger.Debug($"Running cleanup in <{options.CleanupInterval}ms>");
                        await Helper.Async.Sleep(sleepTime, token).ConfigureAwait(false);
                        if (token.IsCancellationRequested)
                        {
                            _logger.Debug($"Cleanup task cancelled");
                            return;
                        }
                        // Refresh options
                        options = OptionsMonitor.CurrentValue;

                        using (new InProcessAction(x => IsRunningCleanup = x))
                        {
                            // Run cleanup
                            lock (_locks)
                            {
                                _logger.Log($"Running cleanup using method <{options.CleanupMethod}> using the configured amount of <{options.CleanupAmount}>. There are currently <{_locks.Count}> known locks with <{_locks.Select(x => x.Value).Sum(x => x.PendingRequests)}> pending locking requests");
                            }

                            using (_logger.TraceAction(LogLevel.Debug, $"Lock cleanup"))
                            {
                                // Check if cleanup needed
                                bool cleanupNeeded = true;
                                switch (options.CleanupMethod)
                                {
                                    case MemoryLockCleanupMethod.Amount:
                                        lock (_locks)
                                        {
                                            cleanupNeeded = _locks.Count > options.CleanupAmount.Value;
                                            if (cleanupNeeded) _logger.Log($"Lock amount is higher than the configured amount of <{options.CleanupAmount}>. Current lock count is <{_locks.Count}>. Triggering cleanup");
                                        }
                                        break;
                                    case MemoryLockCleanupMethod.ProcessMemory:
                                        using (var process = System.Diagnostics.Process.GetCurrentProcess())
                                        {
                                            cleanupNeeded = process.WorkingSet64 > options.CleanupAmount.Value;
                                            if (cleanupNeeded) _logger.Log($"Current process memory is higher than the configured amount of <{options.CleanupAmount}>. Current process memory is <{process.WorkingSet64} bytes>. Triggering cleanup");
                                        }
                                        break;
                                }

                                if (!cleanupNeeded)
                                {
                                    _logger.Log($"No cleanup of locks needed");
                                    continue;
                                }

                                // Get delegates based on method if a lock can be cleaned up
                                Func<MemoryLockInfo, bool> canCleanupPredicate = null;
                                switch (options.CleanupMethod)
                                {
                                    case MemoryLockCleanupMethod.Time:
                                        canCleanupPredicate = new Func<MemoryLockInfo, bool>(x => !x.LastLockDate.HasValue || (DateTime.Now - x.LastLockDate.Value).TotalMilliseconds >= options.CleanupAmount);
                                        break;
                                }
                                canCleanupPredicate = canCleanupPredicate != null ? canCleanupPredicate : new Func<MemoryLockInfo, bool>(x => true);

                                if (token.IsCancellationRequested)
                                {
                                    _logger.Debug($"Cleanup task cancelled");
                                    return;
                                }

                                // Get all locks to cleanup and remove them if applicable
                                lock (_locks)
                                {
                                    var locks = _locks.Select(x => x.Value).ToArray();

                                    foreach (var @lock in locks)
                                    {
                                        if (token.IsCancellationRequested)
                                        {
                                            _logger.Debug($"Cleanup task cancelled");
                                            return;
                                        }

                                        var couldLock = Helper.Lock.TryLockAndExecute(@lock.SyncRoot, () =>
                                        {
                                            // Check if lock itself can be removed
                                            if (@lock.LockedBy != null || !canCleanupPredicate(@lock))
                                            {
                                                _logger.Trace($"Lock <{@lock.Resource}> is not eligible for removal. Skipping cleanup");
                                                return;
                                            }

                                            // Check if the lock has pending requests
                                            if (!@lock.Requests.IsEmpty)
                                            {
                                                _logger.Trace($"Lock <{@lock.Resource}> still has pending requests. Skipping cleanup");
                                                return;
                                            }

                                            // Remove lock
                                            _locks.Remove(@lock.Resource);
                                            _logger.Log($"Lock on resource <{@lock.Resource}> was inactive and has been removed from the provider");
                                        });

                                        if (!couldLock) _logger.Warning($"Could not get a thread lock on <{@lock.Resource}>. Lock is probably still active");
                                    }
                                }
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        _logger.Log($"Error occured while running cleanup on memory locks", ex);
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

                // Stop monitoring for option changes
                _optionsMonitor?.Dispose();

                // Cancel cleanup task if it's running
                if (_cleanupTask != null)
                {
                    using (_logger.TraceAction(LogLevel.Debug, "Dispose: Cancel cleanup task"))
                    {
                        try
                        {
                            _cancellationTokenSource.Cancel();
                            await _cleanupTask.ConfigureAwait(false);
                            _cancellationTokenSource.Dispose();
                        }
                        catch (Exception ex)
                        {
                            exceptions.Add(ex);
                            _logger.Log($"Could not properly stop cleanup task", ex);
                        }
                    }
                }

                // Cancel any pending requests so callers can return
                using (_logger.TraceAction(LogLevel.Debug, "Dispose: Cancel pending lock requests"))
                {
                    lock (_locks)
                    {
                        foreach (var @lock in _locks.Values.Where(x => !x.Requests.IsEmpty))
                        {
                            lock (@lock.SyncRoot)
                            {
                                foreach (var request in @lock.Requests)
                                {
                                    try
                                    {
                                        using (request)
                                        {
                                            var message = $"Cancelled request placed by <{request.Requester}> on resource <{request.Resource}>";
                                            _logger.Log(message);
                                            request.AbortRequest(new OperationCanceledException(message));
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        exceptions.Add(ex);
                                        _logger.Log($"Could not properly cancel locking request made by <{request.Requester}> on resource <{request.Resource}>", ex);
                                    }
                                }
                            }
                        }
                    }
                }

                if (exceptions.HasValue()) throw new AggregateException(exceptions); 
            }
        }
    }
}
