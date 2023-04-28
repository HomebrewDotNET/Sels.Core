using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Processing.BackgroundJob
{
    /// <summary>
    /// Indicates the current status of the job.
    /// </summary>
    public enum BackgroundJobStatus
    {
        /// <summary>
        /// Job is waiting to be picked up.
        /// </summary>
        Queued = 0,
        /// <summary>
        /// Job is waiting to be picked up at a later date.
        /// </summary>
        Scheduled = 1,
        /// <summary>
        /// Job is currently being executed.
        /// </summary>
        Executing = 2,
        /// <summary>
        /// Job could not properly execute.
        /// </summary>
        Failed = 3,
        /// <summary>
        /// Job succesfully executed.
        /// </summary>
        Succeeded = 4,
        /// <summary>
        /// Job is marked for deletion and will be permanently removed after the configured retention.
        /// </summary>
        Deleted = 5
    }
}
