using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Linq;
using Sels.Core.Extensions.Logging;
using Sels.Core.Mediator.Event;
using Sels.Core.Models.Disposables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace Sels.Core.Mediator
{
    /// <inheritdoc cref="INotifier"/>
    public class Notifier : INotifier
    {
        // Fields
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IEventSubscriber _eventSubscriber;
        private readonly IServiceProvider _serviceProvider;

        /// <inheritdoc cref="Notifier"/>
        /// <param name="eventSubscriber">Subscriber used to get global listeners</param>
        /// <param name="serviceProvider">Provider used to resolve typed event and request subscribers</param>
        /// <param name="loggerFactory">Optional factory to create loggers for child instances</param>
        /// <param name="logger">Optional logger for tracing</param>
        public Notifier(IEventSubscriber eventSubscriber, IServiceProvider serviceProvider, ILoggerFactory loggerFactory = null, ILogger<Notifier> logger = null)
        {
            _eventSubscriber = eventSubscriber.ValidateArgument(nameof(eventSubscriber));
            _serviceProvider = serviceProvider.ValidateArgument(nameof(eventSubscriber));
            _loggerFactory = loggerFactory;
            _logger = logger;
        }
        
        /// <inheritdoc/>
        public async Task<int> RaiseEventAsync<TEvent>(object sender, TEvent @event, Action<INotifierEventOptions<TEvent>> eventOptions, CancellationToken token = default)
        {
            using var methodLogger = _logger.TraceMethod(this);
            sender.ValidateArgument(nameof(sender));
            eventOptions.ValidateArgument(nameof(eventOptions));
            @event.ValidateArgument(nameof(@event));

            _logger.Log($"Raising event <{@event}> created by <{sender}>");
            await using (var scope = _serviceProvider.CreateAsyncScope())
            {
                var provider = scope.ServiceProvider;
                var orchestrator = new EventOrchestrator(this, provider, _loggerFactory?.CreateLogger<EventOrchestrator>());
                var options = new NotifierEventOptions<TEvent>(this, orchestrator, sender, @event);
                // Enlist main listeners first
                Enlist(orchestrator, sender, @event);
                // Configure options
                eventOptions(options);

                // Run fire and forget
                if (options.Options.HasFlag(EventOptions.FireAndForget))
                {
                    // TODO: Use task manager to gracefully wait for tasks to complete when IoC containers gets disposed.
                    _ = Task.Run(async () => await RaiseEventAsync(orchestrator, sender, @event, options, token).ConfigureAwait(false));
                    return 0;
                }

                // Execute
                return await RaiseEventAsync(orchestrator, sender, @event, options, token).ConfigureAwait(false);
            }
        }

        private async Task<int> RaiseEventAsync<TEvent>(EventOrchestrator orchestrator, object sender, TEvent @event, NotifierEventOptions<TEvent> eventOptions, CancellationToken token)
        {
            using var methodLogger = _logger.TraceMethod(this);
            sender.ValidateArgument(nameof(sender));
            eventOptions.ValidateArgument(nameof(eventOptions));
            @event.ValidateArgument(nameof(@event));

            var ignoreExceptions = eventOptions.Options.HasFlag(EventOptions.IgnoreExceptions);
            var allowParallelExecution = eventOptions.Options.HasFlag(EventOptions.AllowParallelExecution);

            try
            {
                _logger.Log($"Executing event transaction for event <{@event}> created by <{sender}> with any enlisted event listeners");
                return await orchestrator.ExecuteAsync(allowParallelExecution, token).ConfigureAwait(false);
            }
            catch(Exception ex)
            {
                _logger.Log($"Something went wrong while raising event <{@event}> created by <{sender}>", ex);

                if (!ignoreExceptions) throw;
                return 0;
            }
        }

        /// <summary>
        /// Enlists all event listeners to <paramref name="orchestrator"/> if there are any listening for events of type <typeparamref name="TEvent"/>.
        /// </summary>
        /// <typeparam name="TEvent">The type of event to enlist</typeparam>
        /// <param name="orchestrator">The orchestrator to add the listeners to</param>
        /// <param name="sender">The object that raised <paramref name="event"/></param>
        /// <param name="event">The event that was raised</param>
        /// <returns>Task that will complete when all listeners have been enlisted to <paramref name="orchestrator"/></returns>
        public void Enlist<TEvent>(EventOrchestrator orchestrator, object sender, TEvent @event)
        {
            using var methodLogger = _logger.TraceMethod(this);
            orchestrator.ValidateArgument(nameof(orchestrator));
            sender.ValidateArgument(nameof(sender));
            @event.ValidateArgument(nameof(@event));

            var provider = orchestrator.ServiceProvider;
            // Get listeners
            var runtimeGlobalListeners = _eventSubscriber.GetAllListeners(provider);
            _logger.Debug($"Got <{runtimeGlobalListeners?.Length ?? 0}> listeners subscribed at runtime to all events");
            runtimeGlobalListeners.Execute(x => orchestrator.Enlist(sender, x, @event));

            var runtimeListeners = provider.GetRequiredService<IEventSubscriber<TEvent>>().GetAllListeners();
            _logger.Debug($"Got <{runtimeListeners?.Length ?? 0}> listeners subscribed at runtime to event of type <{typeof(TEvent)}>");
            runtimeListeners.Execute(x => orchestrator.Enlist(sender, x, @event));

            var injectedGlobalListeners = provider.GetServices<IEventListener>()?.ToArray();
            _logger.Debug($"Got <{injectedGlobalListeners?.Length ?? 0}> injected listeners subscribed to all events");
            injectedGlobalListeners.Execute(x => orchestrator.Enlist(sender, x, @event));

            var injectedListeners = provider.GetServices<IEventListener<TEvent>>()?.ToArray();
            _logger.Debug($"Got <{injectedListeners?.Length ?? 0}> injected listeners subscribed to event of type <{typeof(TEvent)}>");
            injectedListeners.Execute(x => orchestrator.Enlist(sender, x, @event));
        }

        /// <inheritdoc cref="INotifierEventOptions{TEvent}"/>
        /// <typeparam name="TEvent"></typeparam>
        private class NotifierEventOptions<TEvent> : INotifierEventOptions<TEvent>
        {
            // Fields
            private readonly Notifier _parent;
            private readonly EventOrchestrator _orchestrator;
            private readonly object _sender;
            private readonly TEvent _event;

            // Properties
            /// <summary>
            /// The configured options.
            /// </summary>
            public EventOptions Options { get; private set; }

            public NotifierEventOptions(Notifier parent, EventOrchestrator eventOrchestrator, object sender, TEvent @event)
            {
                _parent = parent.ValidateArgument(nameof(parent));
                _orchestrator = eventOrchestrator.ValidateArgument(nameof(eventOrchestrator));
                _sender = sender.ValidateArgument(nameof(sender));
                _event = @event.ValidateArgument(nameof(@event));
            }

            /// <inheritdoc/>
            public INotifierEventOptions<TEvent> WithOptions(EventOptions options)
            {
                Options = options;
                return this;
            }
            /// <inheritdoc/>
            public INotifierEventOptions<TEvent> AlsoRaiseAs<T>()
            {
                if (!(_event is T)) throw new ArgumentException($"Event <{_event}> is not assignable to type <{typeof(T)}>");

                _parent.Enlist(_orchestrator, _sender, _event.CastTo<T>());

                return this;
            }
            /// <inheritdoc/>
            public INotifierEventOptions<TEvent> Enlist<T>(T @event)
            {
                @event.ValidateArgument(nameof(@event));
                _parent.Enlist(_orchestrator, _sender, @event);

                return this;
            }
        }
    }
}
