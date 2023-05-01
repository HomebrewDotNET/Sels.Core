namespace Sels.Core.Mediator.Messaging
{
    /// <summary>
    /// Thread safe singleton message subscriber that keeps a list of subscribers.
    /// </summary>
    [Obsolete($"Use the new Sels.Core.Mediator.Event components")]
    internal class SubscriptionManager : IMessageSubscriber
    {
        // Fields
        private readonly Dictionary<Type, List<object>> _subscribers = new ();
        private readonly List<ISubscriber> _allSubscribers = new();
        private readonly object _threadLock = new object();
        private readonly object _allThreadLock = new object();
        private readonly ILogger? _logger;

        ///<inheritdoc cref="SubscriptionManager"/>
        /// <param name="logger">Logger used for tracing</param>
        public SubscriptionManager(ILogger<SubscriptionManager>? logger = null)
        {
            _logger = logger;
        }

        #region Typed
        /// <inheritdoc/>
        public ISubscriber<T>[] GetSubscribers<T>()
        {
            lock (_threadLock)
            {
                var type = typeof(T);
                return _subscribers.ContainsKey(type) ? _subscribers[type].Cast<ISubscriber<T>>().ToArray() : Array.Empty<ISubscriber<T>>();
            }
        }

        /// <inheritdoc/>
        public void Subscribe<T>(ISubscriber<T> subscriber)
        {
            subscriber.ValidateArgument(nameof(subscriber));

            lock (_threadLock)
            {
                _logger.Debug($"Adding <{subscriber}> as subscriber to messages of type <{typeof(T)}>");
                _subscribers.AddValueToList(typeof(T), subscriber);
            }
        }
        /// <inheritdoc/>
        public void Subscribe<T>(object handler, Delegates.Async.AsyncAction<object, T, CancellationToken> action)
        {
            handler.ValidateArgument(nameof(handler));
            action.ValidateArgument(nameof(action));

            Subscribe(new DelegateSubscriber<object, T>(handler, (h, s, m, t) => action(s, m, t)));
        }

        /// <inheritdoc/>
        public void Unsubscribe<T>(ISubscriber<T> subscriber)
        {
            subscriber.ValidateArgument(nameof(subscriber));

            lock (_threadLock)
            {
                _logger.Debug($"Removing <{subscriber}> as subscriber to messages of type <{typeof(T)}>");
                var type = typeof(T);
                if (_subscribers.ContainsKey(type)) _subscribers[type].Remove(subscriber);
            }
        }
        /// <inheritdoc/>
        public void Unsubscribe<T>(object handler)
        {
            handler.ValidateArgument(nameof(handler));

            var subscriber = GetSubscribers<T>().FirstOrDefault(x => x.Handler.Equals(handler));

            if (subscriber != null) Unsubscribe(subscriber);
        }
        #endregion

        #region Untyped
        /// <inheritdoc/>
        public ISubscriber[] GetSubscribers()
        {
            lock (_allThreadLock)
            {
                return _allSubscribers.ToArray();
            }
        }

        /// <inheritdoc/>
        public void Subscribe(ISubscriber subscriber)
        {
            subscriber.ValidateArgument(nameof(subscriber));

            lock (_threadLock)
            {
                _logger.Debug($"Adding <{subscriber}> as subscriber to messages of all types");
                _allSubscribers.Add(subscriber);
            }
        }
        /// <inheritdoc/>
        public void Subscribe(object handler, Delegates.Async.AsyncAction<object, object, CancellationToken> action)
        {
            handler.ValidateArgument(nameof(handler));
            action.ValidateArgument(nameof(action));

            Subscribe(new DelegateSubscriber<object>(handler, (h, s, m, t) => action(s, m, t)));
        }

        /// <inheritdoc/>
        public void Unsubscribe(ISubscriber subscriber)
        {
            subscriber.ValidateArgument(nameof(subscriber));

            lock (_threadLock)
            {
                _logger.Debug($"Removing <{subscriber}> as subscriber to messages of all types");
                _allSubscribers.Remove(subscriber);
            }
        }
        /// <inheritdoc/>
        public void Unsubscribe(object handler)
        {
            handler.ValidateArgument(nameof(handler));

            var subscriber = GetSubscribers().FirstOrDefault(x => x.Handler.Equals(handler));

            if (subscriber != null) Unsubscribe(subscriber);
        }
        #endregion
    }
}
