using Microsoft.Extensions.Logging;
using Sels.Core;
using Sels.Core.Extensions;
using Sels.DistributedLocking.SQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sels.DistributedLocking.MySQL.Migrations
{
    /// <summary>
    /// Contains input parameters for the migrations.
    /// </summary>
    internal static class MigrationState
    {
        // Fields
        private static string _lockTableName = nameof(SqlLock);
        private static string _lockRequestTableName = nameof(SqlLockRequest);
        private static string[] _oldLockTableNames;
        private static string[] _oldLockRequestTableNames;

        /// <summary>
        /// Optional logger for tracing in the migrations.
        /// </summary>
        public static ILogger MigrationLogger { get; set; }
        /// <summary>
        /// The name of the table that contains the lock state.
        /// </summary>
        public static string LockTableName { get => _lockTableName; set => _lockTableName = value.ValidateArgumentNotNullOrWhitespace(nameof(LockTableName)); }
        /// <summary>
        /// The name of the table that contains the pending lock requests.
        /// </summary>
        public static string LockRequestTableName { get => _lockRequestTableName; set => _lockRequestTableName = value.ValidateArgumentNotNullOrWhitespace(nameof(LockRequestTableName)); }

        /// <summary>
        /// The old table names for <see cref="LockTableName"/>. A rename will be executed before any migrations if a table exists with any of the provided names.
        /// </summary>
        public static string[] OldLockTableNames { get => Helper.Collection.Enumerate(nameof(SqlLock), _oldLockTableNames).Where(x => x != null).ToArray(); set => _oldLockTableNames = value; }
        /// <summary>
        /// The old table names for <see cref="LockRequestTableName"/>. A rename will be executed before any migrations if a table exists with any of the provided names.
        /// </summary>
        public static string[] OldLockRequestTableNames { get => Helper.Collection.Enumerate(nameof(SqlLockRequest), _oldLockRequestTableNames).Where(x => x != null).ToArray(); set => _oldLockRequestTableNames = value; }
    }
}
