using System;

namespace Sels.DistributedLocking.Provider
{
    /// <summary>
    /// Contains information about a lock placed on a resource.
    /// </summary>
    public interface ILockInfo
    {
        /// <summary>
        /// The resource the lock is held on.
        /// </summary>
        string Resource { get; }
        /// <summary>
        /// Who locked the resource.
        /// </summary>
        string LockedBy { get; }
        /// <summary>
        /// When the current lock was locked by <see cref="LockedBy"/>. Will be null when the lock is free.
        /// </summary>
        DateTime? LockedAt { get; }
        /// <summary>
        /// The last time when the lock was held by someone.
        /// </summary>
        DateTime? LastLockDate { get; }
        /// <summary>
        /// When the lock is set to expire. When a lock expires others will be able lock it instead. When set to null the lock never expires.
        /// </summary>
        DateTime? ExpiryDate { get; }
        /// <summary>
        /// How many pending requests there are for the current lock.
        /// </summary>
        int PendingRequests { get; }
    }
}
