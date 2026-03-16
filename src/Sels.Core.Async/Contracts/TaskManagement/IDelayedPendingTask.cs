using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Async.TaskManagement
{
    /// <summary>
    /// Represents a managed (anonymous) task that still needs to be queued on the Thread Pool and executed.
    /// Task will be scheduled after a certain delay.
    /// </summary>
    /// <typeparam name="T">The type of the pending task</typeparam>
    public interface IDelayedPendingTask<T> : IPendingTask<T> where T : IManagedAnonymousTask
    {
        /// <summary>
        /// The delay the pending task was created with.
        /// </summary>
        public TimeSpan Delay { get; }
        /// <summary>
        /// The date after which the pending task can be scheduled.
        /// </summary>
        public DateTime ScheduleTime { get; }
    }
}
