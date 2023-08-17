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
    public class ManagedTask : BaseManagedTask, IManagedTask
    {
        /// <inheritdoc cref="ManagedTask"/>
        /// <param name="owner">The instance the managed task is tied to</param>
        /// <param name="name">Optional unique name for the task</param>
        /// <param name="taskOptions">The options for this task</param>
        /// <param name="cancellationToken">Token that the caller can use to cancel the managed task</param>
        public ManagedTask(object owner, string? name, ManagedTaskCreationOptions taskOptions, CancellationToken cancellationToken) : base(taskOptions, cancellationToken)
        {
            Owner = owner.ValidateArgument(nameof(owner));
            Name = name;
            TaskOptions = taskOptions.ValidateArgument(nameof(taskOptions));

            _startSource.SetResult(true);
        }

        // Properties
        /// <summary>
        /// The options used to create the current instance.
        /// </summary>
        public ManagedTaskCreationOptions TaskOptions { get; }
        /// <inheritdoc/>
        public object Owner { get; }
        /// <inheritdoc/>
        public string? Name { get; }

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

        /// <inheritdoc/>
        public override string ToString()
        {
            var totalContinuations = TaskOptions.ContinuationFactories.Length + TaskOptions.AnonymousContinuationFactories.Length;
            var currentContinuations = Continuations.Length + AnonymousContinuations.Length;
            return $"{(Name != null ? $"Managed task <{Name}>" : "Unnamed managed task")} owned by <{Owner}> <{Task.Id}>({Task.Status}){(totalContinuations > 0 ? $"[{currentContinuations}/{totalContinuations}]" : string.Empty)}: {TaskOptions.TaskCreationOptions} | {TaskOptions.ManagedTaskOptions} | {TaskOptions.NamePolicy}";
        }
    }
}
