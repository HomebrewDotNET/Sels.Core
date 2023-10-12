﻿using FluentMigrator;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.DistributedLocking.MySQL.Migrations
{
    /// <summary>
    /// Releases the deployment lock.
    /// </summary>
    [Maintenance(MigrationStage.AfterAll)]
    public class ReleaseDeploymentLock : Migration
    {
        /// <inheritdoc/>
        public override void Up()
        {
            Execute.Sql($"SELECT RELEASE_LOCK('{MigrationState.DeploymentLockName}');");
        }

        /// <inheritdoc/>
        public override void Down()
        {
        }
    }
}
