using Castle.Core.Logging;
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
    /// An anonymous task scheduled on the Thread Pool using a <see cref="ITaskManager"/>.
    /// </summary>
    public class ManagedAnonymousTask : BaseManagedTask
    {
        // Fields
        private AsyncAction<ManagedAnonymousTask> _finalizeAction;

        // Properties
        /// <summary>
        /// The options used to create the current instance.
        /// </summary>
        public ManagedAnonymousTaskCreationOptions TaskOptions { get; }

        /// <inheritdoc cref="ManagedAnonymousTask"/>
        /// <param name="taskOptions">The options for this task</param>
        /// <param name="finalizeAction">The delegate to call to finalize the task</param>
        /// <param name="cancellationToken">Token that the caller can use to cancel the managed task</param>
        public ManagedAnonymousTask(ManagedAnonymousTaskCreationOptions taskOptions, AsyncAction<ManagedAnonymousTask> finalizeAction, CancellationToken cancellationToken) : base(taskOptions, cancellationToken)
        {
            _finalizeAction = finalizeAction.ValidateArgument(nameof(finalizeAction));
            TaskOptions = taskOptions.ValidateArgument(nameof(taskOptions));

            
        }

        /// <inheritdoc/>
        public override void Start()
        {
            base.Start();
            OnFinalized = OnExecuted.ContinueWith(x => _finalizeAction(this));
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            var totalContinuations = TaskOptions.ContinuationFactories.Length + TaskOptions.AnonymousContinuationFactories.Length;
            var currentContinuations = Continuations.Length + AnonymousContinuations.Length;
            return $"Anonymous managed task <{Task.Id}>({Task.Status}){(totalContinuations > 0 ? $"[{currentContinuations}/{totalContinuations}]" : string.Empty)}: {TaskOptions.TaskCreationOptions} | {TaskOptions.ManagedTaskOptions}";
        }
        /// <inheritdoc/>
        protected override async Task TriggerContinuations()
        {
            // Trigger anonymous tasks
            if (TaskOptions.AnonymousContinuationFactories.HasValue())
            {
                foreach (var anonymousFactory in TaskOptions.AnonymousContinuationFactories)
                {
                    _anonymousContinuations ??= new List<IManagedAnonymousTask>();
                    var task = await anonymousFactory(this, Result, Token).ConfigureAwait(false);
                    if (task != null) _anonymousContinuations.Add(task);
                } 
            }

            // Trigger managed tasks
            if (TaskOptions.ContinuationFactories.HasValue())
            {
                foreach (var factory in TaskOptions.ContinuationFactories)
                {
                    _continuations ??= new List<IManagedTask>();
                    var task = await factory(this, Result, Token).ConfigureAwait(false);
                    if (task != null) _continuations.Add(task);
                } 
            }
        }
    }
}
