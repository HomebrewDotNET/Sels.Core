using Sels.Core.Locking.Provider;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Locking
{
    /// <summary>
    /// Thrown when a lock could not be placed within the requested timeout.
    /// </summary>
    public class LockTimeoutException : Exception
    {
        // Properties
        /// <summary>
        /// The lock that could not be placed.
        /// </summary>
        public ILockInfo Lock { get; }
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
        /// <param name="lockInfo"><inheritdoc cref="Lock"/></param>
        /// <param name="timeout"><inheritdoc cref="Timeout"/></param>
        public LockTimeoutException(string requester, ILockInfo lockInfo, TimeSpan timeout) : base($"A lock on resource <{lockInfo?.Resource}> could not be placed by <{requester}> within <{timeout}>")
        {
            Lock = lockInfo ?? throw new ArgumentNullException(nameof(lockInfo));
            Requester = !string.IsNullOrWhiteSpace(requester) ? requester : throw new ArgumentNullException(nameof(lockInfo));
            Timeout = timeout;
        }
    }
}
