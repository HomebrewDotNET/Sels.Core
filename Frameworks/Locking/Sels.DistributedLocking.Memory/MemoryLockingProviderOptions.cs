using Sels.Core.Extensions;
using Sels.Core.Extensions.Validation;
using Sels.Core.Validation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.DistributedLocking.Memory
{
    /// <summary>
    /// Exposes configuration options for <see cref="MemoryLockingProvider"/>.
    /// </summary>
    public class MemoryLockingProviderOptions
    {
        // Fields
        private MemoryLockCleanupMethod _cleanupMethod = MemoryLockCleanupMethod.ProcessMemory;
        private long? _cleanupAmount;

        // Properties
        /// <summary>
        /// The interval that will be used by <see cref="MemoryLockingProvider"/> to perform cleanup of the in-memeory locks to free up memory. When set to <see cref="TimeSpan.Zero"/> no locks will be cleaned up.
        /// </summary>
        public TimeSpan CleanupInterval { get; set; } = TimeSpan.FromMinutes(1);
        /// <summary>
        /// If cleanup of locks is enabled.
        /// </summary>
        public bool IsCleanupEnabled => CleanupInterval.TotalMilliseconds > 0;
        /// <inheritdoc cref="MemoryLockCleanupMethod"/>
        public MemoryLockCleanupMethod CleanupMethod { get => _cleanupMethod; set { _cleanupMethod = value; SetDefaultAmount(); } }
        /// <summary>
        /// The value to configure <see cref="CleanupMethod"/>.
        /// </summary>
        public long? CleanupAmount { get => _cleanupAmount; set { _cleanupAmount = value; SetDefaultAmount(); } }
        /// <summary>
        /// Dictates how stale locks are handled. If set to true a <see cref="StaleLockException"/> or <see cref="ResourceAlreadyLockedException"/> will be thrown when actions are performed on a stale lock. When set to false the action will fail silently.
        /// </summary>
        public bool ThrowOnStaleLock { get; set; } = true;
        /// <summary>
        /// How many milliseconds before a lock expires to extend the expiry date.
        /// </summary>
        public int ExpiryOffset { get; set; } = 1000;

        /// <inheritdoc/>
        public MemoryLockingProviderOptions()
        {
            SetDefaultAmount();
        }

        private void SetDefaultAmount()
        {
            if (!_cleanupAmount.HasValue)
            {
                switch (_cleanupMethod)
                {
                    case MemoryLockCleanupMethod.Time:
                        _cleanupAmount = 600000;
                        break;
                    case MemoryLockCleanupMethod.Amount:
                        _cleanupAmount = 1000;
                        break;
                    case MemoryLockCleanupMethod.Always:
                        _cleanupAmount = 0;
                        break;
                    case MemoryLockCleanupMethod.ProcessMemory:
                        // Equal to 1 gibibyte
                        _cleanupAmount = 1024 * 1024* 1024;
                        break;
                    default: 
                        throw new NotSupportedException($"Cleanup method <{_cleanupMethod}> is not supported");
                }
            }
        }
    }

    /// <summary>
    /// Defines when memory locks should be removed from memory. 
    /// </summary>
    public enum MemoryLockCleanupMethod
    {
        /// <summary>
        /// Inactive locks older than the configured amount (in milliseconds) will be removed.
        /// </summary>
        Time = 0,
        /// <summary>
        /// When the total amount of active locks exceeds the configured amount, all inactive locks will be removed.
        /// </summary>
        Amount = 1,
        /// <summary>
        /// All inactive locks will be removed.
        /// </summary>
        Always = 2,
        /// <summary>
        /// All inactive locks will be removed when the memory usage of the current process exceeds the configured amount (in bytes)
        /// </summary>
        ProcessMemory = 3
    }
}
