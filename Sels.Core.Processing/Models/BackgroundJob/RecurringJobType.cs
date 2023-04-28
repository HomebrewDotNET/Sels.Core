using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Processing.BackgroundJob
{
    /// <summary>
    /// The different types of recurring jobs.
    /// </summary>
    public enum RecurringJobType
    {
        /// <summary>
        /// Recurring jobs that are part of the system and deployed each time a worker nodes starts up. Each node should deploy the same system jobs.
        /// </summary>
        System = 0,
        /// <summary>
        /// Recurring jobs that are tied to a certain node. Will always enqueue jobs on the node it's deployed on.
        /// </summary>
        Node = 1,
        /// <summary>
        /// Manually deployed recurring jobs.
        /// </summary>
        User = 2
    }
}
