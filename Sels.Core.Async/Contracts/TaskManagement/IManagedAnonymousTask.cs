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
        /// The cancellation token used to cancel the current task.
        /// </summary>
        CancellationToken Token { get; }
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
        /// Task that will complete when the current task finishes executing and all continuations are triggered.
        /// </summary>
        Task OnExecuted { get; }
        /// <summary>
        /// Task that will complete when the task manager has handled the execution of the managed (anonymous) task.
        /// </summary>
        Task OnFinalized { get; }
        /// <summary>
        /// The result from executing <see cref="Task"/>. Will be the return value from the task if it executed successfully, null if executed successfully but doesn't return a value or the exception if it failed.
        /// </summary>
        object? Result { get; }
        /// <summary>
        /// The date the managed (anonymous) task was created and scheduled.
        /// </summary>
        DateTime CreatedDate { get; }
        /// <summary>
        /// The date the managed (anonymous) task started executing.
        /// </summary>
        DateTime? StartedDate { get; }
        /// <summary>
        /// How long the managed (anonymous) task executed.
        /// </summary>
        TimeSpan? Duration { get; }
        /// <summary>
        /// The date the managed (anonymous) task finished executing.
        /// </summary>
        DateTime? FinishedDate { get; }

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
