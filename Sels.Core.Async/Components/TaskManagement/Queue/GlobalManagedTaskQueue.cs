using Sels.Core.Async.TaskManagement.Queue;
using Sels.Core.Async.TaskManagement;
using System;
using System.Collections.Generic;
using System.Text;
using Sels.Core.Extensions;
using Microsoft.Extensions.Logging;

namespace Sels.Core.Async.TaskManagement.Queue
{
    /// <inheritdoc cref="IManagedTaskGlobalQueue"/>
    public class GlobalManagedTaskQueue : ManagedTaskQueue, IManagedTaskGlobalQueue
    {
        // Fields
        // Properties
        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc cref="GlobalManagedTaskQueue"/>
        /// <param name="name"><inheritdoc cref="Name"/></param>
        /// <param name="releaseAction">Delegate to call when the current queue can be cleaned up</param>
        /// <param name="taskManager"><inheritdoc cref="ManagedTaskQueue._taskManager"/></param>
        /// <param name="maxConcurrency"><inheritdoc cref="ManagedTaskQueue.Concurrency"/></param>
        /// <param name="logger"><inheritdoc cref="ManagedTaskQueue._logger"/></param>
        public GlobalManagedTaskQueue(string name, Action releaseAction, ITaskManager taskManager, int maxConcurrency, ILogger? logger = null) : base(releaseAction, taskManager, maxConcurrency, logger)
        {
            Name = name.ValidateArgumentNotNullOrWhitespace(nameof(name));
        }
    }
}
