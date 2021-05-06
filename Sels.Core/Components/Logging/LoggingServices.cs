using Microsoft.Extensions.Logging;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.Logging
{
    public static class LoggingServices
    {
        // Fields
        private static object _threadLock = new object();
        private static readonly List<ILogger> _loggers = new List<ILogger>();

        #region Setup
        public static void RegisterLoggers(IEnumerable<ILogger> loggers)
        {
            loggers.ValidateVariable(nameof(loggers));

            lock (_threadLock)
            {
                _loggers.AddRange(loggers);
            }
        }

        public static void RegisterLoggers(ILogger logger)
        {
            logger.ValidateVariable(nameof(logger));

            lock (_threadLock)
            {
                _loggers.Add(logger);
            }
        }

        public static void RegisterLoggers(ILoggerFactory factory, string category)
        {
            factory.ValidateVariable(nameof(factory));
            category.ValidateVariable(nameof(category));

            lock (_threadLock)
            {
                _loggers.Add(factory.CreateLogger(category));
            }
        }

        #endregion

        #region Logging
        public static void Log(Action<IEnumerable<ILogger>> logAction)
        {
            if (_loggers.HasValue() && logAction.HasValue())
            {
                logAction.ForceExecute(_loggers);
            }
        }

        public static TimedLogger CreateTimedLogger(LogLevel logLevel, Func<string> beginMessageFunc, Func<TimeSpan, string> endMessageFunc)
        {
            try
            {
                if (_loggers.HasValue() && beginMessageFunc.HasValue() && endMessageFunc.HasValue())
                {
                    return new StopWatchTimedLogger(_loggers, logLevel, beginMessageFunc, endMessageFunc);
                }
            }
            catch { }
            
            return new NullTimedLogger();
        }
        #endregion
    }
}
