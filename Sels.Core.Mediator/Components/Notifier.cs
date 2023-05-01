using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sels.Core.Extensions.Logging.Advanced;
using Sels.Core.Mediator.Event;
using Sels.Core.Models.Disposables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Mediator
{
    /// <inheritdoc cref="INotifier"/>
    public class Notifier : INotifier
    {
        // Fields
        private readonly ILogger? _logger;
        private readonly IEventSubscriber _eventSubscriber;
        private readonly IServiceProvider _serviceProvider;

        /// <inheritdoc cref="Notifier"/>
        /// <param name="eventSubscriber">Subscriber used to get global listeners</param>
        /// <param name="serviceProvider">Provider used to resolve typed event and request subscribers</param>
        /// <param name="logger">Optional logger for tracing</param>
        public Notifier(IEventSubscriber eventSubscriber, IServiceProvider serviceProvider, ILogger<Notifier>? logger = null)
        {
            _eventSubscriber = Guard.IsNotNull(eventSubscriber);
            _serviceProvider = Guard.IsNotNull(serviceProvider);
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<int> RaiseEventAsync<TEvent>(object sender, TEvent eventData, CancellationToken token = default, EventOptions eventOptions = EventOptions.None)
        {
            using var methodLogger = _logger.TraceMethod(this);
            Guard.IsNotNull(eventData);
            Guard.IsNotNull(sender);

            // Launch current method as fire and forget task if enabled
            if (eventOptions.HasFlag(EventOptions.FireAndForget))
            {
                _logger.Debug($"Event <{eventData}> was raised with fire and forget flag. Starting task");
                _ = Task.Run(async () =>
                {
                    await RaiseEventAsync(sender, eventData, token, eventOptions & ~EventOptions.FireAndForget).ConfigureAwait(false);
                }).ConfigureAwait(false);
                return 0;
            }

            // Setup
            bool ignoreException = eventOptions.HasFlag(EventOptions.IgnoreExceptions);
            bool disableTransaction = eventOptions.HasFlag(EventOptions.NoTransaction);
            var eventLock = new object();
            var transactionSource = new TaskCompletionSource();
            var runningListeners = new List<EventContext>();
            int totalListeners = 0;
            Action onReadyAction = null;
            IDisposable tokenSubscription = NullDisposer.Instance;
            if (disableTransaction)
            {
                _logger.Debug($"Transaction disabled for event <{eventData}>");
                transactionSource.SetResult();
                // No need to set transaction state as it's already completed
                onReadyAction = new Action(() => { });
            }
            else
            {
                // Subscribe to token cancellation so we can abort the pending transaction
                tokenSubscription = token.Register(() => transactionSource.SetException(new OperationCanceledException("Event transaction was cancelled")));
                // Commit transaction when all listeners are ready
                onReadyAction = new Action(() =>
                {
                    lock (eventLock)
                    {
                        if (transactionSource.Task.IsCompleted) return;

                        var readyHandlers = runningListeners.Count(x => x.Ready);
                        if (readyHandlers == totalListeners)
                        {
                            _logger.Debug($"All <{readyHandlers}> event handlers are ready. Commiting transaction for event <{eventData}>");
                            transactionSource.SetResult();
                        }
                        else
                        {
                            _logger.Trace($"Only <{readyHandlers}/{totalListeners}> are ready. Waiting to commit transaction for event <{eventData}>");
                        }
                    }
                });
            }

            // Raise event
            using (tokenSubscription)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var provider = scope.ServiceProvider;
                    // Get listeners
                    var runtimeGlobalListeners = _eventSubscriber.GetAllListeners();
                    _logger.Debug($"Got <{runtimeGlobalListeners?.Length ?? 0}> listeners subscribed at runtime to all events");
                    var runtimeListeners = provider.GetRequiredService<IEventSubscriber<TEvent>>().GetAllListeners();
                    _logger.Debug($"Got <{runtimeListeners?.Length ?? 0}> listeners subscribed at runtime to event of type <{typeof(TEvent)}>");
                    var injectedGlobalListeners = provider.GetServices<IEventListener>()?.ToArray();
                    _logger.Debug($"Got <{injectedGlobalListeners?.Length ?? 0}> injected listeners subscribed to all events");
                    var injectedListeners = provider.GetServices<IEventListener<TEvent>>()?.ToArray();
                    _logger.Debug($"Got <{injectedListeners?.Length ?? 0}> injected listeners subscribed to event of type <{typeof(TEvent)}>");
                    var allListeners = Helper.Collection.EnumerateAll<object>(runtimeGlobalListeners, runtimeListeners, injectedGlobalListeners, injectedListeners).ToArray();
                    totalListeners = allListeners.Length;
                    if (totalListeners == 0)
                    {
                        _logger.Log($"No subscribers listening to event of type <{typeof(TEvent)}>");
                        return 0;
                    }
                    _logger.Log($"Raising event <{eventData}> to <{totalListeners}> subscribers");

                    // Start typed listeners
                    foreach (var listener in Helper.Collection.EnumerateAll(runtimeListeners, injectedListeners))
                    {
                        var context = new EventContext()
                        {
                            Sender = sender,
                            OtherSubscribers = allListeners.Where(x => x != listener).ToArray(),
                            OnReady = onReadyAction,
                            CommitTask = transactionSource.Task,
                            Listener = listener
                        };

                        _logger.Debug($"Raising event to <{listener}>");
                        runningListeners.Add(context);
                        try
                        {
                            context.ExecutionTask = listener.HandleAsync(context, eventData, token);
                        }
                        catch (Exception ex)
                        {
                            context.ExecutionException = ex;
                        }
                    }

                    // Start global listeners
                    foreach (var listener in Helper.Collection.EnumerateAll(runtimeGlobalListeners, injectedGlobalListeners))
                    {
                        var context = new EventContext()
                        {
                            Sender = sender,
                            OtherSubscribers = allListeners.Where(x => x != listener).ToArray(),
                            OnReady = onReadyAction,
                            CommitTask = transactionSource.Task,
                            Listener = listener
                        };

                        _logger.Debug($"Raising event to <{listener}>");
                        runningListeners.Add(context);
                        try
                        {
                            context.ExecutionTask = listener.HandleAsync(context, eventData, token);
                        }
                        catch (Exception ex)
                        {
                            context.ExecutionException = ex;
                        }
                    }

                    // Wait for all listeners to either finish executing or waiting for the transaction task to commit
                    _logger.Log($"Waiting for <{totalListeners}> to finish handling event <{eventData}>");
                    foreach (var executingListener in runningListeners)
                    {
                        try
                        {
                            await executingListener.ExecutionTask.ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            executingListener.ExecutionException = ex;
                        }
                    }

                    // Handle process exceptions
                    var failedListeners = runningListeners.Where(x => x.ExecutionException != null);
                    if (failedListeners.HasValue())
                    {
                        foreach (var listener in failedListeners)
                        {
                            _logger.Log($"Listener <{listener}> failed to handle event <{eventData}>", listener.ExecutionException);
                        }

                        if (!ignoreException) throw new AggregateException($"Some listeners failed to handle event <{eventData}>", failedListeners.Select(x => x.ExecutionException));
                    }

                    return totalListeners;
                }
            }
        }

        private class EventContext : IEventListenerContext
        {
            // Fields
            private Task _executionTask;
            private bool _ready;

            // Properties
            /// <inheritdoc/>
            public object Sender { get; init; }
            /// <inheritdoc/>
            public object[] OtherSubscribers { get; init; }

            /// <summary>
            /// Task containing the execution state of the listener.
            /// </summary>
            public Task ExecutionTask
            {
                get => _executionTask;
                set
                {
                    _executionTask = value;
                    _executionTask.ContinueWith(x => Ready = true);
                }
            }
            /// <summary>
            /// Action raised when the listener either finised execution or is ready to commit.
            /// </summary>
            public Action OnReady { get; init; }
            /// <summary>
            /// The task returned to the listener when they are waiting to be commited.
            /// </summary>
            public Task CommitTask { get; init; }
            /// <summary>
            /// Indicates that the current listener is ready
            /// </summary>
            public bool Ready
            {
                get => _ready;
                set
                {
                    _ready = value;
                    if (_ready)
                    {
                        OnReady?.Invoke();
                    }
                }
            }
            /// <summary>
            /// The listener that the current context is attached to.
            /// </summary>
            public object Listener { get; init; }
            /// <summary>
            /// Any exception throw by executing the listener.
            /// </summary>
            public Exception ExecutionException { get; set; }

            /// <inheritdoc/>
            public Task WaitForCommitAsync()
            {
                Ready = true;
                return CommitTask;
            }
        }
    }
}
