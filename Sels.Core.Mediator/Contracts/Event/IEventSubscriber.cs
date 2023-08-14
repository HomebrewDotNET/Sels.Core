using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Sels.Core.Delegates.Async;

namespace Sels.Core.Mediator.Event
{
    /// <summary>
    /// Allows objects to subscribe to events at runtime.
    /// </summary>
    public interface IEventSubscriber
    {
        #region Get
        /// <summary>
        /// Gets all current event listeners.
        /// </summary>
        /// <param name="serviceProvider">The service scope to resolve any services in</param>
        /// <returns>All listeners that can react to all type of events or an empty array when there aren't any</returns>
        IEventListener[] GetAllListeners(IServiceProvider serviceProvider);

        /// <summary>
        /// Gets all current event listeners that are listening to events of <typeparamref name="TEvent"/>.
        /// </summary>
        /// <param name="serviceProvider">The service scope to resolve any services in</param>
        /// <returns>All listeners that can react to events of type <typeparamref name="TEvent"/> or an empty array when there aren't any</returns>
        IEventListener<TEvent>[] GetAllListeners<TEvent>(IServiceProvider serviceProvider);
        #endregion

        #region Subscribe
        /// <summary>
        /// Subscribes <paramref name="listener"/> to events of type <typeparamref name="TEvent"/>.
        /// </summary>
        /// <param name="listener">The listener that will be able to react to events of type <typeparamref name="TEvent"/></param>
        /// <returns>The active subscription to the event. Disposing the subscription will stop <paramref name="listener"/> from receiving events</returns>
        EventSubscription Subscribe<TEvent>(IEventListener<TEvent> listener);
        /// <summary>
        /// Subscribes <paramref name="listener"/> to all events.
        /// </summary>
        /// <param name="listener">The listener that will be able to react to all events</param>
        /// <returns>The active subscription to the event. Disposing the subscription will stop <paramref name="listener"/> from receiving events</returns>
        EventSubscription Subscribe(IEventListener listener);

        /// <summary>
        /// Subscribes <paramref name="subscriberAction"/> to events of type <typeparamref name="TEvent"/>. When an event is raised, <paramref name="subscriberAction"/> will be called.
        /// </summary>
        /// <param name="subscriberAction">The delegate that will be called to react to events of type <typeparamref name="TEvent"/></param>
        /// <param name="priority"><inheritdoc cref="IMessageHandler.Priority"/></param>
        /// <returns>The active subscription to the event. Disposing the subscription will stop <paramref name="subscriberAction"/> from receiving events</returns>
        EventSubscription Subscribe<TEvent>(AsyncAction<IEventListenerContext, TEvent, CancellationToken> subscriberAction, uint? priority = null);
        /// <summary>
        /// Subscribes <paramref name="subscriberAction"/> to all events. When an event is raised, <paramref name="subscriberAction"/> will be called.
        /// </summary>
        /// <param name="subscriberAction">The delegate that will be called to react to events</param>
        /// <param name="priority"><inheritdoc cref="IMessageHandler.Priority"/></param>
        /// <returns>The active subscription to the event. Disposing the subscription will stop <paramref name="subscriberAction"/> from receiving events</returns>
        EventSubscription Subscribe(AsyncAction<IEventListenerContext, object, CancellationToken> subscriberAction, uint? priority = null);
        #endregion
    }

    /// <summary>
    /// Allows objects to subscribe to events of type <typeparamref name="TEvent"/> at runtime.
    /// </summary>
    public interface IEventSubscriber<TEvent>
    {
        #region Get
        /// <summary>
        /// Gets all current event listeners that are listening to events of <typeparamref name="TEvent"/>.
        /// </summary>
        /// <returns>All listeners that can react to events of type <typeparamref name="TEvent"/></returns>
        IEventListener<TEvent>[] GetAllListeners();
        #endregion

        #region Subscribe
        /// <summary>
        /// Subscribes <paramref name="listener"/> to events of type <typeparamref name="TEvent"/>.
        /// </summary>
        /// <param name="listener">The listener that will be able to react to events of type <typeparamref name="TEvent"/></param>
        /// <returns>The active subscription to the event. Disposing the subscription will stop <paramref name="listener"/> from receiving events</returns>
        EventSubscription Subscribe(IEventListener<TEvent> listener);

        /// <summary>
        /// Subscribes <paramref name="subscriberAction"/> to events of type <typeparamref name="TEvent"/>. When an event is raised, <paramref name="subscriberAction"/> will be called.
        /// </summary>
        /// <param name="subscriberAction">The delegate that will be called to react to events of type <typeparamref name="TEvent"/></param>
        /// <param name="priority"><inheritdoc cref="IMessageHandler.Priority"/></param>
        /// <returns>The active subscription to the event. Disposing the subscription will stop <paramref name="subscriberAction"/> from receiving events</returns>
        EventSubscription Subscribe(AsyncAction<IEventListenerContext, TEvent, CancellationToken> subscriberAction, uint? priority = null);
        #endregion
    }
}
