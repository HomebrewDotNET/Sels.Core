using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sels.Core.Mediator.Event
{
    /// <summary>
    /// Allows an object to listen to events of type <typeparamref name="TEvent"/>.
    /// </summary>
    /// <typeparam name="TEvent">The type of event to listen to</typeparam>
    public interface IEventListener<in TEvent> : IMessageHandler
    {
        /// <summary>
        /// React to raised event <paramref name="event"/>.
        /// </summary>
        /// <param name="context"><inheritdoc cref="IEventListenerContext"/></param>
        /// <param name="event">The event that was raised</param>
        /// <param name="token">Optional token that can be cancelled by the event raiser</param>
        /// <returns>Task containing the execution state</returns>
        Task HandleAsync(IEventListenerContext context, TEvent @event, CancellationToken token);
    }

    /// <summary>
    /// Allows an object to listen to all raised events
    /// </summary>
    public interface IEventListener : IMessageHandler
    {
        /// <summary>
        /// React to raised event <paramref name="event"/>.
        /// </summary>
        /// <param name="context"><inheritdoc cref="IEventListenerContext"/></param>
        /// <param name="event">The event that was raised</param>
        /// <param name="token">Optional token that can be cancelled by the event raiser</param>
        /// <returns>Task containing the execution state</returns>
        Task HandleAsync(IEventListenerContext context, object @event, CancellationToken token);
    }

    /// <summary>
    /// Exposes more information and functionality to event listeners.
    /// </summary>
    public interface IEventListenerContext
    {
        /// <summary>
        /// The object that raised the event.
        /// </summary>
        object Sender { get; }
        /// <summary>
        /// An array of all current subscribers listening to the event.
        /// </summary>
        IMessageHandler[] OtherSubscribers { get; }

        /// <summary>
        /// Allows an event listener to enlist another event to the current transaction. Useful when transforming or adapting events to other event types.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event to enlist</typeparam>
        /// <param name="event">The event to enlist to the current transaction</param>
        /// <returns>Task that will complete the event was enlisted</returns>
        void EnlistEvent<TEvent>(TEvent @event);
        /// <summary>
        /// Can be awaited to synchronize transaction commits accross all subscribers.
        /// </summary>
        /// <returns>Task that will be completed when all other subscribers are ready to commit or completed already</returns>
        Task WaitForCommitAsync();
    }
}
