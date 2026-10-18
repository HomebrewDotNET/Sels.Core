using Sels.ObjectValidationFramework.Profile;
using System;
using System.Collections.Generic;
using System.Text;
using Sels.Core.Async;

namespace Sels.Core.Async.Throtting
{
    /// <summary>
    /// Options for the <see cref="IThrottledWorker{TResult, TContext}"/> that allows you to configure how many tasks can be executed at the same time and how many tasks can be queued before they are executed.
    /// </summary>
    public class ThrottledWorkerOptions
    {
        /// <summary>
        /// The default options for the <see cref="IThrottledWorker{TResult, TContext}"/>.
        /// </summary>
        public static ThrottledWorkerOptions Default { get; } = new ThrottledWorkerOptions()
        {
            MaxConcurrentTasks = Environment.ProcessorCount,
            MaxQueuedTasks = Environment.ProcessorCount * AsyncConstants.Throttling.DefaultQueueProcessorMultiplier,
            StopOnError = true
        };

        /// <summary>
        /// The maximum amount of tasks that can be executed at the same time.
        /// </summary>
        public int MaxConcurrentTasks { get; set; } = AsyncConstants.Throttling.DefaultMaxConcurrentTasks;
        /// <summary>
        /// The maximum amount of tasks that can be queued before they are executed.
        /// Can be set to <see cref="int.MaxValue"/> to allow unlimited queued tasks.
        /// </summary>
        public int MaxQueuedTasks { get; set; } = AsyncConstants.Throttling.DefaultMaxQueuedTasks;
        /// <summary>
        /// If set to true the worker will stop executing tasks if one of the tasks throws an exception.
        /// </summary>
        public bool StopOnError { get; set; } = false;
    }

    /// <summary>
    /// Contains the validation rules for <see cref="ThrottledWorkerOptions"/>.
    /// </summary>
    public class ThrottledWorkerOptionsValidationProfile : ValidationProfile<string>
    {
        /// <inheritdoc cref="ThrottledWorkerOptionsValidationProfile"/>
        public ThrottledWorkerOptionsValidationProfile()
        {
            CreateValidationFor<ThrottledWorkerOptions>()
                .ForProperty(x => x.MaxConcurrentTasks)
                    .MustBeLargerOrEqualTo(AsyncConstants.Throttling.MinimumTaskCount)
                .ForProperty(x => x.MaxQueuedTasks)
                    .MustBeLargerOrEqualTo(AsyncConstants.Throttling.MinimumTaskCount);
        }
    }
}
