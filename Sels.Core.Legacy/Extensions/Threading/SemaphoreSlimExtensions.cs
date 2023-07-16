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
    }
}
