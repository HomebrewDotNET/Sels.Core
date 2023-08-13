using Sels.Core.Async.TaskManagement;
using Sels.Core.Extensions;
using Sels.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sels.Core.Async.TaskManagement
{
    /// <summary>
    /// Base class for creating managed task wrappers.
    /// </summary>
    public abstract class BaseManagedTask : IManagedAnonymousTask
    {
        // Fields
        private readonly ManagedTaskCreationSharedOptions _taskOptions;
        private readonly CancellationTokenSource _cancellationSource = new CancellationTokenSource();
        private readonly TaskCompletionSource<bool> _callbackSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource<bool> _finalizeSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        // State
        /// <summary>
        /// Any continuations that created managed tasks. 
        /// </summary>
        protected List<IManagedTask> _continuations;
        /// <summary>
        /// Any continuations that created managed anonymous tasks. 
        /// </summary>
        protected List<IManagedAnonymousTask> _anonymousContinuations;
        private CancellationTokenRegistration _cancellationRegistration;

        /// <inheritdoc cref="BaseManagedTask"/>
        /// <param name="taskOptions">The options for this task</param>
        /// <param name="cancellationToken">Token that the caller can use to cancel the managed task</param>
        public BaseManagedTask(ManagedTaskCreationSharedOptions taskOptions, CancellationToken cancellationToken)
        {
            _taskOptions = taskOptions.ValidateArgument(nameof(taskOptions));

            // Register to cancellation
            Token = cancellationToken;
            _cancellationRegistration = cancellationToken.Register(Cancel);

            // Set task in faulted state
            if (cancellationToken.IsCancellationRequested)
            {
                var exception = new TaskCanceledException();
                Result = exception;
                Task = Task.FromException(exception);
                _callbackSource.SetResult(true);
            }
            else
            {
                // Schedule work on thread pool
                var scheduledTask = Task.Factory.StartNew(async () =>
                {
                    StartedDate = DateTime.Now;

                    try
                    {
                        using (Helper.Time.CaptureDuration(x => Duration = x))
                        {
                            return await taskOptions.ExecuteDelegate(_cancellationSource.Token); 
                        }
                    }
                    finally
                    {
                        FinishedDate = DateTime.Now;
                    }
                }, _cancellationSource.Token, taskOptions.TaskCreationOptions, TaskScheduler.Default);

                // Handle task result using continuation
                scheduledTask.ContinueWith(OnCompleted, TaskContinuationOptions.AttachedToParent);

                Task = scheduledTask;
            }
        }

        /// <inheritdoc/>
        public CancellationToken Token { get; }
        /// <inheritdoc/>
        public Task Task { get; }
        /// <inheritdoc/>
        public bool CancellationRequested => _cancellationSource.IsCancellationRequested;
        /// <inheritdoc/>
        public ManagedTaskOptions Options => _taskOptions.ManagedTaskOptions;
        /// <inheritdoc/>
        public IReadOnlyDictionary<string, object> Properties => _taskOptions.Properties;
        /// <inheritdoc/>
        public Task OnExecuted => _callbackSource.Task;
        /// <inheritdoc/>
        public Task OnFinalized => _finalizeSource.Task;
        /// <inheritdoc/>
        public object? Result { get; private set; }
        /// <inheritdoc/>
        public DateTime CreatedDate { get; } = DateTime.Now;
        /// <inheritdoc/>
        public DateTime? StartedDate { get; private set; }
        /// <inheritdoc/>
        public TimeSpan? Duration { get; private set; }
        /// <inheritdoc/>
        public DateTime? FinishedDate { get; private set; }


        /// <inheritdoc/>
        public IManagedTask[] Continuations => _continuations?.ToArray() ?? Array.Empty<IManagedTask>();
        /// <inheritdoc/>
        public IManagedAnonymousTask[] AnonymousContinuations => _anonymousContinuations?.ToArray() ?? Array.Empty<IManagedAnonymousTask>();

        /// <inheritdoc/>
        public void Cancel() => _cancellationSource.Cancel();
        /// <inheritdoc/>
        public void CancelAfter(TimeSpan delay) => _cancellationSource.CancelAfter(delay);
        /// <summary>
        /// Completes <see cref="OnFinalized"/>.
        /// </summary>
        public void FinalizeTask() => _finalizeSource.SetResult(true);

        private async Task OnCompleted(Task<Task<object>> completedTask)
        {
            // Get task result
            object result = null;
            try
            {
                var task = await completedTask.ConfigureAwait(false);
                var taskResult = await task.ConfigureAwait(false);
                if (taskResult != null && taskResult != Null.Value) result = taskResult;
            }
            catch (Exception ex)
            {
                result = ex;
            }
            Result = result;

            // Trigger continuations
            try
            {
                await TriggerContinuations();
            }
            catch
            {
                throw;
            }
            finally
            {
                await _cancellationRegistration.DisposeAsync().ConfigureAwait(false);
                _callbackSource.SetResult(true);
            }
        }

        /// <summary>
        /// Trigger the continuations for this task if there are any defined.
        /// </summary>
        protected abstract Task TriggerContinuations();
    }
}
