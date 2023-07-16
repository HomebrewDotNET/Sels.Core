using FluentMigrator;
using System;
using System.Collections.Generic;
using System.Text;
using Sels.Core.Extensions.Logging;
using Sels.Core.Extensions.Logging;
using System.Linq;
using Sels.Core.Extensions;

namespace Sels.DistributedLocking.MySQL.Migrations
{
    /// <summary>
    /// Pre migration that renames the lock tables from any old name to the new name.
    /// </summary>
    [Maintenance(MigrationStage.BeforeAll)]
    public class RenameLockTablesPreMigration : Migration
    {
        /// <inheritdoc/>
        public override void Up()
        {
            //var logger = MigrationState.MigrationLogger;
            //logger.Log($"Checking if rename of lock tables is needed");

            //// Lock table
            //if (!Schema.Table(MigrationState.LockTableName).Exists())
            //{
            //    logger.Log($"Lock table <{MigrationState.LockTableName}> does not exist. Checking if table already exists with an older name");

            //    var oldTableName = MigrationState.OldLockTableNames.FirstOrDefault(x => Schema.Table(x).Exists());

            //    if (oldTableName.HasValue())
            //    {
            //        logger.Log($"Found Lock table with old name <{oldTableName}>. Table will be renamed to <{MigrationState.LockTableName}>");
            //        Rename.Table(oldTableName).To(MigrationState.LockTableName);
            //    }
            //    else
            //    {
            //        logger.Log($"No Lock table exists with an old name. No rename will be executed");

            //    }
            //}

            //// Lock Request table
            //if (!Schema.Table(MigrationState.LockRequestTableName).Exists())
            //{
            //    logger.Log($"Lock Request table <{MigrationState.LockRequestTableName}> does not exist. Checking if table already exists with an older name");

            //    var oldTableName = MigrationState.OldLockRequestTableNames.FirstOrDefault(x => Schema.Table(x).Exists());

            //    if (oldTableName.HasValue())
            //    {
            //        logger.Log($"Found Lock Request table with old name <{oldTableName}>. Table will be renamed to <{MigrationState.LockRequestTableName}>");
            //        Rename.Table(oldTableName).To(MigrationState.LockRequestTableName);
            //    }
            //    else
            //    {
            //        logger.Log($"No Lock Request table exists with an old name. No rename will be executed");

            //    }
            //}
        }

        /// <inheritdoc/>
        public override void Down() => new NotSupportedException();
    }
}
