using static Sels.Core.Delegates.Async;

namespace Sels.Core.Mediator.Messaging
{
    /// <summary>
    /// Allows object to subscribe to messages of a certain type.
    /// </summary>
    public interface IMessageSubscriber
    {
        #region Typed
        /// <summary>
        /// Get all objects subscribed to messages of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of message that was subscribed to</typeparam>
        /// <returns>All subscribed objects or an empty array if none are subscribed</returns>
        ISubscriber<T>[] GetSubscribers<T>();
        /// <summary>
        /// Subscribes <paramref name="subscriber"/> to messages of type <typeparamref name="T"/> so <paramref name="subscriber"/> can receive them when they are sent from other objects.
        /// </summary>
        /// <typeparam name="T">The type of message to subscribe to</typeparam>
        /// <param name="subscriber">The subscriber to received the messages with</param>
        void Subscribe<T>(ISubscriber<T> subscriber);
        /// <summary>
        /// Subscribes <paramref name="handler"/> to messages of type <typeparamref name="T"/> where <paramref name="action"/> will be used to receive the message.
        /// </summary>
        /// <typeparam name="T">The type of message to subscribe to</typeparam>
        /// <param name="handler">Instance used as the subscribing instance, it is later used to unsubscribe</param>
        /// <param name="action">Delegate that received the messages. First arg is the sender, second arg is the message and the third is an optional cancellation token used to cancel a long running task</param>
        void Subscribe<T>(object handler, AsyncAction<object, T, CancellationToken> action);
        /// <summary>
        /// Unsubscribes <paramref name="subscriber"/> so it stops receiving messages of type <typeparamref name="T"/> and it can be garbage collected.
        /// </summary>
        /// <typeparam name="T">The type of message to unsubscribe from</typeparam>
        /// <param name="subscriber">The subscriber to unsubscribe</param>
        void Unsubscribe<T>(ISubscriber<T> subscriber);
        /// <summary>
        /// Unsubscribes <paramref name="handler"/> so it stops receiving messages of type <typeparamref name="T"/> and it can be garbage collected.
        /// </summary>
        /// <typeparam name="T">The type of message to unsubscribe from</typeparam>
        /// <param name="handler">The handler to unsubscribe</param>
        void Unsubscribe<T>(object handler);
        #endregion

        #region Untyped
        /// <summary>
        /// Get all objects subscribed to messages of all types.
        /// </summary>
        /// <returns>All subscribed objects or an empty array if none are subscribed</returns>
        ISubscriber[] GetSubscribers();
        /// <summary>
        /// Subscribes <paramref name="subscriber"/> to messages of all types so <paramref name="subscriber"/> can receive them when they are sent from other objects.
        /// </summary>
        /// <param name="subscriber">The subscriber to received the messages with</param>
        void Subscribe(ISubscriber subscriber);
        /// <summary>
        /// Subscribes <paramref name="handler"/> to messages ofall types where <paramref name="action"/> will be used to receive the message.
        /// </summary>
        /// <param name="handler">Instance used as the subscribing instance, it is later used to unsubscribe</param>
        /// <param name="action">Delegate that received the messages. First arg is the sender, second arg is the message and the third is an optional cancellation token used to cancel a long running task</param>
        void Subscribe(object handler, AsyncAction<object, object, CancellationToken> action);
        /// <summary>
        /// Unsubscribes <paramref name="subscriber"/> so it stops receiving messages of all types and it can be garbage collected.
        /// </summary>
        /// <param name="subscriber">The subscriber to unsubscribe</param>
        void Unsubscribe(ISubscriber subscriber);
        /// <summary>
        /// Unsubscribes <paramref name="handler"/> so it stops receiving messages of all types and it can be garbage collected.
        /// </summary>
        /// <param name="handler">The handler to unsubscribe</param>
        void Unsubscribe(object handler);
        #endregion
    }
}
