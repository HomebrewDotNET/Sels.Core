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
        private SqlLockCleanupMethod _cleanupMethod = SqlLockCleanupMethod.Amount;
        private int? _cleanupAmount;

        // Properties
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
        /// The interval that will be used by <see cref="SqlLockingProvider"/> to do self/database maintenance.
        /// </summary>
        public TimeSpan MaintenanceInterval { get; set; } = TimeSpan.FromMinutes(5);
        /// <summary>
        /// How often <see cref="SqlLockingProvider"/> will try to assign pending requests.
        /// </summary>
        public TimeSpan RequestAssignmentInterval { get; set; } = TimeSpan.FromMilliseconds(500);
        /// <summary>
        /// How often <see cref="SqlLockingProvider"/> will check if requests have been assigned.
        /// </summary>
        public TimeSpan RequestCompletionInterval { get; set; } = TimeSpan.FromMilliseconds(250);
        /// <summary>
        /// How many requests will be checked at the same time by the request completion worker.
        /// </summary>
        public int RequestCheckLimit { get; set; } = 100;

        /// <summary>
        /// The threshold above which a warning will be logged when any method on <see cref="SqlLockingProvider"/> takes longer to execute.
        /// </summary>
        public TimeSpan PerformanceWarningDurationThreshold { get; set; } = TimeSpan.FromMilliseconds(250);
        /// <summary>
        /// The threshold above which an error will be logged when any method on <see cref="SqlLockingProvider"/> takes longer to execute.
        /// </summary>
        public TimeSpan PerformanceErrorDurationThreshold { get; set; } = TimeSpan.FromMilliseconds(1000);

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
                        _cleanupAmount = 10000;
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
