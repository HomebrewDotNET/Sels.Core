using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Processing.BackgroundJob
{
    /// <summary>
    /// Transaction created when scheduling a <see cref="IBackgroundJob"/>.
    /// </summary>
    public interface IBackgroundJobTransaction : IAsyncDisposable
    {
        /// <summary>
        /// The id of the current job.
        /// </summary>
        public string JobId { get; }

        /// <summary>
        /// Schedules the current background job on <paramref name="queue"/>.
        /// </summary>
        /// <param name="queue">The queue to place the job in</param>
        /// <param name="priority">The priority of the job within <paramref name="queue"/></param>
        /// <returns>Current transaction for method chaining</returns>
        IBackgroundJobTransaction OnQueue(string queue, int? priority = null);
        /// <summary>
        /// Shedules the current background job on the default <see cref="ProcessingConstants.BackgroundProcessing.DefaultQueue"/>.
        /// </summary>
        /// <param name="priority">The priority of the job within <see cref="ProcessingConstants.BackgroundProcessing.DefaultQueue"/></param>
        /// <returns>Current transaction for method chaining</returns>
        IBackgroundJobTransaction OnDefaultQueue(int? priority);
        /// <summary>
        /// Schedules the current background job on the <see cref="ProcessingConstants.BackgroundProcessing.NodeQueue"/> for node with <paramref name="nodeId"/>.
        /// </summary>
        /// <param name="nodeId">The id of the node to schedule the background job on</param>
        /// <param name="priority">The priority of the job within <see cref="ProcessingConstants.BackgroundProcessing.NodeQueue"/></param>
        /// <returns>Current transaction for method chaining</returns>
        IBackgroundJobTransaction OnNode(string nodeId, int? priority = null);
        /// <summary>
        /// Schedules the current background job on the current node that is scheduling the background job. Only works if the current process is a worker node.
        /// </summary>
        /// <param name="priority">The priority of the job within <see cref="ProcessingConstants.BackgroundProcessing.NodeQueue"/></param>
        /// <returns>Current transaction for method chaining</returns>
        IBackgroundJobTransaction OnSelf(int? priority = null);

        /// <summary>
        /// Shedules the current background job with a delay. The job will only be picked up after <paramref name="delay"/> when the job is commited.
        /// </summary>
        /// <param name="delay">How much to delay the processing by</param>
        /// <returns>Current transaction for method chaining</returns>
        IBackgroundJobTransaction WithDelay(TimeSpan delay);

        /// <summary>
        /// Adds a parameter with value <paramref name="data"/> to the background job. Background jobs can access this data using <see cref="IBackgroundJobState.Get{T}(string)"/>.
        /// </summary>
        /// <param name="parameter">The name of the parameter to add</param>
        /// <param name="data">The value for the parameters. Should be serializable to json</param>
        /// <returns>Current transaction for method chaining</returns>
        IBackgroundJobTransaction WithParameter(string parameter, object data);

        /// <summary>
        /// The current background job will only execute when the job with id <paramref name="parentJobId"/> finishes processing. Useful when background jobs chain queue each other.
        /// </summary>
        /// <param name="parentJobId">The id of the background job to wait on</param>
        /// <param name="onlyOnSuccess">True if the current background job can only execute if the parent job successfully executes, when set to false the current job will also execute even if the parent job fails</param>
        /// <returns>Current transaction for method chaining</returns>
        IBackgroundJobTransaction AfterParentJob(string parentJobId, bool onlyOnSuccess = true);

        /// <summary>
        /// Commits the current job so it can be processed by the workers nodes.
        /// </summary>
        Task CommitAsync();
    }
}
