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
        public DateTime? LockedAt { get; set; }
        /// <inheritdoc/>
        public DateTime? LastLockDate { get; set; }
        /// <inheritdoc/>
        public DateTime? ExpiryDate { get; set; }
        /// <inheritdoc/>
        public int PendingRequests { get; set; }

        /// <summary>
        /// Converts all dates from utc to local.
        /// </summary>
        /// <returns>Current instance</returns>
        public SqlLock SetFromUtc()
        {
            if (LockedAt.HasValue && LockedAt.Value.Kind == DateTimeKind.Unspecified) LockedAt = DateTime.SpecifyKind(LockedAt.Value, DateTimeKind.Utc).ToLocalTime();
            if (LastLockDate.HasValue && LastLockDate.Value.Kind == DateTimeKind.Unspecified) LastLockDate = DateTime.SpecifyKind(LastLockDate.Value, DateTimeKind.Utc).ToLocalTime();
            if (ExpiryDate.HasValue && ExpiryDate.Value.Kind == DateTimeKind.Unspecified) ExpiryDate = DateTime.SpecifyKind(ExpiryDate.Value, DateTimeKind.Utc).ToLocalTime();

            return this;
        }
    }
}
