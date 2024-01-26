using Sels.ObjectValidationFramework.Profile;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Async.TaskManagement
{
    /// <summary>
    /// Exposes extra options for a <see cref="TaskManager"/>.
    /// </summary>
    public class TaskManagerOptions
    {
        /// <summary>
        /// Determines how many paritions will be created.
        /// Tasks will be assigned a parition.
        /// Locking is performed within each bucket.
        /// More buckets means less concurrency issues.
        /// </summary>
        public int ConcurrencyLevel { get; set; } = Environment.ProcessorCount * 2;

        /// <summary>
        /// How long to wait before we cancel a managed (anonymous) task when cancellation is requested.
        /// This gives managed (anonymous) tasks time to finish executing without throwing <see cref="OperationCanceledException"/>. 
        /// </summary>
        public TimeSpan GracefulCancellationWaitTime { get; set; } = TimeSpan.FromSeconds(1);
        /// <summary>
        /// How long to wait before we cancel a managed (anonymous) task when cancellation is requested if it has <see cref="TaskCreationOptions.LongRunning"/>.
        /// This gives managed (anonymous) tasks time to finish executing without throwing <see cref="OperationCanceledException"/>.
        /// </summary>
        public TimeSpan LongRunningGracefulCancellationWaitTime { get; set; } = TimeSpan.FromSeconds(5);
        /// <summary>
        /// How long to wait for <see cref="IManagedTaskGlobalQueue"/> or <see cref="IManagedTaskLocalQueue"/> to process all items before cancelling the pending tasks.
        /// </summary>
        public TimeSpan GracefulQueueStopTime { get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// How long to gracefully wait for managed tasks to stop.
        /// Sometimes tasks won't be able to stop and deadlock the method.
        /// </summary>
        public TimeSpan DeadlockWaitTime { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// How long to sleep for while waiting for tasks/queues to cancel/stop.
        /// </summary>
        public TimeSpan DisposeSleepTime { get; set; } = TimeSpan.FromSeconds(1);
        /// <summary>
        /// The maximum amount of time to wait for tasks/queues to cancel/stop while disposing.
        /// </summary>
        public TimeSpan MaxDisposeTime { get; set; } = TimeSpan.FromMinutes(1);
    }

    /// <summary>
    /// Contains the validation rules for <see cref="TaskManagerOptions"/>.
    /// </summary>
    public class TaskManagerOptionsValidationProfile : ValidationProfile<string>
    {
        /// <inheritdoc cref="TaskManagerOptionsValidationProfile"/>
        public TaskManagerOptionsValidationProfile()
        {
            CreateValidationFor<TaskManagerOptions>()
                .ForProperty(x => x.DeadlockWaitTime)
                    .ValidIf(x => x.Value > x.Source.GracefulCancellationWaitTime, x => $"Must be larger than {nameof(x.Source.GracefulCancellationWaitTime)}")
                    .ValidIf(x => x.Value > x.Source.LongRunningGracefulCancellationWaitTime, x => $"Must be larger than {nameof(x.Source.LongRunningGracefulCancellationWaitTime)}")
                    .MustBeLargerThan(TimeSpan.Zero)
                .ForProperty(x => x.ConcurrencyLevel)
                    .MustBeLargerOrEqualTo(1);
        }
    }
}
