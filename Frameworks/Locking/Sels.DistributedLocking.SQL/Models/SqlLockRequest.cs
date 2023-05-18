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
        /// <inheritdoc/>
        public TimeSpan? ExpiryTime { get; set; }
        /// <inheritdoc/>
        public bool KeepAlive { get; set; }
        /// <inheritdoc/>
        public DateTimeOffset? Timeout { get; set; }
        /// <inheritdoc/>
        public DateTimeOffset CreatedAt { get; set; }
    }
}
