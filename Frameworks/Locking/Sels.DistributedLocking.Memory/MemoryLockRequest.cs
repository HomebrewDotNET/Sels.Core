using Sels.Core;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Object;
using Sels.DistributedLocking.Provider;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sels.DistributedLocking.Memory
{
    /// <summary>
    /// Represents an in-memory request on a lock.
    /// </summary>
    internal class MemoryLockRequest : ILockRequest, IDisposable
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
                    await Helper.Async.Sleep(sleepTime, _tokenSource.Token).ConfigureAwait(false);
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
                AbortRequest(new OperationCanceledException($"Lock request placed by <{Requester}> on resource <{Resource}> was cancelled by caller"));
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
        /// Indicates that the current request has been completed.
        /// </summary>
        public bool IsCompleted => CallbackTask.IsCompleted;

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
                if (!IsCompleted)
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
                if (!IsCompleted)
                {
                    _taskSource.SetException(exception);
                    return true;
                }
            }
            return false;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Exception exception = null;
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
                    exception = ex;
                }
                finally
                {
                    _tokenSource.Dispose();
                }
            }

            // Abort calling task if not happened already
            AbortRequest(new OperationCanceledException($"Lock request on resource <{Resource}> placed by <{Requester}> was cancelled"));

            if (exception != null) exception.Rethrow();
        }
    }
}
