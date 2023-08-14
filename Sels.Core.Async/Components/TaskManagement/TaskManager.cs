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
using Sels.Core.Async.TaskManagement;

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

        /// <summary>
        /// Proxy constructor.
        /// </summary>
        protected TaskManager()
        {

        }

        /// <inheritdoc/>
        public virtual IManagedAnonymousTask ScheduleAnonymous<TOutput>(Func<CancellationToken, Task<TOutput>> action, Action<IManagedAnonymousTaskCreationOptions<TOutput>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            _logger.Log($"Scheduling new anonymous task");

            var builder = new AnonymousTaskOptionsBuilder<TOutput>();
            options?.Invoke(builder);
            var taskOptions = builder.BuildOptions(this, action);

            return ScheduleAnonymous(taskOptions, token);
        }
        /// <inheritdoc/>
        public virtual IManagedTask Schedule<TOutput>(object owner, Func<CancellationToken, Task<TOutput>> action, Action<IManagedTaskCreationOptions<TOutput>>? options = null, CancellationToken token = default)
        {
            owner.ValidateArgument(nameof(owner));
            action.ValidateArgument(nameof(action));

            _logger.Log($"Scheduling new unnamed managed task for <{owner}>");

            var builder = new ManagedTaskOptionsBuilder<TOutput>();
            options?.Invoke(builder);
            var taskOptions = builder.BuildOptions(this, action);

            return ScheduleUnnamed(owner, taskOptions, token);
        }
        /// <inheritdoc/>
        public virtual IManagedTask TrySchedule<TOutput>(object owner, string? name, Func<CancellationToken, Task<TOutput>> action, Action<IManagedTaskCreationOptions<TOutput>>? options = null, CancellationToken token = default)
        {
            owner.ValidateArgument(nameof(owner));
            action.ValidateArgument(nameof(action));

            _logger.Log($"Trying to schedule new managed task for <{owner}>{(name.HasValue() ? $" with name <{name}>" : string.Empty)}");

            var builder = new ManagedTaskOptionsBuilder<TOutput>();
            options?.Invoke(builder);
            var taskOptions = builder.BuildOptions(this, action);

            return TryScheduleNamed(owner, name, taskOptions, token);
        }
        /// <inheritdoc/>
        public virtual Task<IManagedTask> ScheduleAsync<TOutput>(object owner, string? name, Func<CancellationToken, Task<TOutput>> action, Action<INamedManagedTaskCreationOptions<TOutput>>? options = null, CancellationToken token = default)
        {
            owner.ValidateArgument(nameof(owner));
            action.ValidateArgument(nameof(action));

            _logger.Log($"Trying to schedule new managed task for <{owner}>{(name.HasValue() ? $" with name <{name}>" : string.Empty)} as pending task");

            var builder = new NamedManagedTaskOptionsBuilder<TOutput>();
            options?.Invoke(builder);
            var taskOptions = builder.BuildOptions(this, action);

            try
            {
                return ScheduleNamed(owner, name, taskOptions, token);
            }
            catch (Exception ex)
            {
                return Task.FromException<IManagedTask>(ex);
            }
        }

        /// <inheritdoc/>
        public virtual IManagedTask GetByName(string name, CancellationToken token = default)
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
        public virtual IManagedTask[] GetOwnedBy(object instance, CancellationToken token = default)
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
        public virtual IManagedTask[] CancelAllFor(object instance, CancellationToken token = default)
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
        public virtual async Task<IManagedTask[]> StopAllForAsync(object instance, CancellationToken token = default)
        {
            instance.ValidateArgument(nameof(instance));

            _logger.Log($"Stopping all tasks owned by <{instance}>");

            // Trigger cancellation
            var tasks = CancelAllFor(instance, token);

            // Wait for callback
            await Task.WhenAll(tasks.Select(x => x.OnExecuted)).ConfigureAwait(false);
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
                    task.Cancel();
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
                    task.CancelAfter(cancelTime);
                }
            }
        }

        /// <summary>
        /// Schedules a new anonymous task.
        /// </summary>
        /// <param name="taskOptions">The options for this task</param>
        /// <param name="cancellationToken">Token that the caller can use to cancel the managed task</param>
        /// <returns>Task that completes when the task was scheduled</returns>
        protected virtual IManagedAnonymousTask ScheduleAnonymous(ManagedAnonymousTaskCreationOptions taskOptions, CancellationToken cancellationToken)
        {
            taskOptions.ValidateArgument(nameof(taskOptions));

            _logger.Debug($"Scheduling new anonymous task");
            lock (_anonymousLock)
            {
                var task = new ManagedAnonymousTask(taskOptions, cancellationToken);

                // Use continuation to handle completion
                _ = task.OnExecuted.ContinueWith(x => CompleteTask(task), TaskContinuationOptions.AttachedToParent);
                _anonymousTasks.Add(task);
                return task;
            }
        }
        /// <summary>
        /// Schedules a new unnamed managed task.
        /// </summary>
        /// <param name="owner">The instance the created task will be tied to</param>
        /// <param name="taskOptions">The options for this task</param>
        /// <param name="cancellationToken">Token that the caller can use to cancel the managed task</param>
        /// <returns>Task that completes when the task was scheduled</returns>
        protected virtual ManagedTask ScheduleUnnamed(object owner, ManagedTaskCreationOptions taskOptions, CancellationToken cancellationToken)
        {
            owner.ValidateArgument(nameof(owner));
            taskOptions.ValidateArgument(nameof(taskOptions));

            _logger.Debug($"Scheduling new unnamed managed task for <{owner}>");

            lock (_managedLock)
            {
                var task = new ManagedTask(owner, null, taskOptions, cancellationToken);
                // Use continuation to handle completion
                _ = task.OnExecuted.ContinueWith(x => CompleteTask(task), TaskContinuationOptions.AttachedToParent);

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
        /// <param name="taskOptions">The options for this task</param>
        /// <param name="cancellationToken">Token that the caller can use to cancel the managed task</param>
        /// <returns>Task that completes when the task was scheduled</returns>
        protected virtual async Task<IManagedTask> ScheduleNamed(object owner, string name, ManagedTaskCreationOptions taskOptions, CancellationToken cancellationToken)
        {
            owner.ValidateArgument(nameof(owner));
            taskOptions.ValidateArgument(nameof(taskOptions));

            _logger.Log($"Trying to schedule new managed task with name <{name}> for <{owner}>");

            var (wasScheduled, scheduledTask) = TryStartNamed(owner, name, taskOptions, cancellationToken);

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
                        await scheduledTask.OnFinalized.ConfigureAwait(false);
                        break;
                    case NamedManagedTaskPolicy.GracefulCancelAndStart:
                        _logger.Debug($"Already running managed task with name <{name}> will be cancelled gracefully. After which we try to start");
                        CancelTasks(scheduledTask.AsEnumerable());
                        // Wait for cancellation
                        await scheduledTask.OnFinalized.ConfigureAwait(false);
                        break;
                    case NamedManagedTaskPolicy.WaitAndStart:
                        _logger.Debug($"Waiting until already running managed task with name <{name}> finishes executing. After which we try to start");
                        // Wait for task to finish
                        await scheduledTask.OnFinalized.ConfigureAwait(false);
                        break;
                    case NamedManagedTaskPolicy.Exception:
                        _logger.Debug($"Managed task with name <{name}> is already running. Throwing exception");
                        throw new InvalidOperationException($"Managed task with name <{name}> is already running");
                    default:
                        throw new NotSupportedException($"Policy <{taskOptions.NamePolicy}> is not known");
                }

                (wasScheduled, scheduledTask) = TryStartNamed(owner, name, taskOptions, cancellationToken);
            }

            return scheduledTask;
        }
        /// <summary>
        /// Schedules a new named managed task.
        /// </summary>
        /// <param name="owner">The instance the created task will be tied to</param>
        /// <param name="name">The unique name for the task</param>
        /// <param name="taskOptions">The options for this task</param>
        /// <param name="cancellationToken">Token that the caller can use to cancel the managed task</param>
        /// <returns>Task that completes when the task was scheduled</returns>
        protected virtual ManagedTask TryScheduleNamed(object owner, string name, ManagedTaskCreationOptions taskOptions, CancellationToken cancellationToken)
        {
            owner.ValidateArgument(nameof(owner));
            taskOptions.ValidateArgument(nameof(taskOptions));

            _logger.Log($"Trying to schedule new managed task with name <{name}> for <{owner}>");

            var (wasScheduled, scheduledTask) = TryStartNamed(owner, name, taskOptions, cancellationToken);

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
        /// <param name="taskOptions">The options for this task</param>
        /// <param name="cancellationToken">Token that the caller can use to cancel the managed task</param>
        /// <returns>If the named task was scheduled or not</returns>
        protected virtual (bool WasScheduled, ManagedTask Scheduled) TryStartNamed(object owner, string name, ManagedTaskCreationOptions taskOptions, CancellationToken cancellationToken)
        {
            owner.ValidateArgument(nameof(owner));
            taskOptions.ValidateArgument(nameof(taskOptions));

            if (!name.HasValue()) return (true, ScheduleUnnamed(owner, taskOptions, cancellationToken));

            _logger.Debug($"Trying to start managed task with name <{name}>");
            lock (_managedLock)
            {
                if (!_nameIndex.TryGetValue(name, out var existingTask))
                {
                    // Create new
                    var task = new ManagedTask(owner, name, taskOptions, cancellationToken);
                    // Use continuation to handle completion
                    _ = task.OnExecuted.ContinueWith(x => CompleteTask(task), TaskContinuationOptions.AttachedToParent);

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

            try
            {
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
                        ScheduleAnonymous(task.TaskOptions, task.Token);
                    }
                    else if (task.Options.HasFlag(ManagedTaskOptions.AutoRestart) && !(task.Result is Exception))
                    {
                        _logger.Log(LogLevel.Debug, $"{task} has keep auto restart enabled and executed successfully. Restarting task");
                        ScheduleAnonymous(task.TaskOptions, task.Token);
                    }
                }
            }
            finally
            {
                _logger.Log($"{task} created at <{task.CreatedDate}> was picked up by the Thread Pool at <{task.StartedDate}> running for <{task.Duration}> stopping at <{task.FinishedDate}>");
                task.FinalizeTask();
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

            try
            {
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

                        if (task.Name.HasValue()) await ScheduleNamed(task.Owner, task.Name, task.TaskOptions, task.Token).ConfigureAwait(false);
                        else ScheduleUnnamed(task.Owner, task.TaskOptions, task.Token);
                    }
                    else if (task.Options.HasFlag(ManagedTaskOptions.AutoRestart) && !(task.Result is Exception))
                    {
                        _logger.Log(LogLevel.Debug, $"{task} has keep auto restart enabled and executed successfully. Restarting task");

                        if (task.Name.HasValue()) await ScheduleNamed(task.Owner, task.Name, task.TaskOptions, task.Token).ConfigureAwait(false);
                        else ScheduleUnnamed(task.Owner, task.TaskOptions, task.Token);
                    }
                }
            }
            finally
            {
                _logger.Log($"{task} created at <{task.CreatedDate}> was picked up by the Thread Pool at <{task.StartedDate}> running for <{task.Duration}> stopping at <{task.FinishedDate}>");
                task.FinalizeTask();
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
                        await Task.WhenAll(pending.Where(x => x.CancellationRequested).Select(x => x.OnExecuted)).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        _logger.Log($"Not all managed (anonymous) tasks cancelled gracefully", ex);
                    }
                }
            }
        }

        #region Builders
        private abstract class SharedOptionsBuilder<TOutput, TDerived> : IManagedTaskSharedCreationOptions<TOutput, TDerived>
        {
            // Properties
            protected abstract TDerived Self { get; }
            protected List<Func<CancellationToken, Task>> PreExecutionActions { get; } = new List<Func<CancellationToken, Task>>();
            protected List<Func<CancellationToken, TOutput, Task>> PostExecutionActions { get; } = new List<Func<CancellationToken, TOutput, Task>>();
            protected Dictionary<string, object> Properties { get; } = new Dictionary<string, object>();
            protected TaskCreationOptions TaskCreationOptions { get; private set; }
            protected ManagedTaskOptions ManagedTaskOptions { get; private set; }

            /// <inheritdoc/>
            public TDerived ExecuteAfter(Func<CancellationToken, TOutput, Task> action)
            {
                action.ValidateArgument(nameof(action));
                PostExecutionActions.Add(action);
                return Self;
            }
            /// <inheritdoc/>
            public TDerived ExecuteFirst(Func<CancellationToken, Task> action)
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
            protected void SetOptions(ManagedTaskCreationSharedOptions options, Func<CancellationToken, Task<TOutput>> action)
            {
                options.ValidateArgument(nameof(options));
                action.ValidateArgument(nameof(action));

                options.TaskCreationOptions = TaskCreationOptions;
                options.ManagedTaskOptions = ManagedTaskOptions;
                options.Properties = Properties;
                var preAction = PreExecutionActions;
                var postActions = PostExecutionActions;
                options.ExecuteDelegate = async t =>
                {
                    foreach (var action in PreExecutionActions)
                    {
                        await action(t).ConfigureAwait(false);
                    }

                    var output = await action(t).ConfigureAwait(false);
                    var castedOutput = output != null ? output.CastTo<TOutput>() : default;

                    foreach (var action in PostExecutionActions)
                    {
                        await action(t, castedOutput).ConfigureAwait(false);
                    }

                    return castedOutput;
                };
            }
        }

        private class AnonymousTaskOptionsBuilder<TOutput> : SharedOptionsBuilder<TOutput, IManagedAnonymousTaskCreationOptions<TOutput>>, IManagedAnonymousTaskCreationOptions<TOutput>
        {
            // Properties
            protected List<Func<ITaskManager, IManagedAnonymousTask, object, CancellationToken, Task<IManagedTask>?>> ContinuationFactories { get; } = new List<Func<ITaskManager, IManagedAnonymousTask, object, CancellationToken, Task<IManagedTask>?>>();
            protected List<Func<ITaskManager, IManagedAnonymousTask, object, CancellationToken, IManagedAnonymousTask?>> AnonymousContinuationFactories { get; } = new List<Func<ITaskManager, IManagedAnonymousTask, object, CancellationToken, IManagedAnonymousTask?>>();


            /// <inheritdoc/>
            protected override IManagedAnonymousTaskCreationOptions<TOutput> Self => this;

            /// <inheritdoc/>
            public IManagedAnonymousTaskCreationOptions<TOutput> ContinueWith(Func<ITaskManager, IManagedAnonymousTask, object, CancellationToken, IManagedTask?> continuationFactory)
            {
                continuationFactory.ValidateArgument(nameof(continuationFactory));

                return ContinueWith((m, t, o, c) => continuationFactory(m, t, o, c).ToTaskResult());
            }
            /// <inheritdoc/>
            public IManagedAnonymousTaskCreationOptions<TOutput> ContinueWith(Func<ITaskManager, IManagedAnonymousTask, object, CancellationToken, Task<IManagedTask>?> continuationFactory)
            {
                continuationFactory.ValidateArgument(nameof(continuationFactory));
                ContinuationFactories.Add(continuationFactory);
                return Self;
            }
            /// <inheritdoc/>
            public IManagedAnonymousTaskCreationOptions<TOutput> ContinueWith(Func<ITaskManager, IManagedAnonymousTask, object, CancellationToken, IManagedAnonymousTask?> continuationFactory)
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
            public ManagedAnonymousTaskCreationOptions BuildOptions(ITaskManager taskManager, Func<CancellationToken, Task<TOutput>> action)
            {
                taskManager.ValidateArgument(nameof(taskManager));
                action.ValidateArgument(nameof(action));

                var options = new ManagedAnonymousTaskCreationOptions()
                {
                    ContinuationFactories = ContinuationFactories.Select(x => new AsyncFunc<IManagedAnonymousTask, object, CancellationToken, IManagedTask?>((t, o, c) => x(taskManager, t, o, c))).ToArray(),
                    AnonymousContinuationFactories = AnonymousContinuationFactories.Select(x => new AsyncFunc<IManagedAnonymousTask, object, CancellationToken, IManagedAnonymousTask?>((t, o, c) => Task.FromResult(x(taskManager, t, o, c)))).ToArray()
                };
                SetOptions(options, action);
                return options;
            }
        }

        private abstract class ManagedTaskOptionsBuilder<TOutput, TDerived> : SharedOptionsBuilder<TOutput, TDerived>, IManagedTaskCreationOptions<TOutput, TDerived>
        {
            // Properties
            protected List<Func<ITaskManager, IManagedTask, object, CancellationToken, Task<IManagedTask>?>> ContinuationFactories { get; } = new List<Func<ITaskManager, IManagedTask, object, CancellationToken, Task<IManagedTask>?>>();
            protected List<Func<ITaskManager, IManagedTask, object, CancellationToken, IManagedAnonymousTask?>> AnonymousContinuationFactories { get; } = new List<Func<ITaskManager, IManagedTask, object, CancellationToken, IManagedAnonymousTask?>>();

            /// <inheritdoc/>
            public TDerived ContinueWith(Func<ITaskManager, IManagedTask, object, CancellationToken, IManagedTask?> continuationFactory)
            {
                continuationFactory.ValidateArgument(nameof(continuationFactory));
                return ContinueWith((m, t, o, c) => continuationFactory(m, t, o, c).ToTaskResult());
            }
            /// <inheritdoc/>
            public TDerived ContinueWith(Func<ITaskManager, IManagedTask, object, CancellationToken, Task<IManagedTask>?> continuationFactory)
            {
                continuationFactory.ValidateArgument(nameof(continuationFactory));
                ContinuationFactories.Add(continuationFactory);
                return Self;
            }
            /// <inheritdoc/>
            public TDerived ContinueWith(Func<ITaskManager, IManagedTask, object, CancellationToken, IManagedAnonymousTask?> continuationFactory)
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
            public ManagedTaskCreationOptions SetOptions(ManagedTaskCreationOptions options, ITaskManager taskManager, Func<CancellationToken, Task<TOutput>> action)
            {
                options.ValidateArgument(nameof(options));
                taskManager.ValidateArgument(nameof(taskManager));
                action.ValidateArgument(nameof(action));

                options.ContinuationFactories = ContinuationFactories.Select(x => new AsyncFunc<IManagedTask, object, CancellationToken, IManagedTask?>((t, o, c) => x(taskManager, t, o, c))).ToArray();
                options.AnonymousContinuationFactories = AnonymousContinuationFactories.Select(x => new AsyncFunc<IManagedTask, object, CancellationToken, IManagedAnonymousTask?>((t, o, c) => Task.FromResult(x(taskManager, t, o, c)))).ToArray();

                SetOptions(options, action);
                return options;
            }
        }

        private class ManagedTaskOptionsBuilder<TOutput> : ManagedTaskOptionsBuilder<TOutput, IManagedTaskCreationOptions<TOutput>>, IManagedTaskCreationOptions<TOutput>
        {
            // Properties
            /// <inheritdoc/>
            protected override IManagedTaskCreationOptions<TOutput> Self => this;

            /// <summary>
            /// Converts the current builder in an instance <see cref="ManagedTaskCreationOptions"/>.
            /// </summary>
            /// <param name="taskManager">The task manager requesting the options</param>
            /// <param name="action">The action that will be performed by the task</param>
            /// <returns>An instance creating using the options configured using the current builder</returns>
            public ManagedTaskCreationOptions BuildOptions(ITaskManager taskManager, Func<CancellationToken, Task<TOutput>> action)
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

        private class NamedManagedTaskOptionsBuilder<TOutput> : ManagedTaskOptionsBuilder<TOutput, INamedManagedTaskCreationOptions<TOutput>>, INamedManagedTaskCreationOptions<TOutput>
        {
            // Properties
            /// <inheritdoc/>
            protected override INamedManagedTaskCreationOptions<TOutput> Self => this;
            /// <inheritdoc cref="ManagedTaskCreationOptions.NamePolicy"/>
            protected NamedManagedTaskPolicy NamePolicy { get; private set; }

            /// <inheritdoc/>
            public INamedManagedTaskCreationOptions<TOutput> WithPolicy(NamedManagedTaskPolicy policy)
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
            public ManagedTaskCreationOptions BuildOptions(ITaskManager taskManager, Func<CancellationToken, Task<TOutput>> action)
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