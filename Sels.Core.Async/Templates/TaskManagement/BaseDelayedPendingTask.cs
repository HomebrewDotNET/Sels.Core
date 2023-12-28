using Sels.Core.Async.TaskManagement;
using Sels.Core.Extensions;
using Sels.Core.Scope.Actions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sels.Core.Async.Templates.TaskManagement
{
    /// <summary>
    /// base class for creating a <see cref="IDelayedPendingTask{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the pending task</typeparam>
    public abstract class BaseDelayedPendingTask<T> : IDelayedPendingTask<T> where T : IManagedAnonymousTask
    {
        // Fields
        private readonly ITaskManager _taskManager;
        private readonly System.Timers.Timer _timer;
        private readonly TaskCompletionSource<T> _taskSource = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly Func<ITaskManager, CancellationToken, Task<T>> _scheduleAction;
        private readonly CancellationTokenRegistration _registration;
        private readonly CancellationTokenSource _cancelSource = new CancellationTokenSource();

        // State
        private bool _isScheduling;

        // Properties
        /// <inheritdoc/>
        public DateTime Created { get; } = DateTime.Now;
        /// <inheritdoc/>
        public Task<T> Callback => _taskSource.Task;
        /// <inheritdoc/>
        public bool IsCancelled => _cancelSource.IsCancellationRequested;
        /// <inheritdoc/>
        public TimeSpan Delay { get; }
        /// <inheritdoc/>
        public DateTime ScheduleTime { get; }

        /// <inheritdoc cref="BaseDelayedPendingTask{T}"/>
        /// <param name="scheduleAction">Delegate that schedules the pending task</param>
        /// <param name="taskManager">Task manager to use to schedule the pending task</param>
        /// <param name="delay">How long to delay the pending task by</param>
        /// <param name="token">Token that can be used to cancel the pending task</param>
        public BaseDelayedPendingTask(Func<ITaskManager, CancellationToken, Task<T>> scheduleAction, ITaskManager taskManager, TimeSpan delay, CancellationToken token)
        {
            _scheduleAction = scheduleAction.ValidateArgument(nameof(scheduleAction));
            _taskManager = taskManager.ValidateArgument(nameof(taskManager));
            Delay = delay;
            _registration = token.Register(Cancel);

            _timer = new System.Timers.Timer();
            _timer.AutoReset = false;
            _timer.Interval = Delay.TotalMilliseconds == 0 ? 1 : Delay.TotalMilliseconds;
            _timer.Elapsed += (s, a) => Trigger();
            _timer.Enabled = true;
            ScheduleTime = DateTime.Now.Add(Delay);
        }

        /// <inheritdoc/>
        public void Cancel()
        {
            try
            {
                // Cancel token
                lock (_cancelSource)
                {
                    _cancelSource.Cancel();
                }

                // If not scheduling we cancel task
                lock (_taskSource)
                {
                    if(!_isScheduling)
                    {
                        _taskSource.TrySetCanceled();
                    }
                }
            }
            finally
            {
                Dispose();
            }
        }

        private async void Trigger()
        {
            using(new InProcessAction(x => _isScheduling = x))
            {
                try
                {
                    // Check if cancelled
                    lock (_cancelSource)
                    {
                        if (_cancelSource.IsCancellationRequested) return;
                    }

                    // Schedule action
                    var task = await _scheduleAction(_taskManager, _cancelSource.Token);
                    
                    lock(_taskSource)
                    {
                        _taskSource.TrySetResult(task);
                    }
                }
                catch (OperationCanceledException)
                {
                    lock (_taskSource)
                    {
                        _taskSource.TrySetCanceled();
                    }
                }
                catch (Exception ex)
                {
                    lock (_taskSource)
                    {
                        _taskSource.TrySetException(ex);
                    }
                }
                finally
                {
                    Dispose();
                }
            }
            
        }


        /// <inheritdoc/>
        public void Dispose()
        {
            // Dispose registration
            _registration.Dispose();

            // Dispose token
            lock(_cancelSource)
            {
                _cancelSource.Dispose();
            }

            // Dispose timer
            lock (_timer)
            {
                _timer.Dispose();
            }

            // Complete callback task if not already
            lock (_taskSource)
            {
                _taskSource.TrySetCanceled();
            }
        }
    }
}
