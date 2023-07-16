using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Sels.Core.Logging
{
    /// <summary>
    /// Implements <see cref="TimedLogger"/> using empty methods.
    /// </summary>
    public class NullTimedLogger : TimedLogger
    {
        /// <inheritdoc/>
        public override void Dispose()
        {
            
        }

        /// <inheritdoc/>
        public override void EndLog(Action<TimeSpan, IEnumerable<ILogger>> loggingAction)
        {
           
        }
        /// <inheritdoc/>
        public override void Log(Action<TimeSpan, IEnumerable<ILogger>> loggingAction)
        {
            
        }

    }
}
