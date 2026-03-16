using Sels.Core.Extensions;
using Sels.Core.Mediator.Event;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Sels.Core.Delegates.Async;
using Sels.Core.Extensions.Logging;
using Microsoft.Extensions.Logging;
using System.Linq;
using Sels.Core.Extensions.Collections;
using Sels.Core.Extensions.Threading;
using Sels.Core.Models;
using Sels.Core.Extensions.Calculation;
using Sels.Core.Extensions.Exceptions;

namespace Sels.Core.Mediator.Event
{
    /// <summary>
    /// Orchestates and executes event listeners.
    /// </summary>
    public class EventOrchestrator : IEventTransactionScope
    {
        // Fields
        private readonly Notifier _parent;
        private readonly ILogger _logger;
        private readonly Dictionary<object, List<(object Sender, IMessageHandler Listener, AsyncAction<IEventListenerContext, CancellationToken> ExecuteAsync)>> _enlistedListeners = new Dictionary<object, List<(object Sender, IMessageHandler Listener, AsyncAction<IEventListenerContext, CancellationToken> ExecuteAsync)>>();

        // State
        private Queue<(object Sender, IMessageHandler Listener, object Event, AsyncAction<IEventListenerContext, CancellationToken> ExecuteAsync)> _pending;
        private List<EventExecutionContext> _executing = new List<EventExecutionContext>();

        // Properties
        private Queue<(object Sender, IMessageHandler Listener, object Event, AsyncAction<IEventListenerContext, CancellationToken> ExecuteAsync)> Pending { 
            get
            {
                if(_pending == null)
                {
                    // Get all not executing already ordered
                    var pending = _enlistedListeners.SelectMany(x => x.Value, (x, e) => (e.Sender, e.Listener, x.Key, e.ExecuteAsync))
                                                    .Where(x => !_executing.Any(e => e.ListenerContext.Listener.Equals(x.Listener)))
                                                    .OrderByDescending(x => x.Listener.Priority.HasValue)
                                                    .ThenBy(x => x.Listener.Priority);

                    if (pending.HasValue()) _pending = new Queue<(object Sender, IMessageHandler Listener, object Event, AsyncAction<IEventListenerContext, CancellationToken> ExecuteAsync)>(pending);
                    else _pending = new Queue<(object Sender, IMessageHandler Listener, object Event, AsyncAction<IEventListenerContext, CancellationToken> ExecuteAsync)>();
                }

                return _pending;
            } 
        }
        /// <summary>
        /// The service provider scope used to resolve the listeners.
        /// </summary>
        public IServiceProvider ServiceProvider { get; }

        /// <inheritdoc cref="EventOrchestrator"/>
        /// <param name="parent">The notifier that created the current instance</param>
        /// <param name="serviceProvider"><inheritdoc cref="ServiceProvider"/></param>
        /// <param name="logger">Optional logger for tracing</param>
        public EventOrchestrator(Notifier parent, IServiceProvider serviceProvider, ILogger logger = null)
        {
            _parent = parent.ValidateArgument(nameof(parent));
            ServiceProvider = serviceProvider.ValidateArgument(nameof(serviceProvider));
            _logger = logger;
        }

        /// <inheritdoc/>
        public void Enlist<TEvent>(object sender, IEventListener listener, TEvent @event)
        {
            using var methodLogger = _logger.TraceMethod(this);
            sender.ValidateArgument(nameof(sender));
            listener.ValidateArgument(nameof(listener));
            @event.ValidateArgument(nameof(@event));

            _logger.Log($"Enlisting listener <{listener}> in current transaction for event <{@event}> raised by <{sender}>");
            lock(_enlistedListeners)
            {
                if (_enlistedListeners.ContainsKey(@event) && _enlistedListeners[@event].Any(l => l.Listener.Equals(listener)))
                {
                    _logger.Debug($"Listener <{listener}> already enlisted for event <{@event}>. Skipping");
                }
                else
                {
                    _enlistedListeners.AddValueToList(@event, (sender, listener, (c, t) => listener.HandleAsync(c, @event, t)));

                    // Clear queue as new listeners have been added to transaction
                    _pending = null;
                }
            }
        }
        /// <inheritdoc/>
        public void Enlist<TEvent>(object sender, IEventListener<TEvent> listener, TEvent @event)
        {
            using var methodLogger = _logger.TraceMethod(this);
            sender.ValidateArgument(nameof(sender));
            listener.ValidateArgument(nameof(listener));
            @event.ValidateArgument(nameof(@event));

            _logger.Log($"Enlisting listener <{listener}> in current transaction for event <{@event}> raised by <{sender}>");
            lock (_enlistedListeners)
            {
                if (_enlistedListeners.ContainsKey(@event) && _enlistedListeners[@event].Any(l => l.Listener.Equals(listener)))
                {
                    _logger.Debug($"Listener <{listener}> already enlisted for event <{@event}>. Skipping");
                }
                else
                {
                    _enlistedListeners.AddValueToList(@event, (sender, listener, (c, t) => listener.HandleAsync(c, @event, t)));
                    // Clear queue as new listeners have been added to transaction
                    _pending = null;
                }
            }
        }
        /// <inheritdoc/>
        public async Task<int> ExecuteAsync(bool runInParallel, CancellationToken token)
        {
            using var methodLogger = _logger.TraceMethod(this);
            if (Pending.Count == 0)
            {
                _logger.Log($"No event listeners enlisted so skipping");
                return 0;
            }

            var transactionSource = runInParallel ? new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously) : null;

            CancellationTokenRegistration registration = default;
            if(transactionSource != null) registration = token.Register(() => { lock (transactionSource) { if (!transactionSource.Task.IsCompleted) transactionSource.SetCanceled(); } });

            using var scope = registration;

            _logger.Log($"Executing <{Pending.Count}> event listeners for <{_enlistedListeners.Keys.Count}> events in a transaction");

            Ref<TimeSpan> duration;
            using (Helper.Time.CaptureDuration(out duration))
            {
                try
                {
                    try
                    {
                        while (Pending.Count != 0)
                        {
                            while (Pending.TryDequeue(out var pending))
                            {
                                token.ThrowIfCancellationRequested();
                                _logger.Debug($"Executing listeners <{pending.Listener}> with <{pending.Event}>");
                                var context = new EventExecutionContext(this, transactionSource, pending);
                                _executing.Add(context);

                                
                                if (runInParallel)
                                {
                                    // Execute listener
                                    context.StartExecution(token);
                                }
                                else{
                                    await context.ExecuteAsync(token).ConfigureAwait(false);
                                }
                            }

                            // Wait for all callbacks if running in parallel
                            if (runInParallel)
                            {
                                _logger.Debug($"Parallel execution enabled. Waiting for callbacks");
                                await Task.WhenAll(_executing.Select(x => x.Callback)).ConfigureAwait(false);
                            } 
                        }

                        // Complete transaction
                        _logger.Log($"All event listeners are ready to commit. Completing transaction");
                        if (transactionSource != null)
                        {
                            lock (transactionSource)
                            {
                                if (!transactionSource.Task.IsCompleted)
                                {
                                    transactionSource.TrySetResult(null);
                                }
                            }
                        }
                    }
                    catch (OperationCanceledException) when (token.IsCancellationRequested)
                    {
                        _logger.Log(LogLevel.Warning, $"Received cancellation request while raising event. Cancelling any waiting on transaction");

                        if (transactionSource != null)
                        {
                            lock (transactionSource)
                            {
                                if (!transactionSource.Task.IsCompleted)
                                {
                                    transactionSource.TrySetCanceled();
                                }
                            }
                        }

                        if (_executing.HasValue()) await Task.WhenAll(_executing.Select(x => x.Result)).ConfigureAwait(false);
                        throw;
                    }
                    catch (Exception ex)
                    {
                        _logger.Log($"Could not execute all event listeners successfully. Cancelling any waiting on transaction", ex);

                        if (transactionSource != null)
                        {
                            lock (transactionSource) {
                                if (!transactionSource.Task.IsCompleted)
                                {
                                    transactionSource.TrySetException(ex);
                                }
                            }
                        }

                        if (_executing.HasValue()) await Task.WhenAll(_executing.Select(x => x.Result)).ConfigureAwait(false);
                        throw;
                    }

                    // Wait for event listeners to complete
                    _logger.Log($"Waiting for <{_executing.Count}> event listeners to complete");
                    await Task.WhenAll(_executing.Select(x => x.Result)).ConfigureAwait(false);
                }
                // Unwrap
                catch (AggregateException ex)
                {
                    var innerExceptions = ex.InnerExceptions;

                    // Only one exception so rethrow
                    if(innerExceptions.Count == 1)
                    {
                        innerExceptions.First().Rethrow();
                    }
                    // Only cancelled exceptions so throw just one
                    else if (!innerExceptions.AreAllUnique() && innerExceptions.First() is OperationCanceledException operationCanceledException)
                    {
                        operationCanceledException.Rethrow();
                    }
                    // Filter out duplicates
                    else
                    {
                        throw new AggregateException(innerExceptions.Distinct());
                    }

                    throw;
                }
            }

            _logger.Log($"Orchestrated <{_enlistedListeners.Keys.Count}> events to <{_executing.Count}> event listeners in <{duration.Value.TotalMilliseconds.RoundTo(4)}ms>");
            return _executing.Count;
        }

        private class EventExecutionContext : IEventListenerContext
        {
            // Fields
            private readonly ILogger _logger;
            private readonly EventOrchestrator _parent;
            private readonly TaskCompletionSource<object> _transactionSource;
            private TaskCompletionSource<object> _callbackSource;
            private Task _result;

            // Properties
            public (object Sender, IMessageHandler Listener, object Event, AsyncAction<IEventListenerContext, CancellationToken> ExecuteAsync) ListenerContext { get; }
            /// <summary>
            /// Completed when either the event listener executes successfully or is waiting for the transaction to commit.
            /// </summary>
            public Task Callback => _callbackSource?.Task ?? Task.CompletedTask;
            /// <summary>
            /// Task containing the event listener execution state.
            /// </summary>
            public Task Result => _result ?? Task.CompletedTask;
            /// <inheritdoc/>
            public object Sender => ListenerContext.Sender;
            /// <inheritdoc/>
            public IMessageHandler[] OtherSubscribers { 
                get 
                {
                    lock (_parent._enlistedListeners)
                    {
                        return _parent._enlistedListeners[ListenerContext.Event].Where(x => !x.Listener.Equals(ListenerContext.Listener)).Select(x => x.Listener).ToArray();
                    }
                } 
            }

            public EventExecutionContext(EventOrchestrator parent, TaskCompletionSource<object> transactionSource, (object Sender, IMessageHandler Listener, object Event, AsyncAction<IEventListenerContext, CancellationToken> ExecuteAsync) listener)
            {
                _parent = parent.ValidateArgument(nameof(parent));
                _logger = parent._logger;
                ListenerContext = listener;
                _transactionSource = transactionSource;
            }
            /// <inheritdoc/>
            public void EnlistEvent<TEvent>(TEvent @event)
            {
                @event.ValidateArgument(nameof(@event));
                _parent._parent.Enlist(_parent, ListenerContext.Listener, @event);
            }
            /// <inheritdoc/>
            public async Task WaitForCommitAsync()
            {
                CompleteCallback();
                if (_transactionSource != null) await _transactionSource.Task.ConfigureAwait(false);
                else return;
            }

            /// <summary>
            /// Starts the execition current event listener.
            /// </summary>
            /// <param name="token">Token that can be cancelled by the caller</param>
            public void StartExecution(CancellationToken token)
            {
                _callbackSource = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
                _logger.Log($"Starting executing for <{ListenerContext.Listener}> who has a priority of <{(ListenerContext.Listener.Priority.HasValue ? ListenerContext.Listener.Priority.Value.ToString() : "NULL")}>");
                _result = ListenerContext.ExecuteAsync(this, token);

                // Let orchestrator return if listener executes without transaction
                _result.ContinueWith(x => CompleteCallback(x));
                _logger.Log($"Started executing for <{ListenerContext.Listener}> who has a priority of <{(ListenerContext.Listener.Priority.HasValue ? ListenerContext.Listener.Priority.Value.ToString() : "NULL")}>");
            }

            /// <summary>
            /// Executes the current event listener.
            /// </summary>
            /// <param name="token">Token that can be cancelled by the caller</param>
            public async Task ExecuteAsync(CancellationToken token)
            {
                _logger.Log($"Executing <{ListenerContext.Listener}> who has a priority of <{(ListenerContext.Listener.Priority.HasValue ? ListenerContext.Listener.Priority.Value.ToString() : "NULL")}>");
                 await ListenerContext.ExecuteAsync(this, token).ConfigureAwait(false);

                _logger.Log($"Started executing for <{ListenerContext.Listener}> who has a priority of <{(ListenerContext.Listener.Priority.HasValue ? ListenerContext.Listener.Priority.Value.ToString() : "NULL")}>");
            }

            void CompleteCallback(Task result = null)
            {
                if(_callbackSource != null)
                {
                    lock (_callbackSource)
                    {
                        if (!_callbackSource.Task.IsCompleted)
                        {
                            _logger.Log($"Completing callback for <{ListenerContext.Listener}>");

                            if (result != null) _callbackSource.SetFrom(result);
                            else _callbackSource.SetResult(null);
                        }
                    }
                }             
            }
        }
    }
}
