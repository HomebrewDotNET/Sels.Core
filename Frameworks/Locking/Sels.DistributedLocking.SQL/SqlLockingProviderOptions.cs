using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.DistributedLocking.SQL
{
    /// <summary>
    /// Exposes configuration options for <see cref="SqlLockingProvider"/>.
    /// </summary>
    public class SqlLockingProviderOptions
    {
        // Fields
        private SqlLockCleanupMethod _cleanupMethod;
        private int? _cleanupAmount;

        // Properties
        /// <summary>
        /// The interval that will be used by <see cref="SqlLockingProvider"/> to do database maintenance.
        /// </summary>
        public TimeSpan MaintenanceInterval { get; set; } = new TimeSpan(0, 15, 0);
        /// <summary>
        /// If cleanup of locks is enabled.
        /// </summary>
        public bool IsCleanupEnabled { get; set; }
        /// <inheritdoc cref="SqlLockCleanupMethod"/>
        public SqlLockCleanupMethod CleanupMethod { get => _cleanupMethod; set { _cleanupMethod = value; SetDefaultAmount(); } }
        /// <summary>
        /// The value to configure <see cref="CleanupMethod"/>.
        /// </summary>
        public int? CleanupAmount { get => _cleanupAmount; set { _cleanupAmount = value; SetDefaultAmount(); } }

        /// <summary>
        /// Dictates how stale locks are handled. If set to true a <see cref="StaleLockException"/> or <see cref="ResourceAlreadyLockedException"/> will be thrown when actions are performed on a stale lock. When set to false the action will fail silently.
        /// </summary>
        public bool ThrowOnStaleLock { get; set; } = true;

        /// <summary>
        /// How many milliseconds before a lock expires to extend the expiry date.
        /// </summary>
        public int ExpiryOffset { get; set; } = 1000;

        /// <summary>
        /// How often to check pending requests in milliseconds if they were assigned. Each resource will have it's own poller. Requests are also timed out by the same poller so setting it too high will mean requests will get timed out way past the provided timeout.
        /// </summary>
        public int RequestPollingRate { get; set; } = 1000;

        /// <inheritdoc/>
        public SqlLockingProviderOptions()
        {
            SetDefaultAmount();
        }

        private void SetDefaultAmount()
        {
            if (!_cleanupAmount.HasValue)
            {
                switch (_cleanupMethod)
                {
                    case SqlLockCleanupMethod.Time:
                        _cleanupAmount = 600000;
                        break;
                    case SqlLockCleanupMethod.Amount:
                        _cleanupAmount = 1000;
                        break;
                    case SqlLockCleanupMethod.Always:
                        _cleanupAmount = 0;
                        break;
                    default:
                        throw new NotSupportedException($"Cleanup method <{_cleanupMethod}> is not supported");
                }
            }
        }
    }

    /// <summary>
    /// Defines when inactive/expired locks are removed from the lock table.
    /// </summary>
    public enum SqlLockCleanupMethod
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
        Always = 2
    }
}
