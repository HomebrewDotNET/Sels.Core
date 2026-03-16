using Sels.Core.Async.TaskManagement;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sels.Core.Async.TaskManagement
{
    /// <summary>
    /// Represents a queue where managed (anonymous) tasks can be scheduled and executed on.
    /// Used to throttle work on the Thread Pool.
    /// </summary>
    public interface IManagedTaskQueue 
    {
        /// <summary>
        /// How many tasks can be scheduled and executed in parallel on the queue.
        /// </summary>
        int Concurrency { get; }
        /// <summary>
        /// How many tasks are pending on the queue.
        /// </summary>
        int Pending { get; }
        /// <summary>
        /// How many tasks are currently being processed by the queue.
        /// </summary>
        int Processing { get; }
        /// <summary>
        /// The date when the last item was processed by the queue. Will be null if the queue never processed anything.
        /// </summary>
        DateTime? LastProcessed { get; }
        /// <summary>
        /// How many tasks were processed by the queue.
        /// </summary>
        ulong Processed { get; }

        /// <summary>
        /// Uses <paramref name="schedulerAction"/> to create the task to schedule on the queue.
        /// </summary>
        /// <param name="schedulerAction">Delegate used to create the managed task to schedule</param>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>Object with callback that will complete when the task gets scheduled on the Thread Pool</returns>
        Task<IPendingTask<IManagedTask>> EnqueueAsync(Func<ITaskManager, CancellationToken, Task<IManagedTask>> schedulerAction, CancellationToken token = default);
        /// <summary>
        /// Uses <paramref name="schedulerAction"/> to create the task to schedule on the queue.
        /// </summary>
        /// <param name="schedulerAction">Delegate used to create the managed task to schedule</param>
        /// 
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>Object with callback that will complete when the task gets scheduled on the Thread Pool</returns>
        Task<IPendingTask<IManagedTask>> EnqueueAsync(Func<ITaskManager, CancellationToken, IManagedTask> schedulerAction, CancellationToken token = default);
        /// <summary>
        /// Uses <paramref name="schedulerAction"/> to create the task to schedule on the queue.
        /// </summary>
        /// <param name="schedulerAction">Delegate used to create the managed task to schedule</param>
        /// 
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>Object with callback that will complete when the task gets scheduled on the Thread Pool</returns>
        Task<IPendingTask<IManagedAnonymousTask>> EnqueueAsync(Func<ITaskManager, CancellationToken, IManagedAnonymousTask> schedulerAction, CancellationToken token = default);
    }
}
