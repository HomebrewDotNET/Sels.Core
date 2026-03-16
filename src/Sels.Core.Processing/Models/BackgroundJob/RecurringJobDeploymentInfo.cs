using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Processing.BackgroundJob
{
    /// <summary>
    /// Contains the information needed to deploy a recurring job.
    /// </summary>
    public class RecurringJobDeploymentInfo
    {
        /// <summary>
        /// The id of the recurring job to deploy.
        /// </summary>
        public string Id { get; init; }
        /// <summary>
        /// The cron schedule of the recurring job.
        /// </summary>
        public string Schedule { get; init; }
        /// <summary>
        /// The type of the background job that the recurring job will schedule.
        /// </summary>
        public Type JobType { get; init; }
        /// <summary>
        /// Optional input for the scheduled background jobs.
        /// </summary>
        public object Input { get; init; }
    }
}
