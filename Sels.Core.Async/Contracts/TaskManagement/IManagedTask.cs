using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Async.TaskManagement
{
    /// <summary>
    /// Represents a task that was scheduled using a <see cref="ITaskManager"/> that is tied to an owner.
    /// </summary>
    public interface IManagedTask : IManagedAnonymousTask
    {
        /// <summary>
        /// The instance that scheduled the task.
        /// </summary>
        object Owner { get; }
        /// <summary>
        /// The (unique) name of the task. Can be null.
        /// </summary>
        string? Name { get; }
    }
}
