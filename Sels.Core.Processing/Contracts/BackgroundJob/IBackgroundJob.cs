using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sels.Core.Processing.BackgroundJob
{
    /// <summary>
    /// A stateful background job that can be scheduled on a queue to process something.
    /// </summary>
    public interface IBackgroundJob
    {
        /// <summary>
        /// Allows a job to gain information about itself when executing.
        /// </summary>
        IBackgroundJobContext Context { get; set; }
        /// <inheritdoc cref="IBackgroundJobState"/>
        IBackgroundJobState State { get; set; }
        /// <summary>
        /// Auto injected logger for the current background job. Can be null if logging isn't configured.
        /// </summary>
        ILogger Logger { get; set; } 

        /// <summary>
        /// Executes the current background job.
        /// </summary>
        /// <param name="input">Optional serialized input for the background job</param>
        /// <param name="token">Cancellation token that will be cancelled if the job is requested to stop processing</param>
        /// <returns>Optional result from executing the background job.</returns>
        Task<object> ExecuteAsync(string input, CancellationToken token);
    }
}
