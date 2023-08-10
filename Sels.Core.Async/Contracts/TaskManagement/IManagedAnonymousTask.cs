using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sels.Core.Async.TaskManagement
{
    /// <summary>
    /// Represents an anonymous task that was scheduled using a <see cref="ITaskManager"/>.
    /// </summary>
    public interface IManagedAnonymousTask
    {
        /// <summary>
        /// The input for <see cref="Task"/> if one was provided when scheduling the task.
        /// </summary>
        object? Input { get; }
        /// <summary>
        /// The task that was scheduled on the Thread Pool.
        /// </summary>
        Task Task { get; }
        /// <summary>
        /// If cancellation for the managed task was requested.
        /// </summary>
        bool CancellationRequested { get; }
        /// <summary>
        /// The options used when creating the task.
        /// </summary>
        ManagedTaskOptions Options { get; }
        /// <summary>
        /// Any properties added to <see cref="Task"/>.
        /// </summary>
        IReadOnlyDictionary<string, object> Properties { get; }

        /// <summary>
        /// Task that will complete when the current task finishes executing.
        /// </summary>
        Task Callback { get; }
        /// <summary>
        /// The result from executing <see cref="Task"/>. Will be the return value from the task if it executed successfully, null if executed successfully but doesn't return a value or the exception if it failed.
        /// </summary>
        object? Result { get; }

        /// <summary>
        /// If this task hould be kept alive if it fails with an exception.
        /// </summary>
        bool KeepAlive => Options.HasFlag(ManagedTaskOptions.KeepAlive);
        /// <summary>
        /// If this task can be cancelled instantly when graceful cancellation is requested.
        /// </summary>
        bool FastCancellation => Options.HasFlag(ManagedTaskOptions.GracefulCancellation);

        /// <summary>
        /// Any continuations that were created when the current task finished executing. Will be empty array if none were triggered.
        /// </summary>
        IManagedTask[] Continuations { get; }
        /// <summary>
        /// Any anonymous continuations that were created when the current task finished executing. Will be empty array if none were triggered.
        /// </summary>
        IManagedAnonymousTask[] AnonymousContinuations { get; }

        /// <summary>
        /// Cancels the task if it is running.
        /// </summary>
        void Cancel();
        /// <summary>
        /// Cancels the task after <paramref name="delay"/> if it is still running by then.
        /// </summary>
        /// <param name="delay">How long to wait before cancelling</param>
        void CancelAfter(TimeSpan delay);
    }
}
