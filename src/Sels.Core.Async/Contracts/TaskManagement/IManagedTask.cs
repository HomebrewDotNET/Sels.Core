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
        /// <summary>
        /// If the task is a global task. Only used if <see cref="Name"/> is set. Global task names are shared among all instances, otherwise the names are shared within the same <see cref="Owner"/>.
        /// </summary>
        bool IsGlobal { get; }
    }
}
