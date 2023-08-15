using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sels.Core.Mediator.Event
{
    /// <summary>
    /// Scope used to manage a transaction across multiple event listeners when they are handling a raised event.
    /// </summary>
    public interface IEventTransactionScope
    {
        /// <summary>
        /// Enlists <paramref name="listener"/> to the current transaction.
        /// </summary>
        /// <typeparam name="TEvent">The type of event raised</typeparam>
        /// <param name="sender">Who raised the event</param>
        /// <param name="listener">The listener to execute in the transaction</param>
        /// <param name="event">The event that was raised</param>
        void Enlist<TEvent>(object sender, IEventListener listener, TEvent @event);
        /// <summary>
        /// Enlists <paramref name="listener"/> to the current transaction.
        /// </summary>
        /// <typeparam name="TEvent">The type of event raised</typeparam>
        /// <param name="sender">Who raised the event</param>
        /// <param name="listener">The listener to execute in the transaction</param>
        /// <param name="event">The event that was raised</param>
        void Enlist<TEvent>(object sender, IEventListener<TEvent> listener, TEvent @event);

        /// <summary>
        /// Executes all enlisted listeners.
        /// </summary>
        /// <param name="token">Token to cancel the operation</param>
        /// <param name="runInParallel">If the event listeners can be run in parallel</param>
        /// <returns>The amount of event listeners that received the event</returns>
        Task<int> ExecuteAsync(bool runInParallel, CancellationToken token);
    }
}
