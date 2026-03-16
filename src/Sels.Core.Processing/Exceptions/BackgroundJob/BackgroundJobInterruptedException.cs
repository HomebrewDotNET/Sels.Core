using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Processing.BackgroundJob
{
    /// <summary>
    /// Throw to interrupt the execution of a background job to return a <see cref="IBackgroundJobResult"/>.
    /// </summary>
    public class BackgroundJobInterruptedException : Exception
    {
        /// <summary>
        /// The result to interrupt the background job with.
        /// </summary>
        public IBackgroundJobResult Result { get; }

        /// <inheritdoc cref="BackgroundJobInterruptedException"/>
        /// <param name="result"><inheritdoc cref="Result"/></param>
        public BackgroundJobInterruptedException(IBackgroundJobResult result) : base($"Interrupted background job execution with result of type <{result?.GetType()}>")
        {
            Result = Guard.IsNotNull(result);
        }
    }
}
