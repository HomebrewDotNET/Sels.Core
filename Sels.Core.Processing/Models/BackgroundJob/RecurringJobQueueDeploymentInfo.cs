using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Processing.BackgroundJob
{
    /// <summary>
    /// Contains the information needed to deploy a recurring job with queue information.
    /// </summary>
    public class RecurringJobQueueDeploymentInfo : RecurringJobDeploymentInfo
    {
        /// <summary>
        /// The queue that scheduled background jobs will be placed in.
        /// </summary>
        public string Queue { get; init; }
        /// <summary>
        /// The priority of scheduled background jobs in <see cref="Queue"/>.
        /// </summary>
        public int? Priority { get; init; }
    }
}
