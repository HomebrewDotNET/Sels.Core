using Sels.Core.Scope;
using Sels.Core.Scope.AsyncActions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sels.Core.Extensions.Threading
{
    /// <summary>
    /// Contains static extension methods for <see cref="SemaphoreSlim"/>.
    /// </summary>
    public static class SemaphoreSlimExtensions
    {
        /// <summary>
        /// Places a lock using <paramref name="semaphore"/>. Disposing will release the lock.
        /// </summary>
        /// <param name="semaphore">The semaphore to use for the locking</param>
        /// <param name="token">Optional token to cancel the operation</param>
        /// <returns>Scope that will release the lock when disposed</returns>
        public static Task<IAsyncDisposable> LockAsync(this SemaphoreSlim semaphore, CancellationToken token = default)
        {
            return AsyncLockAction.LockAsync(semaphore, token);
        }

        /// <summary>
        /// Tries to lock <paramref name="semaphore"/>.
        /// </summary>
        /// <param name="semaphore">The semaphore to use for the locking</param>
        /// <param name="lockScope">The scope if the lock was acquired. Disposing will release the lock</param>
        /// <param name="maxWaitTime">How long to wait on the lock. When set to null the method will wait at most 1ms</param>
        /// <returns>True if <paramref name="semaphore"/> was locked, otherwise false</returns>
        public static bool TryLock(this SemaphoreSlim semaphore, out IDisposable lockScope, TimeSpan? maxWaitTime = null)
        {
            semaphore.ValidateArgument(nameof(semaphore));
            lockScope = null;
            if (semaphore.Wait(maxWaitTime ?? TimeSpan.FromMilliseconds(1)))
            {
                lockScope = new ScopedAction(() => { }, () => semaphore.Release());
                return true;
            }

            return false;
        }
    }
}
