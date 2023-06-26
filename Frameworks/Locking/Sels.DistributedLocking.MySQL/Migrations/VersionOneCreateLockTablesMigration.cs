using FluentMigrator;
using Sels.DistributedLocking.SQL;
using System;
using System.Collections.Generic;
using System.Text;
using Sels.Core.Extensions.Logging;
using Sels.Core.Extensions.Logging.Advanced;

namespace Sels.DistributedLocking.MySQL.Migrations
{
    /// <summary>
    /// Migration that creates the inital lock tables and their indexes.
    /// </summary>
    [Migration(1)]
    public class VersionOneCreateLockTablesMigration : AutoReversingMigration
    {
        /// <inheritdoc/>
        public override void Up()
        {
            var logger = MigrationState.MigrationLogger;

            logger.Log($"Deploying version 1 of Lock tables");

            // Lock table
            if (!Schema.Table(MigrationState.LockTableName).Exists())
            {
                Create.Table(MigrationState.LockTableName)
                        .WithColumn("Resource").AsString().PrimaryKey($"PK_{MigrationState.LockTableName}").NotNullable()
                        .WithColumn("LockedBy").AsString().Nullable()
                        .WithColumn("LockedAt").AsCustom("DateTime(6)").Nullable()
                        .WithColumn("LastLockDate").AsCustom("DateTime(6)").Nullable()
                        .WithColumn("ExpiryDate").AsCustom("DateTime(6)").Nullable();
            }
            else
            {
                logger.Warning($"Table with name <{MigrationState.LockTableName}> already exists. Skipping");
            }

            // Lock Request table
            if (!Schema.Table(MigrationState.LockRequestTableName).Exists())
            {
                Create.Table(MigrationState.LockRequestTableName)
                        .WithColumn("Id").AsInt64().PrimaryKey($"PK_{MigrationState.LockRequestTableName}").Identity()
                        .WithColumn("Resource").AsString().NotNullable()
                            .ForeignKey("FK_LockRequest_Lock", MigrationState.LockTableName, "Resource")
                            .Indexed("IX_Resource")
                        .WithColumn("Requester").AsString().NotNullable()
                        .WithColumn("ExpiryTime").AsDouble().Nullable()
                        .WithColumn("KeepAlive").AsBoolean().NotNullable()
                        .WithColumn("Timeout").AsCustom("DateTime(6)").Nullable()
                        .WithColumn("CreatedAt").AsCustom("DateTime(6)").NotNullable();
            }
            else
            {
                logger.Warning($"Table with name <{MigrationState.LockRequestTableName}> already exists. Skipping");
            }

            // Indexes
            if (!Schema.Table(MigrationState.LockTableName).Index("IX_Resource_LockedBy_LastLockDate_ExpiryDate").Exists())
            {
                Create.Index("IX_Resource_LockedBy_LastLockDate_ExpiryDate").OnTable(MigrationState.LockTableName)
                        .OnColumn("Resource").Ascending()
                        .OnColumn("LockedBy").Ascending()
                        .OnColumn("LastLockDate").Descending()
                        .OnColumn("ExpiryDate").Descending();
            }
            else
            {
                logger.Warning($"Index IX_Resource_LockedBy_LastLockDate_ExpiryDate already exists on table <{MigrationState.LockTableName}>. Skipping");
            }

            if (!Schema.Table(MigrationState.LockRequestTableName).Index("IX_Resource_CreatedAt").Exists())
            {
                Create.Index("IX_Resource_CreatedAt").OnTable(MigrationState.LockRequestTableName)
                        .OnColumn("Resource").Ascending()
                        .OnColumn("CreatedAt").Ascending();
            }
            else
            {
                logger.Warning($"Index IX_Resource_LockedBy_LastLockDate_ExpiryDate already exists on table <{MigrationState.LockTableName}>. Skipping");
            }
        }
    }
}
