using Sels.Core.Locking.Provider;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Locking
{
    /// <summary>
    /// Thrown when an action was requested to be performed on a lock but was not held by the requester.
    /// </summary>
    public class ResourceAlreadyLockedException : Exception
    {
        // Properties
        /// <summary>
        /// The lock the action was requested on.
        /// </summary>
        public ILockInfo Lock { get; }
        /// <summary>
        /// The requester who performed the action.
        /// </summary>
        public string Requester { get; }

        /// <inheritdoc cref="LockTimeoutException"/>
        /// <param name="requester"><inheritdoc cref="Requester"/></param>
        /// <param name="lockInfo"><inheritdoc cref="Lock"/></param>
        public ResourceAlreadyLockedException(string requester, ILockInfo lockInfo) : base($"The action requested by <{requester}> could not be performed on lock <{lockInfo?.Resource}> because it is currently not held by <{requester}>")
        {
            Lock = lockInfo ?? throw new ArgumentNullException(nameof(lockInfo));
            Requester = !string.IsNullOrWhiteSpace(requester) ? requester : throw new ArgumentNullException(nameof(lockInfo));
        }
    }
}
