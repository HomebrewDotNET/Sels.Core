using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Async.Contracts.TaskManagement
{
    /// <summary>
    /// Represents a managed (anonymous) task that still needs to be queued.
    /// </summary>
    /// <typeparam name="T">The type of the pending task</typeparam>
    public interface IPendingTask<T>
    {
        /// <summary>
        /// When the task was requested to be scheduled.
        /// </summary>
        DateTime Created { get; }
        /// <summary>
        /// Task that contains the managed (anonymous) task needs to be queued.
        /// </summary>
        Task<T> Callback { get; }
    }
}
