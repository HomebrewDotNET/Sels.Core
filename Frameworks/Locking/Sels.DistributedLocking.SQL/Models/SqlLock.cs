using Sels.DistributedLocking.Provider;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.DistributedLocking.SQL
{
    /// <summary>
    /// Model that maps to an sql row containing lock state.
    /// </summary>
    public class SqlLock : ILockInfo
    {
        /// <inheritdoc/>
        public string Resource { get; set; }
        /// <inheritdoc/>
        public string LockedBy { get; set; }
        /// <inheritdoc/>
        public DateTimeOffset? LockedAt { get; set; }
        /// <inheritdoc/>
        public DateTimeOffset? LastLockDate { get; set; }
        /// <inheritdoc/>
        public DateTimeOffset? ExpiryDate { get; set; }
        /// <inheritdoc/>
        public int PendingRequests { get; set; }

        /// <summary>
        /// All the pending requests for the lock. Useful for joins.
        /// </summary>
        List<SqlLockRequest> Requests { get; set; }
    }
}
