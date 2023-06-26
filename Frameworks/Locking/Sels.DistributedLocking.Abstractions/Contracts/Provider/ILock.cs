using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sels.DistributedLocking.Provider
{
    /// <summary>
    /// Represents a currently held lock on <see cref="ILockInfo.Resource"/>. Disposing the object will release the lock.
    /// </summary>
    public interface ILock : ILockInfo, IAsyncDisposable
    {
        /// <summary>
        /// Checks if the current lock is still held by the requester.
        /// </summary>
        /// <param name="token">Optional loken to cancel the request</param>
        /// <returns></returns>
        Task<bool> HasLockAsync(CancellationToken token = default);
        /// <summary>
        /// Extends the current expiry date by <paramref name="extendTime"/>. If no expiry date is set a new one will be set.
        /// </summary>
        /// <param name="extendTime">By how many time to extend the expiry date for</param>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>True if the expiry date was extended, otherwise false</returns>
        Task<bool> ExtendAsync(TimeSpan extendTime, CancellationToken token = default);
        /// <summary>
        /// Unlocks the current lock. Also called when disposing the lock.
        /// </summary>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>Task containing the execution state</returns>
        /// <returns>True if the expiry date was extended, otherwise false</returns>
        Task<bool> UnlockAsync(CancellationToken token = default);
    }

    /// <summary>
    /// Contains extension methods for <see cref="ILock"/>.
    /// </summary>
    public static class ILockExtensions
    {
        /// <summary>
        /// Checks that <paramref name="lock"/> is still active. If it's not a <see cref="StaleLockException"/> will be thrown.
        /// </summary>
        /// <param name="lock">The lock to check</param>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>Task containing the execution state</returns>
        /// <exception cref="StaleLockException"></exception>
        public static async Task ThrowIfStaleAsync(this ILock @lock, CancellationToken token = default)
        {
            if (!await @lock.HasLockAsync(token).ConfigureAwait(false)) throw new StaleLockException(@lock.LockedBy, @lock);
        }
    }
}
