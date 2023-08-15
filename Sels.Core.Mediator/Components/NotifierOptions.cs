using Sels.ObjectValidationFramework.Profile;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Mediator.Components
{
    /// <summary>
    /// Exposes more options for <see cref="NotifierOptions"/>.
    /// </summary>
    public class NotifierOptions
    {
        /// <inheritdoc cref="FireAndForgetStrategy"/>
        public FireAndForgetStrategy FireAndForgetStrategy { get; set; } = FireAndForgetStrategy.QueuePerType;
        /// <summary>
        /// The name of the global queue when <see cref="FireAndForgetStrategy.GlobalQueue"/> is used.
        /// </summary>
        public string GlobalQueueName { get; } = "FireAndForget";
        /// <summary>
        /// How many workers there are per queue when either <see cref="FireAndForgetStrategy.GlobalQueue"/> or <see cref="FireAndForgetStrategy.QueuePerType"/> is used.
        /// </summary>
        public int QueueConcurrency { get; } = Environment.ProcessorCount / 2;
    }

    /// <summary>
    /// Contains the validation rules for <see cref="NotifierOptions"/>.
    /// </summary>
    public class NotifierOptionsValidationProfile : ValidationProfile<string>
    {
        /// <inheritdoc cref="NotifierOptionsValidationProfile"/>
        public NotifierOptionsValidationProfile()
        {
            CreateValidationFor<NotifierOptions>()
                .ForProperty(x => x.GlobalQueueName)
                    .CannotBeNullOrWhitespace()
                .ForProperty(x => x.QueueConcurrency)
                    .MustBeLargerOrEqualTo(1);
        }
    }

    /// <summary>
    /// Dictates how fire and forget events are queued.
    /// </summary>
    public enum FireAndForgetStrategy
    {
        /// <summary>
        /// Events are scheduled on the thread pool without any throttling.
        /// </summary>
        ThreadPool = 0,
        /// <summary>
        /// Events are scheduled on a global throttled queue.
        /// </summary>
        GlobalQueue = 1,
        /// <summary>
        /// Events are scheduled on a queue tied to the type of the event. Events are throttled based on the event type.
        /// </summary>
        QueuePerType = 2
    }
}
