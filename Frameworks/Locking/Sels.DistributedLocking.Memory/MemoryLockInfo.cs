using Sels.Core.Extensions;
using Sels.DistributedLocking.Provider;
using System;
using System.Collections.Concurrent;
using static Sels.DistributedLocking.Memory.MemoryLockingProvider;

namespace Sels.DistributedLocking.Memory
{
    /// <summary>
    /// Represents information about a lock in memory.
    /// </summary>
    internal class MemoryLockInfo : ILockInfo
    {
        /// <inheritdoc cref="MemoryLockInfo"/>
        /// <param name="resource">The resource the current lock info is being created for</param>
        public MemoryLockInfo(string resource)
        {
            Resource = resource.ValidateArgument(nameof(resource));
        }

        /// <inheritdoc cref="MemoryLockInfo"/>
        /// <param name="copy">The instance to copy state from</param>
        public MemoryLockInfo(MemoryLockInfo copy)
        {
            copy.ValidateArgument(nameof(copy));
            Resource = copy.Resource;
            LockedBy = copy.LockedBy;
            LockedAt = copy.LockedAt;
            LastLockDate = copy.LastLockDate;
            ExpiryDate = copy.ExpiryDate;
            Requests = copy.Requests;
        }

        /// <inheritdoc/>
        public string Resource { get; }
        /// <inheritdoc/>
        public string LockedBy { get; internal set; }
        /// <inheritdoc/>
        public DateTime? LockedAt { get; internal set; }
        /// <inheritdoc/>
        public DateTime? LastLockDate { get; internal set; }
        /// <inheritdoc/>
        public DateTime? ExpiryDate { get; internal set; }
        /// <summary>
        /// Pending locking requests for the current lock.
        /// </summary>
        public ConcurrentQueue<MemoryLockRequest> Requests { get; } = new ConcurrentQueue<MemoryLockRequest>();
        /// <inheritdoc/>
        public int PendingRequests => Requests.Count;
    }
}
