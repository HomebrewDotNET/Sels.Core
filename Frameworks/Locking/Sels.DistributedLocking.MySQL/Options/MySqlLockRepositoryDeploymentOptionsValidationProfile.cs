using Sels.ObjectValidationFramework.Profile;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.DistributedLocking.MySQL.Options
{
    /// <summary>
    /// Contains validation for <see cref="MySqlLockRepositoryDeploymentOptions"/>.
    /// </summary>
    internal class MySqlLockRepositoryDeploymentOptionsValidationProfile : ValidationProfile<string>
    {
        /// <inheritdoc cref="MySqlLockRepositoryDeploymentOptionsValidationProfile"/>
        public MySqlLockRepositoryDeploymentOptionsValidationProfile()
        {
            CreateValidationFor<MySqlLockRepositoryDeploymentOptions>()
                .ForProperty(x => x.LockTableName)
                    .CannotBeNullOrWhitespace()
                .ForProperty(x => x.LockRequestTableName)
                    .CannotBeNullOrWhitespace()
                .ForProperty(x => x.VersionTableName)
                    .CannotBeNullOrWhitespace();
        }
    }
}
