using Microsoft.Extensions.Logging;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Sels.Core.Extensions.Threading;
using Sels.Core.Async.TaskManagement;
using Sels.Core.Extensions.Exceptions;

namespace Sels.Core.Async.Throtting.Components.Throttler
{
    /// <inheritdoc cref="IThrottledWorker{TResult, TContext}"/>
    public class ThrottledWorker<TResult, TContext> : IThrottledWorker<TResult, TContext> where TResult : class where TContext : class
    {
        // Fields
        private readonly TResult _result;
        private readonly TContext _context;
        private readonly ThrottledWorkerOptions _options;
        private readonly SemaphoreSlim _queueLock;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly ILogger? _logger;
        private readonly ITaskManager _taskManager;
        private readonly IManagedTaskLocalQueue _taskQueue;
        private readonly bool _isSub = false;

        // State
        private Exception _failedTask;
        private List<IAsyncDisposable>? _childeren;

        /// <inheritdoc cref="IThrottledWorker{TResult, TContext}"/>
        /// <param name="taskManager">The task manager that will be used to manage the tasks for the worker.</param>
        /// <param name="result">The result object that will be used by the worker.</param>
        /// <param name="context">The context object that will be used by the worker.</param>
        /// <param name="options">The options for configuring the throttled worker.</param>
        /// <param name="cancellationToken">The cancellation token to observe while waiting for tasks to complete.</param>
        /// <param name="logger">The logger that will be used to log information about the worker.</param>
        public ThrottledWorker(ITaskManager taskManager, TResult result, TContext context, ThrottledWorkerOptions options, ILogger<ThrottledWorker<TResult, TContext>> logger, CancellationToken cancellationToken)
            : this(taskManager, result, context, options, cancellationToken, logger)
        {
        }

        /// <summary>Creates a worker with optional logging.</summary>
        /// <param name="taskManager">The task manager used to manage work.</param>
        /// <param name="result">The result object updated by work.</param>
        /// <param name="context">The context passed to work delegates.</param>
        /// <param name="options">The throttling options.</param>
        /// <param name="cancellationToken">The token that cancels owned work.</param>
        /// <param name="logger">An optional worker logger.</param>
        public ThrottledWorker(ITaskManager taskManager, TResult result, TContext context, ThrottledWorkerOptions options, CancellationToken cancellationToken = default, ILogger<ThrottledWorker<TResult, TContext>>? logger = null)
        {
            _taskManager = taskManager.ValidateArgument(nameof(taskManager));
            _result = result.ValidateArgument(nameof(result));
            _context = context.ValidateArgument(nameof(context));
            _options = options.ValidateArgument(nameof(options));
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _queueLock = new SemaphoreSlim(_options.MaxQueuedTasks, _options.MaxQueuedTasks);
            _logger = logger;
            _taskQueue = _taskManager.CreateLocalQueue(this, _options.MaxConcurrentTasks);
        }

        private ThrottledWorker(IManagedTaskLocalQueue queue, SemaphoreSlim queueLock, ITaskManager taskManager, TResult result, TContext context, ThrottledWorkerOptions options, ILogger? logger, CancellationToken cancellationToken)
        {
            _result = result.ValidateArgument(nameof(result));
            _context = context.ValidateArgument(nameof(context));
            _options = options.ValidateArgument(nameof(options));
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _queueLock = queueLock.ValidateArgument(nameof(queueLock));
            _logger = logger;
            _taskQueue = queue.ValidateArgument(nameof(queue));
            _isSub = true;
        }

        /// <inheritdoc/>
        public async Task EnqueueWorkAsync(Func<TResult, TContext, CancellationToken, Task> work, bool wait = false, CancellationToken cancellationToken = default)
        {
            work = work.ValidateArgument(nameof(work));
            if (_options.StopOnError && _failedTask != null)
            {
                _failedTask.Rethrow();
            }

            _logger.Trace($"Enqueuing work for <{{{AsyncLog.Throttling.ResultType}}}> with context <{{{AsyncLog.Throttling.ContextType}}}>", typeof(TResult).Name, typeof(TContext).Name);
            var ca = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cancellationTokenSource.Token);
            var queueLock = await _queueLock.LockAsync(ca.Token).ConfigureAwait(false);

            var pendingTask = await _taskQueue.EnqueueAsync((m, ct) => m.ScheduleAnonymous(c =>
            {
                var newWork = new Func<TResult, TContext, CancellationToken, Task>(async (r, c, ct) =>
                {
                    await using (queueLock)
                    {
                        await work(r, c, ct).ConfigureAwait(false);
                    }
                });
                return newWork(_result, _context, ct);
            }, x => x.OnFail((m, t, e, c) =>
            {
                _failedTask = e;
                _logger.LogException(LogLevel.Error, $"Work failed for <{{{AsyncLog.Throttling.ResultType}}}> with context <{{{AsyncLog.Throttling.ContextType}}}>", e, typeof(TResult).Name, typeof(TContext).Name);
                return null;
            })), ca.Token).ConfigureAwait(false);

            _logger.Trace($"Work enqueued for <{{{AsyncLog.Throttling.ResultType}}}> with context <{{{AsyncLog.Throttling.ContextType}}}>", typeof(TResult).Name, typeof(TContext).Name);

            if (wait)
            {
                await pendingTask.WaitOnFinalization(ca.Token).ConfigureAwait(false);
                var task = await (pendingTask.Callback);
                await task.OnFinalized;
            }
        }

        /// <inheritdoc/>
        public async Task<TResult> WaitForWorkAsync(CancellationToken cancellationToken = default)
        {
            _logger.Trace($"Waiting for <{{{AsyncLog.Throttling.PendingWork}}}> work to complete for <{{{AsyncLog.Throttling.ResultType}}}> with context <{{{AsyncLog.Throttling.ContextType}}}>", _options.MaxQueuedTasks - _queueLock.CurrentCount + _taskQueue.Pending, typeof(TResult).Name, typeof(TContext).Name);
            while (_queueLock.CurrentCount < _options.MaxQueuedTasks)
            {
                if (_options.StopOnError && _failedTask != null)
                {
                    _failedTask.Rethrow();
                }
                await _taskQueue.WaitOnQueuedAsync(cancellationToken).ConfigureAwait(false);
            }

            if (_options.StopOnError && _failedTask != null)
            {
                _failedTask.Rethrow();
            }
            return _result;
        }

        /// <inheritdoc/>
        public IThrottledWorker<TNewResult, TContext> CreateSubWorker<TNewResult>(TNewResult newResult) where TNewResult : class
        {
            newResult = newResult.ValidateArgument(nameof(newResult));
            var subWorker = new ThrottledWorker<TNewResult, TContext>(_taskQueue, _queueLock, _taskManager, newResult, _context, _options, _logger, _cancellationTokenSource.Token);
            _childeren ??= new List<IAsyncDisposable>();
            _childeren.Add(subWorker);
            return subWorker;
        }
        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            _cancellationTokenSource.Cancel();

            if (_childeren != null)
            {
                foreach (var child in _childeren)
                {
                    try
                    {
                        await child.DisposeAsync().ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        _logger.LogException(LogLevel.Error, $"Failed to dispose child worker for <{{{AsyncLog.Throttling.ResultType}}}> with context <{{{AsyncLog.Throttling.ContextType}}}>", e, typeof(TResult).Name, typeof(TContext).Name);
                    }
                }
            }

            if (!_isSub) _taskQueue.Dispose();
        }
    }
}
