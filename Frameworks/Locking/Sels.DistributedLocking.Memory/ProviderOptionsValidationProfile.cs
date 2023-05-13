using Microsoft.Extensions.Logging;
using Sels.ObjectValidationFramework.Profile;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.DistributedLocking.Memory
{
    /// <summary>
    /// Validation profile for validating <see cref="MemoryLockingProviderOptions"/>.
    /// </summary>
    internal class ProviderOptionsValidationProfile : ValidationProfile<string>
    {
        /// <inheritdoc cref="ProviderOptionsValidationProfile"/>
        /// <param name="logger">Optional logger for tracing</param>
        public ProviderOptionsValidationProfile(ILogger<ProviderOptionsValidationProfile> logger = null) : base(logger: logger)
        {
            CreateValidationFor<MemoryLockingProviderOptions>()
                .ForProperty(x => x.ExpiryOffset)
                    .MustBeLargerThan(0)
                .Switch(x => x.CleanupMethod)
                    .Case(MemoryLockCleanupMethod.Time)
                    .Case(MemoryLockCleanupMethod.Amount)
                    .Case(MemoryLockCleanupMethod.ProcessMemory)
                    .Then(b =>
                    {
                        b.ForProperty(x => x.CleanupAmount, ObjectValidationFramework.Target.TargetExecutionOptions.ExitOnInvalid)
                            .CannotBeNull()
                            .ValidIf(x => x.Value.Value > 0, x => $"Must be larger than 0 when cleanup method is set to <{x.Source.CleanupMethod}>. Value was <{x.Value}>");
                    });
        }
    }
}
