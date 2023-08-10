using Sels.Core.Async.TaskManagement;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Sels.Core.Extensions;
using Sels.Core.Models;

namespace Sels.Core.Async.TaskManagement
{
    /// <summary>
    /// An managed task scheduled on the Thread Pool using a <see cref="ITaskManager"/>.
    /// </summary>
    public class ManagedTask : IManagedTask
    {
        // Fields
        private readonly CancellationTokenSource _cancellationSource = new CancellationTokenSource();
        private readonly TaskCompletionSource<bool> _callbackSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        // State
        private List<IManagedTask> _continuations;
        private List<IManagedAnonymousTask> _anonymousContinuations;
        private CancellationTokenRegistration _cancellationRegistration;

        /// <inheritdoc cref="ManagedTask"/>
        /// <param name="owner">The instance the managed task is tied to</param>
        /// <param name="name">Optional unique name for the task</param>
        /// <param name="input">Optional input for the work performed by the task</param>
        /// <param name="taskOptions">The options for this task</param>
        /// <param name="cancellationToken">Token that the caller can use to cancel the managed task</param>
        public ManagedTask(object owner, string? name, object input, ManagedTaskCreationOptions taskOptions, CancellationToken cancellationToken)
        {
            Owner = owner.ValidateArgument(nameof(owner));
            Name = name;
            Input = input;
            TaskOptions = taskOptions.ValidateArgument(nameof(taskOptions));

            // Register to cancellation
            CancellationToken = cancellationToken;
            _cancellationRegistration = cancellationToken.Register(Cancel);

            // Set task in cancelled state
            if (cancellationToken.IsCancellationRequested)
            {
                var cancelSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                Task = cancelSource.Task;

                var exception = new TaskCanceledException();
                Result = exception;
                cancelSource.SetException(exception);
                _callbackSource.SetResult(true);
            }
            else
            {
                // Schedule work on thread pool
                var scheduledTask = Task.Factory.StartNew(() => taskOptions.ExecuteDelegate(input, _cancellationSource.Token), _cancellationSource.Token, taskOptions.TaskCreationOptions & ~TaskCreationOptions.DenyChildAttach, TaskScheduler.Default).Unwrap();

                // Handle task result using continuation
                scheduledTask.ContinueWith(OnCompleted, TaskContinuationOptions.AttachedToParent);

                Task = scheduledTask;
            }
        }

        // Properties
        /// <summary>
        /// The options used to create the current instance.
        /// </summary>
        public ManagedTaskCreationOptions TaskOptions { get; }
        /// <summary>
        /// The cancellation token of the caller that created the first managed task in the chain.
        /// </summary>
        public CancellationToken CancellationToken { get; }
        /// <inheritdoc/>
        public object Owner { get; }
        /// <inheritdoc/>
        public string? Name { get; }
        /// <inheritdoc/>
        public object Input { get; }
        /// <inheritdoc/>
        public Task Task { get; }
        /// <inheritdoc/>
        public bool CancellationRequested => _cancellationSource.IsCancellationRequested;
        /// <inheritdoc/>
        public ManagedTaskOptions Options => TaskOptions.ManagedTaskOptions;
        /// <inheritdoc/>
        public IReadOnlyDictionary<string, object> Properties => TaskOptions.Properties;
        /// <inheritdoc/>
        public Task Callback => _callbackSource.Task;
        /// <inheritdoc/>
        public object? Result { get; private set; }

        /// <inheritdoc/>
        public IManagedTask[] Continuations => _continuations?.ToArray() ?? Array.Empty<IManagedTask>();
        /// <inheritdoc/>
        public IManagedAnonymousTask[] AnonymousContinuations => _anonymousContinuations?.ToArray() ?? Array.Empty<IManagedAnonymousTask>();

        /// <inheritdoc/>
        public void Cancel() => _cancellationSource.Cancel();
        /// <inheritdoc/>
        public void CancelAfter(TimeSpan delay) => _cancellationSource.CancelAfter(delay);

        private async Task OnCompleted(Task<object> completedTask)
        {
            // Get task result
            object result = null;
            try
            {
                var taskResult = await completedTask;
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
                // Trigger anonymous tasks
                foreach (var anonymousFactory in TaskOptions.AnonymousContinuationFactories)
                {
                    _anonymousContinuations ??= new List<IManagedAnonymousTask>();

                    _anonymousContinuations.Add(await anonymousFactory(this, Input, result, CancellationToken).ConfigureAwait(false));
                }

                // Trigger managed tasks
                foreach (var factory in TaskOptions.ContinuationFactories)
                {
                    _continuations ??= new List<IManagedTask>();

                    _continuations.Add(await factory(this, Input, result, CancellationToken).ConfigureAwait(false));
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                await _cancellationRegistration.DisposeAsync();
                _callbackSource.SetResult(true);
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            var totalContinuations = TaskOptions.ContinuationFactories.Length + TaskOptions.AnonymousContinuationFactories.Length;
            var currentContinuations = Continuations.Length + AnonymousContinuations.Length;
            return $"{(Name != null ? $"Managed task <{Name}>" : "Unnamed managed task")} owned by <{Owner}> <{Task.Id}>({Task.Status}){(totalContinuations > 0 ? $"[{currentContinuations}/{totalContinuations}]" : string.Empty)}: {TaskOptions.TaskCreationOptions} | {TaskOptions.ManagedTaskOptions} | {TaskOptions.NamePolicy}";
        }
    }
}
