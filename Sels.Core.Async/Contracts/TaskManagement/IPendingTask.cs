using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sels.Core.Async.TaskManagement
{
    /// <summary>
    /// Represents a managed (anonymous) task that still needs to be queued on the Thread Pool and executed.
    /// </summary>
    /// <typeparam name="T">The type of the pending task</typeparam>
    public interface IPendingTask<T> where T : IManagedAnonymousTask
    {
        /// <summary>
        /// When the task was scheduled.
        /// </summary>
        DateTime Created { get; }
        /// <summary>
        /// Task that will complete when the managed (anonymous) task is created and scheduled on the thread pool.
        /// </summary>
        Task<T> Callback { get; }

        /// <summary>
        /// Waits for the pending task to get scheduled and executed.
        /// </summary>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>The managed task after it was executed</returns>
        public async Task<T> WaitOnExecution(CancellationToken token = default)
        {
            var managedTask = await Helper.Async.WaitOn(Callback, token).ConfigureAwait(false);

            await Helper.Async.WaitOn(managedTask.OnExecuted, token).ConfigureAwait(false);
            return managedTask;
        }

        /// <summary>
        /// Waits for the pending task to get scheduled, executed and finalized.
        /// </summary>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>The managed task after it was executed and finalized</returns>
        public async Task<T> WaitOnFinalization(CancellationToken token = default)
        {
            var managedTask = await Helper.Async.WaitOn(Callback, token).ConfigureAwait(false);

            await Helper.Async.WaitOn(managedTask.OnFinalized, token).ConfigureAwait(false);
            return managedTask;
        }
    }
}
