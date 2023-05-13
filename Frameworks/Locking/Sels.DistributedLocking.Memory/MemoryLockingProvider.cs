using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Sels.Core;
using Sels.Core.Components.Scope.Actions;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Logging.Advanced;
using Sels.Core.Extensions.Object;
using Sels.Core.Extensions.Reflection;
using Sels.DistributedLocking.Provider;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sels.DistributedLocking.Memory
{
    /// <summary>
    /// Provides application wide distributed locks by storing the locks in memory and making use of thread locks to handle concurrency.
    /// </summary>
    public class MemoryLockingProvider : ILockingProvider, IAsyncDisposable
    {
        // Fields
        private readonly Dictionary<string, MemoryLockInfo> _locks = new Dictionary<string, MemoryLockInfo>(StringComparer.OrdinalIgnoreCase);
        private readonly ILogger<MemoryLockingProvider> _logger;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _cleanupTask;

        // Properties
        /// <summary>
        /// Contains configuration for the currnt instance.
        /// </summary>
        public IOptionsMonitor<MemoryLockingProviderOptions> OptionsMonitor { get; }

        /// <summary>
        /// Indicates that the current instance is currently running cleanup on inactive locks.
        /// </summary>
        public bool IsRunningCleanup { get; private set; }

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
            options.OnChange((o, n) =>
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
        public Task<bool> TryLockAsync(string resource, string requester, out ILock lockObject, TimeSpan? expiryTime = null, bool keepAlive = false, CancellationToken token = default)
        {
            using (_logger.TraceMethod(this))
            {
                resource.ValidateArgumentNotNullOrWhitespace(nameof(resource));
                requester.ValidateArgumentNotNullOrWhitespace(nameof(requester));

                _logger.Log($"Attempting to lock resource <{resource}> requested by <{requester}>");

                lockObject = null;
                var memoryLockInfo = GetLockOrSet(resource);
                lock (memoryLockInfo)
                {
                    // Check if we can lock
                    var canLock = false;
                    if(memoryLockInfo.LockedBy == null)
                    {
                        _logger.Debug($"Resource <{resource}> is not locked by anyone. Lock can be acquired by <{requester}>");
                        canLock = true;
                    }
                    else if (memoryLockInfo.ExpiryDate.HasValue && memoryLockInfo.ExpiryDate.Value < DateTime.Now)
                    {
                        _logger.Debug($"Resource <{resource}> is currently held by <{memoryLockInfo.LockedBy}> but expired at <{memoryLockInfo.ExpiryDate}>. Lock will be transfered to <{requester}>");
                        canLock = true;
                    }

                    // Check if there are pending requests first as they have priority
                    if (canLock)
                    {
                        _logger.Debug($"Checking if there are pending requests on resource <{resource}>");
                        if (TryAssignPendingRequest(resource))
                        {
                            _logger.Log($"Resource was not locked but had pending requests. Lock is now assigned to <{memoryLockInfo.LockedBy}>");
                            return Task.FromResult(false);
                        }
                    }
                    else if (memoryLockInfo.LockedBy.Equals(requester, StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.Debug($"Resource <{resource}> is already locked by requester <{requester}>. Lock will be updated");
                        canLock = true;
                    }

                    // Create lock
                    if (canLock)
                    {
                        memoryLockInfo.LockedBy = requester;
                        memoryLockInfo.ExpiryDate = expiryTime.HasValue ? DateTime.Now.Add(expiryTime.Value) : (DateTime?)null;
                        memoryLockInfo.LastLockDate = DateTime.Now;
                        memoryLockInfo.LockedAt = DateTime.Now;
                        _logger.Log($"Resource <{resource}> is now held by <{requester}>");
                        lockObject = new MemoryLock(this, memoryLockInfo, keepAlive, expiryTime ?? TimeSpan.Zero, _logger);
                        return Task.FromResult(true);
                    }

                    _logger.Log($"Lock on resource <{resource}> could not be acquired by <{requester}>. Currently held by <{memoryLockInfo.LockedBy}>");
                    return Task.FromResult(false);
                }
            }
        }
        /// <inheritdoc/>
        public Task<ILock> LockAsync(string resource, string requester, TimeSpan? expiryTime = null, bool keepAlive = false, TimeSpan? timeout = null, CancellationToken token = default)
        {
            using (_logger.TraceMethod(this))
            {
                resource.ValidateArgumentNotNullOrWhitespace(nameof(resource));
                requester.ValidateArgumentNotNullOrWhitespace(nameof(requester));

                _logger.Log($"Waiting for resource <{resource}> to be locked by <{requester}>");
                var memoryLock = GetLockOrSet(resource);
                lock (memoryLock)
                {
                    // Method will always run sync so we can just use .Result
                    if (!TryLockAsync(resource, requester, out var @lock, expiryTime, keepAlive).Result)
                    {
                        _logger.Log($"Resource <{resource}> is already locked. Creating lock request for <{requester}>");

                        // Create and store request
                        var request = new MemoryLockRequest(requester, memoryLock, timeout, token)
                        {
                            ExpiryTime = expiryTime,
                            KeepAlive = keepAlive
                        };
                        memoryLock.Requests.Enqueue(request);

                        _logger.Log($"Lock request created for requester <{requester}> on resource <{resource}>");
                        return request.CallbackTask;
                    }

                    _logger.Log($"Resource <{resource}> was not locked and is now held by <{requester}>");
                    return Task.FromResult(@lock);
                }
            }
        }
        /// <inheritdoc/>
        public Task<ILockInfo> GetAsync(string resource, CancellationToken token = default)
        {
            using (_logger.TraceMethod(this))
            {
                return Task.FromResult(GetLockOrSet(resource).CastTo<ILockInfo>());
            }
        }
        /// <inheritdoc/>
        public Task<ILockRequest[]> GetPendingRequestsAsync(string resource, CancellationToken token = default)
        {
            using (_logger.TraceMethod(this))
            {
                resource.ValidateArgumentNotNullOrWhitespace(nameof(resource));

                var memoryLock = GetLockOrSet(resource);
                lock (memoryLock)
                {
                    return Task.FromResult(memoryLock.Requests.OrderBy(x => x.CreatedAt).Cast<ILockRequest>().ToArray());
                }
            }
        }
        /// <inheritdoc/>
        public Task<ILockInfo[]> QueryAsync(string filter = null, int page = 0, int pageSize = 100, Expression<Func<ILockInfo, object>> sortBy = null, bool sortDescending = false)
        {
            using (_logger.TraceMethod(this))
            {
                lock (_locks)
                {
                    _logger.Log($"Querying locks");
                    pageSize.ValidateArgumentLarger(nameof(pageSize), 0);

                    var enumerator = _locks.Select(x => x.Value).Cast<ILockInfo>();
                    // Apply filter
                    if (filter.HasValue()) enumerator = enumerator.Where(x => x.Resource.Contains(filter));
                    // Apply sorting
                    if (sortBy.HasValue())
                    {
                        if (!sortBy.TryExtractProperty(out _)) throw new NotSupportedException($"{nameof(sortBy)} does not point to a property on {typeof(ILockInfo)}");

                        if (sortDescending)
                        {
                            enumerator = enumerator.OrderByDescending(sortBy.Compile());
                        }
                        else
                        {
                            enumerator = enumerator.OrderBy(sortBy.Compile());
                        }
                    }
                    // Apply pagination
                    if(page > 0)
                    {
                        enumerator = enumerator.Skip((page - 1) * pageSize).Take(pageSize);
                    }

                    // Return result
                    var result = enumerator.Select(x =>
                    {
                        lock (x)
                        {
                            return new MemoryLockInfo(x.CastTo<MemoryLockInfo>());
                        }
                    }).Cast<ILockInfo>().ToArray();

                    _logger.Log($"Query returned <{result.Length}> results");
                    return Task.FromResult(result);
                }
            }
        }

        /// <summary>
        /// Checks if lock <paramref name="resource"/> is still held by <paramref name="requester"/>.
        /// </summary>
        /// <param name="resource">The lock to check</param>
        /// <param name="requester">Who is supposed to have the lock</param>
        /// <returns>True if lock <paramref name="resource"/> is still held by <paramref name="requester"/></returns>
        public bool HasLock(string resource, string requester)
        {
            using (_logger.TraceMethod(this))
            {
                resource.ValidateArgumentNotNullOrWhitespace(nameof(resource));
                requester.ValidateArgumentNotNullOrWhitespace(nameof(requester));

                var memoryLock = GetLockOrSet(resource);
                lock (memoryLock)
                {
                    var hasLock = memoryLock.LockedBy.Equals(requester, StringComparison.OrdinalIgnoreCase);
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
        public bool TryExtend(string resource, string requester, TimeSpan extendTime, out ILockInfo lockInfo)
        {
            using (_logger.TraceMethod(this))
            {
                resource.ValidateArgumentNotNullOrWhitespace(nameof(resource));
                requester.ValidateArgumentNotNullOrWhitespace(nameof(requester));

                _logger.Log($"Trying to extend expiry date on lock <{resource}> requested by <{requester}>");
                var memoryLock = GetLockOrSet(resource);
                lockInfo = memoryLock;
                lock (memoryLock)
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
        public bool Unlock(string resource, string requester)
        {
            using (_logger.TraceMethod(this))
            {
                resource.ValidateArgumentNotNullOrWhitespace(nameof(resource));
                requester.ValidateArgumentNotNullOrWhitespace(nameof(requester));

                _logger.Log($"Trying to unlock resource <{resource}> requested by <{requester}>");

                var memoryLock = GetLockOrSet(resource);
                lock (memoryLock)
                {
                    // Check if lock is being held
                    if (!ValidateLock(requester, memoryLock))
                    {
                        _logger.Warning($"Lock <{memoryLock.Resource}> is not held anymore by <{requester}>. Can't unlock");
                        return false;
                    }

                    // Release the lock
                    memoryLock.LockedBy = null;
                    memoryLock.LockedAt = null;
                    memoryLock.ExpiryDate = null;

                    TryAssignPendingRequest(resource);

                    _logger.Log($"Lock <{resource}> was unlocked by <{requester}>");
                    
                    return true;
                }
            }
        }

        /// <summary>
        /// Tries to assign the next pending lock request on <paramref name="resource"/>.
        /// </summary>
        /// <param name="resource">The resource to check the pending requests for</param>
        public bool TryAssignPendingRequest(string resource)
        {
            using (_logger.TraceMethod(this))
            {
                resource.ValidateArgumentNotNullOrWhitespace(nameof(resource));

                var memoryLock = GetLockOrSet(resource);
                var tasks = new List<Task>();

                lock (memoryLock)
                {
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
                return false;    
            }
        }

        /// <summary>
        /// Event handler called when lock <paramref name="resource"/> expires.
        /// </summary>
        /// <param name="resource">The resource that expired</param>
        public void OnLockExpired(string resource)
        {
            using (_logger.TraceMethod(this))
            {
                resource.ValidateArgumentNotNullOrWhitespace(nameof(resource));
                _logger.Log($"Lock on resource <{resource}> expired. Trying to assign request if any are pending");

                var @lock = GetLockOrSet(resource);
                lock (@lock)
                {
                    if (TryAssignPendingRequest(resource))
                    {
                        _logger.Log($"Request was assigned to expired lock on resource <{resource}>");
                    }
                    else
                    {
                        _logger.Log($"No pending requests for expired lock on resource <{resource}>. Resetting lock");
                        @lock.LockedBy = null;
                        @lock.LockedAt = null;
                        @lock.ExpiryDate = null;
                    }
                }               
            }
        }

        private MemoryLockInfo GetLockOrSet(string resource)
        {
            resource.ValidateArgumentNotNullOrWhitespace(nameof(resource));

            lock (_locks)
            {
                return _locks.TryGetOrSet(resource, () => new MemoryLockInfo(resource));
            }
        }

        private bool ValidateLock(string requester, MemoryLockInfo memoryLock)
        {
            requester.ValidateArgumentNotNullOrWhitespace(nameof(requester));
            memoryLock.ValidateArgument(nameof(memoryLock));

            lock (memoryLock)
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
                        _logger.Debug($"Running cleanup in <{options.CleanupInterval}>");
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

                                        var couldLock = Helper.Lock.TryLockAndExecute(@lock, () =>
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
        public async ValueTask DisposeAsync()
        {
            List<Exception> exceptions = new List<Exception>();
            // Cancel cleanup task if it's running
            if(_cleanupTask != null)
            {
                using (_logger.TraceAction(LogLevel.Debug, "Dispose: Cancel cleanup task")) {
                    try
                    {
                        _cancellationTokenSource.Cancel();
                        await _cleanupTask.ConfigureAwait(false);
                        _cancellationTokenSource.Dispose();
                    }
                    catch(Exception ex)
                    {
                        exceptions.Add(ex);
                        _logger.Log($"Could not properly stop cleanup task", ex);
                    }
                }
            }

            // Cancel any pending requests so callers can return
            using(_logger.TraceAction(LogLevel.Debug, "Dispose: Cancel pending lock requests"))
            {
                lock (_locks)
                {
                    foreach (var @lock in _locks.Values.Where(x => !x.Requests.IsEmpty))
                    {
                        lock(@lock)
                        {
                            foreach(var request in @lock.Requests)
                            {
                                try
                                {
                                    using (request)
                                    {
                                        _logger.Trace($"Cancelling locking request made by <{request.Requester}> on resource <{request.Resource}>");
                                        request.AbortRequest(new ObjectDisposedException(nameof(MemoryLockingProvider)));
                                    }
                                }
                                catch(Exception ex)
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

        #region Classes
        /// <summary>
        /// Represents a lock placed on a resource in memory.
        /// </summary>
        private class MemoryLock : MemoryLockInfo, ILock
        {
            // Fields
            private readonly object _threadLock = new object();
            private readonly MemoryLockingProvider _provider;
            private bool unlockCalled = false;
            private CancellationTokenSource _cancellationTokenSource;
            private Task _expiryTask;
            private bool _keepAlive;
            private TimeSpan _extendTime;
            private ILogger _logger;

            // Properties

            /// <inheritdoc cref="MemoryLock"/>
            /// <param name="provider">The provider creating the lock</param>
            /// <param name="memoryLock">Information about the lock being placed</param>
            /// <param name="keepAlive">If the lock needs to be kept alive when an expiry date is set</param>
            /// <param name="extendTime">By how much time to extend the expiry date when <paramref name="keepAlive"/> is enabled</param>
            /// <param name="logger">Optional logger for tracing</param>
            public MemoryLock(MemoryLockingProvider provider, MemoryLockInfo memoryLock, bool keepAlive, TimeSpan extendTime, ILogger logger) : base(memoryLock?.Resource)
            {
                _provider = provider;
                _keepAlive = keepAlive;
                _extendTime = extendTime;
                _logger = logger;

                // Copy over properties
                LockedBy = memoryLock.LockedBy;
                LockedAt = memoryLock.LockedAt;
                LastLockDate = memoryLock.LastLockDate;
                ExpiryDate = memoryLock.ExpiryDate;

                TryStartExpiryTask();
            }

            /// <inheritdoc/>
            public Task<bool> HasLockAsync(CancellationToken token = default)
            {
                using (_logger.TraceMethod(this))
                {
                    return Task.FromResult(_provider.HasLock(Resource, LockedBy));
                }
            }
            /// <inheritdoc/>
            public async Task ExtendAsync(TimeSpan extendTime, CancellationToken token = default)
            {
                using (_logger.TraceMethod(this))
                {
                    // Stop keep expiry task if it's running
                    if (_expiryTask != null)
                    {
                        try
                        {
                            _logger.Debug($"Expiry task is running for lock <{Resource}> held by <{LockedBy}>. Cancelling task before extending the expiry date");
                            _cancellationTokenSource.Cancel();
                            await _expiryTask.ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            _logger.Warning($"Could not properly stop expiry task for lock <{Resource}> held by <{LockedBy}>", ex);
                            _expiryTask = null;
                        }
                    }

                    lock (_threadLock)
                    {
                        if(_provider.TryExtend(Resource, LockedBy, extendTime, out var memoryLock))
                        {
                            ExpiryDate = memoryLock.ExpiryDate;
                        }

                        TryStartExpiryTask();
                    }
                }
            }

            /// <inheritdoc/>
            public async Task UnlockAsync(CancellationToken token = default)
            {
                using (_logger.TraceMethod(this))
                {
                    _logger.Log($"Releasing lock on resource <{Resource}> held by <{LockedBy}>");

                    // Stop keep expiry task if it's running
                    if (_expiryTask != null)
                    {
                        try
                        {
                            _logger.Debug($"Expiry task is running for lock <{Resource}> held by <{LockedBy}>. Cancelling task before releasing the lock");
                            _cancellationTokenSource.Cancel();
                            await _expiryTask.ConfigureAwait(false);
                        }
                        catch(Exception ex)
                        {
                            _logger.Warning($"Could not properly stop expiry task for lock <{Resource}> held by <{LockedBy}>", ex);
                            _expiryTask = null;
                        }
                    }

                    // Try unlock
                    if(_provider.Unlock(Resource, LockedBy))
                    {
                        _logger.Log($"Released lock on resource <{Resource}> held by <{LockedBy}>");
                    }
                    else
                    {
                        _logger.Warning($"Could not release lock <{Resource}> supposed to be held by <{LockedBy}>. Lock is probably stale");
                    }
                    unlockCalled = true;                    
                }
            }

            private void TryStartExpiryTask()
            {
                using (_logger.TraceMethod(this))
                {
                    lock (_threadLock)
                    {
                        if (!ExpiryDate.HasValue) {
                            _logger.Debug($"No expiry set on lock <{Resource}> held by <{LockedBy}> so no need to start expiry task");
                            return;
                        }

                        _cancellationTokenSource = _cancellationTokenSource == null ? new CancellationTokenSource() : _cancellationTokenSource;
                        if (_keepAlive)
                        {
                            _logger.Debug($"Keep alive is enabled on lock <{Resource}> held by <{LockedBy}> with an expiry date set at <{ExpiryDate}>. Starting extend task");

                            _expiryTask = ExtendLockDuringLifetime(_cancellationTokenSource.Token);
                        }
                        else
                        {
                            _logger.Debug($"Keep alive is disabled on lock <{Resource}> held by <{LockedBy}> with an expiry date set at <{ExpiryDate}>. Starting expiry notifier task");
                            _expiryTask = NotifyExpiryDateExceeded(_cancellationTokenSource.Token);
                        }
                    }                   
                }
            }

            private async Task ExtendLockDuringLifetime(CancellationToken token)
            {
                using (_logger.TraceMethod(this))
                {
                    try
                    {
                        while (!token.IsCancellationRequested)
                        {
                            if (!ExpiryDate.HasValue)
                            {
                                _logger.Warning($"Keep alive task for lock <{Resource}> held by <{LockedBy}> is running but no expiry date is set. Stopping task");
                                return;
                            }

                            // Sleep until we can extend the lock
                            var sleepTime = (ExpiryDate.Value - LockedAt.Value).TotalMilliseconds.ChangeType<int>() - _provider.OptionsMonitor.CurrentValue.ExpiryOffset;
                            _logger.Debug($"Extending expiry date for lock <{Resource}> held by <{LockedBy}> in <{sleepTime}ms> before it expires at <{ExpiryDate.Value}>");
                            await Helper.Async.Sleep(sleepTime, token).ConfigureAwait(false);
                            if (token.IsCancellationRequested) return;

                            // Extend lock
                            _logger.Debug($"Extending expiry date for lock <{Resource}> held by <{LockedBy}> by <{_extendTime}>");
                            if (_provider.TryExtend(Resource, LockedBy, _extendTime, out var memoryLock))
                            {
                                ExpiryDate = memoryLock.ExpiryDate;
                                _logger.Debug($"New expiry date for lock <{Resource}> held by <{LockedBy}> is <{ExpiryDate}>");
                            }
                            else
                            {
                                _logger.Warning($"Keep alive task on lock <{Resource}> supposed to be held by <{LockedBy}> could not extend expiry date. Lock is probably stale. Stopping");
                                return;
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        _logger.Warning($"Something when wrong while trying to extend the expiry for lock <{Resource}> held by <{LockedBy}>. Starting notify task", ex);
                        _expiryTask = NotifyExpiryDateExceeded(token);
                    }                   
                }
            }

            private async Task NotifyExpiryDateExceeded(CancellationToken token)
            {
                using (_logger.TraceMethod(this))
                {
                    var sleepTime = (ExpiryDate.Value - DateTime.Now.AddMilliseconds(_provider.OptionsMonitor.CurrentValue.ExpiryOffset));
                    _logger.Debug($"Notifying provider in <{sleepTime.TotalMilliseconds}ms> that lock <{Resource}> held by <{LockedBy}> expired");
                    await Helper.Async.Sleep(sleepTime.TotalMilliseconds.ChangeType<int>(), token).ConfigureAwait(false);
                    if(token.IsCancellationRequested) return;
                    if (!await HasLockAsync(token).ConfigureAwait(false)) return;
                    _provider.OnLockExpired(Resource);
                }
            }

            /// <inheritdoc/>
            public async ValueTask DisposeAsync()
            {
                using (_logger.TraceMethod(this))
                {
                    try
                    {
                        if (!unlockCalled)
                        {
                            await UnlockAsync().ConfigureAwait(false);
                        }
                    }
                    catch(Exception ex)
                    {
                        _logger.Log($"Error occured while unlocking task during dispose", ex);
                        throw;
                    }                    
                }
            }
        }

        /// <summary>
        /// Represents information about a lock in memory.
        /// </summary>
        private class MemoryLockInfo : ILockInfo
        {
            /// <inheritdoc cref="MemoryLockInfo"/>
            /// <param name="resource">The resource the current lock info is being created for</param>
            public MemoryLockInfo(string resource)
            {
                Resource = resource.ValidateArgument(nameof(resource));
            }

            /// <inheritdoc cref="MemoryLockInfo"/>
            /// <param name="copy">The instance to copy state from</param>
            public MemoryLockInfo(MemoryLockInfo copy)
            {
                copy.ValidateArgument(nameof(copy));
                Resource = copy.Resource;
                LockedBy = copy.LockedBy;
                LockedAt = copy.LockedAt;
                LastLockDate = copy.LastLockDate;
                ExpiryDate = copy.ExpiryDate;
                Requests = copy.Requests;
            }

            /// <inheritdoc/>
            public string Resource { get; }
            /// <inheritdoc/>
            public string LockedBy { get; internal set; }
            /// <inheritdoc/>
            public DateTime? LockedAt { get; internal set; }
            /// <inheritdoc/>
            public DateTime? LastLockDate { get; internal set; }
            /// <inheritdoc/>
            public DateTime? ExpiryDate { get; internal set; }
            /// <summary>
            /// Pending locking requests for the current lock.
            /// </summary>
            public ConcurrentQueue<MemoryLockRequest> Requests { get; } = new ConcurrentQueue<MemoryLockRequest>();
            /// <inheritdoc/>
            public int PendingRequests => Requests.Count;
        }

        /// <summary>
        /// Represents an in-memory request on a lock.
        /// </summary>
        private class MemoryLockRequest : ILockRequest, IDisposable
        {
            // Fields
            private readonly object _threadLock = new object();
            private readonly TaskCompletionSource<ILock> _taskSource = new TaskCompletionSource<ILock>();
            private readonly CancellationTokenRegistration _cancellationTokenRegistration;
            private readonly CancellationTokenSource _tokenSource;
            private readonly Task _timeoutTask;

            /// <inheritdoc cref="MemoryLockRequest"/>
            /// <param name="requester">Who created the request</param>
            /// <param name="memoryLock">The lock the request is placed on</param>
            /// <param name="cancellationToken">The cancellation token provided by the caller of the request</param>
            /// <param name="timeout">When the current request times out. When set to null the request never times out</param>
            public MemoryLockRequest(string requester, MemoryLockInfo memoryLock, TimeSpan? timeout, CancellationToken cancellationToken)
            {
                Requester = requester.ValidateArgument(nameof(requester));
                Resource = memoryLock.ValidateArgument(nameof(memoryLock)).Resource;
                Timeout = timeout.HasValue ? DateTime.Now.Add(timeout.Value) : (DateTime?)null;

                // Launch fire and forget task to time out the request
                if (Timeout.HasValue)
                {
                    _tokenSource = new CancellationTokenSource();
                    _timeoutTask = Task.Run(async () =>
                    {
                        var sleepTime = Timeout.Value - DateTime.Now;
                        await Helper.Async.Sleep(sleepTime.TotalMilliseconds.ChangeType<int>(), _tokenSource.Token).ConfigureAwait(false);
                        if (_tokenSource.Token.IsCancellationRequested) return;
                        AbortRequest(new LockTimeoutException(requester, memoryLock, timeout.Value));
                    });
                }

                // Add handler when cancellation token is cancelled so we can abort the caller task
                _cancellationTokenRegistration = cancellationToken.Register(() =>
                {
                    // Cancel internal tasks
                    _tokenSource?.Cancel();

                    // Abort request
                    AbortRequest(new TaskCanceledException());
                });
            }
            /// <inheritdoc/>
            public string Resource { get; }
            /// <inheritdoc/>
            public string Requester { get; }
            /// <inheritdoc/>
            public TimeSpan? ExpiryTime { get; internal set; }
            /// <inheritdoc/>
            public bool KeepAlive { get; internal set; }
            /// <inheritdoc/>
            public DateTime? Timeout { get; }
            /// <inheritdoc/>
            public DateTime CreatedAt { get; } = DateTime.Now;
            /// <summary>
            /// The task returned to caller when they request a lock. 
            /// </summary>
            public Task<ILock> CallbackTask => _taskSource.Task;

            /// <summary>
            /// Assigns the lock to the caller of the request.
            /// </summary>
            /// <param name="memoryLock">The lock to assign</param>
            /// <returns>True if the lock was assigned or false if the request was modified while calling this method</returns>
            public bool AssignLock(MemoryLock memoryLock)
            {
                memoryLock.ValidateArgument(nameof(memoryLock));

                lock (_threadLock)
                {
                    if (!CallbackTask.IsCompleted)
                    {
                        _taskSource.SetResult(memoryLock);
                        return true;
                    }
                }
                return false;
            }

            /// <summary>
            /// Aborts the current request.
            /// </summary>
            /// <param name="exception">Exception containing the reason why the request was cancelled</param>
            /// <returns>True if the lock was assigned or false if the request was modified while calling this method</returns>
            public bool AbortRequest(Exception exception)
            {
                exception.ValidateArgument(nameof(exception));

                lock (_threadLock)
                {
                    if (!CallbackTask.IsCompleted)
                    {
                        _taskSource.SetException(exception);
                        return true;
                    }
                }
                return false;
            }

            /// <inheritdoc/>
            public async void Dispose()
            {
                Exception exception = null;
                // Dispose registration to avoid leaks
                _cancellationTokenRegistration.Dispose();

                // Cancel internal tasks
                if(_timeoutTask != null)
                {
                    try
                    {
                        _tokenSource.Cancel();
                        await _timeoutTask.ConfigureAwait(false);
                        _tokenSource.Dispose();
                    }
                    catch(Exception ex)
                    {
                        exception = ex;
                    }
                }

                // Abort calling task if not happened already
                AbortRequest(new ObjectDisposedException(nameof(MemoryLockRequest)));

                if (exception != null) exception.Rethrow();
            }
        }
        #endregion
    }
}
