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
        /// How long to wait before we cancel a managed (anonymous) task when cancellation is requested.
        /// This gives managed (anonymous) tasks time to finish executing without throwing <see cref="OperationCanceledException"/>. 
        /// </summary>
        public TimeSpan GracefulCancellationWaitTime { get; set; } = TimeSpan.FromSeconds(1);
        /// <summary>
        /// How long to wait before we cancel a managed (anonymous) task when cancellation is requested if it has <see cref="TaskCreationOptions.LongRunning"/>.
        /// This gives managed (anonymous) tasks time to finish executing without throwing <see cref="OperationCanceledException"/>.
        /// </summary>
        public TimeSpan LongRunningGracefulCancellationWaitTime { get; set; } = TimeSpan.FromSeconds(5);
    }
}
