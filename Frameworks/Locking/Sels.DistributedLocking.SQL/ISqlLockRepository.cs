using Sels.Core.Data.Contracts.Repository;
using Sels.DistributedLocking.Provider;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sels.DistributedLocking.SQL
{
    /// <summary>
    /// Manages lock state using an sql database.
    /// </summary>
    public interface ISqlLockRepository
    {
        /// <summary>
        /// Creates a new transactions.
        /// </summary>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>Transaction that can be used with the current repository</returns>
        Task<IRepositoryTransaction> CreateTransactionAsync(CancellationToken token);

        /// <summary>
        /// Fetches the state of the lock placed on <paramref name="resource"/>
        /// </summary>
        /// <param name="transaction">The transaction to execute the operation in</param>
        /// <param name="resource">The resource the lock is placed on</param>
        /// <param name="countRequests">Whether or not to set <see cref="SqlLock.PendingRequests"/></param>
        /// <param name="forUpdate">If a lock should be placed on the selected record</param>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>The state of lock or null if no lock exists on <paramref name="resource"/></returns>
        Task<SqlLock> GetLockByResourceAsync(IRepositoryTransaction transaction, string resource, bool countRequests, bool forUpdate, CancellationToken token);

        /// <summary>
        /// Fetches all pending locking requests for <paramref name="resource"/>.
        /// </summary>
        /// <param name="transaction">The transaction to execute the operation in</param>
        /// <param name="resource">The resource to get the pending requests for</param>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>All pending requests on <paramref name="resource"/> or null if there aren't any pending requests</returns>
        Task<SqlLockRequest[]> GetAllLockRequestsByResourceAsync(IRepositoryTransaction transaction, string resource, CancellationToken token);

        /// <summary>
        /// Tries to assign a lock on resource <paramref name="resource"/> to <paramref name="requester"/>.
        /// </summary>
        /// <param name="transaction">The transaction to execute the operation in</param>
        /// <param name="resource">The resource to lock</param>
        /// <param name="requester">Who is requesting the lock</param>
        /// <param name="expiryDate">The expiry date for the lock if it could be placed</param>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>The latest state of the lock</returns>
        Task<SqlLock> TryAssignLockToAsync(IRepositoryTransaction transaction, string resource, string requester, DateTime? expiryDate, CancellationToken token);

        /// <summary>
        /// Unlock resource <paramref name="resource"/> if it still held by <paramref name="requester"/>.
        /// </summary>
        /// <param name="transaction">The transaction to execute the operation in</param>
        /// <param name="resource">The resource to unlock</param>
        /// <param name="requester">Who is supposed to be holding the lock</param>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>WasUnlocked set to true if <paramref name="resource"/> was still held by <paramref name="requester"/>, otherwise false. CurrentState contains the current state of the sql lock</returns>
        Task<(bool WasUnlocked, SqlLock CurrentState)> TryUnlockAsync(IRepositoryTransaction transaction, string resource, string requester, CancellationToken token);

        /// <summary>
        /// Extends the expiry date for lock on resource <paramref name="resource"/> or sets it if it null. Should only be updated if resource is held by <paramref name="requester"/>.
        /// </summary>
        /// <param name="transaction">The transaction to execute the operation in</param>
        /// <param name="resource">The resource to extend the expiry date for</param>
        /// <param name="requester">Who is supposed to be holding the lock</param>
        /// <param name="extendTime">By how much to extend the time for</param>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>The current state of the lock</returns>
        Task<SqlLock> TryUpdateExpiryDateAsync(IRepositoryTransaction transaction, string resource, string requester, TimeSpan extendTime, CancellationToken token);

        /// <summary>
        /// Creates <paramref name="request"/>.
        /// </summary>
        /// <param name="transaction">The transaction to execute the operation in</param>
        /// <param name="request">The request persist</param>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>The inserted request</returns>
        Task<SqlLockRequest> CreateRequestAsync(IRepositoryTransaction transaction, SqlLockRequest request, CancellationToken token);

        /// <summary>
        /// Deletes all lock requests in <paramref name="ids"/>.
        /// </summary>
        /// <param name="transaction">The transaction to execute the operation in</param>
        /// <param name="ids">The ids of the requests to delete</param>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>Task containing the execution state</returns>
        Task DeleteAllRequestsById(IRepositoryTransaction transaction, long[] ids, CancellationToken token);

        /// <summary>
        /// Returns all the ids in <paramref name="ids"/> of lock requests that have been deleted.
        /// </summary>
        /// <param name="transaction">The transaction to execute the operation in</param>
        /// <param name="ids">The ids of lock requests to check</param>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>The ids in <paramref name="ids"/> of requests that have been deleted</returns>
        Task<long[]> GetDeletedRequestIds(IRepositoryTransaction transaction, long[] ids, CancellationToken token);

        /// <summary>
        /// Searches for all matching sql locks.
        /// </summary>
        /// <param name="transaction">The transaction to execute the operation in</param>
        /// <param name="searchCriteria">The search criteria for the query</param>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>All known locks matching the search parameters with pagination applied (if applicable) and the total amount of matching locks regardless of pagination</returns>
        Task<(SqlLock[] Results, int TotalMatching)> SearchAsync(IRepositoryTransaction transaction, SqlQuerySearchCriteria searchCriteria, CancellationToken token = default);

        /// <summary>
        /// Deletes all free/expired locks that have no pending requests.
        /// </summary>
        /// <param name="transaction">The transaction to execute the operation in</param>
        /// <param name="inactiveTime">If set only locks that haven't been modified in <paramref name="inactiveTime"/>ms will be removed</param>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>How many rows were deleted</returns>
        Task<int> DeleteInActiveLocksAsync(IRepositoryTransaction transaction, int? inactiveTime = null , CancellationToken token = default);

        /// <summary>
        /// Get the amount of locks.
        /// </summary>
        /// <param name="transaction">The transaction to execute the operation in</param>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>The amount of locks.</returns>
        Task<int> GetLockAmountAsync(IRepositoryTransaction transaction, CancellationToken token = default);

        /// <summary>
        /// Clears all the locks and requests from the repository.
        /// </summary>
        /// <param name="transaction">The transaction to execute the operation in</param>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>Task containing the execution state</returns>
        Task ClearAllAsync(IRepositoryTransaction transaction, CancellationToken token = default);

        /// <summary>
        /// Force unlocks resource <paramref name="resource"/>.
        /// </summary>
        /// <param name="transaction">The transaction to execute the operation in</param>
        /// <param name="resource">The resource to force unlock</param>
        /// <param name="removePendingRequests">If any pending requests need to be removed as well</param>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>Task containing the execution state</returns>
        Task ForceUnlockAsync(IRepositoryTransaction transaction, string resource, bool removePendingRequests, CancellationToken token = default);
    }
}
