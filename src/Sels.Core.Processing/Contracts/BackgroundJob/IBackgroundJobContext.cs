using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Processing.BackgroundJob
{
    /// <summary>
    /// Contains information about a <see cref="IBackgroundJob"/>.
    /// </summary>
    public interface IBackgroundJobContext
    {
        /// <summary>
        /// The unique id of background job.
        /// </summary>
        string Id { get; }
        /// <summary>
        /// The id of the recurring job that created the current instance. Will be null if the job was manually queued.
        /// </summary>
        string RecurringJobId { get; }
        /// <summary>
        /// The queue that the job was placed in.
        /// </summary>
        string Queue { get; }
        /// <summary>
        /// The current priority of the job in <see cref="Queue"/>.
        /// </summary>
        int? Priority { get; }
        /// <summary>
        /// The unique id (when on the same node) of the last worker that executed the background job.
        /// </summary>
        string WorkerId { get; }
        /// <summary>
        /// Can either be the node the job was queued on if the job is still pending on a local queue or the last node that processed the job.
        /// </summary>
        string NodeId { get; }
        /// <summary>
        /// The date the current job was created.
        /// </summary>
        DateTime CreationDate { get; }
        /// <summary>
        /// The current retry count that indicates how many times the job failed before. Will be 0 when executing for the first time.
        /// </summary>
        int CurrentRetryCount { get; }
        /// <summary>
        /// The configured max retry count for jobs.
        /// </summary>
        int MaxRetryCount { get; }
        /// <summary>
        /// True if the current job is on it's last retry meaning if it fails gain it won't be automatically requeued, otherwise false.
        /// </summary>
        bool IsOnLastRetry => CurrentRetryCount >= MaxRetryCount;
    }
}
