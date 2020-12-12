using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.Logging
{
    public class NullTimedLogger : TimedLogger
    {
        public override void EndLog(Action<TimeSpan, IEnumerable<ILogger>> loggingAction)
        {
           
        }

        public override void Log(Action<TimeSpan, IEnumerable<ILogger>> loggingAction)
        {
            
        }

    }
}
