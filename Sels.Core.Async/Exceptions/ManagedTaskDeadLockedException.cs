using Sels.Core.Async.TaskManagement;
using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sels.Core.Async
{
    /// <summary>
    /// Thrown when a managed task(s) are deadlocked because they didn't cancel in the requested timeframe.
    /// </summary>
    public class ManagedTaskDeadlockedException : Exception
    {
        // Properties
        /// <summary>
        /// The task that was deadlocked.
        /// </summary>
        public IManagedTask[] Tasks { get; }

        /// <inheritdoc cref="ManagedTaskDeadlockedException"/>
        /// <param name="tasks"><inheritdoc cref="Tasks"/></param>
        public ManagedTaskDeadlockedException(IEnumerable<IManagedTask> tasks) : base(CreateMessage(tasks))
        {
            Tasks = tasks.ValidateArgumentNotNullOrEmpty(nameof(tasks)).ToArray();
        }

        private static string CreateMessage(IEnumerable<IManagedTask> tasks)
        {
            tasks.ValidateArgumentNotNullOrEmpty(nameof(tasks));
            var message = new StringBuilder();

            message.AppendLine($"The following managed tasks are deadlocked:");
            foreach (IManagedTask task in tasks)
            {
                message.AppendLine(task.ToString());
            }

            return message.ToString();
        }
    }
}
