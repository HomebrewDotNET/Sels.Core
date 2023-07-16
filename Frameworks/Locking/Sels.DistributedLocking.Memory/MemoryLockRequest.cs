using Microsoft.Extensions.Logging;
using Sels.Core;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions;
using Sels.DistributedLocking.Provider;
using System;
using System.Threading;
using System.Threading.Tasks;
using Sels.Core.Extensions.Logging;
using Sels.Core.Extensions.Reflection;
using Sels.Core.Extensions.Exceptions;

namespace Sels.DistributedLocking.Memory
{
    /// <summary>
    /// Represents an in-memory request on a lock.
    /// </summary>
    internal class MemoryLockRequest : IPendingLockRequest, IDisposable
    {
        // Fields
        private readonly object _threadLock;
        private readonly TaskCompletionSource<ILock> _taskSource = new TaskCompletionSource<ILock>(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly CancellationTokenRegistration _cancellationTokenRegistration;
        private readonly CancellationTokenSource _tokenSource;
        private readonly Task _timeoutTask;
        private readonly ILogger _logger;

        /// <inheritdoc cref="MemoryLockRequest"/>
        /// <param name="requester">Who created the request</param>
        /// <param name="memoryLock">The lock the request is placed on</param>
        /// <param name="logger">Optional logger for tracing</param>
        /// <param name="cancellationToken">The cancellation token provided by the caller of the request</param>
        /// <param name="timeout">When the current request times out. When set to null the request never times out</param>
        public MemoryLockRequest(string requester, MemoryLockInfo memoryLock, TimeSpan? timeout, ILogger logger, CancellationToken cancellationToken)
        {
            _threadLock = memoryLock.SyncRoot;
            Requester = requester.ValidateArgument(nameof(requester));
            Resource = memoryLock.ValidateArgument(nameof(memoryLock)).Resource;
            Timeout = timeout.HasValue ? DateTime.Now.Add(timeout.Value) : (DateTime?)null;
            _logger = logger;

            // Add handler when cancellation token is cancelled so we can abort the caller task
            _cancellationTokenRegistration = cancellationToken.Register(() =>
            {
                // Cancel internal tasks
                _tokenSource?.Cancel();

                // Abort request
                AbortRequest(new OperationCanceledException($"Lock request placed by <{Requester}> on resource <{Resource}> was cancelled by caller"));
            });
            cancellationToken.ThrowIfCancellationRequested();

            // Launch fire and forget task to time out the request
            if (Timeout.HasValue)
            {
                _tokenSource = new CancellationTokenSource();
                _timeoutTask = Task.Run(async () =>
                {
                    var sleepTime = Timeout.Value - DateTime.Now;
                    await Helper.Async.Sleep(sleepTime, _tokenSource.Token).ConfigureAwait(false);
                    if (_tokenSource.Token.IsCancellationRequested) return;
                    AbortRequest(new LockTimeoutException(requester, memoryLock, timeout.Value));
                });
            }
        }

        /// <summary>
        /// <inheritdoc cref="MemoryLockRequest"/>
        /// Instantly completes the request with <paramref name="memoryLock"/>.
        /// </summary>
        /// <param name="memoryLock">The acquired lock</param>
        /// <param name="timeout">When the current request times out. When set to null the request never times out</param>
        /// <param name="logger">Optional logger for tracing</param>
        public MemoryLockRequest(MemoryLock memoryLock, TimeSpan? timeout, ILogger logger)
        {
            _threadLock = memoryLock.SyncRoot;
            memoryLock.ValidateArgument(nameof(memoryLock));
            Requester = memoryLock.LockedBy;
            Resource = memoryLock.ValidateArgument(nameof(memoryLock)).Resource;
            Timeout = timeout.HasValue ? DateTime.Now.Add(timeout.Value) : (DateTime?)null;
            _logger = logger;

            AssignLock(memoryLock);
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
        public Task<ILock> Callback => _taskSource.Task;
        /// <summary>
        /// Indicates that the current request has been completed.
        /// </summary>
        public bool IsCompleted => Callback.IsCompleted;

        /// <summary>
        /// Assigns the lock to the caller of the request.
        /// </summary>
        /// <param name="memoryLock">The lock to assign</param>
        /// <returns>True if the lock was assigned or false if the request was modified while calling this method</returns>
        public bool AssignLock(MemoryLock memoryLock)
        {
            memoryLock.ValidateArgument(nameof(memoryLock));

            _logger.Debug($"Trying to assign lock on resource <{memoryLock.Resource}> to request created by <{Requester}> at <{CreatedAt}>");
            lock (_threadLock)
            {
                if (!IsCompleted)
                {
                    _taskSource.SetResult(memoryLock);
                    _logger.Log($"Assigned lock on resource <{memoryLock.Resource}> to request created by <{Requester}> at <{CreatedAt}>");
                    return true;
                }
                _logger.Warning($"Could not assign lock on resource <{memoryLock.Resource}> to request created by <{Requester}> at <{CreatedAt}> because the request was already completed");
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

            _logger.Debug($"Trying to abort request on resource <{Resource}> created by <{Requester}> at <{CreatedAt}> with exception <{exception.GetType().GetDisplayName()}({exception.Message})>");
            lock (_threadLock)
            {
                if (!IsCompleted)
                {
                    _taskSource.SetException(exception);
                    _logger.Log($"Aborted request on resource <{Resource}> created by <{Requester}> at <{CreatedAt}> with exception <{exception.GetType().GetDisplayName()}({exception.Message})>");
                    return true;
                }

                _logger.Warning($"Could not abort request placed on resource <{Resource}> created by <{Requester}> at <{CreatedAt}> with exception <{exception.GetType().GetDisplayName()}({exception.Message})> because the request was already completed");
            }
            return false;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Exception exception = null;
            _logger.Debug($"Disposing request placed on resource <{Resource}> created by <{Requester}> at <{CreatedAt}>");

            try
            {
                // Dispose registration to avoid leaks
                _cancellationTokenRegistration.Dispose();

                // Cancel internal tasks
                if (_timeoutTask != null)
                {
                    try
                    {
                        _tokenSource.Cancel();
                        Helper.Sync.WaitOn(_timeoutTask, TimeSpan.FromSeconds(2));
                    }
                    catch (Exception ex)
                    {
                        _logger.Log($"Error occured while waiting on timeout task to cancel for request placed on resource <{Resource}> created by <{Requester}> at <{CreatedAt}>", ex);
                        exception = ex;
                    }
                    finally
                    {
                        _tokenSource.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Log($"Error occured while disposing request placed on resource <{Resource}> created by <{Requester}> at <{CreatedAt}>", ex);
                exception = ex;
            }
            finally
            {
                // Abort calling task if not happened already
                if (!IsCompleted) AbortRequest(new OperationCanceledException($"Lock request on resource <{Resource}> placed by <{Requester}> was cancelled"));
                _logger.Debug($"Disposed request placed on resource <{Resource}> created by <{Requester}> at <{CreatedAt}>");
            }

            if (exception != null) exception.Rethrow();
        }
    }
}
