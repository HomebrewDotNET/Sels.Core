using Sels.Core.Processing.BackgroundJob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Processing.BackgroundJob
{
    /// <summary>
    ///  Signals the worker node that it should place the current job in a different queue.
    /// </summary>
    public class RequeueJobResult : IBackgroundJobResult
    {
        /// <summary>
        /// The new queue for the current job.
        /// </summary>
        public string Queue { get; }
        /// <summary>
        /// The new priority for the current job in <see cref="Queue"/>.
        /// </summary>
        public int? Priority { get; }
        /// <inheritdoc/>
        public object ActualResult { get; init; }
        /// <inheritdoc/>
        public string Reason { get; init; }

        /// <inheritdoc cref="RequeueJobResult"/>
        /// <param name="queue"><inheritdoc cref="Queue"/></param>
        /// <param name="priority"><inheritdoc cref="Priority"/></param>
        public RequeueJobResult(string queue, int? priority)
        {
            Queue = Guard.IsNotNullOrWhitespace(queue);
            Priority = priority.HasValue ? Guard.IsLargerOrEqual(priority.Value, 1) : null;
        }
    }
}
