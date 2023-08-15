using Microsoft.Extensions.Logging;
using Sels.Core.Async.Queue;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Async.TaskManagement.Queue
{
    /// <inheritdoc cref="IManagedTaskLocalQueue"/>
    public class LocalManagedTaskQueue : ManagedTaskQueue, IManagedTaskLocalQueue
    {
        // Fields
        // Properties
        /// <inheritdoc/>
        public object Owner { get; }

        /// <inheritdoc cref="ManagedTaskQueue"/>
        /// <param name="owner">The instance the queue is tied to</param>
        /// <param name="releaseAction">Delegate to call when the current queue can be cleaned up</param>
        /// <param name="taskManager"><inheritdoc cref="ManagedTaskQueue._taskManager"/></param>
        /// <param name="maxConcurrency"><inheritdoc cref="ManagedTaskQueue.Concurrency"/></param>
        /// <param name="logger"><inheritdoc cref="ManagedTaskQueue._logger"/></param>
        public LocalManagedTaskQueue(object owner, Action releaseAction, ITaskManager taskManager, int maxConcurrency, ILogger? logger = null) : base(releaseAction, taskManager, maxConcurrency, logger)
        {
            Owner = owner.ValidateArgument(nameof(owner));
        }
    }
}
