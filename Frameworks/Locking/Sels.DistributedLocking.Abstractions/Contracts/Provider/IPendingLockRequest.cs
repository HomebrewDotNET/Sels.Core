using Sels.DistributedLocking.Provider;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sels.DistributedLocking.Provider
{
    /// <summary>
    /// The lock request placed on a resource by a caller. Contains a callback task completed when the pending request gets assigned.
    /// </summary>
    public interface IPendingLockRequest : ILockRequest
    {
        /// <summary>
        /// True if <see cref="Callback"/> was completed already, otherwise false.
        /// </summary>
        bool Completed => Callback.IsCompleted;
        /// <summary>
        /// The task that will complete when a lock request gets assigned.
        /// </summary>
        Task<ILock> Callback { get; }
    }
}
