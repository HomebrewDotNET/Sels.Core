using Sels.Core.Locking.Provider;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Locking
{
    /// <summary>
    /// Thrown when a lock is currently not being held anymore by the requester. This can happen when requesters are shared or when locks expire.
    /// </summary>
    public class StaleLockException : Exception
    {
        // Properties
        /// <summary>
        /// The owner of the stale lock.
        /// </summary>
        public string Requester { get; }
        /// <summary>
        /// The stale lock.
        /// </summary>
        public ILockInfo Lock { get; }

        /// <inheritdoc cref="StaleLockException"/>
        /// <param name="requester"><inheritdoc cref="Requester"/></param>
        /// <param name="staleLock"><inheritdoc cref="Lock"/></param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public StaleLockException(string requester, ILockInfo staleLock) : base($"Lock <{staleLock?.Resource}> is currently not held anymore by <{requester}>")
        {
            Requester = !string.IsNullOrWhiteSpace(requester) ? requester : throw new ArgumentException($"{nameof(requester)} cannot be null, empty or whitespace"); 
            Lock = staleLock ?? throw new ArgumentNullException(nameof(staleLock));
        }
    }
}
