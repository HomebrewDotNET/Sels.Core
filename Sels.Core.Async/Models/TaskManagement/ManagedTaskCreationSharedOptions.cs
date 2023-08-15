using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Sels.Core.Delegates.Async;

namespace Sels.Core.Async.TaskManagement
{
    /// <summary>
    /// Contains common options shared by managed and managed anonymous tasks during creation.
    /// </summary>
    public abstract class ManagedTaskCreationSharedOptions
    {
        /// <summary>
        /// The task options used to schedule the task on the Thread Pool.
        /// </summary>
        public TaskCreationOptions TaskCreationOptions { get; internal set; }
        /// <summary>
        /// The management task options used by the <see cref="ITaskManager"/> to manage the task.
        /// </summary>
        public ManagedTaskOptions ManagedTaskOptions { get; internal set; }
        /// <summary>
        /// The delegate executed by the managed task on the Thread Pool.
        /// </summary>
        public Func<CancellationToken, Task<object>> ExecuteDelegate { get; internal set; }
        /// <summary>
        /// The properties tied to the managed task.
        /// </summary>
        public IReadOnlyDictionary<string, object> Properties { get; internal set; }
    }
}
