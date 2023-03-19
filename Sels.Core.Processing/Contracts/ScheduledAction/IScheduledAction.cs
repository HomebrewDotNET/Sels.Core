using System;
using System.Threading;

namespace Sels.Core.Processing.Contracts.ScheduledAction
{
    /// <summary>
    /// Runs code based on a schedule
    /// </summary>
    public interface IScheduledAction
    {
        /// <summary>
        /// Time when this action last ran. Null if it hasn't run once.
        /// </summary>
        DateTime? LastRunTime { get; }
        /// <summary>
        /// Estimated time when this action will execute. Null if not running.
        /// </summary>
        DateTime? EstimatedNextRunTime { get; }
        /// <summary>
        /// If this action is currently running and is executing code on a schedule.
        /// </summary>
        bool IsRunning { get; }
        /// <summary>
        /// The action to execute. The arg is the cancellation token that will be used when calling <see cref="Stop"/>.
        /// </summary>
        Action<CancellationToken> Action { get; set; }
        /// <summary>
        /// Optional delegate to handle any exceptions that are thrown.
        /// </summary>
        Action<Exception> ExceptionHandler { get; set; }
        /// <summary>
        /// If this action should stop running when an uncaught exception is thrown.
        /// </summary>
        bool HaltOnException { get; set; }
        /// <summary>
        /// Starts this action so code starts running on the internally defined schedule.
        /// </summary>
        void Start();
        /// <summary>
        /// Execute <see cref="Action"/> and then start executing on the internally defined schedule.
        /// </summary>
        void ExecuteAndStart();
        /// <summary>
        /// Stops this action and waits if it still executing.
        /// </summary>
        void Stop();
    }
}
