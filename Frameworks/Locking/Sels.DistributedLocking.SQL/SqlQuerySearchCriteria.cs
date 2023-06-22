using Sels.Core.Extensions;
using Sels.DistributedLocking.Provider;
using System;
using System.Collections.Generic;

namespace Sels.DistributedLocking.SQL
{
    /// <summary>
    /// Contains the search criteria from <see cref="ILockQueryCriteria"/>.
    /// </summary>
    public class SqlQuerySearchCriteria : ILockQueryCriteria
    {
        // Properties
        /// <summary>
        /// The filters for the query. IsFullMatch will be set to true for EqualTo comparisons, false for LIKE. If Filter is null and IsFullMatch then equality check should be IS NULL.
        /// </summary>
        public List<(string Column, string Filter, bool IsFullMatch)> Filters { get; private set; } = new List<(string Column, string Filter, bool IsFullMatch)>();
        /// <summary>
        /// Null for no filter, false to show non expired locks, true to show only expired locks.
        /// </summary>
        public bool? ShowExpiredOnly { get; set; }
        /// <summary>
        /// The pagination to apply. Null if not required.
        /// </summary>
        public (int Page, int PageSize)? Pagination { get; private set; }
        /// <summary>
        /// The columns to sort by.
        /// </summary>
        public List<(string Column, bool SortDescending)> SortColumns { get; private set; } = new List<(string Column, bool SortDescending)>();
        /// <summary>
        /// If pending requests should also be set in the query.
        /// </summary>
        public bool IncludePendingRequests { get; set; }
        /// <summary>
        /// A lock must have equal or higher amount of pending requests before being returned in the query, null means no filter.
        /// </summary>
        public int? PendingRequestFilter { get; set; }

        /// <inheritdoc cref="SqlQuerySearchCriteria"/>
        /// <param name="configurator">Delegate that configures this instance</param>
        public SqlQuerySearchCriteria(Action<ILockQueryCriteria> configurator)
        {
            configurator.ValidateArgument(nameof(configurator))(this);
        }

        /// <inheritdoc/>
        ILockQueryCriteria ILockQueryCriteria.WithFilterOnLockedBy(string filter)
        {
            filter.ValidateArgument(nameof(filter));
            Filters.Add((nameof(SqlLock.LockedBy), filter, false));
            return this;
        }
        /// <inheritdoc/>
        ILockQueryCriteria ILockQueryCriteria.WithFilterOnResource(string filter)
        {
            filter.ValidateArgument(nameof(filter));
            Filters.Add((nameof(SqlLock.Resource), filter, false));
            return this;
        }
        /// <inheritdoc/>
        ILockQueryCriteria ILockQueryCriteria.WithLockedByEqualTo(string lockOwner)
        {
            Filters.Add((nameof(SqlLock.LockedBy), lockOwner, true));
            return this;
        }
        /// <inheritdoc/>
        ILockQueryCriteria ILockQueryCriteria.WithOnlyExpired()
        {
            ShowExpiredOnly = true;
            return this;
        }
        /// <inheritdoc/>
        ILockQueryCriteria ILockQueryCriteria.WithOnlyNotExpired()
        {
            ShowExpiredOnly = true;
            return this;
        }
        /// <inheritdoc/>
        ILockQueryCriteria ILockQueryCriteria.WithPagination(int page, int pageSize)
        {
            page.ValidateArgumentLarger(nameof(page), 0);
            page.ValidateArgumentLarger(nameof(pageSize), 0);

            Pagination = (page, pageSize);
            return this;
        }
        /// <inheritdoc/>
        ILockQueryCriteria ILockQueryCriteria.WithPendingRequest()
        {
            IncludePendingRequests = true;
            return this;
        }
        ILockQueryCriteria ILockQueryCriteria.WithPendingRequestsLargerThan(int amount)
        {
            amount.ValidateArgumentLargerOrEqual(nameof(amount), 0);
            PendingRequestFilter = amount;
            IncludePendingRequests = true;
            return this;
        }

        /// <inheritdoc/>
        ILockQueryCriteria ILockQueryCriteria.OrderByExpiryDate(bool sortDescending) => SortBy(nameof(SqlLock.ExpiryDate), sortDescending);
        /// <inheritdoc/>
        ILockQueryCriteria ILockQueryCriteria.OrderByLastLockDate(bool sortDescending) => SortBy(nameof(SqlLock.LastLockDate), sortDescending);
        /// <inheritdoc/>
        ILockQueryCriteria ILockQueryCriteria.OrderByLockedAt(bool sortDescending) => SortBy(nameof(SqlLock.LockedAt), sortDescending);
        /// <inheritdoc/>
        ILockQueryCriteria ILockQueryCriteria.OrderByLockedBy(bool sortDescending) => SortBy(nameof(SqlLock.LockedBy), sortDescending);
        /// <inheritdoc/>
        ILockQueryCriteria ILockQueryCriteria.OrderByResource(bool sortDescending) => SortBy(nameof(SqlLock.Resource), sortDescending);

        private ILockQueryCriteria SortBy(string column, bool sortDescending)
        {
            column.ValidateArgument(nameof(column));
            SortColumns.Add((column, sortDescending));
            return this;
        }
    }
}
