using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Processing
{
    /// <summary>
    /// Contains constants related to background processing
    /// </summary>
    public static class ProcessingConstants
    {
        /// <summary>
        /// Contains constants related to the BackgroundJob domain.
        /// </summary>
        public static class BackgroundProcessing
        {
            /// <summary>
            /// The special queue name that indicates that a background job is placed on the queue of a specific node. This queue has the highest priority and each node always has it's own unique queue.
            /// </summary>
            public const string NodeQueue = "Local";
            /// <summary>
            /// The default implicit queue for all background jobs when no custom queue is provided. This queue has the lowest priority and is always used by all workers nodes.
            /// </summary>
            public const string DefaultQueue = "Global";
        }
    }
}
