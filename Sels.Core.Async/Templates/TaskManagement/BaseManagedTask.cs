using Sels.Core.Async.TaskManagement;
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
    /// Base class for creating managed task wrappers.
    /// </summary>
    public abstract class BaseManagedTask : IManagedAnonymousTask, IAsyncDisposable
    {
        // Fields
        private readonly ManagedTaskCreationSharedOptions _taskOptions;
        private readonly CancellationTokenSource _cancellationSource = new CancellationTokenSource();

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
        }

        /// <summary>
        /// Schedules the task on the thread pool.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public virtual void Start()
        {
            lock (_cancellationSource)
            {
                if (Task != null) throw new InvalidOperationException($"Task already started");

                // Schedule work on thread pool
                var scheduledTask = Task.Factory.StartNew(async () =>
                {
                    _cancellationSource.Token.ThrowIfCancellationRequested();
                    StartedDate = DateTime.Now;

                    try
                    {
                        using (Helper.Time.CaptureDuration(x => Duration = x))
                        {
                            return await _taskOptions.ExecuteDelegate(_cancellationSource.Token);
                        }
                    }
                    finally
                    {
                        FinishedDate = DateTime.Now;
                    }
                }, _cancellationSource.Token, _taskOptions.TaskCreationOptions & ~TaskCreationOptions.AttachedToParent, TaskScheduler.Default).Unwrap();

                Task = scheduledTask;
                // Handle task result using continuation
                OnExecuted = scheduledTask.ContinueWith(OnCompleted);
            }
        }

        /// <inheritdoc/>
        /// 
        public CancellationToken Token { get; }
        /// <inheritdoc/>
        public Task Task { get; private set; }
        /// <inheritdoc/>
        public bool CancellationRequested { get { lock (_cancellationSource) { return _cancellationSource.IsCancellationRequested; } } }
        /// <inheritdoc/>
        public ManagedTaskOptions Options => _taskOptions.ManagedTaskOptions;
        /// <inheritdoc/>
        public IReadOnlyDictionary<string, object> Properties => _taskOptions.Properties;
        /// <inheritdoc/>
        public Task OnExecuted { get; private set; }
        /// <inheritdoc/>
        public Task OnFinalized { get; protected set; }
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
        public void Cancel() { lock (_cancellationSource) { _cancellationSource.Cancel(); } }
        /// <inheritdoc/>
        public void CancelAfter(TimeSpan delay) { lock (_cancellationSource) { _cancellationSource.CancelAfter(delay); } }

        private async Task OnCompleted(Task<object> completedTask)
        {
            // Get task result
            object result = null;
            try
            {
                var taskResult = await completedTask.ConfigureAwait(false);
                if (taskResult != null && taskResult != Null.Value) result = taskResult;
            }
            catch (Exception ex)
            {
                result = ex;
            }
            Result = result;

            await TriggerContinuations().ConfigureAwait(false);
        } 

        /// <summary>
        /// Trigger the continuations for this task if there are any defined.
        /// </summary>
        protected abstract Task TriggerContinuations();
        
        /// <inheritdoc/>
        public ValueTask DisposeAsync()
        {
            lock (_cancellationSource)
            {
                if (!_cancellationSource.IsCancellationRequested) _cancellationSource.Cancel();
            }

            return _cancellationRegistration.DisposeAsync();
        }
    }
}
