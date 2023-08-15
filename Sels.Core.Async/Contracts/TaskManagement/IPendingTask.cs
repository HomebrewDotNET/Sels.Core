using System;
using System.Collections.Generic;
using System.Text;
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
    }
}
