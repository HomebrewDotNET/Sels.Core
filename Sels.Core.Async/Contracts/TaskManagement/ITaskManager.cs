using Sels.Core.Async.Contracts.TaskManagement;
using Sels.Core.Extensions;
using Sels.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        /// <typeparam name="TInput">The input for the task to schedule</typeparam>
        /// <typeparam name="TOutput">The output returned by the task</typeparam>
        /// <param name="owner">The instance to tie the task to</param>
        /// <param name="input">The input for the task</param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        IManagedTask Schedule<TInput, TOutput>(object owner, TInput input, AsyncFunc<TInput, CancellationToken, TOutput> action, Action<IManagedTaskCreationOptions<TInput, TOutput>>? options = null, CancellationToken token = default);
        /// <summary>
        /// Logs a request to schedule a new named managed task tied to <paramref name="owner"/>.
        /// </summary>
        /// <typeparam name="TInput">The input for the task to schedule</typeparam>
        /// <typeparam name="TOutput">The output returned by the task</typeparam>
        /// <param name="owner">The instance to tie the task to</param>
        /// <param name="name">Optional unique name for the task</param>
        /// <param name="input">The input for the task</param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        IManagedTask TrySchedule<TInput, TOutput>(object owner, string? name, TInput input, AsyncFunc<TInput, CancellationToken, TOutput> action, Action<IManagedTaskCreationOptions<TInput, TOutput>>? options = null, CancellationToken token = default);
        /// <summary>
        /// Schedules a new named managed task tied to <paramref name="owner"/>.
        /// </summary>
        /// <typeparam name="TInput">The input for the task to schedule</typeparam>
        /// <typeparam name="TOutput">The output returned by the task</typeparam>
        /// <param name="owner">The instance to tie the task to</param>
        /// <param name="name">Optional unique name for the task</param>
        /// <param name="input">The input for the task</param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        Task<IManagedTask> ScheduleAsync<TInput, TOutput>(object owner, string? name, TInput input, AsyncFunc<TInput, CancellationToken, TOutput> action, Action<INamedManagedTaskCreationOptions<TInput, TOutput>>? options = null, CancellationToken token = default)
            => Schedule<TInput, TOutput>(owner, name, input, action, options, token).Callback;
        /// <summary>
        /// Logs a request to schedule a new named managed task tied to <paramref name="owner"/>.
        /// </summary>
        /// <typeparam name="TInput">The input for the task to schedule</typeparam>
        /// <typeparam name="TOutput">The output returned by the task</typeparam>
        /// <param name="owner">The instance to tie the task to</param>
        /// <param name="name">Optional unique name for the task</param>
        /// <param name="input">The input for the task</param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>Object that contains a callback tha will complete when the requested task is scheduled</returns>
        IPendingTask<IManagedTask> Schedule<TInput, TOutput>(object owner, string? name, TInput input, AsyncFunc<TInput, CancellationToken, TOutput> action, Action<INamedManagedTaskCreationOptions<TInput, TOutput>>? options = null, CancellationToken token = default);
        /// <summary>
        /// Schedules a new managed anonymous task.
        /// </summary>
        /// <typeparam name="TInput">The input for the task to schedule</typeparam>
        /// <typeparam name="TOutput">The output returned by the task</typeparam>
        /// <param name="input">The input for the task</param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        IManagedAnonymousTask ScheduleAnonymous<TInput, TOutput>(TInput input, AsyncFunc<TInput, CancellationToken, TOutput> action, Action<IManagedAnonymousTaskCreationOptions<TInput, TOutput>>? options = null, CancellationToken token = default);

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
        /// <typeparam name="TInput">The input for the task to schedule</typeparam>
        /// <typeparam name="TOutput">The output returned by the task</typeparam>
        /// <param name="owner">The instance to tie the task to</param>
        /// <param name="input">The input for the task</param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        IManagedTask Schedule<TInput, TOutput>(object owner, TInput input, AsyncFunc<TInput, CancellationToken, Task<TOutput>> action, Action<IManagedTaskCreationOptions<TInput, TOutput>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return Schedule(owner, input, (i, t) => action(i,t).Unwrap(), options, token);
        }
        /// <summary>
        /// Schedules a new managed task tied to <paramref name="owner"/>.
        /// </summary>
        /// <typeparam name="TInput">The input for the task to schedule</typeparam>
        /// <typeparam name="TOutput">The output returned by the task</typeparam>
        /// <param name="owner">The instance to tie the task to</param>
        /// <param name="input">The input for the task</param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        IManagedTask Schedule<TInput, TOutput>(object owner, TInput input, Func<TInput, CancellationToken, TOutput> action, Action<IManagedTaskCreationOptions<TInput, TOutput>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return Schedule(owner, input, (i, t) => Task.FromResult(action(i, t)), options, token);
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
        IManagedTask Schedule<TOutput>(object owner, AsyncFunc<CancellationToken, TOutput> action, Action<IManagedTaskCreationOptions<Null, TOutput>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return Schedule<Null, TOutput>(owner, Null.Value, (i, t) => action(t), options, token);
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
        IManagedTask Schedule<TOutput>(object owner, AsyncFunc<CancellationToken, Task<TOutput>> action, Action<IManagedTaskCreationOptions<Null, TOutput>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return Schedule<Null, TOutput>(owner, Null.Value, (i, t) => action(t).Unwrap(), options, token);
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
        IManagedTask Schedule<TOutput>(object owner, Func<CancellationToken, TOutput> action, Action<IManagedTaskCreationOptions<Null, TOutput>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return Schedule(owner, Null.Value, (i, t) => Task.FromResult(action(t)), options, token);
        }

        /// <summary>
        /// Schedules a new managed task tied to <paramref name="owner"/>.
        /// </summary>
        /// <typeparam name="TInput">The input for the task to schedule</typeparam>
        /// <param name="owner">The instance to tie the task to</param>
        /// <param name="input">The input for the task</param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        IManagedTask Schedule<TInput>(object owner, TInput input, AsyncAction<TInput, CancellationToken> action, Action<IManagedTaskCreationOptions<TInput, Null>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return Schedule(owner, input, async (i, t) =>
            {
                await action(i, t).ConfigureAwait(false);
                return Null.Value;
            }, options, token);
        }
        /// <summary>
        /// Schedules a new managed task tied to <paramref name="owner"/>.
        /// </summary>
        /// <typeparam name="TInput">The input for the task to schedule</typeparam>
        /// <param name="owner">The instance to tie the task to</param>
        /// <param name="input">The input for the task</param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        IManagedTask Schedule<TInput>(object owner, TInput input, Action<TInput, CancellationToken> action, Action<IManagedTaskCreationOptions<TInput, Null>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return Schedule(owner, input, (i, t) => {
                action(i, t);
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
        IManagedTask Schedule(object owner, AsyncAction<CancellationToken> action, Action<IManagedTaskCreationOptions<Null, Null>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return Schedule<Null, Null>(owner, Null.Value, async (i, t) =>
            {
                await action(t);
                return Null.Value;
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
        IManagedTask Schedule(object owner, Action<CancellationToken> action, Action<IManagedTaskCreationOptions<Null, Null>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return Schedule(owner, Null.Value, (i, t) => {
                action(t);
                return Task.FromResult(Null.Value);
            }, options, token);
        }
        #endregion

        #region TrySchedule
        /// <summary>
        /// Schedules a new named managed task tied to <paramref name="owner"/> if it isn't started yet.
        /// </summary>
        /// <typeparam name="TInput">The input for the task to schedule</typeparam>
        /// <typeparam name="TOutput">The output returned by the task</typeparam>
        /// <param name="owner">The instance to tie the task to</param>
        /// <param name="name">Optional unique name for the task</param>
        /// <param name="input">The input for the task</param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        IManagedTask TrySchedule<TInput, TOutput>(object owner, string? name, TInput input, AsyncFunc<TInput, CancellationToken, Task<TOutput>> action, Action<IManagedTaskCreationOptions<TInput, TOutput>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return TrySchedule(owner, name, input, (i, t) => action(i, t).Unwrap(), options, token);
        }
        /// <summary>
        /// Schedules a new named managed task tied to <paramref name="owner"/> if it isn't started yet.
        /// </summary>
        /// <typeparam name="TInput">The input for the task to schedule</typeparam>
        /// <typeparam name="TOutput">The output returned by the task</typeparam>
        /// <param name="owner">The instance to tie the task to</param>
        /// <param name="name">Optional unique name for the task</param>
        /// <param name="input">The input for the task</param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        IManagedTask TrySchedule<TInput, TOutput>(object owner, string? name, TInput input, Func<TInput, CancellationToken, TOutput> action, Action<IManagedTaskCreationOptions<TInput, TOutput>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return TrySchedule(owner, name, input, (i, t) => Task.FromResult(action(i, t)), options, token);
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
        IManagedTask TrySchedule<TOutput>(object owner, string? name, AsyncFunc<CancellationToken, TOutput> action, Action<IManagedTaskCreationOptions<Null, TOutput>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return TrySchedule(owner, name, Null.Value, (i, t) => action(t), options, token);
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
        IManagedTask TrySchedule<TOutput>(object owner, string? name, AsyncFunc<CancellationToken, Task<TOutput>> action, Action<IManagedTaskCreationOptions<Null, TOutput>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return TrySchedule(owner, name, Null.Value, (i, t) => action(t).Unwrap(), options, token);
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
        IManagedTask TrySchedule<TOutput>(object owner, string? name, Func<CancellationToken, TOutput> action, Action<IManagedTaskCreationOptions<Null, TOutput>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return TrySchedule(owner, name, Null.Value, (i, t) => Task.FromResult(action(t)), options, token);
        }

        /// <summary>
        /// Schedules a new named managed task tied to <paramref name="owner"/> if it isn't started yet.
        /// </summary>
        /// <typeparam name="TInput">The input for the task to schedule</typeparam>
        /// <param name="owner">The instance to tie the task to</param>
        /// <param name="name">Optional unique name for the task</param>
        /// <param name="input">The input for the task</param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        IManagedTask TrySchedule<TInput>(object owner, string? name, TInput input, AsyncAction<TInput, CancellationToken> action, Action<IManagedTaskCreationOptions<TInput, Null>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return TrySchedule(owner, name, input, async (i, t) =>
            {
                await action(i, t).ConfigureAwait(false);
                return Null.Value;
            }, options, token);
        }
        /// <summary>
        /// Schedules a new named managed task tied to <paramref name="owner"/> if it isn't started yet.
        /// </summary>
        /// <typeparam name="TInput">The input for the task to schedule</typeparam>
        /// <param name="owner">The instance to tie the task to</param>
        /// <param name="name">Optional unique name for the task</param>
        /// <param name="input">The input for the task</param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        IManagedTask TrySchedule<TInput>(object owner, string? name, TInput input, Action<TInput, CancellationToken> action, Action<IManagedTaskCreationOptions<TInput, Null>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return TrySchedule(owner, name, input, (i, t) => {
                action(i, t);
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
        IManagedTask TrySchedule(object owner, string? name, AsyncAction<CancellationToken> action, Action<IManagedTaskCreationOptions<Null, Null>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return TrySchedule(owner, name, Null.Value, async (i, t) =>
            {
                await action(t);
                return Null.Value;
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
        IManagedTask TrySchedule(object owner, string? name, Action<CancellationToken> action, Action<IManagedTaskCreationOptions<Null, Null>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return TrySchedule(owner, name, Null.Value, (i, t) => {
                action(t);
                return Task.FromResult(Null.Value);
            }, options, token);
        }
        #endregion

        #region ScheduleAsync
        /// <summary>
        /// Schedules a new named managed task tied to <paramref name="owner"/> if it isn't started yet.
        /// </summary>
        /// <typeparam name="TInput">The input for the task to schedule</typeparam>
        /// <typeparam name="TOutput">The output returned by the task</typeparam>
        /// <param name="owner">The instance to tie the task to</param>
        /// <param name="name">Optional unique name for the task</param>
        /// <param name="input">The input for the task</param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        Task<IManagedTask> ScheduleAsync<TInput, TOutput>(object owner, string? name, TInput input, AsyncFunc<TInput, CancellationToken, Task<TOutput>> action, Action<INamedManagedTaskCreationOptions<TInput, TOutput>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return ScheduleAsync(owner, name, input, (i, t) => action(i, t).Unwrap(), options, token);
        }
        /// <summary>
        /// Schedules a new named managed task tied to <paramref name="owner"/> if it isn't started yet.
        /// </summary>
        /// <typeparam name="TInput">The input for the task to schedule</typeparam>
        /// <typeparam name="TOutput">The output returned by the task</typeparam>
        /// <param name="owner">The instance to tie the task to</param>
        /// <param name="name">Optional unique name for the task</param>
        /// <param name="input">The input for the task</param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        Task<IManagedTask> ScheduleAsync<TInput, TOutput>(object owner, string? name, TInput input, Func<TInput, CancellationToken, TOutput> action, Action<INamedManagedTaskCreationOptions<TInput, TOutput>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return ScheduleAsync(owner, name, input, (i, t) => Task.FromResult(action(i, t)), options, token);
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
        Task<IManagedTask> ScheduleAsync<TOutput>(object owner, string? name, AsyncFunc<CancellationToken, TOutput> action, Action<INamedManagedTaskCreationOptions<Null, TOutput>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return ScheduleAsync(owner, name, Null.Value, (i, t) => action(t), options, token);
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
        Task<IManagedTask> ScheduleAsync<TOutput>(object owner, string? name, AsyncFunc<CancellationToken, Task<TOutput>> action, Action<INamedManagedTaskCreationOptions<Null, TOutput>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return ScheduleAsync(owner, name, Null.Value, (i, t) => action(t).Unwrap(), options, token);
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
        Task<IManagedTask> ScheduleAsync<TOutput>(object owner, string? name, Func<CancellationToken, TOutput> action, Action<INamedManagedTaskCreationOptions<Null, TOutput>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return ScheduleAsync(owner, name, Null.Value, (i, t) => Task.FromResult(action(t)), options, token);
        }

        /// <summary>
        /// Schedules a new named managed task tied to <paramref name="owner"/> if it isn't started yet.
        /// </summary>
        /// <typeparam name="TInput">The input for the task to schedule</typeparam>
        /// <param name="owner">The instance to tie the task to</param>
        /// <param name="name">Optional unique name for the task</param>
        /// <param name="input">The input for the task</param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        Task<IManagedTask> ScheduleAsync<TInput>(object owner, string? name, TInput input, AsyncAction<TInput, CancellationToken> action, Action<INamedManagedTaskCreationOptions<TInput, Null>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return ScheduleAsync(owner, name, input, async (i, t) =>
            {
                await action(i, t).ConfigureAwait(false);
                return Null.Value;
            }, options, token);
        }
        /// <summary>
        /// Schedules a new named managed task tied to <paramref name="owner"/> if it isn't started yet.
        /// </summary>
        /// <typeparam name="TInput">The input for the task to schedule</typeparam>
        /// <param name="owner">The instance to tie the task to</param>
        /// <param name="name">Optional unique name for the task</param>
        /// <param name="input">The input for the task</param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        Task<IManagedTask> ScheduleAsync<TInput>(object owner, string? name, TInput input, Action<TInput, CancellationToken> action, Action<INamedManagedTaskCreationOptions<TInput, Null>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return ScheduleAsync(owner, name, input, (i, t) => {
                action(i, t);
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
        Task<IManagedTask> ScheduleAsync(object owner, string? name, AsyncAction<CancellationToken> action, Action<INamedManagedTaskCreationOptions<Null, Null>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return ScheduleAsync(owner, name, Null.Value, async (i, t) =>
            {
                await action(t);
                return Null.Value;
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
        Task<IManagedTask> ScheduleAsync(object owner, string? name, Action<CancellationToken> action, Action<INamedManagedTaskCreationOptions<Null, Null>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return ScheduleAsync(owner, name, Null.Value, (i, t) => {
                action(t);
                return Task.FromResult(Null.Value);
            }, options, token);
        }
        #endregion

        #region Schedule Pending
        /// <summary>
        /// Logs a request to schedule a new named managed task tied to <paramref name="owner"/>.
        /// </summary>
        /// <typeparam name="TInput">The input for the task to schedule</typeparam>
        /// <typeparam name="TOutput">The output returned by the task</typeparam>
        /// <param name="owner">The instance to tie the task to</param>
        /// <param name="name">Optional unique name for the task</param>
        /// <param name="input">The input for the task</param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>Object that contains a callback tha will complete when the requested task is scheduled</returns>
        IPendingTask<IManagedTask> Schedule<TInput, TOutput>(object owner, string? name, TInput input, AsyncFunc<TInput, CancellationToken, Task<TOutput>> action, Action<INamedManagedTaskCreationOptions<TInput, TOutput>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return Schedule(owner, name, input, (i, t) => action(i, t).Unwrap(), options, token);
        }
        /// <summary>
        /// Logs a request to schedule a new named managed task tied to <paramref name="owner"/>.
        /// </summary>
        /// <typeparam name="TInput">The input for the task to schedule</typeparam>
        /// <typeparam name="TOutput">The output returned by the task</typeparam>
        /// <param name="owner">The instance to tie the task to</param>
        /// <param name="name">Optional unique name for the task</param>
        /// <param name="input">The input for the task</param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>Object that contains a callback tha will complete when the requested task is scheduled</returns>
        IPendingTask<IManagedTask> Schedule<TInput, TOutput>(object owner, string? name, TInput input, Func<TInput, CancellationToken, TOutput> action, Action<INamedManagedTaskCreationOptions<TInput, TOutput>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return Schedule(owner, name, input, (i, t) => Task.FromResult(action(i, t)), options, token);
        }

        /// <summary>
        /// Logs a request to schedule a new named managed task tied to <paramref name="owner"/>.
        /// </summary>
        /// <typeparam name="TOutput">The output returned by the task</typeparam>
        /// <param name="owner">The instance to tie the task to</param>
        /// <param name="name">Optional unique name for the task</param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options foSchedules a new named managed task tied to <paramref name="owner"/> if it isn't started yet.r the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>Object that contains a callback tha will complete when the requested task is scheduled</returns>
        IPendingTask<IManagedTask> Schedule<TOutput>(object owner, string? name, AsyncFunc<CancellationToken, TOutput> action, Action<INamedManagedTaskCreationOptions<Null, TOutput>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return Schedule(owner, name, Null.Value, (i, t) => action(t), options, token);
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
        /// <returns>Object that contains a callback tha will complete when the requested task is scheduled</returns>
        IPendingTask<IManagedTask> Schedule<TOutput>(object owner, string? name, AsyncFunc<CancellationToken, Task<TOutput>> action, Action<INamedManagedTaskCreationOptions<Null, TOutput>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return Schedule(owner, name, Null.Value, (i, t) => action(t).Unwrap(), options, token);
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
        /// <returns>Object that contains a callback tha will complete when the requested task is scheduled</returns>
        IPendingTask<IManagedTask> Schedule<TOutput>(object owner, string? name, Func<CancellationToken, TOutput> action, Action<INamedManagedTaskCreationOptions<Null, TOutput>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return Schedule(owner, name, Null.Value, (i, t) => Task.FromResult(action(t)), options, token);
        }

        /// <summary>
        /// Logs a request to schedule a new named managed task tied to <paramref name="owner"/>.
        /// </summary>
        /// <typeparam name="TInput">The input for the task to schedule</typeparam>
        /// <param name="owner">The instance to tie the task to</param>
        /// <param name="name">Optional unique name for the task</param>
        /// <param name="input">The input for the task</param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>Object that contains a callback tha will complete when the requested task is scheduled</returns>
        IPendingTask<IManagedTask> Schedule<TInput>(object owner, string? name, TInput input, AsyncAction<TInput, CancellationToken> action, Action<INamedManagedTaskCreationOptions<TInput, Null>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return Schedule(owner, name, input, async (i, t) =>
            {
                await action(i, t).ConfigureAwait(false);
                return Null.Value;
            }, options, token);
        }
        /// <summary>
        /// Logs a request to schedule a new named managed task tied to <paramref name="owner"/>.
        /// </summary>
        /// <typeparam name="TInput">The input for the task to schedule</typeparam>
        /// <param name="owner">The instance to tie the task to</param>
        /// <param name="name">Optional unique name for the task</param>
        /// <param name="input">The input for the task</param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>Object that contains a callback tha will complete when the requested task is scheduled</returns>
        IPendingTask<IManagedTask> Schedule<TInput>(object owner, string? name, TInput input, Action<TInput, CancellationToken> action, Action<INamedManagedTaskCreationOptions<TInput, Null>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return Schedule(owner, name, input, (i, t) => {
                action(i, t);
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
        /// <returns>Object that contains a callback tha will complete when the requested task is scheduled</returns>
        IPendingTask<IManagedTask> Schedule(object owner, string? name, AsyncAction<CancellationToken> action, Action<INamedManagedTaskCreationOptions<Null, Null>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return Schedule(owner, name, Null.Value, async (i, t) =>
            {
                await action(t);
                return Null.Value;
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
        /// <returns>Object that contains a callback tha will complete when the requested task is scheduled</returns>
        IPendingTask<IManagedTask> Schedule(object owner, string? name, Action<CancellationToken> action, Action<INamedManagedTaskCreationOptions<Null, Null>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return Schedule(owner, name, Null.Value, (i, t) => {
                action(t);
                return Task.FromResult(Null.Value);
            }, options, token);
        }
        #endregion

        #region ScheduleAnonymous
        /// <summary>
        /// Schedules a new managed anonymous task.
        /// </summary>
        /// <typeparam name="TInput">The input for the task to schedule</typeparam>
        /// <typeparam name="TOutput">The output returned by the task</typeparam>
        /// <param name="input">The input for the task</param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        IManagedAnonymousTask ScheduleAnonymous<TInput, TOutput>(TInput input, AsyncFunc<TInput, CancellationToken, Task<TOutput>> action, Action<IManagedAnonymousTaskCreationOptions<TInput, TOutput>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return ScheduleAnonymous(input, (i, t) => action(i, t).Unwrap(), options, token);
        }
        /// <summary>
        /// Schedules a new managed anonymous task.
        /// </summary>
        /// <typeparam name="TInput">The input for the task to schedule</typeparam>
        /// <typeparam name="TOutput">The output returned by the task</typeparam>
        /// <param name="input">The input for the task</param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        IManagedAnonymousTask ScheduleAnonymous<TInput, TOutput>(TInput input, Func<TInput, CancellationToken, TOutput> action, Action<IManagedAnonymousTaskCreationOptions<TInput, TOutput>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return ScheduleAnonymous(input, (i, t) => Task.FromResult(action(i, t)), options, token);
        }

        /// <summary>
        /// Schedules a new managed anonymous task.
        /// </summary>
        /// <typeparam name="TOutput">The output returned by the task</typeparam>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        IManagedAnonymousTask ScheduleAnonymous<TOutput>(AsyncFunc<CancellationToken, TOutput> action, Action<IManagedAnonymousTaskCreationOptions<Null, TOutput>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return ScheduleAnonymous<Null, TOutput>(Null.Value, (i, t) => action(t), options, token);
        }
        /// <summary>
        /// Schedules a new managed anonymous task.
        /// </summary>
        /// <typeparam name="TOutput">The output returned by the task</typeparam>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        IManagedAnonymousTask ScheduleAnonymous<TOutput>(AsyncFunc<CancellationToken, Task<TOutput>> action, Action<IManagedAnonymousTaskCreationOptions<Null, TOutput>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return ScheduleAnonymous<Null, TOutput>(Null.Value, (i, t) => action(t).Unwrap(), options, token);
        }
        /// <summary>
        /// Schedules a new managed anonymous task.
        /// </summary>
        /// <typeparam name="TOutput">The output returned by the task</typeparam>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        IManagedAnonymousTask ScheduleAnonymous<TOutput>(Func<CancellationToken, TOutput> action, Action<IManagedAnonymousTaskCreationOptions<Null, TOutput>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return ScheduleAnonymous(Null.Value, (i, t) => Task.FromResult(action(t)), options, token);
        }

        /// <summary>
        /// Schedules a new managed anonymous task.
        /// </summary>
        /// <typeparam name="TInput">The input for the task to schedule</typeparam>
        /// <param name="input">The input for the task</param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        IManagedAnonymousTask ScheduleAnonymous<TInput>(TInput input, AsyncAction<TInput, CancellationToken> action, Action<IManagedAnonymousTaskCreationOptions<TInput, Null>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return ScheduleAnonymous(input, async (i, t) =>
            {
                await action(i, t).ConfigureAwait(false);
                return Null.Value;
            }, options, token);
        }
        /// <summary>
        /// Schedules a new managed anonymous task.
        /// </summary>
        /// <typeparam name="TInput">The input for the task to schedule</typeparam>
        /// <param name="input">The input for the task</param>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        IManagedAnonymousTask ScheduleAnonymous<TInput>(TInput input, Action<TInput, CancellationToken> action, Action<IManagedAnonymousTaskCreationOptions<TInput, Null>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return ScheduleAnonymous(input, (i, t) => {
                action(i, t);
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
        IManagedAnonymousTask ScheduleAnonymous(AsyncAction<CancellationToken> action, Action<IManagedAnonymousTaskCreationOptions<Null, Null>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return ScheduleAnonymous<Null, Null>(Null.Value, async (i, t) =>
            {
                await action(t);
                return Null.Value;
            }, options, token);
        }
        /// <summary>
        /// Schedules a new managed anonymous task.
        /// </summary>
        /// <param name="action">Delegate that will be executed by the managed task</param>
        /// <param name="options">Optional delegate for configuring the options for the managed task</param>
        /// <param name="token">Optional token to cancel the request / managed task</param>
        /// <returns>The managed task that was scheduled</returns>
        IManagedAnonymousTask ScheduleAnonymous(Action<CancellationToken> action, Action<IManagedAnonymousTaskCreationOptions<Null, Null>>? options = null, CancellationToken token = default)
        {
            action.ValidateArgument(nameof(action));

            return ScheduleAnonymous(Null.Value, (i, t) => {
                action(t);
                return Task.FromResult(Null.Value);
            }, options, token);
        }
        #endregion
    }
}
