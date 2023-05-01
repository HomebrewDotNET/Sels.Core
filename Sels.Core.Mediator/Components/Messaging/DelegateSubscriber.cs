using static Sels.Core.Delegates.Async;

namespace Sels.Core.Mediator.Messaging
{
    /// <summary>
    /// Subscriber that delegates messages to a handler of type <typeparamref name="THandler"/> using a delegate.
    /// </summary>
    /// <typeparam name="THandler">The type of the handler</typeparam>
    /// <typeparam name="TMessage">The type of the message to subscribe to</typeparam>
    [Obsolete($"Use the new Sels.Core.Mediator.Event components")]
    internal class DelegateSubscriber<THandler, TMessage> : ISubscriber<TMessage>
    {
        // Fields
        private readonly THandler _handler;
        private readonly AsyncAction<THandler, object, TMessage, CancellationToken> _action;

        /// <inheritdoc/>
        public object Handler => _handler;

        /// <inheritdoc cref="DelegateSubscriber{THandler, TMessage}"/>
        /// <param name="handler">The instance to delegate the message to</param>
        /// <param name="action">Delegate that handles any received message using <paramref name="action"/>. First arg is the object to handle the message with, second arg is the object that sent the message, third arg is the message received and the forth is an optional token to cancel a long running task</param>
        public DelegateSubscriber(THandler handler, AsyncAction<THandler, object, TMessage, CancellationToken> action)
        {
            _handler = handler.ValidateArgument(nameof(handler));
            _action = action.ValidateArgument(nameof(action));
        }

        /// <inheritdoc/>
        public Task ReceiveAsync(object sender, TMessage message, CancellationToken token = default)
        {
            message.ValidateArgument(nameof(message));

            return _action(_handler, sender, message, token);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return _handler.ToString();
        }
    }

    /// <summary>
    /// Subscriber that delegates messages to a handler of type <typeparamref name="THandler"/> using a delegate.
    /// </summary>
    /// <typeparam name="THandler">The type of the handler</typeparam>
    [Obsolete($"Use the new Sels.Core.Mediator.Event components")]
    internal class DelegateSubscriber<THandler> : ISubscriber
    {
        // Fields
        private readonly THandler _handler;
        private readonly AsyncAction<THandler, object, object, CancellationToken> _action;

        /// <inheritdoc/>
        public object Handler => _handler;

        /// <inheritdoc cref="DelegateSubscriber{THandler, TMessage}"/>
        /// <param name="handler">The instance to delegate the message to</param>
        /// <param name="action">Delegate that handles any received message using <paramref name="action"/>. First arg is the object to handle the message with, second arg is the object that sent the message, third arg is the message received and the forth is an optional token to cancel a long running task</param>
        public DelegateSubscriber(THandler handler, AsyncAction<THandler, object, object, CancellationToken> action)
        {
            _handler = handler.ValidateArgument(nameof(handler));
            _action = action.ValidateArgument(nameof(action));
        }

        /// <inheritdoc/>
        public Task ReceiveAsync(object sender, object message, CancellationToken token = default)
        {
            message.ValidateArgument(nameof(message));

            return _action(_handler, sender, message, token);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return _handler.ToString();
        }
    }
}
