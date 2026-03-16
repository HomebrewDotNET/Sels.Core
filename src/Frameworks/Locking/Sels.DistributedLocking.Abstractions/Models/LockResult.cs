using Sels.DistributedLocking.Provider;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.DistributedLocking.Abstractions.Models
{
    /// <inheritdoc cref="ILockResult"/>
    public class LockResult : ILockResult
    {
        /// <inheritdoc/>
        public bool Success => AcquiredLock != null;
        /// <inheritdoc/>
        public ILock AcquiredLock { get; private set; }
        /// <inheritdoc/>
        public ILockInfo CurrentLockState { get; private set; }

        /// <summary>
        /// Creates a succesful lock result.
        /// </summary>
        /// <param name="acquiredLock">The lock that was acquired</param>
        /// <exception cref="ArgumentNullException"></exception>
        public LockResult(ILock acquiredLock) : this((ILockInfo)acquiredLock)
        {
            AcquiredLock = acquiredLock ?? throw new ArgumentNullException(nameof(acquiredLock));
        }

        /// <summary>
        /// Creates an unsuccesful lock result.
        /// </summary>
        /// <param name="currentLockState">The current state of the lock</param>
        /// <exception cref="ArgumentNullException"></exception>
        public LockResult(ILockInfo currentLockState)
        {
            CurrentLockState = currentLockState ?? throw new ArgumentNullException(nameof(currentLockState));
        }
    }
}
