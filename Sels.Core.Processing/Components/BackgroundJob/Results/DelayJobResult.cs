using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Processing.BackgroundJob
{
    /// <summary>
    /// Signals the worker node that it should delay the execution of the current background job.
    /// </summary>
    public class DelayJobResult : RequeueJobResult
    {
        /// <summary>
        /// How much to delay the execution for.
        /// </summary>
        TimeSpan Delay { get; }

        /// <inheritdoc cref="DelayJobResult"/>
        /// <param name="delay"><inheritdoc cref="Delay"/></param>
        /// <param name="queue"><inheritdoc cref="RequeueJobResult.Queue"/></param>
        /// <param name="priority"><inheritdoc cref="RequeueJobResult.Priority"/></param>
        public DelayJobResult(TimeSpan delay, string queue, int? priority) : base(queue, priority)
        {
            Delay = delay;
        }
    }
}
