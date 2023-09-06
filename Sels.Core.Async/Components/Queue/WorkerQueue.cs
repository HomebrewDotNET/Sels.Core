using Microsoft.Extensions.Logging;
using Sels.Core.Async.TaskManagement;
using Sels.Core.Dispose;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Linq;
using Sels.Core.Extensions.Logging;
using Sels.Core.Extensions.Reflection;
using Sels.Core.Extensions.Threading;
using Sels.Core.Scope.Actions;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using static Sels.Core.Delegates;

namespace Sels.Core.Async.Queue
{
    /// <summary>
    /// Asynchronous queue that acts as a buffer for work that needs to be scheduled on workers.
    /// Exposes events that can be handled asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the elements</typeparam>
    public class WorkerQueue<T> : IAsyncExposedDisposable, IEnumerable<T>
    {
        // Statics
        private static int Seed = 0;
        private static object SeedLock = new object();

        // Fields
        private readonly List<EventHandlerSubscription> _subscriptions = new List<EventHandlerSubscription>();
        private readonly ConcurrentQueue<DequeueRequest> _requests = new ConcurrentQueue<DequeueRequest>();
        private readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();
        private readonly SemaphoreSlim _asyncLock = new SemaphoreSlim(1, 1);
        /// <summary>
        /// Optional logger for tracing.
        /// </summary>
        private readonly ILogger? _logger;
        private int _eventSequence = 0;

        // Properties
        /// <summary>
        /// The unique id of the queue.
        /// </summary>
        public int Id { get; }
        /// <summary>
        /// The maximum amount of items that can be enqueued. When set to null there isn't any limit.
        /// </summary>
        public int? MaxSize { get; }
        /// <summary>
        /// How many items are enqueued.
        /// </summary>
        public int Count => _queue.Count;
        /// <summary>
        /// Task manager used to manage event handlers.
        /// </summary>
        public ITaskManager TaskManager { get; }
        /// <summary>
        /// Optional delegate that can be used to release any remaining items in the queue if the queue itself gets disposed.
        /// </summary>
        public Func<T, Task> OnDisposeHandler { get; set; } = async (t) =>
        {
            if (t is IAsyncDisposable asyncDisposable) await asyncDisposable.DisposeAsync();
            else if (t is IDisposable disposable) disposable.Dispose();
        };
        /// <inheritdoc/>
        public bool? IsDisposed { get; private set; }

        /// <inheritdoc cref="WorkerQueue{T}"/>
        /// <param name="taskManager"><inheritdoc cref="TaskManager"/></param>
        /// <param name="maxSize"><inheritdoc cref="MaxSize"/></param>
        /// <param name="initalItems">Initial items to add to the queue</param>
        /// <param name="logger"><inheritdoc cref="_logger"/></param>
        public WorkerQueue(ITaskManager taskManager, int? maxSize, IEnumerable<T> initalItems, ILogger? logger = null)
        {
            TaskManager = taskManager.ValidateArgument(nameof(taskManager));
            _logger = logger;

            MaxSize = maxSize.HasValue ? maxSize.Value.ValidateArgumentLargerOrEqual(nameof(maxSize), 1) : (int?)null;
            initalItems.ValidateArgument(nameof(initalItems));

            lock (SeedLock)
            {
                Id = ++Seed;
            }

            initalItems.Execute(x => _queue.Enqueue(x));
        }
        /// <inheritdoc cref="WorkerQueue{T}"/>
        /// <param name="taskManager"><inheritdoc cref="TaskManager"/></param>
        /// <param name="maxSize"><inheritdoc cref="MaxSize"/></param>
        /// <param name="logger"><inheritdoc cref="_logger"/></param>
        public WorkerQueue(ITaskManager taskManager, int? maxSize, ILogger? logger = null) : this(taskManager, maxSize, Enumerable.Empty<T>(), logger)
        {
        }
        /// <inheritdoc cref="WorkerQueue{T}"/>
        /// <param name="taskManager"><inheritdoc cref="TaskManager"/></param>
        /// <param name="initalItems">Initial items to add to the queue</param>
        /// <param name="logger"><inheritdoc cref="_logger"/></param>
        public WorkerQueue(ITaskManager taskManager, IEnumerable<T> initalItems, ILogger? logger = null) : this(taskManager, null, initalItems, logger)
        {
        }
        /// <inheritdoc cref="WorkerQueue{T}"/>
        /// <param name="taskManager"><inheritdoc cref="TaskManager"/></param>
        /// <param name="logger"><inheritdoc cref="_logger"/></param>
        public WorkerQueue(ITaskManager taskManager, ILogger? logger = null) : this(taskManager, (int?)null, logger)
        {

        }

        /// <summary>
        /// Enqueues <paramref name="item"/>.
        /// Method will throw if queue is full.
        /// </summary>
        /// <param name="item">The item to enqueue</param>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>Task that will complete when the item is enqueued</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task EnqueueAsync(T item, CancellationToken token = default)
        {
            using var methodLogger = _logger.TraceMethod(this);
            if (IsDisposed.HasValue) throw new ObjectDisposedException(GetType().GetDisplayName(false));
            item.ValidateArgument(nameof(item));
            if (!await TryEnqueueAsync(item, token).ConfigureAwait(false)) throw new InvalidOperationException($"Worker queue already contains the maximum allowed items ({MaxSize})");
        }
        /// <summary>
        /// Tries to enqueue <paramref name="item"/>.
        /// </summary>
        /// <param name="item">The item to enqueue</param>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>True if the item was enqueued, or false if the queue is full</returns>
        public async Task<bool> TryEnqueueAsync(T item, CancellationToken token = default)
        {
            using var methodLogger = _logger.TraceMethod(this);
            if (IsDisposed.HasValue) throw new ObjectDisposedException(GetType().GetDisplayName(false));
            item.ValidateArgument(nameof(item));

            _logger.Log($"Trying to enqueue item <{item}>");
            await using (await LockAsync(token).ConfigureAwait(false))
            {
                // Queue full
                if (MaxSize.HasValue && Count >= MaxSize)
                {
                    _logger.Warning($"Queue is already full. Rejecting item");
                    return false;
                }

                // Try assign first
                while (_requests.TryDequeue(out var request))
                {
                    lock (request)
                    {
                        using (request)
                        {
                            if (request.Callback.IsCompleted) continue;
                            request.Assign(item);
                            _logger.Log($"Item <{item}> assigned to next request");
                            return true;
                        }
                    }

                }

                _queue.Enqueue(item);
                _logger.Log($"Item <{item}> added to queue");
                TriggerEvents(true, token);
                return true;
            }
        }
        /// <summary>
        /// Tries to dequeue the next item from the queue.
        /// </summary>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>Dequeued: true if the queue wasn't empty, otherwise false | Item: The item if one was dequeued, otherwise the default value for the type</returns>
        public async Task<(bool Dequeued, T Item)> TryDequeueAsync(CancellationToken token = default)
        {
            using var methodLogger = _logger.TraceMethod(this);
            if (IsDisposed.HasValue) throw new ObjectDisposedException(GetType().GetDisplayName(false));

            await using (await LockAsync(token).ConfigureAwait(false))
            {
                if (_queue.TryDequeue(out var item))
                {
                    _logger.Log($"Item <{item}> was dequeued");
                    return (true, item);
                }
                else
                {
                    _logger.Log($"Queue is empty");
                    return (false, default);
                }
            }
        }
        /// <summary>
        /// Dequeue the next item in the queue.
        /// If the queue is empty the method call will block until more items are enqueued.
        /// </summary>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>Task that will complete when an item gets assigned</returns>
        public async Task<T> DequeueAsync(CancellationToken token = default)
        {
            using var methodLogger = _logger.TraceMethod(this);
            if (IsDisposed.HasValue) throw new ObjectDisposedException(GetType().GetDisplayName(false));

            DequeueRequest request = null;
            await using (await LockAsync(token).ConfigureAwait(false))
            {
                if (_queue.TryDequeue(out var item))
                {
                    _logger.Log($"Item <{item}> was dequeued");
                    TriggerEvents(false, token);
                    return item;
                }
                else
                {
                    _logger.Log($"Queue is empty. Logging request for caller");
                    request = new DequeueRequest(token);
                    _requests.Enqueue(request);
                }
            }

            return await request.Callback;
        }

        /// <summary>
        /// Subscribe to queue changes with <paramref name="handler"/>.
        /// </summary>
        /// <param name="canTrigger">Delegate that dictates when to trigger <paramref name="handler"/>. First arg is the current queue size, second arg indicates if the queue increased or decreased (True on increase, false on decrease)</param>
        /// <param name="handler">Delegate that will be called when <paramref name="canTrigger"/> returns true</param>
        /// <param name="ensureTrigger">If <paramref name="handler"/> needs to be queued even if it is already running. When set to false and if it is already running <paramref name="handler"/> won't be called</param>
        /// <returns>An active subscription to the event. Disposing the object will stop <paramref name="handler"/> from receiving events</returns>
        public WorkerQueueEventSubscription OnQueueChanged(Func<int, bool, bool> canTrigger, Func<CancellationToken, Task> handler, bool ensureTrigger = false)
        {
            using var methodLogger = _logger.TraceMethod(this);
            if (IsDisposed.HasValue) throw new ObjectDisposedException(GetType().GetDisplayName(false));
            canTrigger.ValidateArgument(nameof(canTrigger));
            handler.ValidateArgument(nameof(handler));

            _logger.Log($"Adding new subscription to queue changes for worker queue <{Id}>");
            lock (_subscriptions)
            {
                var sequence = ++_eventSequence;
                var subscription = new EventHandlerSubscription()
                {
                    Sequence = sequence,
                    Name = $"WorkerQueue({Id}).Event.{sequence}",
                    CanTrigger = canTrigger,
                    EventHandler = handler,
                    EnsureTrigger = ensureTrigger
                };

                _subscriptions.Add(subscription);
                return new WorkerQueueEventSubscription(() =>
                {
                    lock (_subscriptions)
                    {
                        _subscriptions.Remove(subscription);
                    }
                });
            }
        }
        /// <summary>
        /// Triggers <paramref name="handler"/> when the queue becomes empty.
        /// </summary>
        /// <param name="handler">Delegate that will be called when queue count reaches 0</param>
        /// <param name="ensureTrigger">If <paramref name="handler"/> needs to be queued even if it is already running. When set to false and if it is already running <paramref name="handler"/> won't be called</param>
        /// <returns>An active subscription to the event. Disposing the object will stop <paramref name="handler"/> from receiving events</returns>
        public WorkerQueueEventSubscription OnEmptyQueue(Func<CancellationToken, Task> handler, bool ensureTrigger = true) => OnQueueChanged((s, i) => s == 0 && !i, handler, ensureTrigger);
        /// <summary>
        /// Triggers <paramref name="handler"/> when the queue count reaches <see cref="MaxSize"/>.
        /// </summary>
        /// <param name="handler">Delegate that will be called when queue count reaches <see cref="MaxSize"/></param>
        /// <param name="ensureTrigger">If <paramref name="handler"/> needs to be queued even if it is already running. When set to false and if it is already running <paramref name="handler"/> won't be called</param>
        /// <returns>An active subscription to the event. Disposing the object will stop <paramref name="handler"/> from receiving events</returns>
        public WorkerQueueEventSubscription OnFullQueue(Func<CancellationToken, Task> handler, bool ensureTrigger = true)
        {
            if (!MaxSize.HasValue) throw new InvalidOperationException($"Queue does not have a max size set so queue can never be full");
            return OnQueueChanged((s, i) => s == MaxSize.Value && i, handler, ensureTrigger);
        }

        /// <summary>
        /// Triggers <paramref name="handler"/> when queue count drops to <paramref name="size"/>.
        /// </summary>
        /// <param name="size">The queue count when to trigger <paramref name="handler"/></param>
        /// <param name="handler">Delegate that will be called when queue count reaches <see cref="MaxSize"/></param>
        /// <param name="ensureTrigger">If <paramref name="handler"/> needs to be queued even if it is already running. When set to false and if it is already running <paramref name="handler"/> won't be called</param>
        /// <returns>An active subscription to the event. Disposing the object will stop <paramref name="handler"/> from receiving events</returns>
        public WorkerQueueEventSubscription OnQueueBelow(uint size, Func<CancellationToken, Task> handler, bool ensureTrigger = false) => OnQueueChanged((s, i) => s == size && !i, handler, ensureTrigger);
        /// <summary>
        /// Triggers <paramref name="handler"/> when queue count increases to <paramref name="size"/>.
        /// </summary>
        /// <param name="size">The queue count when to trigger <paramref name="handler"/></param>
        /// <param name="handler">Delegate that will be called when queue count reaches <see cref="MaxSize"/></param>
        /// <param name="ensureTrigger">If <paramref name="handler"/> needs to be queued even if it is already running. When set to false and if it is already running <paramref name="handler"/> won't be called</param>
        /// <returns>An active subscription to the event. Disposing the object will stop <paramref name="handler"/> from receiving events</returns>
        public WorkerQueueEventSubscription OnQueueAbove(uint size, Func<CancellationToken, Task> handler, bool ensureTrigger = false) => OnQueueChanged((s, i) => s == size && i, handler, ensureTrigger);

        /// <summary>
        /// Subscribes to work added to the queue with <paramref name="itemHandler"/>.
        /// </summary>
        /// <param name="workerAmount">How many threads to subscribe with</param>
        /// <param name="itemHandler">Delegate used to handle dequeued items</param>
        /// <param name="cancellationToken">Optional token to cancel the scheduled tasks</param>
        /// <returns>An active subscription that can be disposed to stop receiving work with <paramref name="itemHandler"/></returns>
        public WorkerQueueSubscription<T> Subscribe(int workerAmount, Func<T, CancellationToken, Task> itemHandler, CancellationToken cancellationToken = default)
        {
            using var methodLogger = _logger.TraceMethod(this);
            if (IsDisposed.HasValue) throw new ObjectDisposedException(GetType().GetDisplayName(false));
            workerAmount.ValidateArgumentLargerOrEqual(nameof(workerAmount), 1);
            itemHandler.ValidateArgument(nameof(itemHandler));

            return new WorkerQueueSubscription<T>(workerAmount, this, itemHandler, cancellationToken);
        }

        private void TriggerEvents(bool queueIncreased, CancellationToken token = default)
        {
            using var methodLogger = _logger.TraceMethod(this);

            _logger.Debug($"Queue size {(queueIncreased ? "increased" : "decreased")} to {Count}. Triggering event handlers");
            EventHandlerSubscription[] subscriptions = null;
            lock (_subscriptions)
            {
                subscriptions = _subscriptions.ToArray();
            }

            foreach (var handler in subscriptions.Where(x => x.CanTrigger(Count, queueIncreased)))
            {
                _logger.Debug($"Triggering handler <{handler.Name}>");
                _ = TaskManager.ScheduleActionAsync(this, handler.Name, handler.EventHandler, x => x.ExecuteFirst(() => _logger.Debug($"Executing handler <{handler.Name}>"))
                                                                                                    .ExecuteAfter(() => _logger.Debug($"Executed handler <{handler.Name}>"))
                                                                                                    .WithPolicy(handler.EnsureTrigger ? NamedManagedTaskPolicy.WaitAndStart : NamedManagedTaskPolicy.TryStart)
                                                   , token);
                _logger.Debug($"Handler <{handler.Name}> has been scheduled");
            }
        }
        /// <summary>
        /// Gets an exclusive lock on the queue.
        /// </summary>
        /// <param name="token">Optional token to cancel the lock request</param>
        /// <returns>Object used to release the lock</returns>
        private Task<IAsyncDisposable> LockAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return _asyncLock.LockAsync(token);
        }
        /// <inheritdoc/>
        public IEnumerator<T> GetEnumerator() => _queue.GetEnumerator();
        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            if (IsDisposed.HasValue) return;
            using (new ExecutedAction(x => IsDisposed = x))
            {
                var exceptions = new List<Exception>();
                _logger.Log($"Disposing worker queue <{Id}>");

                // Cancel events
                try
                {
                    _logger.Debug($"Stopping tasks tied to worker queue <{Id}>");

                    await TaskManager.StopAllForAsync(this).ConfigureAwait(false);
                }
                catch (AggregateException aggrEx) when (aggrEx.InnerExceptions.All(x => x.IsAssignableTo<OperationCanceledException>()))
                {
                    _logger.Debug($"All tasks cancelled for worker queue <{Id}>");
                }
                catch (Exception ex)
                {
                    _logger.Log($"Something went wrong stopping tasks for worker queue <{Id}>", ex);
                    exceptions.Add(ex);
                }

                // Release items
                _logger.Debug($"Getting lock to release any remaining items");
                await using (await LockAsync().ConfigureAwait(false))
                {
                    _logger.Debug($"There are <{Count}> items remaining in the queue");

                    while (_queue.TryDequeue(out var item))
                    {
                        _logger.Debug($"Item <{item}> was still in the queue. Releasing");

                        try
                        {
                            if (OnDisposeHandler != null)
                            {
                                await OnDisposeHandler(item).ConfigureAwait(false);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.Log($"Something went wrong releasing item <{item}>", ex);
                            exceptions.Add(ex);
                        }
                    }
                }

                // Throw on issues
                if (exceptions.HasValue())
                {
                    var exceptionsToThrow = exceptions.SelectMany(x => x is AggregateException aggregate ? aggregate.InnerExceptions : x.AsEnumerable()).Where(x => !x.IsAssignableTo<OperationCanceledException>()).ToArray();
                    if (exceptionsToThrow.HasValue()) throw new AggregateException(exceptionsToThrow);
                }
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"WorkerQueue<{typeof(T).GetDisplayName(false)}>({Id}): Pending items: {(MaxSize.HasValue ? $"{Count}/{MaxSize}" : Count.ToString())} | Waiting callers: {_requests.Count}";
        }

        #region Request
        private class DequeueRequest : IDisposable
        {
            // Fields
            private readonly TaskCompletionSource<T> _taskSource = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
            private readonly CancellationTokenRegistration _tokenRegistration;

            public DequeueRequest(CancellationToken token)
            {
                _tokenRegistration = token.Register(Cancel);
                token.ThrowIfCancellationRequested();
            }

            // Properties
            /// <summary>
            /// Task that will complete when an item from the queue gets assigned to the request.
            /// </summary>
            public Task<T> Callback => _taskSource.Task;

            /// <summary>
            /// Assigns <paramref name="item"/> to the caller.
            /// </summary>
            /// <param name="item">The item assigned to the caller</param>
            public void Assign(T item)
            {
                lock (this)
                {
                    if (!_taskSource.Task.IsCompleted) _taskSource.SetResult(item.ValidateArgument(nameof(item)));
                }
            }
            /// <summary>
            /// Cancels the request and allows the caller to return.
            /// </summary>
            public void Cancel()
            {
                lock (this)
                {
                    if (!_taskSource.Task.IsCompleted) _taskSource.SetCanceled();
                }
            }
            /// <inheritdoc/>
            public void Dispose()
            {
                _tokenRegistration.Dispose();
            }
        }

        private class EventHandlerSubscription
        {
            /// <summary>
            /// Unique name for the event handler.
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// The unique sequence number for this event.
            /// </summary>
            public int Sequence { get; set; }
            /// <summary>
            /// Delegate that dictates whether or not an event needs to be raised.
            /// </summary>
            public Func<int, bool, bool> CanTrigger { get; set; }
            /// <summary>
            /// If the event handler needs to be triggered even if it is already running.
            /// </summary>
            public bool EnsureTrigger { get; set; }
            /// <summary>
            /// The delegate to trigger to handle the event.
            /// </summary>
            public Func<CancellationToken, Task> EventHandler { get; set; }
        }
        #endregion
    }
}
