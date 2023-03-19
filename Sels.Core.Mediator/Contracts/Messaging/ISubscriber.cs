namespace Sels.Core.Mediator.Messaging
{
    /// <summary>
    /// Allows an object to receive messages of type <typeparamref name="T"/> from messagers.
    /// </summary>
    /// <typeparam name="T">The type of message to subscribe to</typeparam>
    public interface ISubscriber<in T> : IMessageHandler
    {
        /// <summary>
        /// Handles <paramref name="message"/> which was sent from another object.
        /// </summary>
        /// <param name="sender">The object that sent the message</param>
        /// <param name="message">The message that was sent</param>
        /// <param name="token">Optional token for cancelling the request</param>
        /// <returns>Task for awaiting the execution</returns>
        Task ReceiveAsync(object sender, T message, CancellationToken token = default);
    }

    /// <summary>
    /// Allows an object to receive messages of all messagers.
    /// </summary>
    public interface ISubscriber : IMessageHandler
    {
        /// <summary>
        /// Handles <paramref name="message"/> which was sent from another object.
        /// </summary>
        /// <param name="sender">The object that sent the message</param>
        /// <param name="message">The message that was sent</param>
        /// <param name="token">Optional token for cancelling the request</param>
        /// <returns>Task for awaiting the execution</returns>
        Task ReceiveAsync(object sender, object message, CancellationToken token = default);
    }
    /// <summary>
    /// Exposes the object that can handle messages.
    /// </summary>
    public interface IMessageHandler
    {
        /// <summary>
        /// The actual object that handles the message.
        /// </summary>
        public object Handler { get; }

    }
}
