using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using Sels.Core.Async.TaskManagement;
using Sels.Core.Async.Throtting;
using Sels.Core.Async.Throtting.Components.Throttler;

namespace Sels.Core.Async.Tests
{
    /// <summary>Creates controlled worker dependencies for unit tests.</summary>
    internal static class TestHelper
    {
        /// <summary>Creates a worker harness with a substitute manager and a gated local queue.</summary>
        public static WorkerHarness<TResult, TContext> CreateWorkerHarness<TResult, TContext>(TResult result, TContext context, ThrottledWorkerOptions? options = null, ILogger<ThrottledWorker<TResult, TContext>>? logger = null, CancellationToken cancellationToken = default)
            where TResult : class
            where TContext : class
        {
            return new WorkerHarness<TResult, TContext>(result, context, options ?? new ThrottledWorkerOptions { MaxQueuedTasks = 1 }, logger, cancellationToken);
        }
    }

    /// <summary>Owns a test worker and controls scheduling, execution, and finalization.</summary>
    internal sealed class WorkerHarness<TResult, TContext> : IAsyncDisposable
        where TResult : class
        where TContext : class
    {
        private Func<CancellationToken, Task>? _scheduledAction;
        private readonly TaskCompletionSource<bool> _finalized = new(TaskCreationOptions.RunContinuationsAsynchronously);

        /// <summary>The mutable result supplied to the worker.</summary>
        public TResult Result { get; }

        /// <summary>The context supplied to the worker.</summary>
        public TContext Context { get; }

        /// <summary>The substitute manager used by the worker.</summary>
        public ITaskManager TaskManager { get; }

        /// <summary>The substitute queue used by the worker.</summary>
        public IManagedTaskLocalQueue Queue { get; }

        /// <summary>The pending scheduled task returned by the queue.</summary>
        private PendingTaskStub PendingTask { get; }

        /// <summary>The managed task returned for accepted work.</summary>
        public IManagedAnonymousTask ManagedTask { get; }

        /// <summary>The worker under test.</summary>
        public ThrottledWorker<TResult, TContext> Worker { get; }

        /// <summary>Initializes all controlled dependencies and captures the work delegate.</summary>
        public WorkerHarness(TResult result, TContext context, ThrottledWorkerOptions options, ILogger<ThrottledWorker<TResult, TContext>>? logger, CancellationToken cancellationToken)
        {
            Result = result;
            Context = context;
            TaskManager = Substitute.For<ITaskManager>();
            Queue = Substitute.For<IManagedTaskLocalQueue>();
            ManagedTask = Substitute.For<IManagedAnonymousTask>();
            PendingTask = new PendingTaskStub(ManagedTask);

            ManagedTask.OnFinalized.Returns(_finalized.Task);
            TaskManager.CreateLocalQueue(Arg.Any<object>(), Arg.Any<int>()).Returns(Queue);
            TaskManager.ScheduleAnonymous<Task>(
                    Arg.Do<Func<CancellationToken, Task>>(action => _scheduledAction = action),
                    Arg.Any<Action<IManagedAnonymousTaskCreationOptions<Task>>>(),
                    Arg.Any<CancellationToken>())
                .Returns(ManagedTask);
            Queue.WaitOnQueuedAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
            Queue.EnqueueAsync(
                    Arg.Any<Func<ITaskManager, CancellationToken, IManagedAnonymousTask>>(),
                    Arg.Any<CancellationToken>())
                .Returns(call =>
                {
                    var scheduler = call.Arg<Func<ITaskManager, CancellationToken, IManagedAnonymousTask>>();
                    var accepted = scheduler(TaskManager, call.Arg<CancellationToken>());
                    PendingTask.Accept(accepted);
                    return Task.FromResult<IPendingTask<IManagedAnonymousTask>>(PendingTask);
                });

            Worker = new ThrottledWorker<TResult, TContext>(
                TaskManager,
                Result,
                Context,
                options,
                logger ?? NullLogger<ThrottledWorker<TResult, TContext>>.Instance,
                cancellationToken);
        }

        /// <summary>Starts the captured worker callback and returns its still-running task.</summary>
        public Task StartScheduledWork(CancellationToken cancellationToken = default)
        {
            var scheduledAction = _scheduledAction ?? throw new InvalidOperationException("No scheduled worker action was captured.");
            return scheduledAction(cancellationToken);
        }

        /// <summary>Completes the task-manager finalization signal.</summary>
        public void CompleteFinalization() => _finalized.TrySetResult(true);

        /// <inheritdoc/>
        public ValueTask DisposeAsync() => Worker.DisposeAsync();

        private sealed class PendingTaskStub : IPendingTask<IManagedAnonymousTask>
        {
            private readonly TaskCompletionSource<IManagedAnonymousTask> _callback = new(TaskCreationOptions.RunContinuationsAsynchronously);
            private bool _isCancelled;

            public DateTime Created { get; } = DateTime.UtcNow;

            public Task<IManagedAnonymousTask> Callback => _callback.Task;

            public bool IsCancelled => _isCancelled;

            public PendingTaskStub(IManagedAnonymousTask managedTask)
            {
                _callback.TrySetResult(managedTask);
            }

            public void Accept(IManagedAnonymousTask managedTask) => _callback.TrySetResult(managedTask);

            public void Cancel() => _isCancelled = true;

            public void Dispose() => Cancel();
        }
    }
}