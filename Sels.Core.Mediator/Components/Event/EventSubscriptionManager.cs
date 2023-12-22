using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sels.Core.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Sels.Core.Extensions.Logging;

namespace Sels.Core.Mediator.Event
{
    /// <summary>
    /// Manages event subscribers that subscribe to events at runtime.
    /// </summary>
    public class EventSubscriptionManager : IEventSubscriber
    {
        // Fields
        private readonly ILogger _logger;
        private readonly List<IEventListener> _listeners = new List<IEventListener>();
        private readonly IServiceProvider _serviceProvider;

        /// <inheritdoc cref="EventSubscriptionManager"/>
        /// <param name="serviceProvider">Service provider used to resolve typed <see cref="IEventSubscriber{TEvent}"/></param>
        /// <param name="logger">Optional logger for tracing</param>
        public EventSubscriptionManager(IServiceProvider serviceProvider, ILogger<EventSubscriptionManager> logger = null)
        {
            _serviceProvider = serviceProvider.ValidateArgument(nameof(serviceProvider));
            _logger = logger;
        }

        /// <inheritdoc/>
        public IEventListener[] GetAllListeners(IServiceProvider serviceProvider)
        {
            using var methodLogger = _logger.TraceMethod(this);

            lock (_listeners)
            {
                return _listeners.ToArray();
            }
        }
        /// <inheritdoc/>
        public IEventListener<TEvent>[] GetAllListeners<TEvent>(IServiceProvider serviceProvider)
        {
            using var methodLogger = _logger.TraceMethod(this);
            serviceProvider.ValidateArgument(nameof(serviceProvider));

            return serviceProvider.GetRequiredService<IEventSubscriber<TEvent>>().GetAllListeners();
        }
        /// <inheritdoc/>
        public EventSubscription Subscribe<TEvent>(IEventListener<TEvent> listener)
        {
            using var methodLogger = _logger.TraceMethod(this);
            listener.ValidateArgument(nameof(listener));

            using (var scope = _serviceProvider.CreateScope())
            {
                return scope.ServiceProvider.GetRequiredService<IEventSubscriber<TEvent>>().Subscribe(listener);
            }
        }
        /// <inheritdoc/>
        public EventSubscription Subscribe<TEvent>(Delegates.Async.AsyncAction<IEventListenerContext, TEvent, CancellationToken> subscriberAction, ushort? priority = null)
        {
            using var methodLogger = _logger.TraceMethod(this);
            subscriberAction.ValidateArgument(nameof(subscriberAction));

            using (var scope = _serviceProvider.CreateScope())
            {
                return scope.ServiceProvider.GetRequiredService<IEventSubscriber<TEvent>>().Subscribe(subscriberAction, priority);
            }
        }

        /// <inheritdoc/>
        public EventSubscription Subscribe(IEventListener listener)
        {
            using var methodLogger = _logger.TraceMethod(this);
            listener.ValidateArgument(nameof(listener));

            _listeners.Add(listener);
            _logger.Log($"Subscribed listener <{listener}> to all events");

            return new EventSubscription(listener, null, () => Unsubscribe(listener));
        }
        /// <inheritdoc/>
        public EventSubscription Subscribe(Delegates.Async.AsyncAction<IEventListenerContext, object, CancellationToken> subscriberAction, ushort? priority = null)
        {
            using var methodLogger = _logger.TraceMethod(this);
            subscriberAction.ValidateArgument(nameof(subscriberAction));
            return Subscribe(new DelegateEventListener(subscriberAction) { Priority = priority });
        }

        private void Unsubscribe(IEventListener listener)
        {
            using var methodLogger = _logger.TraceMethod(this);
            listener.ValidateArgument(nameof(listener));
            _logger.Log($"Unsubscribing <{listener}> from all events");
            _listeners.Remove(listener);
        }
    }

    /// <summary>
    /// Manages event subscribers that subscribe to events of type <typeparamref name="TEvent"/> at runtime.
    /// </summary>
    public class EventSubscriptionManager<TEvent> : IEventSubscriber<TEvent>
    {
        // Fields
        private readonly ILogger _logger;
        private readonly List<IEventListener<TEvent>> _listeners = new List<IEventListener<TEvent>>();

        /// <inheritdoc cref="EventSubscriptionManager{TEvent}"/>
        /// <param name="logger">Optional logger for tracing</param>
        public EventSubscriptionManager(ILogger<EventSubscriptionManager<TEvent>> logger = null)
        {
            _logger = logger;
        }

        /// <inheritdoc/>
        public IEventListener<TEvent>[] GetAllListeners() 
        {
            using var methodLogger = _logger.TraceMethod(this);
            lock (_listeners)
            {
                return _listeners.ToArray();
            }
        }
        /// <inheritdoc/>
        public EventSubscription Subscribe(IEventListener<TEvent> listener)
        {
            using var methodLogger = _logger.TraceMethod(this);
            listener.ValidateArgument(nameof(listener));

            _listeners.Add(listener);
            _logger.Log($"Subscribed listener <{listener}> to events of type <{typeof(TEvent)}>");
            return new EventSubscription(listener, typeof(TEvent), () => Unsubscribe(listener));
        }
        /// <inheritdoc/>
        public EventSubscription Subscribe(Delegates.Async.AsyncAction<IEventListenerContext, TEvent, CancellationToken> subscriberAction, ushort? priority = null)
        {
            using var methodLogger = _logger.TraceMethod(this);
            return Subscribe(new DelegateEventListener<TEvent>(subscriberAction) { Priority = priority });
        }

        private void Unsubscribe(IEventListener<TEvent> listener)
        {
            using var methodLogger = _logger.TraceMethod(this);
            listener.ValidateArgument(nameof(listener));
            _logger.Log($"Unsubscribing <{listener}> from events of type <{typeof(TEvent)}>");
            _listeners.Remove(listener);
        }
    }
}
