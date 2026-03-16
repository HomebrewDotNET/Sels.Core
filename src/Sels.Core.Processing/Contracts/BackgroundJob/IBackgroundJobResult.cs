using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sels.Core.Processing.BackgroundJob
{
    /// <summary>
    /// Allows jobs to return specially typed results that allows the worker node to change the state of the background job after execution. Some examples for what it can be used for: Delaying the current job, failing the current job to avoid retries, deleting the job because it didn't process anything, ...
    /// </summary>
    public interface IBackgroundJobResult
    {
        /// <summary>
        /// The actual result of the background job.
        /// </summary>
        public object ActualResult { get;  }
        /// <summary>
        /// The reason for the result.
        /// </summary>
        public string Reason { get;  }
    }
}
