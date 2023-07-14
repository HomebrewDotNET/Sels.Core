using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sels.DistributedLocking.Provider
{
    /// <summary>
    /// Service for placing distributed locks on resources.
    /// </summary>
    public interface ILockingProvider
    {
        /// <summary>
        /// Tries to place a lock on <paramref name="resource"/>.
        /// </summary>
        /// <param name="resource">The name of the resource to lock</param>
        /// <param name="requester">Who is requesting the lock</param>
        /// <param name="expiryTime">Optional time when the lock will expire. If set to null the lock will not expire on it's own</param>
        /// <param name="keepAlive">To keep the lock alive when <paramref name="expiryTime"/> is set. Will extend the expiry date by <paramref name="expiryTime"/> right before it expires</param>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>Tuple where Success will be true if <paramref name="requester"/> locked <paramref name="resource"/>, otherwise false if someone else has locked <paramref name="resource"/>. Lock will be the lock if Success is true</returns>
        Task<ILockResult> TryLockAsync(string resource, string requester, TimeSpan? expiryTime = null, bool keepAlive = false, CancellationToken token = default);

        /// <summary>
        /// Tries to place a lock on <paramref name="resource"/>. If the lock is currently held the method call will block until <paramref name="requester"/> manages to place the lock.
        /// </summary>
        /// <param name="resource">The name of the resource to lock</param>
        /// <param name="requester">Who is requesting the lock</param>
        /// <param name="expiryTime">Optional time when the lock will expire. If set to null the lock will not expire on it's own</param>
        /// <param name="keepAlive">To keep the lock alive when <paramref name="expiryTime"/> is set. Will extend the expiry date by <paramref name="expiryTime"/> right before it expires</param>
        /// <param name="timeout">Optional timeout that can be set. If a lock could not be placed within <paramref name="timeout"/> a <see cref="LockTimeoutException"/> will be thrown</param>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>The lock on <paramref name="resource"/> currently held by <paramref name="requester"/></returns>
        Task<ILock> LockAsync(string resource, string requester, TimeSpan? expiryTime = null, bool keepAlive = false, TimeSpan? timeout = null, CancellationToken token = default);

        /// <summary>
        /// Fetches the current locking state on <paramref name="resource"/>.
        /// </summary>
        /// <param name="resource">The resource to get the locking state for</param>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>The current locking state for <paramref name="resource"/></returns>
        Task<ILockInfo> GetAsync(string resource, CancellationToken token = default);

        /// <summary>
        /// Fetches all the current pending lock requests for <paramref name="resource"/>.
        /// </summary>
        /// <param name="resource">The resource to get the pending requests for</param>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>All currently pending requests or an empty array when there are no pending requests</returns>
        Task<ILockRequest[]> GetPendingRequestsAsync(string resource, CancellationToken token = default);

        /// <summary>
        /// Queries all currently known locks with <paramref name="searchCriteria"/> applied.
        /// </summary>
        /// <param name="searchCriteria">Configured the search criteria for the query</param>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>All currently known locks with <paramref name="searchCriteria"/> applied</returns>
        Task<ILockQueryResult> QueryAsync(Action<ILockQueryCriteria> searchCriteria, CancellationToken token = default);

        /// <summary>
        /// Forces an unlock of the lock held on <paramref name="resource"/>.
        /// Should be used with caution as this could lead to concurrency issues depending on how lock usage is implemented.
        /// </summary>
        /// <param name="resource">The resource to unlock</param>
        /// <param name="removePendingRequests">If any pending requests should be removed as well</param>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>Task containing the execution state</returns>
        Task ForceUnlockAsync(string resource, bool removePendingRequests = false, CancellationToken token = default);
    }
}
