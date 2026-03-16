using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Async.TaskManagement
{
    /// <summary>
    /// <inheritdoc cref="IManagedTaskQueue"/>.
    /// Global queues have a unique name and are shared within the same application.
    /// Disposing the queue will remove the reference to it. When a queue is empty and does not contain any references it will be removed.
    /// </summary>
    public interface IManagedTaskGlobalQueue : IManagedTaskQueue, IDisposable
    {
        /// <summary>
        /// The unique name of the queue.
        /// </summary>
        string Name { get; }
    }
}
