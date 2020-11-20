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
    public class TimedLogger : IDisposable
    {
        // Fields
        private readonly LogLevel _logLevel;
        private readonly ILogger[] _loggers;
        private readonly Stopwatch _stopWatch;

        private readonly object _threadLock = new object();

        private bool _isDisposed;

        // Delegates
        private readonly Func<TimeSpan, string> _endMessageFunc;

        public TimedLogger(ILogger logger, LogLevel logLevel, Func<string> beginMessageFunc, Func<TimeSpan, string> endMessageFunc) : this(logger.ItemToArrayOrDefault(), logLevel, beginMessageFunc, endMessageFunc)
        {

        }

        public TimedLogger(IEnumerable<ILogger> loggers, LogLevel logLevel, Func<string> beginMessageFunc, Func<TimeSpan, string> endMessageFunc)
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
            Log(x => message);
        }

        public void Log(Func<TimeSpan, string> messageFunc, Exception exception = null)
        {
            Log(_logLevel, messageFunc, exception);         
        }

        public void Log(LogLevel level, Func<TimeSpan, string> messageFunc, Exception exception = null)
        {
            if (_stopWatch.HasValue())
            {
                messageFunc.ValidateVariable(nameof(messageFunc));

                if (exception != null)
                {
                    _loggers.LogException(level, messageFunc(_stopWatch.Elapsed), exception);
                }
                else
                {
                    _loggers.LogMessage(level, messageFunc(_stopWatch.Elapsed));
                }

            }
        }

        public void Dispose()
        {
            EndLog(_endMessageFunc);
        }

        public void EndLog(Func<TimeSpan, string> endMessageFunc, Exception exception = null)
        {
            try
            {
                lock (_threadLock)
                {
                    if (!_isDisposed && endMessageFunc.HasValue())
                    {
                        if (_stopWatch.HasValue())
                        {
                            _stopWatch.Stop();
                            Log(endMessageFunc, exception);
                        }

                        _isDisposed = true;
                    }
                }
            }
            catch { }        
        }
    }
}
