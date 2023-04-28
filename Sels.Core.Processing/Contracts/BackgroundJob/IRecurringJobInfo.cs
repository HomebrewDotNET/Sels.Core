using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Processing.BackgroundJob
{
    /// <summary>
    /// Contains information about a recurring background job with it's current state.
    /// </summary>
    public interface IRecurringJobInfo
    {
        /// <summary>
        /// The id of the recurring job.
        /// </summary>
        string Id { get; }
        /// <summary>
        /// The type of the background job that the recurring jobs queues.
        /// </summary>
        Type JobType { get; }
        /// <summary>
        /// The type of the recurring job. Indicates how it is deployed.
        /// </summary>
        RecurringJobType RecurringJobType { get; }
        /// <summary>
        /// The node the recurring job is deployed on when <see cref="RecurringJobType"/> is set to <see cref="RecurringJobType.Node"/>.
        /// </summary>
        string NodeId { get; }
        /// <summary>
        /// The cron schedule of the background job.
        /// </summary>
        string Schedule { get; }
        /// <summary>
        /// The last time the recurring job queued a background job.
        /// </summary>
        DateTimeOffset? LastQueueDate { get; }
        /// <summary>
        /// The id of the last created background job.
        /// </summary>
        string LastJobId { get; }
        /// <summary>
        /// The time that the next background job will be queued.
        /// </summary>
        DateTimeOffset NextQueueDate { get; }

        /// <summary>
        /// Fetches the current state of the recurring job and updates the current object.
        /// </summary>
        Task RefreshAsync();
        /// <summary>
        /// Deletes the current recurring job. Only allowed when <see cref="RecurringJobType"/> is <see cref="RecurringJobType.User"/>.
        /// </summary>
        /// <param name="deleteRunningJob">If there is currently a job running created by the current recurring job it will be cancelled and deleted when set to true</param>
        Task DeleteAsync(bool deleteRunningJob = true);
    }
}
