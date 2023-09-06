using Sels.DistributedLocking.Provider;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.DistributedLocking
{
    /// <summary>
    /// Thrown when a lock could not be placed within the requested timeout.
    /// </summary>
    public class LockTimeoutException : Exception
    {
        // Properties
        /// <summary>
        /// The name of the resource where the lock could not be placed upon.
        /// </summary>
        public string Resource { get; }
        /// <summary>
        /// The requested timeout.
        /// </summary>
        public TimeSpan Timeout { get; }
        /// <summary>
        /// Who requested the lock.
        /// </summary>
        public string Requester { get; }

        /// <inheritdoc cref="LockTimeoutException"/>
        /// <param name="requester"><inheritdoc cref="Requester"/></param>
        /// <param name="resource"><inheritdoc cref="Resource"/></param>
        /// <param name="timeout"><inheritdoc cref="Timeout"/></param>
        public LockTimeoutException(string requester, string resource, TimeSpan timeout) : base($"A lock on resource <{resource}> could not be placed by <{requester}> within <{timeout}>")
        {
            Resource = resource ?? throw new ArgumentNullException(nameof(resource));
            Requester = !string.IsNullOrWhiteSpace(requester) ? requester : throw new ArgumentNullException(nameof(requester));
            Timeout = timeout;
        }
    }
}
