using Sels.DistributedLocking.Provider;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.DistributedLocking.SQL
{
    /// <summary>
    /// Model that maps to an sql row containing a pending lock request for a resource.
    /// </summary>
    public class SqlLockRequest : ILockRequest
    {
        /// <summary>
        /// The unique id of the request. (Primary key)
        /// </summary>
        public long Id { get; set; }
        /// <inheritdoc/>
        public string Resource { get; set; }
        /// <inheritdoc/>
        public string Requester { get; set; }
        /// <summary>
        /// How many seconds to keep the lock alive for after it is placed.
        /// </summary>
        public double? ExpiryTime { get; set; }
        /// <inheritdoc/>
        public bool KeepAlive { get; set; }
        /// <inheritdoc/>
        public DateTime? Timeout { get; set; }
        /// <inheritdoc/>
        public DateTime CreatedAt { get; set; }

        TimeSpan? ILockRequest.ExpiryTime => ExpiryTime != null ? TimeSpan.FromSeconds(ExpiryTime.Value) : (TimeSpan?)null;

        /// <summary>
        /// Converts all dates to utc kind.
        /// </summary>
        /// <returns>Current instance</returns>
        public SqlLockRequest SetFromUtc()
        {
            if (Timeout.HasValue && Timeout.Value.Kind == DateTimeKind.Unspecified) Timeout = DateTime.SpecifyKind(Timeout.Value, DateTimeKind.Utc).ToLocalTime();
            if(CreatedAt.Kind == DateTimeKind.Unspecified) CreatedAt = DateTime.SpecifyKind(CreatedAt, DateTimeKind.Utc).ToLocalTime();

            return this;
        }
    }
}
