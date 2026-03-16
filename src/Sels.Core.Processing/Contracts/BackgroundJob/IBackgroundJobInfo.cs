using Sels.Core.Processing.BackgroundJob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Processing.BackgroundJob
{
    /// <summary>
    /// Contains information about a background job with it's current state.
    /// </summary>
    public interface IBackgroundJobInfo : IBackgroundJobContext
    {
        /// <summary>
        /// The type of the background job.
        /// </summary>
        Type JobType { get; }
        /// <summary>
        /// The current status of the job.
        /// </summary>
        BackgroundJobStatus Status { get; }
        /// <summary>
        /// The date the current job will be able to be executed. Only set when <see cref="Status"/> is set to <see cref="BackgroundJobStatus.Scheduled"/>.
        /// </summary>
        DateTimeOffset? EnqueueDate { get; }

        /// <summary>
        /// True if the job either succesfully executed or failed with no retries left.
        /// </summary>
        bool IsCompleted => Status == BackgroundJobStatus.Failed || Status == BackgroundJobStatus.Succeeded;
        /// <summary>
        /// True if the job is currently idle and waiting to be executed.
        /// </summary>
        bool IsPending => Status == BackgroundJobStatus.Queued || Status == BackgroundJobStatus.Scheduled;

        /// <summary>
        /// Requeues the current job so it can be executed again.
        /// </summary>
        /// <returns>Transaction to commit the background job.</returns>
        IBackgroundJobTransaction Requeue();

        /// <summary>
        /// Fetches the current state of the job and updates the current object.
        /// </summary>
        Task RefreshAsync();
        /// <summary>
        /// Will cancel the job if it's running and will go in a failed status.
        /// </summary>
        Task CancelAsync();
        /// <summary>
        /// Deletes the current job.
        /// </summary>
        Task DeleteAsync();
    }
}
