using Sels.Core.Dispose;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Fluent;
using Sels.Core.Extensions.Threading;
using Sels.Core.Extensions.Logging;
using Sels.Core.Scope.Actions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Xml.Linq;
using Microsoft.Extensions.Options;
using Sels.Core.Extensions.Collections;
using Sels.Core.Extensions.Conversion;
using static Sels.Core.Delegates.Async;
using Sels.Core.Async.Contracts.TaskManagement;

namespace Sels.Core.Async.TaskManagement
{
    /// <inheritdoc cref="ITaskManager"/>
    public class TaskManager : ITaskManager, IAsyncExposedDisposable
    {
        // Fields
        private readonly object _anonymousLock = new object();
        private readonly object _managedLock = new object();
        private readonly HashSet<ManagedAnonymousTask> _anonymousTasks = new HashSet<ManagedAnonymousTask>();
        private readonly HashSet<ManagedTask> _managedTasks = new HashSet<ManagedTask>();
        private readonly Dictionary<string, ManagedTask> _nameIndex = new Dictionary<string, ManagedTask>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<object, List<ManagedTask>> _ownerIndex = new Dictionary<object, List<ManagedTask>>();
        private readonly ILogger? _logger;
        private readonly IOptionsMonitor<TaskManagerOptions> _optionsMonitor;

        // Properties
        /// <inheritdoc/>
        public bool? IsDisposed { get; private set; }

        /// <inheritdoc cref="TaskManager"/>
        /// <param name="optionsMonitor">Used to access the options for this instance</param>
        /// <param name="logger">Optional logger for tracing</param>
        public TaskManager(IOptionsMonitor<TaskManagerOptions> optionsMonitor, ILogger<TaskManager>? logger = null)
        {
            _optionsMonitor = optionsMonitor.ValidateArgument(nameof(optionsMonitor));
            _logger = logger;
        }

        /// <inheritdoc/>
        public IManagedAnonymousTask ScheduleAnonymous<TInput, TOutput>(TInput input, AsyncFunc<TInput, CancellationToken, TOutput> action, Action<IManagedAnonymousTaskCreationOptions<TInput, TOutput>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            _logger.Log($"Scheduling new anonymous task");

            var builder = new AnonymousTaskOptionsBuilder<TInput, TOutput>();
            options?.Invoke(builder);
            var taskOptions = builder.BuildOptions(this, action);

            return ScheduleAnonymous(input, taskOptions, token);
        }
        /// <inheritdoc/>
        public IManagedTask Schedule<TInput, TOutput>(object owner, TInput input, AsyncFunc<TInput, CancellationToken, TOutput> action, Action<IManagedTaskCreationOptions<TInput, TOutput>>? options = null, CancellationToken token = default)
        {
            owner.ValidateArgument(nameof(owner));
            action.ValidateArgument(nameof(action));

            _logger.Log($"Scheduling new unnamed managed task for <{owner}>");

            var builder = new ManagedTaskOptionsBuilder<TInput, TOutput>();
            options?.Invoke(builder);
            var taskOptions = builder.BuildOptions(this, action);

            return ScheduleUnnamed(owner, input, taskOptions, token);
        }
        /// <inheritdoc/>
        public IManagedTask TrySchedule<TInput, TOutput>(object owner, string? name, TInput input, AsyncFunc<TInput, CancellationToken, TOutput> action, Action<IManagedTaskCreationOptions<TInput, TOutput>>? options = null, CancellationToken token = default)
        {
            owner.ValidateArgument(nameof(owner));
            action.ValidateArgument(nameof(action));

            _logger.Log($"Trying to schedule new managed task for <{owner}>{(name.HasValue() ? $" with name <{name}>" : string.Empty)}");

            var builder = new ManagedTaskOptionsBuilder<TInput, TOutput>();
            options?.Invoke(builder);
            var taskOptions = builder.BuildOptions(this, action);

            return TryScheduleNamed(owner, name, input, taskOptions, token);
        }
        /// <inheritdoc/>
        public IPendingTask<IManagedTask> Schedule<TInput, TOutput>(object owner, string? name, TInput input, AsyncFunc<TInput, CancellationToken, TOutput> action, Action<INamedManagedTaskCreationOptions<TInput, TOutput>>? options = null, CancellationToken token = default)
        {
            owner.ValidateArgument(nameof(owner));
            action.ValidateArgument(nameof(action));

            _logger.Log($"Trying to schedule new managed task for <{owner}>{(name.HasValue() ? $" with name <{name}>" : string.Empty)} as pending task");

            var builder = new NamedManagedTaskOptionsBuilder<TInput, TOutput>();
            options?.Invoke(builder);
            var taskOptions = builder.BuildOptions(this, action);

            var pendingTask = new PendingTask<IManagedTask>();
            try
            {
                pendingTask.Callback = ScheduleNamed(owner, name, input, taskOptions, token);
            }
            catch(Exception ex)
            {
                pendingTask.Callback = Task.FromException<IManagedTask>(ex);
            }
            return pendingTask;
        }
        
        /// <inheritdoc/>
        public IManagedTask GetByName(string name, CancellationToken token = default)
        {
            name.ValidateArgument(nameof(name));

            _logger.Log($"Fetching managed task with name <{name}>");
            lock (_managedLock)
            {
                if (_nameIndex.TryGetValue(name, out var task) && !task.Task.IsCompleted)
                {
                    return task;
                }
                return null;
            }
        }
        /// <inheritdoc/>
        public IManagedTask[] GetOwnedBy(object instance, CancellationToken token = default)
        {
            instance.ValidateArgument(nameof(instance));

            _logger.Log($"Fetching all managed tasks owned by <{instance}>");
            lock (_managedLock)
            {
                if (_ownerIndex.TryGetValue(instance, out var tasks))
                {
                    return tasks.Where(x => !x.Task.IsCompleted).ToArray();
                }
                return Array.Empty<IManagedTask>();
            }
        }
        /// <inheritdoc/>
        public IManagedTask[] CancelAllFor(object instance, CancellationToken token = default)
        {
            instance.ValidateArgument(nameof(instance));

            // Get tasks to cancel
            _logger.Log($"Sending cancellation request to all managed tasks owned by <{instance}>");
            var tasksToCancel = GetOwnedBy(instance, token);
            _logger.Debug($"Got <{tasksToCancel.Length}> managed tasks to cancel for <{instance}>");

            // Trigger cancellation
            CancelTasks(tasksToCancel);

            return tasksToCancel;
        }
        /// <inheritdoc/>
        public async Task<IManagedTask[]> StopAllForAsync(object instance, CancellationToken token = default)
        {
            instance.ValidateArgument(nameof(instance));

            _logger.Log($"Stopping all tasks owned by <{instance}>");

            // Trigger cancellation
            var tasks = CancelAllFor(instance, token);

            // Wait for callback
            await Task.WhenAll(tasks.Select(x => x.Callback));
            return tasks;
        }
        /// <summary>
        /// Sends cancellation request to all tasks returned by <paramref name="tasks"/>.
        /// </summary>
        /// <param name="tasks">The task to cancel</param>
        protected virtual void CancelTasks(IEnumerable<IManagedAnonymousTask> tasks)
        {
            tasks.ValidateArgument(nameof(tasks));

            foreach (var task in tasks)
            {
                if (task.Options.HasFlag(ManagedTaskOptions.GracefulCancellation))
                {
                    _logger.Trace($"{task} has flag <{ManagedTaskOptions.GracefulCancellation}> set. Cancelling instantly");
                }
                else if (task.Task.CreationOptions.HasFlag(TaskCreationOptions.LongRunning))
                {
                    var cancelTime = _optionsMonitor.CurrentValue.LongRunningGracefulCancellationWaitTime;
                    _logger.Trace($"{task} has flag <{TaskCreationOptions.LongRunning}> set. Cancel will be triggered in <{cancelTime}>");
                    task.CancelAfter(cancelTime);
                }
                else
                {
                    var cancelTime = _optionsMonitor.CurrentValue.GracefulCancellationWaitTime;
                    _logger.Trace($"{task} will be cancelled in <{cancelTime}>");
                }
            }
        }

        /// <summary>
        /// Schedules a new anonymous task.
        /// </summary>
        /// <param name="input">Optional input for the work performed by the task</param>
        /// <param name="taskOptions">The options for this task</param>
        /// <param name="cancellationToken">Token that the caller can use to cancel the managed task</param>
        /// <returns>Task that completes when the task was scheduled</returns>
        protected virtual IManagedAnonymousTask ScheduleAnonymous(object input, ManagedAnonymousTaskCreationOptions taskOptions, CancellationToken cancellationToken)
        {
            taskOptions.ValidateArgument(nameof(taskOptions));

            _logger.Debug($"Scheduling new anonymous task");
            lock (_anonymousLock)
            {
                var task = new ManagedAnonymousTask(input, taskOptions, cancellationToken);

                // Use continuation to handle completion
                _ = task.Callback.ContinueWith(x => CompleteTask(task), TaskContinuationOptions.AttachedToParent);
                _anonymousTasks.Add(task);
                return task;
            }
        }
        /// <summary>
        /// Schedules a new unnamed managed task.
        /// </summary>
        /// <param name="owner">The instance the created task will be tied to</param>
        /// <param name="input">Optional input for the work performed by the task</param>
        /// <param name="taskOptions">The options for this task</param>
        /// <param name="cancellationToken">Token that the caller can use to cancel the managed task</param>
        /// <returns>Task that completes when the task was scheduled</returns>
        protected virtual ManagedTask ScheduleUnnamed(object owner, object input, ManagedTaskCreationOptions taskOptions, CancellationToken cancellationToken)
        {
            owner.ValidateArgument(nameof(owner));
            taskOptions.ValidateArgument(nameof(taskOptions));

            _logger.Debug($"Scheduling new unnamed managed task for <{owner}>");

            lock (_managedLock)
            {
                var task = new ManagedTask(owner, null, input, taskOptions, cancellationToken);
                // Use continuation to handle completion
                _ = task.Callback.ContinueWith(x => CompleteTask(task), TaskContinuationOptions.AttachedToParent);

                _managedTasks.Add(task);

                // Update index
                _ownerIndex.AddValueToList(owner, task);
                return task;
            }
        }
        /// <summary>
        /// Schedules a new named managed task.
        /// </summary>
        /// <param name="owner">The instance the created task will be tied to</param>
        /// <param name="name">The unique name for the task</param>
        /// <param name="input">Optional input for the work performed by the task</param>
        /// <param name="taskOptions">The options for this task</param>
        /// <param name="cancellationToken">Token that the caller can use to cancel the managed task</param>
        /// <returns>Task that completes when the task was scheduled</returns>
        protected virtual async Task<IManagedTask> ScheduleNamed(object owner, string name, object input, ManagedTaskCreationOptions taskOptions, CancellationToken cancellationToken)
        {
            owner.ValidateArgument(nameof(owner));
            taskOptions.ValidateArgument(nameof(taskOptions));

            _logger.Log($"Trying to schedule new managed task with name <{name}> for <{owner}>");

            var (wasScheduled, scheduledTask) = TryStartNamed(owner, name, input, taskOptions, cancellationToken);

            while (!wasScheduled)
            {
                _logger.Debug($"Managed task with name <{name}> is already running. Using policy <{taskOptions.NamePolicy}>");
                switch (taskOptions.NamePolicy)
                {
                    case NamedManagedTaskPolicy.TryStart:
                        _logger.Debug($"Managed task with name <{name}> is already running. Not starting a new one");
                        return scheduledTask;
                    case NamedManagedTaskPolicy.CancelAndStart:
                        _logger.Debug($"Already running managed task with name <{name}> will be cancelled instantly. After which we try to start");
                        scheduledTask.Cancel();
                        // Wait for cancellation
                        await scheduledTask.Callback;
                        break;
                    case NamedManagedTaskPolicy.GracefulCancelAndStart:
                        _logger.Debug($"Already running managed task with name <{name}> will be cancelled gracefully. After which we try to start");
                        CancelTasks(scheduledTask.AsEnumerable());
                        // Wait for cancellation
                        await scheduledTask.Callback;
                        break;
                    case NamedManagedTaskPolicy.WaitAndStart:
                        _logger.Debug($"Waiting until already running managed task with name <{name}> finishes executing. After which we try to start");
                        // Wait for task to finish
                        await scheduledTask.Callback;
                        break;
                    default:
                        throw new NotSupportedException($"Policy <{taskOptions.NamePolicy}> is not known");
                }

                (wasScheduled, scheduledTask) = TryStartNamed(owner, name, input, taskOptions, cancellationToken);
            }

            return scheduledTask;
        }
        /// <summary>
        /// Schedules a new named managed task.
        /// </summary>
        /// <param name="owner">The instance the created task will be tied to</param>
        /// <param name="name">The unique name for the task</param>
        /// <param name="input">Optional input for the work performed by the task</param>
        /// <param name="taskOptions">The options for this task</param>
        /// <param name="cancellationToken">Token that the caller can use to cancel the managed task</param>
        /// <returns>Task that completes when the task was scheduled</returns>
        protected virtual ManagedTask TryScheduleNamed(object owner, string name, object input, ManagedTaskCreationOptions taskOptions, CancellationToken cancellationToken)
        {
            owner.ValidateArgument(nameof(owner));
            taskOptions.ValidateArgument(nameof(taskOptions));

            _logger.Log($"Trying to schedule new managed task with name <{name}> for <{owner}>");

            var (wasScheduled, scheduledTask) = TryStartNamed(owner, name, input, taskOptions, cancellationToken);

            if (!wasScheduled)
            {
                _logger.Debug($"Managed task with name <{name}> is already running. Not starting a new one");
            }

            return scheduledTask;
        }
        /// <summary>
        /// Try to schedule a new named managed task.
        /// </summary>
        /// <param name="owner">The instance the created task will be tied to</param>
        /// <param name="name">The unique name for the task</param>
        /// <param name="input">Optional input for the work performed by the task</param>
        /// <param name="taskOptions">The options for this task</param>
        /// <param name="cancellationToken">Token that the caller can use to cancel the managed task</param>
        /// <returns>If the named task was scheduled or not</returns>
        protected virtual (bool WasScheduled, ManagedTask Scheduled) TryStartNamed(object owner, string name, object input, ManagedTaskCreationOptions taskOptions, CancellationToken cancellationToken)
        {
            owner.ValidateArgument(nameof(owner));
            taskOptions.ValidateArgument(nameof(taskOptions));

            if (!name.HasValue()) return (true, ScheduleUnnamed(owner, input, taskOptions, cancellationToken));

            _logger.Debug($"Trying to start managed task with name <{name}>");
            lock (_managedLock)
            {
                if (!_nameIndex.TryGetValue(name, out var existingTask))
                {
                    // Create new
                    var task = new ManagedTask(owner, name, input, taskOptions, cancellationToken);
                    // Use continuation to handle completion
                    _ = task.Callback.ContinueWith(x => CompleteTask(task), TaskContinuationOptions.AttachedToParent);

                    _managedTasks.Add(task);

                    // Update index
                    _ownerIndex.AddValueToList(owner, task);
                    _nameIndex.Add(name, task);
                    _logger.Debug($"Scheduled {task}");
                    return (true, task);
                }
                else
                {
                    _logger.Warning($"{existingTask} already running");
                    return (false, existingTask);
                }
            }
        }

        /// <summary>
        /// Finalizes the executed <paramref name="task"/>.
        /// </summary>
        /// <param name="task">The task to finalize</param>
        /// <returns>Task that will complete when <paramref name="task"/> is finalized</returns>
        protected virtual void CompleteTask(ManagedAnonymousTask task)
        {
            _logger.Debug($"Waiting for anonymous lock to finalize {task}");

            // Finalize task
            lock (_anonymousLock)
            {
                _logger.Debug($"Got anonymous lock to finalize {task}");

                // Remove current task
                _anonymousTasks.Remove(task);

                _logger.Debug($"Finalized {task}");
            }

            // Check if restart is needed
            if (!task.CancellationRequested)
            {
                if (task.Options.HasFlag(ManagedTaskOptions.KeepAlive) && task.Result is Exception exception && !(exception is OperationCanceledException))
                {
                    _logger.Log(LogLevel.Debug, exception, $"{task} has keep alive enabled and failed with an exception. Restarting task");
                    ScheduleAnonymous(task.Input, task.TaskOptions, task.CancellationToken);
                }
                else if (task.Options.HasFlag(ManagedTaskOptions.AutoRestart) && !(task.Result is Exception))
                {
                    _logger.Log(LogLevel.Debug, $"{task} has keep auto restart enabled and executed successfully. Restarting task");
                    ScheduleAnonymous(task.Input, task.TaskOptions, task.CancellationToken);
                }
            }
        }
        /// <summary>
        /// Finalizes the executed <paramref name="task"/>.
        /// </summary>
        /// <param name="task">The task to finalize</param>
        /// <returns>Task that will complete when <paramref name="task"/> is finalized</returns>
        protected virtual async Task CompleteTask(ManagedTask task)
        {
            _logger.Debug($"Waiting for managed lock to finalize {task}");

            // Finalize
            lock (_managedLock)
            {
                _logger.Debug($"Got managed lock to finalize {task}");

                // Remove current task
                _managedTasks.Remove(task);

                // Update indexes
                var ownerIndex = _ownerIndex[task.Owner];
                ownerIndex.Remove(task);
                if (!ownerIndex.HasValue()) _ownerIndex.Remove(task.Owner);
                if (task.Name.HasValue()) _nameIndex.Remove(task.Name);

                _logger.Debug($"Finalized {task}");
            }

            // Check if restart is needed
            if (!task.CancellationRequested)
            {
                if (task.Options.HasFlag(ManagedTaskOptions.KeepAlive) && task.Result is Exception exception)
                {
                    _logger.Log(LogLevel.Debug, exception, $"{task} has keep alive enabled and failed with an exception. Restarting task");

                    if (task.Name.HasValue()) await ScheduleNamed(task.Owner, task.Name, task.Input, task.TaskOptions, task.CancellationToken).ConfigureAwait(false);
                    else ScheduleUnnamed(task.Owner, task.Input, task.TaskOptions, task.CancellationToken);
                }
                else if (task.Options.HasFlag(ManagedTaskOptions.AutoRestart) && !(task.Result is Exception))
                {
                    _logger.Log(LogLevel.Debug, $"{task} has keep auto restart enabled and executed successfully. Restarting task");

                    if (task.Name.HasValue()) await ScheduleNamed(task.Owner, task.Name, task.Input, task.TaskOptions, task.CancellationToken).ConfigureAwait(false);
                    else ScheduleUnnamed(task.Owner, task.Input, task.TaskOptions, task.CancellationToken);
                }
            }
        }

        /// <inheritdoc/>
        public virtual async ValueTask DisposeAsync()
        {
            if (IsDisposed.HasValue) return;
            using (new ExecutedAction(x => IsDisposed = x))
            {
                _logger.Log($"Disposing task manager");

                while (_anonymousTasks.HasValue() || _managedTasks.HasValue())
                {
                    var pending = new HashSet<IManagedAnonymousTask>();
                    // Get tasks to cancel
                    lock (_anonymousLock)
                    {
                        _logger.Log($"Sending cancellation to <{_anonymousTasks.Count}> anonymous tasks");
                        pending.Intersect(_anonymousTasks.Where(x => !x.CancellationRequested));
                    }
                    lock (_managedLock)
                    {
                        _logger.Log($"Sending cancellation to <{_managedTasks.Count}> managed tasks");
                        pending.Intersect(_managedTasks.Where(x => !x.CancellationRequested));
                    }

                    // Trigger cancellation
                    CancelTasks(pending);

                    // Wait for cancellation
                    try
                    {
                        _logger.Log($"Waiting for <{pending.Count}> managed (anonymous) tasks to cancel");
                        await Task.WhenAll(pending.Where(x => x.CancellationRequested).Select(x => x.Callback));
                    }
                    catch (Exception ex)
                    {
                        _logger.Log($"Not all managed (anonymous) tasks cancelled gracefully", ex);
                    }
                }
            }
        }

        #region Builders
        private abstract class SharedOptionsBuilder<TInput, TOutput, TDerived> : IManagedTaskSharedCreationOptions<TInput, TOutput, TDerived>
        {
            // Properties
            protected abstract TDerived Self { get; }
            protected List<Delegates.Async.AsyncAction<TInput, CancellationToken>> PreExecutionActions { get; } = new List<Delegates.Async.AsyncAction<TInput, CancellationToken>>();
            protected List<Delegates.Async.AsyncAction<TInput, TOutput, CancellationToken>> PostExecutionActions { get; } = new List<Delegates.Async.AsyncAction<TInput, TOutput, CancellationToken>>();
            protected Dictionary<string, object> Properties { get; } = new Dictionary<string, object>();
            protected TaskCreationOptions TaskCreationOptions { get; private set; }
            protected ManagedTaskOptions ManagedTaskOptions { get; private set; }

            /// <inheritdoc/>
            public TDerived ExecuteAfter(Delegates.Async.AsyncAction<TInput, TOutput, CancellationToken> action)
            {
                action.ValidateArgument(nameof(action));
                PostExecutionActions.Add(action);
                return Self;
            }
            /// <inheritdoc/>
            public TDerived ExecuteFirst(Delegates.Async.AsyncAction<TInput, CancellationToken> action)
            {
                action.ValidateArgument(nameof(action));
                PreExecutionActions.Add(action);
                return Self;
            }
            /// <inheritdoc/>
            public TDerived SetProperties(Action<IDictionary<string, object>> action)
            {
                action.ValidateArgument(nameof(action));
                action(Properties);
                return Self;
            }
            /// <inheritdoc/>
            public TDerived WithCreationOptions(TaskCreationOptions options)
            {
                TaskCreationOptions = options;
                return Self;
            }
            /// <inheritdoc/>
            public TDerived WithManagedOptions(ManagedTaskOptions options)
            {
                ManagedTaskOptions = options;
                return Self;
            }

            /// <summary>
            /// Sets the configured options on <paramref name="options"/>.
            /// </summary>
            /// <param name="options">The instance to set the options on</param>
            /// <param name="action">The action that will be performed by the task</param>
            protected void SetOptions(ManagedTaskCreationSharedOptions options, AsyncFunc<TInput, CancellationToken, TOutput> action)
            {
                options.ValidateArgument(nameof(options));
                action.ValidateArgument(nameof(action));

                options.TaskCreationOptions = TaskCreationOptions;
                options.ManagedTaskOptions = ManagedTaskOptions;
                options.Properties = Properties;
                var preAction = PreExecutionActions;
                var postActions = PostExecutionActions;
                options.ExecuteDelegate = async (i, c) =>
                {
                    var input = i != null ? i.CastTo<TInput>() : default;
                    foreach(var action in PreExecutionActions)
                    {
                        await action(input, c).ConfigureAwait(false);
                    }

                    var output = await action(input, c).ConfigureAwait(false);
                    var castedOutput = output != null ? output.CastTo<TOutput>() : default;

                    foreach (var action in PostExecutionActions)
                    {
                        await action(input, castedOutput, c).ConfigureAwait(false);
                    }

                    return castedOutput;
                };
            }
        }

        private class AnonymousTaskOptionsBuilder<TInput, TOutput> : SharedOptionsBuilder<TInput, TOutput, IManagedAnonymousTaskCreationOptions<TInput, TOutput>>, IManagedAnonymousTaskCreationOptions<TInput, TOutput>
        {
            // Properties
            protected List<Delegates.Async.AsyncFunc<ITaskManager, IManagedAnonymousTask, TInput, object, CancellationToken, IManagedTask?>> ContinuationFactories { get; } = new List<Delegates.Async.AsyncFunc<ITaskManager, IManagedAnonymousTask, TInput, object, CancellationToken, IManagedTask?>>();
            protected List<Func<ITaskManager, IManagedAnonymousTask, TInput, object, CancellationToken, IManagedAnonymousTask?>> AnonymousContinuationFactories { get; } = new List<Func<ITaskManager, IManagedAnonymousTask, TInput, object, CancellationToken, IManagedAnonymousTask?>>();


            /// <inheritdoc/>
            protected override IManagedAnonymousTaskCreationOptions<TInput, TOutput> Self => this;

            /// <inheritdoc/>
            public IManagedAnonymousTaskCreationOptions<TInput, TOutput> ContinueWith(Func<ITaskManager, IManagedAnonymousTask, TInput, object, CancellationToken, IManagedTask?> continuationFactory)
            {
                continuationFactory.ValidateArgument(nameof(continuationFactory));

                return ContinueWith((m, t, i, o, c) => Task.FromResult(continuationFactory(m, t, i, o, c)));
            }
            /// <inheritdoc/>
            public IManagedAnonymousTaskCreationOptions<TInput, TOutput> ContinueWith(Delegates.Async.AsyncFunc<ITaskManager, IManagedAnonymousTask, TInput, object, CancellationToken, IManagedTask?> continuationFactory)
            {
                continuationFactory.ValidateArgument(nameof(continuationFactory));
                ContinuationFactories.Add(continuationFactory);
                return Self;
            }
            /// <inheritdoc/>
            public IManagedAnonymousTaskCreationOptions<TInput, TOutput> ContinueWith(Func<ITaskManager, IManagedAnonymousTask, TInput, object, CancellationToken, IManagedAnonymousTask?> continuationFactory)
            {
                continuationFactory.ValidateArgument(nameof(continuationFactory));
                AnonymousContinuationFactories.Add(continuationFactory);
                return Self;
            }

            /// <summary>
            /// Converts the current builder in an instance <see cref="ManagedAnonymousTaskCreationOptions"/>.
            /// </summary>
            /// <param name="taskManager">The task manager requesting the options</param>
            /// <param name="action">The action that will be performed by the task</param>
            /// <returns>An instance creating using the options configured using the current builder</returns>
            public ManagedAnonymousTaskCreationOptions BuildOptions(ITaskManager taskManager, AsyncFunc<TInput, CancellationToken, TOutput> action)
            {
                taskManager.ValidateArgument(nameof(taskManager));
                action.ValidateArgument(nameof(action));

                var options = new ManagedAnonymousTaskCreationOptions()
                {
                    ContinuationFactories = ContinuationFactories.Select(x => new AsyncFunc<IManagedAnonymousTask, object, object, CancellationToken, IManagedTask?>((t, i, o, c) => x(taskManager, t, i.CastToOrDefault<TInput>(), o, c))).ToArray(),
                    AnonymousContinuationFactories = AnonymousContinuationFactories.Select(x => new AsyncFunc<IManagedAnonymousTask, object, object, CancellationToken, IManagedAnonymousTask?>((t, i, o, c) => Task.FromResult(x(taskManager, t, i.CastToOrDefault<TInput>(), o, c)))).ToArray()
                };
                SetOptions(options, action);
                return options;
            }
        }

        private abstract class ManagedTaskOptionsBuilder<TInput, TOutput, TDerived> : SharedOptionsBuilder<TInput, TOutput, TDerived>, IManagedTaskCreationOptions<TInput, TOutput, TDerived>
        {
            // Properties
            protected List<Delegates.Async.AsyncFunc<ITaskManager, IManagedTask, TInput, object, CancellationToken, IManagedTask?>> ContinuationFactories { get; } = new List<Delegates.Async.AsyncFunc<ITaskManager, IManagedTask, TInput, object, CancellationToken, IManagedTask?>>();
            protected List<Func<ITaskManager, IManagedTask, TInput, object, CancellationToken, IManagedAnonymousTask?>> AnonymousContinuationFactories { get; } = new List<Func<ITaskManager, IManagedTask, TInput, object, CancellationToken, IManagedAnonymousTask?>>();

            /// <inheritdoc/>
            public TDerived ContinueWith(Func<ITaskManager, IManagedTask, TInput, object, CancellationToken, IManagedTask?> continuationFactory)
            {
                continuationFactory.ValidateArgument(nameof(continuationFactory));
                return ContinueWith((m, t, i, o, c) => Task.FromResult(continuationFactory(m, t, i, o, c)));
            }
            /// <inheritdoc/>
            public TDerived ContinueWith(AsyncFunc<ITaskManager, IManagedTask, TInput, object, CancellationToken, IManagedTask?> continuationFactory)
            {
                continuationFactory.ValidateArgument(nameof(continuationFactory));
                ContinuationFactories.Add(continuationFactory);
                return Self;
            }
            /// <inheritdoc/>
            public TDerived ContinueWith(Func<ITaskManager, IManagedTask, TInput, object, CancellationToken, IManagedAnonymousTask?> continuationFactory)
            {
                continuationFactory.ValidateArgument(nameof(continuationFactory));
                AnonymousContinuationFactories.Add(continuationFactory);
                return Self;
            }

            /// <summary>
            /// Sets the configured options on <paramref name="options"/>.
            /// </summary>
            /// <param name="options">The instance to set the options on</param>
            /// <param name="taskManager">The task manager requesting the options</param>
            /// <param name="action">The action that will be performed by the task</param>
            public ManagedTaskCreationOptions SetOptions(ManagedTaskCreationOptions options, ITaskManager taskManager, AsyncFunc<TInput, CancellationToken, TOutput> action)
            {
                options.ValidateArgument(nameof(options));
                taskManager.ValidateArgument(nameof(taskManager));
                action.ValidateArgument(nameof(action));

                options.ContinuationFactories = ContinuationFactories.Select(x => new AsyncFunc<IManagedTask, object, object, CancellationToken, IManagedTask?>((t, i, o, c) => x(taskManager, t, i.CastToOrDefault<TInput>(), o, c))).ToArray();
                options.AnonymousContinuationFactories = AnonymousContinuationFactories.Select(x => new AsyncFunc<IManagedTask, object, object, CancellationToken, IManagedAnonymousTask?>((t, i, o, c) => Task.FromResult(x(taskManager, t, i.CastToOrDefault<TInput>(), o, c)))).ToArray();

                SetOptions(options, action);
                return options;
            }
        }

        private class ManagedTaskOptionsBuilder<TInput, TOutput> : ManagedTaskOptionsBuilder<TInput, TOutput, IManagedTaskCreationOptions<TInput, TOutput>>, IManagedTaskCreationOptions<TInput, TOutput>
        {
            // Properties
            /// <inheritdoc/>
            protected override IManagedTaskCreationOptions<TInput, TOutput> Self => this;

            /// <summary>
            /// Converts the current builder in an instance <see cref="ManagedTaskCreationOptions"/>.
            /// </summary>
            /// <param name="taskManager">The task manager requesting the options</param>
            /// <param name="action">The action that will be performed by the task</param>
            /// <returns>An instance creating using the options configured using the current builder</returns>
            public ManagedTaskCreationOptions BuildOptions(ITaskManager taskManager, AsyncFunc<TInput, CancellationToken, TOutput> action)
            {
                taskManager.ValidateArgument(nameof(taskManager));
                action.ValidateArgument(nameof(action));

                var options = new ManagedTaskCreationOptions()
                {
                    NamePolicy = NamedManagedTaskPolicy.TryStart
                };
                SetOptions(options, taskManager, action);
                return options;
            }
        }

        private class NamedManagedTaskOptionsBuilder<TInput, TOutput> : ManagedTaskOptionsBuilder<TInput, TOutput, INamedManagedTaskCreationOptions<TInput, TOutput>>, INamedManagedTaskCreationOptions<TInput, TOutput>
        {
            // Properties
            /// <inheritdoc/>
            protected override INamedManagedTaskCreationOptions<TInput, TOutput> Self => this;
            /// <inheritdoc cref="ManagedTaskCreationOptions.NamePolicy"/>
            protected NamedManagedTaskPolicy NamePolicy { get; private set; }

            /// <inheritdoc/>
            public INamedManagedTaskCreationOptions<TInput, TOutput> WithPolicy(NamedManagedTaskPolicy policy)
            {
                NamePolicy = policy;
                return Self;
            }

            /// <summary>
            /// Converts the current builder in an instance <see cref="ManagedTaskCreationOptions"/>.
            /// </summary>
            /// <param name="taskManager">The task manager requesting the options</param>
            /// <param name="action">The action that will be performed by the task</param>
            /// <returns>An instance creating using the options configured using the current builder</returns>
            public ManagedTaskCreationOptions BuildOptions(ITaskManager taskManager, AsyncFunc<TInput, CancellationToken, TOutput> action)
            {
                taskManager.ValidateArgument(nameof(taskManager));
                action.ValidateArgument(nameof(action));

                var options = new ManagedTaskCreationOptions()
                {
                    NamePolicy = NamePolicy
                };
                SetOptions(options, taskManager, action);
                return options;
            }
        }
        #endregion

        #region Pendin task
        private class PendingTask<T> : IPendingTask<T>
        {
            /// <inheritdoc/>
            public DateTime Created { get; } = DateTime.Now;
            /// <inheritdoc/>
            public Task<T> Callback { get; set; }
        }
        #endregion
    }
}