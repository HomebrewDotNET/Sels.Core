using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sels.Core.Async.TaskManagement
{
    /// <summary>
    /// Extra options that modify the behaviour how (anonymous) managed tasks are managed.
    /// </summary>
    [Flags]
    public enum ManagedTaskOptions
    {
        /// <summary>
        /// No options selected.
        /// </summary>
        None = 0,
        /// <summary>
        /// Hint that tells a <see cref="ITaskManager"/> that it can cancel a (anonymous) managed task without it throwing an exception.
        /// Causes the tasks to be cancelled faster.
        /// </summary>
        GracefulCancellation = 1,
        /// <summary>
        /// Hint that tells a <see cref="ITaskManager"/> that a (anonymous) managed task should be kept alive until cancellation. 
        /// If the (anonymous) managed task throws an exception it will be restarted. (except for <see cref="OperationCanceledException"/> and any other configured exceptions)
        /// </summary>
        KeepAlive = 2,
        /// <summary>
        /// Hint that tells a <see cref="ITaskManager"/> that a (anonymous) managed task should be restarted once it executes succesfully until cancellation.
        /// </summary>
        AutoRestart = 4,
        /// <summary>
        /// The (anonymous) managed task will be restarted if it either completes or fails until it is cancelled.
        /// </summary>
        KeepRunning = AutoRestart | KeepAlive
    }
}
