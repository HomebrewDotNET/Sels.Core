using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Async.TaskManagement
{
    /// <summary>
    /// Defines the policy when multiple tasks with the same name are created.
    /// </summary>
    public enum NamedManagedTaskPolicy
    {
        /// <summary>
        /// Managed task is only created if none exist already or if the previous one finished executing.
        /// </summary>
        TryStart = 0,
        /// <summary>
        /// Instantly cancels the managed task if it is running and creates a new one.
        /// </summary>
        CancelAndStart = 1,
        /// <summary>
        /// Gracefully wait for a managed task if it is already running and only cancel if the wait time exceeds the configured amount. 
        /// After either gracefully waiting or cancelling the task, a new one will be created.
        /// </summary>
        GracefulCancelAndStart = 2,
        /// <summary>
        /// Wait on an already running managed task and only create a new one if the previous one finishes executing.
        /// </summary>
        WaitAndStart = 3,
        /// <summary>
        /// Thrown an <see cref="InvalidOperationException"/> if a task with the same name is already running.
        /// </summary>
        Exception = 4
    }
}
