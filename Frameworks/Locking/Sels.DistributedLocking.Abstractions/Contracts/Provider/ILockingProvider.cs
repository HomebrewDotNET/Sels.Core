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
        /// Queries all currently known locks.
        /// </summary>
        /// <param name="filter">Optional string to filter on <see cref="ILockInfo.Resource"/>. Locks will be returned if they contain <paramref name="filter"/>. Null will returns all locks</param>
        /// <param name="page">Used to specify what page to return when pagination is preferred when getting the locks. Setting the page to lower than 0 means no pagination will be applied</param>
        /// <param name="pageSize">How many items per page to return when <paramref name="page"/> is set to a value higher than 0</param>
        /// <param name="sortBy">Optional expression that points to the property on <see cref="ILockInfo"/> to sort by</param>
        /// <param name="sortDescending">True to sort <paramref name="sortBy"/> descending, otherwise false for ascending</param>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>All currently known locks</returns>
        Task<ILockQueryResult> QueryAsync(string filter = null, int page = 0, int pageSize = 100, Expression<Func<ILockInfo, object>> sortBy = null, bool sortDescending = false, CancellationToken token = default);
    }

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
        /// <returns>Task containing the execution state</returns>
        Task ExtendAsync(TimeSpan extendTime, CancellationToken token = default);
        /// <summary>
        /// Unlocks the current lock. Also called when disposing the lock.
        /// </summary>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>Task containing the execution state</returns>
        Task UnlockAsync(CancellationToken token = default);
    }

    /// <summary>
    /// Contains information about a lock placed on a resource.
    /// </summary>
    public interface ILockInfo
    {
        /// <summary>
        /// The resource the lock is held on.
        /// </summary>
        string Resource { get; }
        /// <summary>
        /// Who locked the resource.
        /// </summary>
        string LockedBy { get; }
        /// <summary>
        /// When the current lock was locked by <see cref="LockedBy"/>. Will be null when the lock is free.
        /// </summary>
        DateTimeOffset? LockedAt { get; }
        /// <summary>
        /// The last time when the lock was held by someone.
        /// </summary>
        DateTimeOffset? LastLockDate { get; }
        /// <summary>
        /// When the lock is set to expire. When a lock expires others will be able lock it instead. When set to null the lock never expires.
        /// </summary>
        DateTimeOffset? ExpiryDate { get; }
        /// <summary>
        /// How many pending requests there are for the current lock.
        /// </summary>
        int PendingRequests { get; }
    }

    /// <summary>
    /// Represent a request on a lock that will be placed once the mentioned lock is unlocked.
    /// </summary>
    public interface ILockRequest
    {
        /// <summary>
        /// The resource that the request is placed on.
        /// </summary>
        string Resource { get; }
        /// <summary>
        /// Who requested the lock.
        /// </summary>
        string Requester { get; }
        /// <summary>
        /// How long the lock will be held after placing it. When set to null the lock will not expire.
        /// </summary>
        TimeSpan? ExpiryTime { get; }
        /// <summary>
        /// If the lock will be kept alive after it's placed.
        /// </summary>
        bool KeepAlive { get; }
        /// <summary>
        /// When the current request expires. When set to null the request will never expire.
        /// </summary>
        DateTimeOffset? Timeout { get; }
        /// <summary>
        /// When the request was created.
        /// </summary>
        DateTimeOffset CreatedAt { get; }
    }

    /// <summary>
    /// The result for trying to lock a resource.
    /// </summary>
    public interface ILockResult
    {
        /// <summary>
        /// True if the resource could be locked, otherwise false.
        /// </summary>
        bool Success { get; }
        /// <summary>
        /// The acquired lock if <see cref="Success"/> is set to true.
        /// </summary>
        ILock AcquiredLock { get; }
        /// <summary>
        /// The current lock state regardless if the lock could be placed.
        /// </summary>
        ILockInfo CurrentLockState { get; }
    }

    /// <summary>
    /// The result from querying for locks.
    /// </summary>
    public interface ILockQueryResult
    {
        /// <summary>
        /// The query results.
        /// </summary>
        ILockInfo[] Results { get; }
        /// <summary>
        /// How many total pages there are.
        /// </summary>
        int MaxPages { get; }
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
