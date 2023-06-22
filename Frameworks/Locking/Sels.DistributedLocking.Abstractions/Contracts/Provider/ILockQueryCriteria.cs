using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.DistributedLocking.Provider
{
    /// <summary>
    /// Configures the search criteria when querying for locks.
    /// </summary>
    public interface ILockQueryCriteria
    {
        /// <summary>
        /// Adds a filter on <see cref="ILockInfo.Resource"/>.
        /// </summary>
        /// <param name="filter">The filter to match, empty string means anything not null</param>
        /// <returns>Current criteria for method chaining</returns>
        ILockQueryCriteria WithFilterOnResource(string filter);
        /// <summary>
        /// Adds a filter on <see cref="ILockInfo.LockedBy"/>.
        /// </summary>
        /// <param name="filter">The filter to match, empty string means anything not null</param>
        /// <returns>Current criteria for method chaining</returns>
        ILockQueryCriteria WithFilterOnLockedBy(string filter);
        /// <summary>
        /// Returns all locks locked by <paramref name="lockOwner"/>.
        /// </summary>
        /// <param name="lockOwner">The lock owner to return the locks for, can be null to get the free locks</param>
        /// <returns>Current criteria for method chaining</returns>
        ILockQueryCriteria WithLockedByEqualTo(string lockOwner);
        /// <summary>
        /// Returns all locks with more than <paramref name="amount"/> pending locks.
        /// </summary>
        /// <param name="amount">The thresshold of pending requests above which to return the locks</param>
        /// <returns>Current criteria for method chaining</returns>
        ILockQueryCriteria WithPendingRequestsLargerThan(int amount);
        /// <summary>
        /// Returns only expired locks.
        /// </summary>
        /// <returns>Current criteria for method chaining</returns>
        ILockQueryCriteria WithOnlyExpired();
        /// <summary>
        /// Returns only non expired locks.
        /// </summary>
        /// <returns>Current criteria for method chaining</returns>
        ILockQueryCriteria WithOnlyNotExpired();
        /// <summary>
        /// Returns only locked locks.
        /// </summary>
        /// <returns>Current criteria for method chaining</returns>
        ILockQueryCriteria WithOnlyLocked() => WithFilterOnLockedBy(string.Empty);
        /// <summary>
        /// Returns only free locks.
        /// </summary>
        /// <returns>Current criteria for method chaining</returns>
        ILockQueryCriteria WithOnlyNotLocked() => WithLockedByEqualTo(null);
        /// <summary>
        /// Applies pagination to the search results.
        /// </summary>
        /// <param name="page">The page to returns the results from</param>
        /// <param name="pageSize">How many results to return per page</param>
        /// <returns>Current criteria for method chaining</returns>
        ILockQueryCriteria WithPagination(int page, int pageSize);
        /// <summary>
        /// Also counts <see cref="ILockInfo.PendingRequests"/>.
        /// Disabled by default to improve performance. Can be enabled by default depending on the implementation.
        /// </summary>
        /// <returns>Current criteria for method chaining</returns>
        ILockQueryCriteria WithPendingRequest();

        /// <summary>
        /// Orders the query results by <see cref="ILockInfo.Resource"/>.
        /// </summary>
        /// <param name="sortDescending">True to order descending, otherwise ascending</param>
        /// <returns>Current criteria for method chaining</returns>
        ILockQueryCriteria OrderByResource(bool sortDescending =  false);
        /// <summary>
        /// Orders the query results by <see cref="ILockInfo.LockedBy"/>.
        /// </summary>
        /// <param name="sortDescending">True to order descending, otherwise ascending</param>
        /// <returns>Current criteria for method chaining</returns>
        ILockQueryCriteria OrderByLockedBy(bool sortDescending = false);
        /// <summary>
        /// Orders the query results by <see cref="ILockInfo.LastLockDate"/>.
        /// </summary>
        /// <param name="sortDescending">True to order descending, otherwise ascending</param>
        /// <returns>Current criteria for method chaining</returns>
        ILockQueryCriteria OrderByLastLockDate(bool sortDescending = false);
        /// <summary>
        /// Orders the query results by <see cref="ILockInfo.LockedAt"/>.
        /// </summary>
        /// <param name="sortDescending">True to order descending, otherwise ascending</param>
        /// <returns>Current criteria for method chaining</returns>
        ILockQueryCriteria OrderByLockedAt(bool sortDescending = false);
        /// <summary>
        /// Orders the query results by <see cref="ILockInfo.ExpiryDate"/>.
        /// </summary>
        /// <param name="sortDescending">True to order descending, otherwise ascending</param>
        /// <returns>Current criteria for method chaining</returns>
        ILockQueryCriteria OrderByExpiryDate(bool sortDescending = false);
    }
}
