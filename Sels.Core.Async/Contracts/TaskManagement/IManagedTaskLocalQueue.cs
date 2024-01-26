using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Async.TaskManagement
{
    /// <summary>
    /// <inheritdoc cref="IManagedTaskQueue"/>
    /// Local queues are tied to an object.
    /// Disposing the queue will remove the reference to it. When a queue is empty and does not contain any references it will be removed.
    /// </summary>
    public interface IManagedTaskLocalQueue : IManagedTaskQueue, IDisposable
    {
        /// <summary>
        /// The instance the queue is tied to.
        /// </summary>
        object Owner { get; }
    }
}
