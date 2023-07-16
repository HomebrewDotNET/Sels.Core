using System;
using System.Collections.Generic;
using System.Text;
using Sels.DistributedLocking.MySQL.Repository;

namespace Sels.DistributedLocking.MySQL.Options
{
    /// <summary>
    /// Exposes extra options for the deployment of the database schema for <see cref="MySqlLockRepository"/>.
    /// </summary>
    public class MySqlLockRepositoryDeploymentOptions
    {
        /// <summary>
        /// Set to true to deploy the database schema automatically, set to false when the tables will be deployed manually.
        /// Table structure should match the following objects (Property names are the column names): Sels.DistributedLocking.SQL.SqlLock and Sels.DistributedLocking.SQL.SqlLockRequest 
        /// </summary>
        public bool DeploySchema { get; set; } = true;
        /// <summary>
        /// The name of the table that contains the locks.
        /// Name should be static. Changing the name while the schema was already deployed will cause issues because the queries won't be in sync with the schema.
        /// </summary>
        public string LockTableName { get; set; } = "Distributed.Lock";
        /// <summary>
        /// The name of the table that contains the pending lock requests.
        /// Name should be static. Changing the name while the schema was already deployed will cause issues because the queries won't be in sync with the schema.
        /// </summary>
        public string LockRequestTableName { get; set; } = "Distributed.LockRequest";
        /// <summary>
        /// The name of the table that contains the current schema version.
        /// Changing the name will cause the schema to be deployed again.
        /// </summary>
        public string VersionTableName { get; set; } = "Distributed.SchemaVersion";
        /// <summary>
        /// If exception from the automatic schema deployment should be ignored.
        /// </summary>
        public bool IgnoreMigrationExceptions { get; set; } = true; 

        /// <summary>
        /// The old table names for <see cref="LockTableName"/>. A rename will be executed before any migrations if a table exists with any of the provided names.
        /// </summary>
        private string[] OldLockTableNames { get; set; }
        /// <summary>
        /// The old table names for <see cref="LockRequestTableName"/>. A rename will be executed before any migrations if a table exists with any of the provided names.
        /// </summary>
        private string[] OldLockRequestTableNames { get; set; }
    }
}
