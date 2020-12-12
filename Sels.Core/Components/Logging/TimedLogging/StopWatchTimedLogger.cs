using Microsoft.Extensions.Logging;
using Sels.Core.Extensions.General.Generic;
using Sels.Core.Extensions.General.Validation;
using Sels.Core.Extensions.Logging;
using Sels.Core.Extensions.Object;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Sels.Core.Components.Logging
{
    public class StopWatchTimedLogger : TimedLogger
    {
        // Fields
        private readonly LogLevel _logLevel;
        private readonly ILogger[] _loggers;
        private readonly Stopwatch _stopWatch;

        private readonly object _threadLock = new object();

        private bool _isDisposed;

        // Delegates
        private readonly Func<TimeSpan, string> _endMessageFunc;

        public StopWatchTimedLogger(ILogger logger, LogLevel logLevel, Func<string> beginMessageFunc, Func<TimeSpan, string> endMessageFunc) : this(logger.ItemToArrayOrDefault(), logLevel, beginMessageFunc, endMessageFunc)
        {

        }

        public StopWatchTimedLogger(IEnumerable<ILogger> loggers, LogLevel logLevel, Func<string> beginMessageFunc, Func<TimeSpan, string> endMessageFunc)
        {
            beginMessageFunc.ValidateVariable(nameof(beginMessageFunc));
            endMessageFunc.ValidateVariable(nameof(endMessageFunc));

            _logLevel = logLevel;
            _endMessageFunc = endMessageFunc;
            
            if (loggers.HasValue(logLevel))
            {
                _loggers = loggers.ToArray();
                _stopWatch = new Stopwatch();
                _stopWatch.Start();

                Log(beginMessageFunc());
            }
        }

        private void Log(string message)
        {
            _loggers.LogMessage(_logLevel, message);
        }

        public override void Log(Action<TimeSpan, IEnumerable<ILogger>> loggingAction)
        {
            loggingAction.ValidateVariable(nameof(loggingAction));

            loggingAction(_stopWatch.Elapsed, _loggers);
        }

        public override void Dispose()
        {
            EndLog((x, y) => y.LogMessage(_logLevel, _endMessageFunc(x)));
        }

        public override void EndLog(Action<TimeSpan, IEnumerable<ILogger>> loggingAction)
        {
            try
            {
                lock (_threadLock)
                {
                    if (!_isDisposed)
                    {
                        if (_stopWatch.HasValue())
                        {
                            _stopWatch.Stop();

                            Log(loggingAction);
                        }

                        _isDisposed = true;
                    }
                }
            }
            catch { }        
        }
    }
}
