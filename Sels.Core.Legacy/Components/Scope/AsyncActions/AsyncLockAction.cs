using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sels.Core.Components.Scope.AsyncActions
{
    /// <summary>
    /// Scoped action for executing concurrent actions in the scope using a <see cref="SemaphoreSlim"/>. Action manages the locking and releasing of the lock
    /// </summary>
    public class AsyncLockAction : AsyncScopedAction
    {
        ///<inheritdoc cref="AsyncLockAction"/>
        /// <param name="semaphore">The semaphore to use for the locking</param>
        public AsyncLockAction(SemaphoreSlim semaphore) : base(x => PlaceLock(semaphore, x), x => UnLock(semaphore, x))
        {
            
        }

        private static Task PlaceLock(SemaphoreSlim semaphore, CancellationToken token)
        {
            semaphore.ValidateArgument(nameof(semaphore));

            return semaphore.WaitAsync(token);
        }

        private static Task UnLock(SemaphoreSlim semaphore, CancellationToken token)
        {
            semaphore.ValidateArgument(nameof(semaphore));

            semaphore.Release();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Places a lock using <paramref name="semaphore"/>. Disposing will release the lock.
        /// </summary>
        /// <param name="semaphore">The semaphore to use for the locking</param>
        /// <param name="token">Optional token to cancel the operation</param>
        /// <returns>Scope that will release the lock when disposed</returns>
        public static Task<IAsyncDisposable> LockAsync(SemaphoreSlim semaphore, CancellationToken token = default)
        {
            return new AsyncLockAction(semaphore).StartAsync();
        }
    }
}
