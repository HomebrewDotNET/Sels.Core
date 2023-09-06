using Microsoft.Extensions.Logging;
using Sels.ObjectValidationFramework.Profile;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.DistributedLocking.SQL
{
    /// <summary>
    /// Validation profile for validating <see cref="SqlLockingProviderOptions"/>.
    /// </summary>
    internal class ProviderOptionsValidationProfile : ValidationProfile<string>
    {
        /// <inheritdoc cref="ProviderOptionsValidationProfile"/>
        /// <param name="logger">Optional logger for tracing</param>
        public ProviderOptionsValidationProfile(ILogger<ProviderOptionsValidationProfile> logger = null) : base(logger: logger)
        {
            CreateValidationFor<SqlLockingProviderOptions>()
                .ForProperty(x => x.ExpiryOffset)
                    .MustBeLargerThan(0)
                .ForProperty(x => x.MaintenanceInterval)
                    .MustBeLargerThan(TimeSpan.FromMilliseconds(100))
                .ForProperty(x => x.RequestAssignmentInterval)
                    .MustBeLargerThan(TimeSpan.FromMilliseconds(10))
                .ForProperty(x => x.RequestCompletionInterval)
                    .MustBeLargerThan(TimeSpan.FromMilliseconds(10))
                .ForProperty(x => x.RequestCheckLimit)
                    .MustBeLargerOrEqualTo(1)
                .ForProperty(x => x.PerformanceErrorDurationThreshold)
                    .ValidIf(x => x.Value > x.Source.PerformanceWarningDurationThreshold, x => $"Must be larger than <{nameof(x.Source.PerformanceWarningDurationThreshold)}>")
                .Switch(x => x.CleanupMethod)
                    .Case(SqlLockCleanupMethod.Time)
                    .Case(SqlLockCleanupMethod.Amount)
                    .Then(b =>
                    {
                        b.ForProperty(x => x.CleanupAmount, ObjectValidationFramework.Target.TargetExecutionOptions.ExitOnInvalid)
                            .CannotBeNull()
                            .ValidIf(x => x.Value.Value > 0, x => $"Must be larger than 0 when cleanup method is set to <{x.Source.CleanupMethod}>. Value was <{x.Value}>");
                    });
        }
    }
}
