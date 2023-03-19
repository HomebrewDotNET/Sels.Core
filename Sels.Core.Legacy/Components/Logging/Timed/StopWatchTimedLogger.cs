using Microsoft.Extensions.Logging;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Sels.Core.Components.Logging
{
    /// <summary>
    /// Implements <see cref="TimedLogger"/> using a <see cref="Stopwatch"/>.
    /// </summary>
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

        /// <inheritdoc cref="StopWatchTimedLogger"/>
        /// <param name="logger">The logger to use for tracing</param>
        /// <param name="logLevel">What log level to use for the begin and end messages</param>
        /// <param name="beginMessageFunc">The delegate that returns the message to log when the timers starts</param>
        /// <param name="endMessageFunc">The delegate that returns the message to log when the end message needs to be logged</param>
        public StopWatchTimedLogger(ILogger logger, LogLevel logLevel, Func<string> beginMessageFunc, Func<TimeSpan, string> endMessageFunc) : this(logger.AsArrayOrDefault(), logLevel, beginMessageFunc, endMessageFunc)
        {

        }
        /// <inheritdoc cref="StopWatchTimedLogger"/>
        /// <param name="loggers">The loggers to use for tracing</param>
        /// <param name="logLevel">What log level to use for the begin and end messages</param>
        /// <param name="beginMessageFunc">The delegate that returns the message to log when the timers starts</param>
        /// <param name="endMessageFunc">The delegate that returns the message to log when the end message needs to be logged</param>
        public StopWatchTimedLogger(IEnumerable<ILogger> loggers, LogLevel logLevel, Func<string> beginMessageFunc, Func<TimeSpan, string> endMessageFunc)
        {
            beginMessageFunc.ValidateArgument(nameof(beginMessageFunc));
            endMessageFunc.ValidateArgument(nameof(endMessageFunc));

            _logLevel = logLevel;
            _endMessageFunc = endMessageFunc;
            
            if (loggers.HasValue(x => x.IsEnabled(logLevel)))
            {
                _loggers = loggers.ToArray();
                _stopWatch = new Stopwatch();
                _stopWatch.Start();

                Log(beginMessageFunc());
            }
        }

        private void Log(string message)
        {
            if (_loggers.HasValue())
            {
                _loggers.LogMessage(_logLevel, message);
            }            
        }

        /// <inheritdoc/>
        public override void Log(Action<TimeSpan, IEnumerable<ILogger>> loggingAction)
        {
            loggingAction.ValidateArgument(nameof(loggingAction));

            if(_stopWatch.HasValue() && _loggers.HasValue())
            {
                loggingAction(_stopWatch.Elapsed, _loggers);
            }            
        }
        /// <inheritdoc/>
        public override void Dispose()
        {
            EndLog((x, y) => y.LogMessage(_logLevel, _endMessageFunc(x)));
        }
        /// <inheritdoc/>
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
