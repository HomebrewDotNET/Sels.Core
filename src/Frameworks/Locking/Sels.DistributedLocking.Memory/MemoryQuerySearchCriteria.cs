using Sels.Core.Extensions;
using Sels.DistributedLocking.Provider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Sels.Core.Extensions.Collections;

namespace Sels.DistributedLocking.Memory
{
    /// <summary>
    /// Captures the search criteria from <see cref="ILockQueryCriteria"/>.
    /// </summary>
    internal class MemoryQuerySearchCriteria : ILockQueryCriteria
    {
        // Fields
        private Dictionary<string, List<Predicate<ILockInfo>>> _predicates;
        private List<(Expression<Func<MemoryLockInfo, object>> PropertyToSortBy, bool SortDescending)> _sortExpressions;

        // Properties
        /// <summary>
        /// The predicates to limit which locks are returned.
        /// </summary>
        public Dictionary<string, List<Predicate<ILockInfo>>> Predicates
        {
            get
            {
                if (_predicates == null) _predicates = new Dictionary<string, List<Predicate<ILockInfo>>>();
                return _predicates;
            }
        }
        /// <summary>
        /// Defines the sort order.
        /// </summary>
        public List<(Expression<Func<MemoryLockInfo, object>> PropertyToSortBy, bool SortDescending)> SortExpressions
        {
            get
            {
                if (_sortExpressions == null) _sortExpressions = new List<(Expression<Func<MemoryLockInfo, object>> PropertyToSortBy, bool SortDescending)>();
                return _sortExpressions;
            }
        }
        /// <summary>
        /// The pagination to apply. Null if not required.
        /// </summary>
        public (int Page, int PageSize)? Pagination { get; private set; }

        /// <inheritdoc cref="MemoryQuerySearchCriteria"/>
        /// <param name="configurator">Delegate that configures this instance</param>
        public MemoryQuerySearchCriteria(Action<ILockQueryCriteria> configurator)
        {
            configurator.ValidateArgument(nameof(configurator))(this);
        }

        /// <summary>
        /// Applies all configured filters on <paramref name="locks"/>.
        /// </summary>
        /// <param name="locks">The enumerator to filter</param>
        /// <returns><paramref name="locks"/> with filter applied</returns>
        public IEnumerable<MemoryLockInfo> ApplyFilter(IEnumerable<MemoryLockInfo> locks)
        {
            locks.ValidateArgument(nameof(locks));

            if (!_predicates.HasValue()) return locks;

            return locks.Where(l => _predicates.All(p => p.Value.Any(v => v(l))));
        }

        /// <summary>
        /// Sorts <paramref name="locks"/> with any configured sorting rules.
        /// </summary>
        /// <param name="locks">The locks to sort</param>
        /// <returns><paramref name="locks"/> sorted by any configured sorting rules</returns>
        public IEnumerable<MemoryLockInfo> ApplySorting(IEnumerable<MemoryLockInfo> locks)
        {
            locks.ValidateArgument(nameof(locks));

            if (_sortExpressions.HasValue())
            {
                IOrderedEnumerable<MemoryLockInfo> current = null;

                foreach (var (expression, sortDescending) in _sortExpressions)
                {
                    if (current == null)
                    {
                        if (sortDescending)
                        {
                            current = locks.OrderByDescending(expression.Compile());
                        }
                        else
                        {
                            current = locks.OrderBy(expression.Compile());
                        }
                    }
                    else
                    {
                        if (sortDescending)
                        {
                            current = current.ThenByDescending(expression.Compile());
                        }
                        else
                        {
                            current = current.ThenBy(expression.Compile());
                        }
                    }
                }

                return current;
            }

            return locks;
        }

        /// <inheritdoc/>
        ILockQueryCriteria ILockQueryCriteria.WithFilterOnLockedBy(string filter)
        {
            filter.ValidateArgument(nameof(filter));

            if (filter == string.Empty) Predicates.AddValueToList(nameof(ILockInfo.LockedBy), x => x.LockedBy != null);
            else Predicates.AddValueToList(nameof(ILockInfo.LockedBy), x => x.LockedBy != null && x.LockedBy.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0);
            return this;
        }
        /// <inheritdoc/>
        ILockQueryCriteria ILockQueryCriteria.WithFilterOnResource(string filter)
        {
            filter.ValidateArgument(nameof(filter));

            if (filter == string.Empty) Predicates.AddValueToList(nameof(ILockInfo.Resource), x => x.Resource != null);
            else Predicates.AddValueToList(nameof(ILockInfo.Resource), x => x.Resource != null && x.Resource.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0);
            return this;
        }
        /// <inheritdoc/>
        ILockQueryCriteria ILockQueryCriteria.WithLockedByEqualTo(string lockOwner)
        {
            if (lockOwner == null) Predicates.AddValueToList(nameof(ILockInfo.LockedBy), x => x.LockedBy == null);
            else Predicates.AddValueToList(nameof(ILockInfo.LockedBy), x => x.LockedBy != null && x.LockedBy.Equals(lockOwner, StringComparison.OrdinalIgnoreCase));

            return this;
        }
        /// <inheritdoc/>
        ILockQueryCriteria ILockQueryCriteria.WithPendingRequestsLargerThan(int amount)
        {
            amount.ValidateArgumentLargerOrEqual(nameof(amount), 0);
            Predicates.AddValueToList(nameof(ILockInfo.PendingRequests), x => x.PendingRequests > amount);
            return this;
        }
        /// <inheritdoc/>
        ILockQueryCriteria ILockQueryCriteria.WithOnlyExpired()
        {
            Predicates.AddValueToList(nameof(ILockInfo.ExpiryDate), x => x.ExpiryDate.HasValue && x.ExpiryDate.Value < DateTime.Now);
            return this;
        }
        /// <inheritdoc/>
        ILockQueryCriteria ILockQueryCriteria.WithOnlyNotExpired()
        {
            Predicates.AddValueToList(nameof(ILockInfo.ExpiryDate), x => x.ExpiryDate.HasValue && x.ExpiryDate.Value >= DateTime.Now);
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
        ILockQueryCriteria ILockQueryCriteria.IncludePendingRequest()
        {
            // Always enabled
            return this;
        }

        /// <inheritdoc/>
        ILockQueryCriteria ILockQueryCriteria.OrderByExpiryDate(bool sortDescending) => SortBy(x => x.ExpiryDate, sortDescending);
        /// <inheritdoc/>
        ILockQueryCriteria ILockQueryCriteria.OrderByLastLockDate(bool sortDescending) => SortBy(x => x.LastLockDate, sortDescending);
        /// <inheritdoc/>
        ILockQueryCriteria ILockQueryCriteria.OrderByLockedAt(bool sortDescending) => SortBy(x => x.LockedAt, sortDescending);
        /// <inheritdoc/>
        ILockQueryCriteria ILockQueryCriteria.OrderByLockedBy(bool sortDescending) => SortBy(x => x.LockedBy, sortDescending);
        /// <inheritdoc/>
        ILockQueryCriteria ILockQueryCriteria.OrderByResource(bool sortDescending) => SortBy(x => x.Resource, sortDescending);

        private ILockQueryCriteria SortBy(Expression<Func<MemoryLockInfo, object>> sortExpression, bool sortDescending)
        {
            sortExpression.ValidateArgument(nameof(sortExpression));
            SortExpressions.Add((sortExpression, sortDescending));
            return this;
        }
    }
}
