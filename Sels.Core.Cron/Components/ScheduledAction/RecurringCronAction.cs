using NCrontab;
using Sels.Core.Contracts.ScheduledAction;
using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Timers;
using SystemTimer = System.Timers.Timer;

namespace Sels.Core.Cron.Components.ScheduledAction
{
    /// <summary>
    /// Runs code on a schedule using a timer.
    /// </summary>
    public class RecurringCronAction : IScheduledAction
    {
        // Fields
        private CancellationTokenSource _tokenSource;
        private readonly object _threadLock = new object();
        private readonly CrontabSchedule _schedule;
        private readonly SystemTimer _timer;

        // Properties
        /// <inheritdoc/>
        public DateTime? LastRunTime { get; private set; }
        /// <inheritdoc/>
        public DateTime? EstimatedNextRunTime { get; private set; }
        /// <inheritdoc/>
        public bool IsRunning { get; internal set; }
        /// <inheritdoc/>
        public Action<CancellationToken> Action { get; set; }
        /// <inheritdoc/>
        public Action<Exception> ExceptionHandler { get; set; }
        /// <inheritdoc/>
        public bool HaltOnException { get; set; }

        /// <summary>
        /// Runs code on a schedule using a timer.
        /// </summary>
        /// <param name="schedule">Schedule in the cron format</param>
        public RecurringCronAction(string schedule)
        {
            schedule.ValidateArgumentNotNullOrEmpty(nameof(schedule));

            _schedule = CrontabSchedule.Parse(schedule);
            
            _timer = new SystemTimer(SetNextRunTimeAndGetInterval())
            {
                AutoReset = true
            };
            _timer.Elapsed += (x, y) => Execute();            
        }

        /// <summary>
        /// Runs code on a schedule using a timer.
        /// </summary>
        /// <param name="schedule">Schedule in the cron format</param>
        /// <param name="action">The action to execute</param>
        public RecurringCronAction(Action<CancellationToken> action, string schedule) : this(schedule)
        {
            Action = action.ValidateArgument(nameof(action));
        }

        /// <inheritdoc/>
        public void Start()
        {
            if (IsRunning) throw new InvalidOperationException($"Scheduled action is already running");
            lock (_threadLock)
            {
                _tokenSource = new CancellationTokenSource();
                _timer.Start();
                IsRunning = true;
            }
        }
        /// <inheritdoc/>
        public void Stop()
        {
            if (!IsRunning) throw new InvalidOperationException($"Scheduled action is not running");
            _tokenSource.Cancel();
            lock (_threadLock)
            {
                _timer.Stop();
                IsRunning = false;
            }
        }

        /// <inheritdoc/>
        public void ExecuteAndStart()
        {
            lock (_threadLock)
            {
                Start();
                Execute(true);
            }
        }

        private void Execute(bool wasDirectExecution = false)
        {
            lock (_threadLock)
            {
                // If stop was called right before excuting this method.
                if (!IsRunning) return;

                _timer.Stop();

                Action.ValidateArgument(nameof(Action));

                try
                {
                    Action(_tokenSource.Token);
                }
                catch (Exception ex)
                {
                    if (wasDirectExecution) throw;
                    ExceptionHandler?.Invoke(ex);
                    if (HaltOnException)
                    {
                        Stop();
                        return;
                    }
                }

                LastRunTime = DateTime.Now;
                _timer.Interval = SetNextRunTimeAndGetInterval();
                _timer.Start();
            }
        }

        private double SetNextRunTimeAndGetInterval()
        {
            EstimatedNextRunTime = _schedule.GetNextOccurrence(DateTime.Now);

            return (EstimatedNextRunTime - DateTime.Now).Value.TotalMilliseconds;
        }
    }
}
