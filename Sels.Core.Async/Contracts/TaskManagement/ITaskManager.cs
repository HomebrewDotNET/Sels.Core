﻿using Sels.Core.Async.TaskManagement;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Threading;
using Sels.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Sels.Core.Delegates.Async;

namespace Sels.Core.Async.TaskManagement
{
    /// <summary>
    /// Services that manages <see cref="Task"/>(s) created by other services and schedules them on the thread pool.
    /// Makes sure tasks are properly stopped to ensure graceful shutdown.
    /// </summary>
    public interface ITaskManager
    {
        /// <summary>
        /// Schedules a new managed task tied to <paramref name="owner"/>.
        /// </summary>
        /// <typeparam name="TOutput">The output returned by the task</typeparam>
        /// <param name="owner">The instance to tie the task to</param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        IManagedTask Schedule<TOutput>(object owner, Func<CancellationToken, Task<TOutput>> action, Action<IManagedTaskCreationOptions<TOutput>>? options = null, CancellationToken token = default);
        /// <summary>
        /// Logs a request to schedule a new named managed task tied to <paramref name="owner"/>.
        /// </summary>
        /// <typeparam name="TOutput">The output returned by the task</typeparam>
        /// <param name="owner">The instance to tie the task to</param>
        /// <param name="name">Optional unique name for the task</param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        IManagedTask TrySchedule<TOutput>(object owner, string? name, Func<CancellationToken, Task<TOutput>> action, Action<IManagedTaskCreationOptions<TOutput>>? options = null, CancellationToken token = default);
        /// <summary>
        /// Schedules a new named managed task tied to <paramref name="owner"/>.
        /// </summary>
        /// <typeparam name="TOutput">The output returned by the task</typeparam>
        /// <param name="owner">The instance to tie the task to</param>
        /// <param name="name">Optional unique name for the task</param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        Task<IManagedTask> ScheduleAsync<TOutput>(object owner, string? name, Func<CancellationToken, Task<TOutput>> action, Action<INamedManagedTaskCreationOptions<TOutput>>? options = null, CancellationToken token = default);
        /// <summary>
        /// Schedules a new managed anonymous task.
        /// </summary>
        /// <typeparam name="TOutput">The output returned by the task</typeparam>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        IManagedAnonymousTask ScheduleAnonymous<TOutput>(Func<CancellationToken, Task<TOutput>> action, Action<IManagedAnonymousTaskCreationOptions<TOutput>>? options = null, CancellationToken token = default);

        /// <summary>
        /// Gets the managed task with <see cref="IManagedTask.Name"/> set to <paramref name="name"/> if it exists and is still running.
        /// </summary>
        /// <param name="name">The name of the managed task to get</param>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>The managed task with <see cref="IManagedTask.Name"/> set to <paramref name="name"/> if it exists</returns>
        IManagedTask GetByName(string name, CancellationToken token = default);

        /// <summary>
        /// Gets all running managed tasks for instance <paramref name="instance"/>.
        /// </summary>
        /// <param name="instance">The instance to fetch the running tasks for</param>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>All managed tasks owned by <paramref name="instance"/> or an empty array if there aren't any</returns>
        IManagedTask[] GetOwnedBy(object instance, CancellationToken token = default);

        /// <summary>
        /// Cancels all running managed tasks for instance <paramref name="instance"/> (if not cancelled already).
        /// Method does not wait on the cancellation.
        /// </summary>
        /// <param name="instance">The instance to cancel the tasks for</param>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>All the managed tasks cancellation was requested on</returns>
        IManagedTask[] CancelAllFor(object instance, CancellationToken token = default);

        /// <summary>
        /// Cancels all running managed tasks for instance <paramref name="instance"/> (if not cancelled already) and waits for all managed tasks to stop. 
        /// </summary>
        /// <param name="instance">The instance to cancel the tasks for</param>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>All managed tasks that were stopped or an empty array if none were stopped</returns>
        Task<IManagedTask[]> StopAllForAsync(object instance, CancellationToken token = default);

        #region Schedule
        /// <summary>
        /// Schedules a new managed task tied to <paramref name="owner"/>.
        /// </summary>
        /// <typeparam name="TOutput">The output returned by the task</typeparam>
        /// <param name="owner">The instance to tie the task to</param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        IManagedTask Schedule<TOutput>(object owner, Func<CancellationToken, TOutput> action, Action<IManagedTaskCreationOptions<TOutput>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return Schedule(owner, (t) => {
                return action(t).ToTaskResult();
            }, options, token);
        }

        /// <summary>
        /// Schedules a new managed task tied to <paramref name="owner"/>.
        /// </summary>
        /// <param name="owner">The instance to tie the task to</param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        IManagedTask ScheduleAction(object owner, Action<CancellationToken> action, Action<IManagedTaskCreationOptions<Null>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return Schedule(owner, (t) => {
                action(t);
                return Task.FromResult(Null.Value);
            }, options, token);
        }
        /// <summary>
        /// Schedules a new managed task tied to <paramref name="owner"/>.
        /// </summary>
        /// <param name="owner">The instance to tie the task to</param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        IManagedTask ScheduleAction(object owner, Func<CancellationToken, Task> action, Action<IManagedTaskCreationOptions<Null>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return Schedule<Null>(owner, async (t) =>
            {
                await action(t).ConfigureAwait(false);
                return Null.Value;
            }, options, token);
        }

        /// <summary>
        /// Schedules a new managed task tied to <paramref name="owner"/>.
        /// </summary>
        /// <typeparam name="TOutput">The output returned by the task</typeparam>
        /// <param name="owner">The instance to tie the task to</param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        IManagedTask Schedule<TOutput>(object owner, Func<TOutput> action, Action<IManagedTaskCreationOptions<TOutput>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return Schedule(owner, (t) => {
                return action().ToTaskResult();
            }, options, token);
        }

        /// <summary>
        /// Schedules a new managed task tied to <paramref name="owner"/>.
        /// </summary>
        /// <param name="owner">The instance to tie the task to</param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        IManagedTask ScheduleAction(object owner, Action action, Action<IManagedTaskCreationOptions<Null>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return Schedule(owner, (t) => {
                action();
                return Task.FromResult(Null.Value);
            }, options, token);
        }
        /// <summary>
        /// Schedules a new managed task tied to <paramref name="owner"/>.
        /// </summary>
        /// <param name="owner">The instance to tie the task to</param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        IManagedTask ScheduleAction(object owner, Func<Task> action, Action<IManagedTaskCreationOptions<Null>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return Schedule<Null>(owner, async (t) =>
            {
                await action().ConfigureAwait(false);
                return Null.Value;
            }, options, token);
        }
        #endregion

        #region TrySchedule
        /// <summary>
        /// Logs a request to schedule a new named managed task tied to <paramref name="owner"/>.
        /// </summary>
        /// <typeparam name="TOutput">The output returned by the task</typeparam>
        /// <param name="owner">The instance to tie the task to</param>
        /// <param name="name">Optional unique name for the task</param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        IManagedTask TrySchedule<TOutput>(object owner, string name, Func<CancellationToken, TOutput> action, Action<IManagedTaskCreationOptions<TOutput>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return TrySchedule(owner, name, (t) => {
                return action(t).ToTaskResult();
            }, options, token);
        }

        /// <summary>
        /// Logs a request to schedule a new named managed task tied to <paramref name="owner"/>.
        /// </summary>
        /// <param name="owner">The instance to tie the task to</param>
        /// <param name="name">Optional unique name for the task</param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        IManagedTask TryScheduleAction(object owner, string name, Action<CancellationToken> action, Action<IManagedTaskCreationOptions<Null>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return TrySchedule(owner, name, (t) => {
                action(t);
                return Task.FromResult(Null.Value);
            }, options, token);
        }
        /// <summary>
        /// Logs a request to schedule a new named managed task tied to <paramref name="owner"/>.
        /// </summary>
        /// <param name="owner">The instance to tie the task to</param>
        /// <param name="name">Optional unique name for the task</param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        IManagedTask TryScheduleAction(object owner, string name, Func<CancellationToken, Task> action, Action<IManagedTaskCreationOptions<Null>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return TrySchedule<Null>(owner, name, async (t) =>
            {
                await action(t).ConfigureAwait(false);
                return Null.Value;
            }, options, token);
        }

        /// <summary>
        /// Logs a request to schedule a new named managed task tied to <paramref name="owner"/>.
        /// </summary>
        /// <typeparam name="TOutput">The output returned by the task</typeparam>
        /// <param name="owner">The instance to tie the task to</param>
        /// <param name="name">Optional unique name for the task</param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        IManagedTask TrySchedule<TOutput>(object owner, string name, Func<TOutput> action, Action<IManagedTaskCreationOptions<TOutput>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return TrySchedule(owner, name, (t) => {
                return action().ToTaskResult();
            }, options, token);
        }

        /// <summary>
        /// Logs a request to schedule a new named managed task tied to <paramref name="owner"/>.
        /// </summary>
        /// <param name="owner">The instance to tie the task to</param>
        /// <param name="name">Optional unique name for the task</param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        IManagedTask TryScheduleAction(object owner, string name, Action action, Action<IManagedTaskCreationOptions<Null>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return TrySchedule(owner, name, (t) => {
                action();
                return Task.FromResult(Null.Value);
            }, options, token);
        }
        /// <summary>
        /// Logs a request to schedule a new named managed task tied to <paramref name="owner"/>.
        /// </summary>
        /// <param name="owner">The instance to tie the task to</param>
        /// <param name="name">Optional unique name for the task</param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        IManagedTask TryScheduleAction(object owner, string name, Func<Task> action, Action<IManagedTaskCreationOptions<Null>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return TrySchedule<Null>(owner, name, async (t) =>
            {
                await action().ConfigureAwait(false);
                return Null.Value;
            }, options, token);
        }
        #endregion

        #region ScheduleAsync
        /// <summary>
        /// Schedules a new named managed task tied to <paramref name="owner"/> if it isn't started yet.
        /// </summary>
        /// <typeparam name="TOutput">The output returned by the task</typeparam>
        /// <param name="owner">The instance to tie the task to</param>
        /// <param name="name">Optional unique name for the task</param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        Task<IManagedTask> ScheduleAsync<TOutput>(object owner, string? name, Func<CancellationToken, TOutput> action, Action<INamedManagedTaskCreationOptions<TOutput>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return ScheduleAsync(owner, name, (t) => {
                return action(t).ToTaskResult();
            }, options, token);
        }

        /// <summary>
        /// Schedules a new named managed task tied to <paramref name="owner"/> if it isn't started yet.
        /// </summary>
        /// <param name="owner">The instance to tie the task to</param>
        /// <param name="name">Optional unique name for the task</param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        Task<IManagedTask> ScheduleActionAsync(object owner, string? name, Action<CancellationToken> action, Action<INamedManagedTaskCreationOptions<Null>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return ScheduleAsync(owner, name, (t) => {
                action(t);
                return Task.FromResult(Null.Value);
            }, options, token);
        }
        /// <summary>
        /// Schedules a new named managed task tied to <paramref name="owner"/> if it isn't started yet.
        /// </summary>
        /// <param name="owner">The instance to tie the task to</param>
        /// <param name="name">Optional unique name for the task</param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        Task<IManagedTask> ScheduleActionAsync(object owner, string? name, Func<CancellationToken, Task> action, Action<INamedManagedTaskCreationOptions<Null>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return ScheduleAsync(owner, name, async (t) => {
                await action(t).ConfigureAwait(false);
                return Null.Value;
            }, options, token);
        }

        /// <summary>
        /// Schedules a new named managed task tied to <paramref name="owner"/> if it isn't started yet.
        /// </summary>
        /// <typeparam name="TOutput">The output returned by the task</typeparam>
        /// <param name="owner">The instance to tie the task to</param>
        /// <param name="name">Optional unique name for the task</param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        Task<IManagedTask> ScheduleAsync<TOutput>(object owner, string? name, Func<TOutput> action, Action<INamedManagedTaskCreationOptions<TOutput>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return ScheduleAsync(owner, name, (t) => {
                return action().ToTaskResult();
            }, options, token);
        }

        /// <summary>
        /// Schedules a new named managed task tied to <paramref name="owner"/> if it isn't started yet.
        /// </summary>
        /// <param name="owner">The instance to tie the task to</param>
        /// <param name="name">Optional unique name for the task</param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        Task<IManagedTask> ScheduleActionAsync(object owner, string? name, Action action, Action<INamedManagedTaskCreationOptions<Null>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return ScheduleAsync(owner, name, (t) => {
                action();
                return Task.FromResult(Null.Value);
            }, options, token);
        }
        /// <summary>
        /// Schedules a new named managed task tied to <paramref name="owner"/> if it isn't started yet.
        /// </summary>
        /// <param name="owner">The instance to tie the task to</param>
        /// <param name="name">Optional unique name for the task</param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        Task<IManagedTask> ScheduleActionAsync(object owner, string? name, Func<Task> action, Action<INamedManagedTaskCreationOptions<Null>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return ScheduleAsync(owner, name, async (t) => {
                await action().ConfigureAwait(false);
                return Null.Value;
            }, options, token);
        }
        #endregion

        #region ScheduleAnonymous
        /// <summary>
        /// Schedules a new managed anonymous task.
        /// </summary>
        /// <typeparam name="TOutput">The output returned by the task</typeparam>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        IManagedAnonymousTask ScheduleAnonymous<TOutput>(Func<CancellationToken, TOutput> action, Action<IManagedAnonymousTaskCreationOptions<TOutput>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return ScheduleAnonymous((t) => {
                return action(t).ToTaskResult();
            }, options, token);
        }

        /// <summary>
        /// Schedules a new managed anonymous task.
        /// </summary>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        IManagedAnonymousTask ScheduleAnonymousAction(Action<CancellationToken> action, Action<IManagedAnonymousTaskCreationOptions<Null>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return ScheduleAnonymous((t) => {
                action(t);
                return Task.FromResult(Null.Value);
            }, options, token);
        }
        /// <summary>
        /// Schedules a new managed anonymous task.
        /// </summary>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        IManagedAnonymousTask ScheduleAnonymousAction(Func<CancellationToken, Task> action, Action<IManagedAnonymousTaskCreationOptions<Null>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return ScheduleAnonymous(async (t) => {
                await action(t).ConfigureAwait(false);
                return Null.Value;
            }, options, token);
        }

        /// <summary>
        /// Schedules a new managed anonymous task.
        /// </summary>
        /// <typeparam name="TOutput">The output returned by the task</typeparam>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        IManagedAnonymousTask ScheduleAnonymous<TOutput>(Func<TOutput> action, Action<IManagedAnonymousTaskCreationOptions<TOutput>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return ScheduleAnonymous((t) => {
                return action().ToTaskResult();
            }, options, token);
        }

        /// <summary>
        /// Schedules a new managed anonymous task.
        /// </summary>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        IManagedAnonymousTask ScheduleAnonymousAction(Action action, Action<IManagedAnonymousTaskCreationOptions<Null>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return ScheduleAnonymous((t) => {
                action();
                return Task.FromResult(Null.Value);
            }, options, token);
        }
        /// <summary>
        /// Schedules a new managed anonymous task.
        /// </summary>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        IManagedAnonymousTask ScheduleAnonymousAction(Func<Task> action, Action<IManagedAnonymousTaskCreationOptions<Null>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return ScheduleAnonymous(async (t) => {
                await action().ConfigureAwait(false);
                return Null.Value;
            }, options, token);
        }
        #endregion
    }
}