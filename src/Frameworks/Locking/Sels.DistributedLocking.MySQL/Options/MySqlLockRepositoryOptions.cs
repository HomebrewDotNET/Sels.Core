using Microsoft.Extensions.Caching.Memory;
using Sels.DistributedLocking.MySQL.Repository;
using Sels.ObjectValidationFramework.Profile;
using Sels.SQL.QueryBuilder.Builder;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.DistributedLocking.MySQL.Options
{
    /// <summary>
    /// Exposes extra options for <see cref="MySqlLockRepository"/> and <see cref="MariaDbLockRepository"/>.
    /// </summary>
    public class MySqlLockRepositoryOptions
    {
        /// <summary>
        /// The threshold above which a warning will be logged when any method on <see cref="MySqlLockRepository"/> or <see cref="MariaDbLockRepository"/> takes longer to execute.
        /// </summary>
        public TimeSpan PerformanceWarningDurationThreshold { get; set; } = TimeSpan.FromMilliseconds(100);
        /// <summary>
        /// The threshold above which an error will be logged when any method on <see cref="MySqlLockRepository"/> or <see cref="MariaDbLockRepository"/>  takes longer to execute.
        /// </summary>
        public TimeSpan PerformanceErrorDurationThreshold { get; set; } = TimeSpan.FromMilliseconds(250);
    }
    /// <summary>
    /// Contains the validation rules for <see cref="MySqlLockRepositoryOptions"/>.
    /// </summary>
    internal class MySqlLockRepositoryOptionsValidationProfile : ValidationProfile<string>
    {
        /// <inheritdoc cref="MySqlLockRepositoryOptionsValidationProfile"/>
        public MySqlLockRepositoryOptionsValidationProfile()
        {
            CreateValidationFor<MySqlLockRepositoryOptions>()
                .ForProperty(x => x.PerformanceErrorDurationThreshold)
                    .ValidIf(x => x.Value > x.Source.PerformanceWarningDurationThreshold, x => $"Must be larger than <{nameof(x.Source.PerformanceWarningDurationThreshold)}>");
        }
    }
}
