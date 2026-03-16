using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Async.Queue
{
    /// <summary>
    /// An active subscriptions to an event in a <see cref="WorkerQueue{T}"/>. Disposing the subscription will stop the event handler from receiving events.
    /// </summary>
    public struct WorkerQueueEventSubscription : IDisposable
    {
        // Fields
        private readonly Action? _releaseAction;

        /// <inheritdoc cref="WorkerQueueEventSubscription"/>
        /// <param name="releaseAction">Delegate that will be called when the object gets disposed</param>
        public WorkerQueueEventSubscription(Action? releaseAction = null)
        {
            _releaseAction = releaseAction;
        }

        /// <summary>
        /// Cancels the subscriptions.
        /// </summary>
        public void Dispose() => _releaseAction?.Invoke();
    }
}
