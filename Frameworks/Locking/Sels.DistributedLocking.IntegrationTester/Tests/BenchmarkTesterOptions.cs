using Sels.ObjectValidationFramework.Profile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.DistributedLocking.IntegrationTester.Tests
{
    /// <summary>
    /// Exposes extra options for  <see cref="BenchmarkTester"/>.
    /// </summary>
    public class BenchmarkTesterOptions
    {
        /// <summary>
        /// How many locks need to be persisted before starting the benchmark tests.
        /// </summary>
        public int StorageSize { get; set; } = 1000000;
        /// <summary>
        /// The ration of persisted locks that will have an expiry date set.
        /// </summary>
        public double ExpiryRatio { get; set; } = 0.5;
        /// <summary>
        /// The ratio of persisted locks with expiry date that will be expired.
        /// </summary>
        public double ExpiredRatio { get; set; } = 0.5;
        /// <summary>
        /// The ration of persisted locks without an expiry date that will be unlocked.
        /// </summary>
        public double UnlockedRatio { get; set; } = 0.5;
        /// <summary>
        /// The maximum allowed run time for each test.
        /// </summary>
        public TimeSpan RunTime { get; set; } = TimeSpan.FromMinutes(1);
        /// <summary>
        /// The maximum attempts per workers to execute.
        /// </summary>
        public int MaximumAttempts { get; set; } = 1000;
        /// <summary>
        /// How many workers will be used to benchmark.
        /// </summary>
        public int Workers { get; set; } = Environment.ProcessorCount * 2;
        /// <summary>
        /// The pool of resources that will be shared by workers when benchmarking collision between workers.
        /// </summary>
        public int ResourcePoolSize { get; set; } = Environment.ProcessorCount;
        /// <summary>
        /// The size of the result set to return when benchmarking query performance.
        /// </summary>
        public int QueryResultSetSize { get; set; } = 1000;
        /// <summary>
        /// Defines the ratio when running mixed tests. Used to determine to lock using TryLockAsync or LockAsync.
        /// </summary>
        public double TryLockToLockRatio { get; set; } = 0.75;
    }

    /// <summary>
    /// Contains the validation rules for <see cref="BenchmarkTesterOptions"/>.
    /// </summary>
    public class BenchmarkTesterOptionsValidationProfile : ValidationProfile<string>
    {
        /// <inheritdoc cref="BenchmarkTesterOptionsValidationProfile"/>
        public BenchmarkTesterOptionsValidationProfile()
        {
            CreateValidationFor<BenchmarkTesterOptions>()
                .ForProperty(x => x.RunTime)
                    .ValidIf(x => x.Value.TotalMilliseconds >= 100, x => $"Total should be larger or equal to 100 milliseconds")
                .ForProperty(x => x.Workers)
                    .MustBeLargerOrEqualTo(2)
                .ForProperty(x => x.MaximumAttempts)
                    .MustBeLargerOrEqualTo(1)
                .ForProperty(x => x.StorageSize)
                    .MustBeLargerOrEqualTo(0)
                .ForProperty(x => x.TryLockToLockRatio)
                    .MustBeLargerOrEqualTo(0.01)
                    .MustBeSmallerOrEqualTo(1.0)
                .ForProperty(x => x.ExpiryRatio)
                    .MustBeLargerOrEqualTo(0.01)
                    .MustBeSmallerOrEqualTo(1.0)
                .ForProperty(x => x.ExpiredRatio)
                    .MustBeLargerOrEqualTo(0.01)
                    .MustBeSmallerOrEqualTo(1.0)
                .ForProperty(x => x.UnlockedRatio)
                    .MustBeLargerOrEqualTo(0.01)
                    .MustBeSmallerOrEqualTo(1.0)
                .ForProperty(x => x.ResourcePoolSize)
                    .MustBeLargerOrEqualTo(1)
                .ForProperty(x => x.QueryResultSetSize)
                    .MustBeLargerOrEqualTo(1); ;
        }
    }
}
