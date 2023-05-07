using Microsoft.Extensions.DependencyInjection;
using Sels.Core.Extensions.Conversion;

namespace Sels.Core.Mediator.Messaging
{
    /// <summary>
    /// Messanger that relies on DI to get the subscribers to send to.
    /// </summary>
    /// <typeparam name="T">The type of the message that can be sent</typeparam>
    [Obsolete($"Use the new Sels.Core.Mediator.Event components")]
    internal class Messenger<T> : IMessanger<T>
    {
        // Fields
        private readonly IMessageSubscriber _messageSubscriber;
        private readonly ISubscriber<T>[]? _subscribers;
        private readonly ISubscriber[]? _untypedSubscribers;
        private readonly ILogger? _logger;

        /// <inheritdoc cref="Messenger{T}"/>
        /// <param name="messageSubscriber">Manager used to get the runtime subscribers</param>
        /// <param name="subscribers">The global subscribers defined by the DI container</param>
        /// <param name="untypedSubscribers">The global subscribers defined by the DI container that can receive all messages</param>
        /// <param name="logger">Logger used for tracing</param>
        public Messenger(IMessageSubscriber messageSubscriber, IEnumerable<ISubscriber<T>>? subscribers = null, IEnumerable<ISubscriber>? untypedSubscribers = null, ILogger<Messenger<T>>? logger = null)
        {
            _messageSubscriber = messageSubscriber.ValidateArgument(nameof(messageSubscriber));
            _subscribers = subscribers.ToArrayOrDefault();
            _untypedSubscribers = untypedSubscribers.ToArrayOrDefault();
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<int> SendAsync(object sender, T message, CancellationToken token = default)
        {
            using (_logger.TraceMethod(this))
            {
                sender.ValidateArgument(nameof(sender));
                message.ValidateArgument(nameof(message));

                _logger.Log($"Received message <{message}> from <{sender}>. Sending to all subscribers");
                var tasks = new List<(object Subscriber, Task Task)>();

                // Typed subscribers
                var subscribers = Helper.Collection.EnumerateAll(_messageSubscriber.GetSubscribers<T>(), _subscribers).Where(x => x != null);
                tasks.AddRange(subscribers.Select(x =>
                {
                    _logger.Debug($"Sending <{message}> from <{sender}> to <{x}>");
                    return (x.CastTo<object>(), x.ReceiveAsync(sender, message, token));
                }));

                // Untyped subscribers
                var untypedSubscribers = Helper.Collection.EnumerateAll(_messageSubscriber.GetSubscribers(), _untypedSubscribers).Where(x => x != null);
                tasks.AddRange(untypedSubscribers.Select(x =>
                {
                    _logger.Debug($"Sending <{message}> from <{sender}> to <{x}>");
                    return (x.CastTo<object>(), x.ReceiveAsync(sender, message, token));
                }));

                // Gather all results
                var exceptions = new List<Exception>();
                foreach(var subscriberTask in tasks)
                {
                    try
                    {
                        await subscriberTask.Task;
                    }
                    catch(Exception ex)
                    {
                        _logger.Log($"Subscriber <{subscriberTask.Subscriber}> could not handle message <{message}>", ex);
                        exceptions.Add(ex);
                    }
                }

                return exceptions.HasValue() ? throw new AggregateException(exceptions) : tasks.Count;
            }
        }
    }

    /// <summary>
    /// Messanger that relies on DI to get the subscribers to send to.
    /// </summary>
    [Obsolete($"Use the new Sels.Core.Mediator.Event components")]
    internal class Messenger : IMessanger
    {
        // Fields
        private readonly IServiceProvider _provider;
        private readonly ILogger? _logger;

        /// <inheritdoc cref="Messenger{T}"/>
        /// <param name="provider">Service provider to resolve the messanger to send message with.</param>
        /// <param name="logger">Logger used for tracing</param>
        public Messenger(IServiceProvider provider, ILogger<Messenger>? logger = null)
        {
            _provider = provider.ValidateArgument(nameof(provider));
            _logger = logger;
        }

        /// <inheritdoc />
        public Task<int> SendAsync<T>(object sender, T message, CancellationToken token = default)
        {
            using var methodLogger = _logger.TraceMethod(this);

            sender.ValidateArgument(nameof(sender));
            message.ValidateArgument(nameof(message));

            var messanger = _provider.GetService<IMessanger<T>>();

            if(messanger != null)
            {
                return messanger.SendAsync(sender, message, token);
            }
            else
            {
                _logger.Warning($"No messanger registered for messages of type <{typeof(T)}>. Skipping");
                return Task.FromResult(0);
            }
        }
    }
}
