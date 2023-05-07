using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Mediator.Event
{
    /// <summary>
    /// Manages event subscribers that subscribe to events at runtime.
    /// </summary>
    public class EventSubscriptionManager : IEventSubscriber
    {
        // Fields
        private readonly ILogger? _logger;
        private readonly SynchronizedCollection<IEventListener> _listeners = new SynchronizedCollection<IEventListener>();
        private readonly IServiceProvider _serviceProvider;

        /// <inheritdoc cref="EventSubscriptionManager"/>
        /// <param name="serviceProvider">Service provider used to resolve typed <see cref="IEventSubscriber{TEvent}"/></param>
        /// <param name="logger">Optional logger for tracing</param>
        public EventSubscriptionManager(IServiceProvider serviceProvider, ILogger<EventSubscriptionManager>? logger = null)
        {
            _serviceProvider = Guard.IsNotNull(serviceProvider);
            _logger = logger;
        }

        /// <inheritdoc/>
        public IEventListener[] GetAllListeners()
        {
            using var methodLogger = _logger.TraceMethod(this);

            lock (_listeners.SyncRoot)
            {
                return _listeners.ToArray();
            }
        }
        /// <inheritdoc/>
        public IEventListener<TEvent>[] GetAllListeners<TEvent>()
        {
            using var methodLogger = _logger.TraceMethod(this);

            using(var scope = _serviceProvider.CreateScope())
            {
                return scope.ServiceProvider.GetRequiredService<IEventSubscriber<TEvent>>().GetAllListeners();
            }
        }
        /// <inheritdoc/>
        public void Subscribe<TEvent>(IEventListener<TEvent> listener)
        {
            using var methodLogger = _logger.TraceMethod(this);

            using (var scope = _serviceProvider.CreateScope())
            {
                scope.ServiceProvider.GetRequiredService<IEventSubscriber<TEvent>>().Subscribe(listener);
            }
        }
        /// <inheritdoc/>
        public void Subscribe<TEvent>(object listener, Delegates.Async.AsyncAction<IEventListenerContext, TEvent, CancellationToken> subscriberAction)
        {
            using var methodLogger = _logger.TraceMethod(this);

            using (var scope = _serviceProvider.CreateScope())
            {
                scope.ServiceProvider.GetRequiredService<IEventSubscriber<TEvent>>().Subscribe(listener, subscriberAction);
            }
        }
        /// <inheritdoc/>
        public void Unsubscribe<TEvent>(IEventListener<TEvent> listener)
        {
            using var methodLogger = _logger.TraceMethod(this);

            using (var scope = _serviceProvider.CreateScope())
            {
                scope.ServiceProvider.GetRequiredService<IEventSubscriber<TEvent>>().Unsubscribe(listener);
            }
        }
        /// <inheritdoc/>
        public void Unsubscribe<TEvent>(object listener)
        {
            using var methodLogger = _logger.TraceMethod(this);

            using (var scope = _serviceProvider.CreateScope())
            {
                scope.ServiceProvider.GetRequiredService<IEventSubscriber<TEvent>>().Unsubscribe(listener);
            }
        }

        /// <inheritdoc/>
        public void Subscribe(IEventListener listener)
        {
            using var methodLogger = _logger.TraceMethod(this);
            Guard.IsNotNull(listener);
            Guard.IsNotNull(listener.Handler);


            _listeners.Add(listener);
            _logger.Log($"Subscribed listener <{listener}> handled by <{listener.Handler}> to all events");
        }
        /// <inheritdoc/>
        public void Subscribe(object listener, Delegates.Async.AsyncAction<IEventListenerContext, object, CancellationToken> subscriberAction)
        {
            using var methodLogger = _logger.TraceMethod(this);
            Subscribe(new DelegateEventListener(listener, subscriberAction));
        }
        /// <inheritdoc/>
        public void Unsubscribe(IEventListener listener)
        {
            using var methodLogger = _logger.TraceMethod(this);
            if (_listeners.Remove(listener))
            {
                _logger.Log($"Unsubscribed listener <{listener}> handled by <{listener.Handler}> to all events");
            }
            else
            {
                _logger.Warning($"Listener <{listener}> handled by <{listener.Handler}> was not subscribed to all events so can't unsubscribe");
            }
        }
        /// <inheritdoc/>
        public void Unsubscribe(object listener)
        {
            using var methodLogger = _logger.TraceMethod(this);

            IEventListener? eventSubscriber = null;
            lock (_listeners.SyncRoot)
            {
                eventSubscriber = _listeners.FirstOrDefault(x => x.Handler == listener);
            }

            if (eventSubscriber != null)
            {
                Unsubscribe(eventSubscriber);
            }
            else
            {
                _logger.Warning($"Could not find a listener subscribed to all events that is handled by <{listener}> so can't unsubscribe");
            }
        }
    }

    /// <summary>
    /// Manages event subscribers that subscribe to events of type <typeparamref name="TEvent"/> at runtime.
    /// </summary>
    public class EventSubscriptionManager<TEvent> : IEventSubscriber<TEvent>
    {
        // Fields
        private readonly ILogger? _logger;
        private readonly SynchronizedCollection<IEventListener<TEvent>> _listeners = new SynchronizedCollection<IEventListener<TEvent>>();

        /// <inheritdoc cref="EventSubscriptionManager{TEvent}"/>
        /// <param name="logger">Optional logger for tracing</param>
        public EventSubscriptionManager(ILogger<EventSubscriptionManager<TEvent>>? logger = null)
        {
            _logger = logger;
        }

        /// <inheritdoc/>
        public IEventListener<TEvent>[] GetAllListeners() 
        {
            using var methodLogger = _logger.TraceMethod(this);
            lock (_listeners.SyncRoot)
            {
                return _listeners.ToArray();
            }
        }

        /// <inheritdoc/>
        public void Subscribe(IEventListener<TEvent> listener)
        {
            using var methodLogger = _logger.TraceMethod(this);
            Guard.IsNotNull(listener);
            Guard.IsNotNull(listener.Handler);
            

            _listeners.Add(listener);
            _logger.Log($"Subscribed listener <{listener}> handled by <{listener.Handler}> to events of type <{typeof(TEvent)}>");
        }
        /// <inheritdoc/>
        public void Subscribe(object listener, Delegates.Async.AsyncAction<IEventListenerContext, TEvent, CancellationToken> subscriberAction)
        {
            using var methodLogger = _logger.TraceMethod(this);
            Subscribe(new DelegateEventListener<TEvent>(listener, subscriberAction));
        }
        /// <inheritdoc/>
        public void Unsubscribe(IEventListener<TEvent> listener)
        {
            using var methodLogger = _logger.TraceMethod(this);
            if (_listeners.Remove(listener))
            {
                _logger.Log($"Unsubscribed listener <{listener}> handled by <{listener.Handler}> to events of type <{typeof(TEvent)}>");
            }
            else
            {
                _logger.Warning($"Listener <{listener}> handled by <{listener.Handler}> was not subscribed to events of type <{typeof(TEvent)}> so can't unsubscribe");
            }
        }
        /// <inheritdoc/>
        public void Unsubscribe(object listener)
        {
            using var methodLogger = _logger.TraceMethod(this);

            IEventListener<TEvent>? eventSubscriber = null;
            lock(_listeners.SyncRoot)
            {
                eventSubscriber = _listeners.FirstOrDefault(x => x.Handler == listener);
            }

            if (eventSubscriber != null)
            {
                Unsubscribe(eventSubscriber);
            }
            else
            {
                _logger.Warning($"Could not find a listener subscribed to events of type <{typeof(TEvent)}> that is handled by <{listener}> so can't unsubscribe");
            }
        }
    }
}
