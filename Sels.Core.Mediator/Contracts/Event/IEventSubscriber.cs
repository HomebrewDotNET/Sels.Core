using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        /// <returns>All listeners that can react to all type of events</returns>
        IEventListener[] GetAllListeners();

        /// <summary>
        /// Gets all current event listeners that are listening to events of <typeparamref name="TEvent"/>.
        /// </summary>
        /// <returns>All listeners that can react to events of type <typeparamref name="TEvent"/></returns>
        IEventListener<TEvent>[] GetAllListeners<TEvent>();
        #endregion

        #region Subscribe
        /// <summary>
        /// Subscribes <paramref name="listener"/> to events of type <typeparamref name="TEvent"/>.
        /// </summary>
        /// <param name="listener">The listener that will be able to react to events of type <typeparamref name="TEvent"/></param>
        void Subscribe<TEvent>(IEventListener<TEvent> listener);
        /// <summary>
        /// Unsubscribes <paramref name="listener"/> so it stops listening to events of type <typeparamref name="TEvent"/>.
        /// </summary>
        /// <param name="listener">The listener that will be unsubscribed</param>
        void Unsubscribe<TEvent>(IEventListener<TEvent> listener);
        /// <summary>
        /// Subscribes <paramref name="listener"/> to all events.
        /// </summary>
        /// <param name="listener">The listener that will be able to react to all events</param>
        void Subscribe(IEventListener listener);
        /// <summary>
        /// Unsubscribes <paramref name="listener"/> so it stops listening to all events.
        /// </summary>
        /// <param name="listener">The listener that will be unsubscribed</param>
        void Unsubscribe(IEventListener listener);

        /// <summary>
        /// Subscribes <paramref name="listener"/> to events of type <typeparamref name="TEvent"/> where the events can be reacted to using <paramref name="subscriberAction"/>.
        /// </summary>
        /// <param name="listener">The object that's listening using <paramref name="subscriberAction"/></param>
        /// <param name="subscriberAction">The delegate that will be called to react to events of type <typeparamref name="TEvent"/></param>
        void Subscribe<TEvent>(object listener, AsyncAction<IEventListenerContext, TEvent, CancellationToken> subscriberAction);
        /// <summary>
        /// Unsubscribes <paramref name="listener"/> so it stops listening to events of type <typeparamref name="TEvent"/>.
        /// </summary>
        /// <param name="listener">The listener that will be unsubscribed</param>
        void Unsubscribe<TEvent>(object listener);
        /// <summary>
        /// Subscribes <paramref name="listener"/> to all events where the events can be reacted to using <paramref name="subscriberAction"/>.
        /// </summary>
        /// <param name="listener">The listener that will be able to react to all events using <paramref name="subscriberAction"/></param>
        /// <param name="subscriberAction">The delegate that will be called to react to events</param>
        void Subscribe(object listener, AsyncAction<IEventListenerContext, object, CancellationToken> subscriberAction);
        /// <summary>
        /// Unsubscribes <paramref name="listener"/> so it stops listening to all events.
        /// </summary>
        /// <param name="listener">The listener that will be unsubscribed</param>
        void Unsubscribe(object listener);
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
        void Subscribe(IEventListener<TEvent> listener);
        /// <summary>
        /// Unsubscribes <paramref name="listener"/> so it stops listening to events of type <typeparamref name="TEvent"/>.
        /// </summary>
        /// <param name="listener">The listener that will be unsubscribed</param>
        void Unsubscribe(IEventListener<TEvent> listener);

        /// <summary>
        /// Subscribes <paramref name="listener"/> to events of type <typeparamref name="TEvent"/> where the events can be reacted to using <paramref name="subscriberAction"/>.
        /// </summary>
        /// <param name="listener">The object that's listening using <paramref name="subscriberAction"/></param>
        /// <param name="subscriberAction">The delegate that will be called to react to events of type <typeparamref name="TEvent"/></param>
        void Subscribe(object listener, AsyncAction<IEventListenerContext, TEvent, CancellationToken> subscriberAction);
        /// <summary>
        /// Unsubscribes <paramref name="listener"/> so it stops listening to events of type <typeparamref name="TEvent"/>.
        /// </summary>
        /// <param name="listener">The listener that will be unsubscribed</param>
        void Unsubscribe(object listener);
        #endregion
    }
}
