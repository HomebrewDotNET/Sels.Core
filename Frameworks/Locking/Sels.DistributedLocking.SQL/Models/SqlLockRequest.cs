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
        /// <summary>
        /// Indicates that the lock request is assigned. Lock request can then be completed and removed afterwards.
        /// </summary>
        public bool IsAssigned { get; set; }

        TimeSpan? ILockRequest.ExpiryTime => ExpiryTime != null ? TimeSpan.FromSeconds(ExpiryTime.Value) : (TimeSpan?)null;

        /// <summary>
        /// Converts all dates from utc to local.
        /// </summary>
        /// <returns>Current instance</returns>
        public SqlLockRequest SetToLocal()
        {
            if (Timeout.HasValue && Timeout.Value.Kind != DateTimeKind.Local) Timeout = DateTime.SpecifyKind(Timeout.Value, DateTimeKind.Utc).ToLocalTime();
            if(CreatedAt.Kind != DateTimeKind.Local) CreatedAt = DateTime.SpecifyKind(CreatedAt, DateTimeKind.Utc).ToLocalTime();

            return this;
        }

        /// <summary>
        /// Converts all dates to utc.
        /// </summary>
        /// <returns>Current instance</returns>
        public SqlLockRequest SetToUtc()
        {
            if (Timeout.HasValue && Timeout.Value.Kind != DateTimeKind.Utc) Timeout = Timeout.Value.ToUniversalTime();
            if (CreatedAt.Kind != DateTimeKind.Utc) CreatedAt = CreatedAt.ToUniversalTime();

            return this;
        }
    }
}
