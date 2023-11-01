using Microsoft.Extensions.Logging;
using Sels.Core.Async.Queue;
using Sels.Core.Dispose;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Logging;
using Sels.Core.Extensions.Reflection;
using Sels.Core.Extensions.Threading;
using Sels.Core.Scope.Actions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sels.Core.Async.TaskManagement.Queue
{
    /// <inheritdoc cref="IManagedTaskQueue"/>
    public abstract class ManagedTaskQueue : IManagedTaskQueue, IAsyncExposedDisposable, IDisposable
    {
        // Fields
        private readonly object _lock = new object();
        private readonly Action _releaseAction;

        private readonly WorkerQueue<PendingTask> _queue;
        /// <summary>
        /// The task manager that created the current queue
        /// </summary>
        protected readonly ITaskManager _taskManager;
        /// <summary>
        /// Optional logger for tracing.
        /// </summary>
        protected readonly ILogger? _logger;

        // State
        /// <summary>
        /// How many references there are to the queue.
        /// </summary>
        public int CurrentReferences { get; private set; }

        // Properties
        /// <inheritdoc/>
        public int Concurrency { get; }
        /// <inheritdoc/>
        public int Pending => _queue.Count;
        /// <inheritdoc/>
        public bool? IsDisposed { get; private set; }
        /// <summary>
        /// How long to wait pending work in a queue to be processed before cancelling the pending items.
        /// </summary>
        public TimeSpan GracefulStopTime { get; set; }

        /// <inheritdoc cref="ManagedTaskQueue"/>
        /// <param name="releaseAction">Delegate to call when the current queue can be cleaned up</param>
        /// <param name="taskManager"><inheritdoc cref="_taskManager"/></param>
        /// <param name="maxConcurrency"><inheritdoc cref="Concurrency"/></param>
        /// <param name="logger"><inheritdoc cref="_logger"/></param>
        public ManagedTaskQueue(Action releaseAction, ITaskManager taskManager, int maxConcurrency, ILogger? logger = null)
        {
            _releaseAction = releaseAction.ValidateArgument(nameof(releaseAction));
            _taskManager = taskManager.ValidateArgument(nameof(taskManager));
            Concurrency = maxConcurrency.ValidateArgumentLargerOrEqual(nameof(maxConcurrency), 1);
            _logger = logger;

            _queue = new WorkerQueue<PendingTask>(taskManager, logger)
            {
                OnDisposeHandler = x =>
                {
                    x.Cancel();
                    return Task.CompletedTask;
                }
            };

            _queue.OnEmptyQueue(x => {
                _logger.Debug($"Queue is empty. Trying to release");
                TryRelease();
                return Task.CompletedTask;
            }, true);

            _ = _queue.Subscribe(Concurrency, EnqueuePending);
        }

        /// <inheritdoc/>
        public async Task<IPendingTask<IManagedTask>> EnqueueAsync(Func<ITaskManager, CancellationToken, Task<IManagedTask>> schedulerAction, CancellationToken token = default)
        {
            using var methodLogger = _logger.TraceMethod(this);
            if (IsDisposed.HasValue) throw new ObjectDisposedException(GetType().GetDisplayName(false));
            schedulerAction.ValidateArgument(nameof(schedulerAction));

            var pendingTask = new PendingManagedTask(schedulerAction, token);
            await _queue.EnqueueAsync(pendingTask, token);
            return pendingTask;
        }
        /// <inheritdoc/>
        public Task<IPendingTask<IManagedTask>> EnqueueAsync(Func<ITaskManager, CancellationToken, IManagedTask> schedulerAction, CancellationToken token = default)
        {
            using var methodLogger = _logger.TraceMethod(this);
            if (IsDisposed.HasValue) throw new ObjectDisposedException(GetType().GetDisplayName(false));
            schedulerAction.ValidateArgument(nameof(schedulerAction));

            return EnqueueAsync((t, c) => schedulerAction(t, c).ToTaskResult(), token);
        }
        /// <inheritdoc/>
        public async Task<IPendingTask<IManagedAnonymousTask>> EnqueueAsync(Func<ITaskManager, CancellationToken, IManagedAnonymousTask> schedulerAction, CancellationToken token = default)
        {
            using var methodLogger = _logger.TraceMethod(this);
            if (IsDisposed.HasValue) throw new ObjectDisposedException(GetType().GetDisplayName(false));
            schedulerAction.ValidateArgument(nameof(schedulerAction));

            var pendingTask = new PendingAnonymousTask(schedulerAction, token);
            await _queue.EnqueueAsync(pendingTask, token);
            return pendingTask;
        }

        private async Task EnqueuePending(PendingTask pending, CancellationToken token)
        {
            using var pendingScope = pending;
            try
            {
                if (pending.IsCancelled) return;

                var cancellationSource = new CancellationTokenSource();
                using var workerRegistration = token.Register(cancellationSource.Cancel);
                using var pendingRegistration = pending.CancellationToken.Register(cancellationSource.Cancel);

                _logger.Debug($"Enqueueing next pending task");
                var task = await pending.ScheduleTask(_taskManager, cancellationSource.Token).ConfigureAwait(false);
                _logger.Log($"Scheduled <{task}>. Waiting for completion");
                await Helper.Async.WaitOn(task.OnFinalized, cancellationSource.Token).ConfigureAwait(false);
                _logger.Log($"Task <{task}> finished executing. Checking queue");
            }
            catch (OperationCanceledException)
            {
                if(pending.IsCancelled) return;
                pending.Cancel();
            }
            catch(Exception ex)
            {
                pending.Error(ex);
            }
        }

        /// <summary>
        /// Called when the queue is referenced.
        /// </summary>
        public void TrackReference()
        {
            _logger.Log($"Adding reference to queue");
            lock (_lock)
            {
                CurrentReferences++;
            }
        }

        private void TryRelease()
        {
            lock (_lock)
            {
                if (CurrentReferences > 0)
                {
                    _logger.Debug($"Queue is still referenced. Can't release");
                }
                else if (Pending > 0)
                {
                    _logger.Debug($"Queue still has pending work. Can't release");
                }
                else
                {
                    _logger.Log($"Releasing queue");
                    _releaseAction();
                }
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _logger.Log($"Removing reference to queue. Trying to release");
            lock (_lock)
            {
                CurrentReferences--;
                TryRelease();
            }
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync() {
            if (IsDisposed.HasValue) return;
            using (new ExecutedAction(x => IsDisposed = x))
            {
                _logger.Log($"Waiting up to <{GracefulStopTime}> for pending work to flush from the queue");

                var cancellationSource = new CancellationTokenSource();
                using(_queue.OnEmptyQueue(t => { cancellationSource.Cancel(); return Task.CompletedTask; }, true))
                {
                    if(_queue.Count > 0)
                    {
                        await Helper.Async.Sleep(GracefulStopTime, cancellationSource.Token);
                    }
                }

                _logger.Log($"Disposing task queue and cancelling any remaining pending tasks");
                await _queue.DisposeAsync().ConfigureAwait(false);
            }
        }

        private class PendingManagedTask : PendingTask, IPendingTask<IManagedTask>
        {
            // Fields
            private readonly TaskCompletionSource<IManagedTask> _taskSource = new TaskCompletionSource<IManagedTask>(TaskCreationOptions.RunContinuationsAsynchronously);
            private readonly Func<ITaskManager, CancellationToken, Task<IManagedTask>> _scheduleAction;
            private readonly CancellationToken _token;
            private readonly CancellationTokenRegistration _registration;

            // Properties
            /// <inheritdoc/>
            public DateTime Created { get; } = DateTime.Now;
            /// <inheritdoc/>
            public Task<IManagedTask> Callback => _taskSource.Task;
            /// <inheritdoc/>
            public override bool IsCancelled => _taskSource.Task.IsCanceled;
            /// <inheritdoc/>
            public override CancellationToken CancellationToken => _token;

            public PendingManagedTask(Func<ITaskManager, CancellationToken, Task<IManagedTask>> scheduleAction, CancellationToken token)
            {
                _scheduleAction = scheduleAction.ValidateArgument(nameof(scheduleAction));

                _token = token;
                _registration = token.Register(Cancel);
            }
            /// <inheritdoc/>
            public override async Task<IManagedAnonymousTask> ScheduleTask(ITaskManager taskManager, CancellationToken cancellationToken)
            {
                taskManager.ValidateArgument(nameof(taskManager));

                try
                {
                    var scheduledTask = await _scheduleAction(taskManager, cancellationToken);
                    _taskSource.SetResult(scheduledTask);
                    return scheduledTask;
                }
                catch(Exception ex)
                {
                    _taskSource.SetException(ex);
                    throw;
                }
            }
            /// <inheritdoc/>
            public override void Cancel()
            {
                lock (_taskSource)
                {
                    if (!_taskSource.Task.IsCompleted && _taskSource.Task.Status != TaskStatus.Running) _taskSource.SetCanceled();
                }
            }
            /// <inheritdoc/>
            public override void Error(Exception exception)
            {
                lock (_taskSource)
                {
                    if (!_taskSource.Task.IsCompleted && _taskSource.Task.Status != TaskStatus.Running) _taskSource.SetException(exception);
                }
            }

            /// <inheritdoc/>
            public override void Dispose() => _registration.Dispose();
        }

        private class PendingAnonymousTask : PendingTask, IPendingTask<IManagedAnonymousTask>
        {
            // Fields
            private readonly TaskCompletionSource<IManagedAnonymousTask> _taskSource = new TaskCompletionSource<IManagedAnonymousTask>(TaskCreationOptions.RunContinuationsAsynchronously);
            private readonly Func<ITaskManager, CancellationToken, IManagedAnonymousTask> _scheduleAction;
            private readonly CancellationToken _token;
            private readonly CancellationTokenRegistration _registration;

            // Properties
            /// <inheritdoc/>
            public DateTime Created { get; } = DateTime.Now;
            /// <inheritdoc/>
            public Task<IManagedAnonymousTask> Callback => _taskSource.Task;
            /// <inheritdoc/>
            public override bool IsCancelled => _taskSource.Task.IsCanceled;
            /// <inheritdoc/>
            public override CancellationToken CancellationToken => _token;

            public PendingAnonymousTask(Func<ITaskManager, CancellationToken, IManagedAnonymousTask> scheduleAction, CancellationToken token)
            {
                _scheduleAction = scheduleAction.ValidateArgument(nameof(scheduleAction));
                _token = token;
                _registration = token.Register(Cancel);
            }

            /// <inheritdoc/>
            public override Task<IManagedAnonymousTask> ScheduleTask(ITaskManager taskManager, CancellationToken cancellationToken)
            {
                taskManager.ValidateArgument(nameof(taskManager));

                try
                {
                    var scheduledTask = _scheduleAction(taskManager, cancellationToken);
                    _taskSource.SetResult(scheduledTask);
                    return scheduledTask.ToTaskResult();
                }
                catch (Exception ex)
                {
                    _taskSource.SetException(ex);
                    throw;
                }
            }
            /// <inheritdoc/>
            public override void Dispose() => _registration.Dispose();

            /// <inheritdoc/>
            public override void Cancel() {
                lock (_taskSource)
                {
                    if (!_taskSource.Task.IsCompleted && _taskSource.Task.Status != TaskStatus.Running) _taskSource.SetCanceled();
                }
            }
            /// <inheritdoc/>
            public override void Error(Exception exception)
            {
                lock (_taskSource)
                {
                    if (!_taskSource.Task.IsCompleted && _taskSource.Task.Status != TaskStatus.Running) _taskSource.SetException(exception);
                }
            }
        }

        private abstract class PendingTask : IDisposable
        {
            public abstract Task<IManagedAnonymousTask> ScheduleTask(ITaskManager taskManager, CancellationToken cancellationToken);
            public abstract void Cancel();
            public abstract void Error(Exception exception);
            public abstract bool IsCancelled { get; }
            public abstract CancellationToken CancellationToken { get; }

            public abstract void Dispose();
        }
    }
}
