using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.Logging
{
    public abstract class TimedLogger : IDisposable
    {
        // Abstractions
        public abstract void Log(Action<TimeSpan, IEnumerable<ILogger>> loggingAction);

        public abstract void EndLog(Action<TimeSpan, IEnumerable<ILogger>> loggingAction);

        // Virtuals
        public virtual void Dispose()
        {
            
        }
    }
}
