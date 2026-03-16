using System;

namespace Sels.DistributedLocking.Provider
{
    /// <summary>
    /// Represent a request on a lock that will be placed once the mentioned lock is unlocked.
    /// </summary>
    public interface ILockRequest
    {
        /// <summary>
        /// The resource that the request is placed on.
        /// </summary>
        string Resource { get; }
        /// <summary>
        /// Who requested the lock.
        /// </summary>
        string Requester { get; }
        /// <summary>
        /// How long the lock will be held after placing it. When set to null the lock will not expire.
        /// </summary>
        TimeSpan? ExpiryTime { get; }
        /// <summary>
        /// If the lock will be kept alive after it's placed.
        /// </summary>
        bool KeepAlive { get; }
        /// <summary>
        /// When the current request expires. When set to null the request will never expire.
        /// </summary>
        DateTime? Timeout { get; }
        /// <summary>
        /// When the request was created.
        /// </summary>
        DateTime CreatedAt { get; }
    }
}
