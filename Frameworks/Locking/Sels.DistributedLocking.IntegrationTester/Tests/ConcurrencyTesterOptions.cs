using Sels.ObjectValidationFramework.Profile;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.DistributedLocking.IntegrationTester.Tests
{
    /// <summary>
    /// Exposes extra options for <see cref="ConcurrencyTester"/>.
    /// </summary>
    public class ConcurrencyTesterOptions
    {
        /// <summary>
        /// The maximum allowed run time for each test.
        /// </summary>
        public TimeSpan RunTime { get; set; } = TimeSpan.FromMinutes(1);
        /// <summary>
        /// The maximum attempts per workers to execute.
        /// </summary>
        public int MaximumAttempts { get; set; } = 1000;
        /// <summary>
        /// How many milliseconds a worker can sleep at max after attempting to lock.
        /// </summary>
        public int MaxSleepTime { get; set; } = 100;
        /// <summary>
        /// How many milliseconds a worker can sleep at min after attempting to lock.
        /// </summary>
        public int MinSleepTime { get; set; } = 1;
        /// <summary>
        /// The allowed deviation when checking for lock collision in ms. 
        /// Because there is a delay between the unlock and saving the unlock date there are a lot of false positieves. 
        /// If the difference is below or equal to the deviation it won't be logged as collision.
        /// </summary>
        public int CollisionDeviation { get; set; } = 10;
        /// <summary>
        /// How many workers will be used to test for concurrency.
        /// </summary>
        public int Workers { get; set; } = Environment.ProcessorCount * 2;
        /// <summary>
        /// Defines the ratio when running mixed tests. Used to determine to lock using TryLockAsync or LockAsync.
        /// </summary>
        public double TryLockToLockRatio { get; set; } = 0.75;
    }
    /// <summary>
    /// Contains the validation rules for <see cref="ConcurrencyTesterOptions"/>.
    /// </summary>
    public class ConcurrencyTesterOptionsValidationProfile : ValidationProfile<string>
    {
        /// <inheritdoc cref="ConcurrencyTesterOptionsValidationProfile"/>
        public ConcurrencyTesterOptionsValidationProfile()
        {
            CreateValidationFor<ConcurrencyTesterOptions>()
                .ForProperty(x => x.RunTime)
                    .ValidIf(x => x.Value.TotalMilliseconds >= 100, x => $"Total should be larger or equal to 100 milliseconds")
                .ForProperty(x => x.Workers)
                    .MustBeLargerOrEqualTo(2)
                .ForProperty(x => x.MaximumAttempts)
                    .MustBeLargerOrEqualTo(1)
                .ForProperty(x => x.MaxSleepTime)
                    .MustBeLargerOrEqualTo(1)
                    .ValidIf(x => x.Value > x.Source.MinSleepTime, x => $"Maximum ({x.Value}) sleep time must be larger than the minimum ({x.Source.MinSleepTime})")
                .ForProperty(x => x.MinSleepTime)
                    .MustBeLargerOrEqualTo(0)
                .ForProperty(x => x.CollisionDeviation)
                    .MustBeLargerOrEqualTo(0)
                .ForProperty(x => x.TryLockToLockRatio)
                    .MustBeLargerOrEqualTo(0.01)
                    .MustBeSmallerOrEqualTo(1.0);
        }
    }
}
