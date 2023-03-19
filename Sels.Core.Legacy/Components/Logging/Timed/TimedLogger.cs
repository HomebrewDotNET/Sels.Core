using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Sels.Core.Components.Logging
{
    /// <summary>
    /// Logger that keeps track of the elapsed time since it was started. Allows for logging how long certain actions took to execute.
    /// </summary>
    public abstract class TimedLogger : IDisposable
    {
        // Abstractions
        /// <summary>
        /// Used to log a message when the timed logger is still running.
        /// </summary>
        /// <param name="loggingAction">Delegate to log something using the internal loggers. Arg is the currently elapsed time since starting the timed logger</param>
        public abstract void Log(Action<TimeSpan, IEnumerable<ILogger>> loggingAction);
        /// <summary>
        /// Used to overwrite the default end message. Internal timer is stopped. Is the same as <see cref="Dispose"/>.
        /// </summary>
        /// <param name="loggingAction">Delegate to log something using the internal loggers. Arg is the currently elapsed time since starting the timed logger</param>
        public abstract void EndLog(Action<TimeSpan, IEnumerable<ILogger>> loggingAction);

        // Virtuals
        /// <summary>
        /// Logs the end message.
        /// </summary>
        public abstract void Dispose();
    }
}
