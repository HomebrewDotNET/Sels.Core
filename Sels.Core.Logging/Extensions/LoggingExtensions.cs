using Microsoft.Extensions.Logging;
using Sels.Core.Components.Logging;
using Sels.Core.Components.Serialization;
using Sels.Core.Contracts.Serialization;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Extensions.Logging
{
    /// <summary>
    /// Exposes some simple additonal logging methods on ILoggers.
    /// </summary>
    public static class LoggingExtensions
    {
        #region Logger
        #region Message
        public static void LogMessage(this ILogger logger, LogLevel level, string message, params object[] args)
        {
            try
            {
                if (logger.HasValue() && logger.IsEnabled(level))
                {
                    logger.Log(level, message, args);
                }
            }
            catch { }
        }

        public static void LogMessage(this ILogger logger, LogLevel level, Func<string> messageFunc, params object[] args)
        {
            try
            {
                if (logger.HasValue() && logger.IsEnabled(level))
                {
                    logger.Log(level, messageFunc(), args);
                }
            }
            catch { }
        }
        #endregion

        #region Exception
        public static void LogException(this ILogger logger, LogLevel level, Exception exception)
        {
            try
            {
                if (logger.HasValue() && logger.IsEnabled(level))
                {
                    logger.Log(level, exception, null);
                }
            }
            catch { }
        }

        public static void LogException(this ILogger logger, LogLevel level, string message, Exception exception, params object[] args)
        {
            try
            {
                if (logger.HasValue() && logger.IsEnabled(level))
                {
                    logger.Log(level, exception, message, args);
                }
            }
            catch { }
        }

        public static void LogException(this ILogger logger, LogLevel level, Func<string> messageFunc, Exception exception, params object[] args)
        {
            try
            {
                if (logger.HasValue() && logger.IsEnabled(level))
                {
                    logger.Log(level, exception, messageFunc(), args);
                }
            }
            catch { }
        }
        #endregion

        #region Object
        public static void LogObject(this ILogger logger, LogLevel level, params object[] objects)
        {
            try
            {
                if (logger.HasValue() && logger.IsEnabled(level) & objects.HasValue())
                {
                    logger.Log(level, objects.Where(x => x.HasValue()).Select(x => x.SerializeAsJson()).JoinStringNewLine());
                }
            }
            catch { }
        }

        public static void LogObject(this ILogger logger, LogLevel level, string message, params object[] objects)
        {
            try
            {
                if (logger.HasValue() && logger.IsEnabled(level))
                {
                    logger.Log(level, Helper.Strings.JoinStringsNewLine(message, objects.Where(x => x.HasValue()).Select(x => x.SerializeAsJson()).JoinStringNewLine()));
                }
            }
            catch { }
        }

        public static void LogObject(this ILogger logger, LogLevel level, string message, Exception exception, params object[] objects)
        {
            try
            {
                if (logger.HasValue() && logger.IsEnabled(level))
                {
                    logger.Log(level, exception, Helper.Strings.JoinStringsNewLine(message, objects.Where(x => x.HasValue()).Select(x => serializer.Serialize(x)).JoinStringNewLine()));
                }
            }
            catch { }
        }

        public static void LogObject(this ILogger logger, LogLevel level, Func<string> messageFunc, params object[] objects)
        {
            try
            {
                if (logger.HasValue() && logger.IsEnabled(level))
                {
                    logger.Log(level, Helper.Strings.JoinStringsNewLine(messageFunc(), objects.Where(x => x.HasValue()).Select(x => serializer.Serialize(x)).JoinStringNewLine()));
                }
            }
            catch { }
        }

        public static void LogObject(this ILogger logger, LogLevel level, Func<string> messageFunc, Exception exception, params object[] objects)
        {
            try
            {
                if (logger.HasValue() && logger.IsEnabled(level))
                {
                    logger.Log(level, exception, Helper.Strings.JoinStringsNewLine(messageFunc(), objects.Where(x => x.HasValue()).Select(x => serializer.Serialize(x)).JoinStringNewLine()));
                }
            }
            catch { }
        }
        #endregion
        #endregion

        #region Loggers
        #region Message
        public static void LogMessage(this IEnumerable<ILogger> loggers, LogLevel level, string message, params object[] args)
        {
            loggers.ForceExecute(x => LogMessage(x, level, message, args));
        }

        public static void LogMessage(this IEnumerable<ILogger> loggers, LogLevel level, Func<string> messageFunc, params object[] args)
        {
            loggers.ForceExecute(x => LogMessage(x, level, messageFunc, args));
        }
        #endregion

        #region Exception
        public static void LogException(this IEnumerable<ILogger> loggers, LogLevel level, Exception exception)
        {
            loggers.ForceExecute(x => LogException(x, level, exception));
        }

        public static void LogException(this IEnumerable<ILogger> loggers, LogLevel level, string message, Exception exception, params object[] args)
        {
            loggers.ForceExecute(x => LogException(x, level, message, exception, args));
        }

        public static void LogException(this IEnumerable<ILogger> loggers, LogLevel level, Func<string> messageFunc, Exception exception, params object[] args)
        {
            loggers.ForceExecute(x => LogException(x, level, messageFunc, exception, args));
        }
        #endregion

        #region Object
        public static void LogObject(this IEnumerable<ILogger> loggers, LogLevel level, params object[] objects)
        {
            loggers.ForceExecute(x => LogObject(x, level, objects));
        }

        public static void LogObject(this IEnumerable<ILogger> loggers, LogLevel level, string message, params object[] objects)
        {
            loggers.ForceExecute(x => LogObject(x, level, message, objects));
        }

        public static void LogObject(this IEnumerable<ILogger> loggers, LogLevel level, string message, Exception exception, params object[] objects)
        {
            loggers.ForceExecute(x => LogObject(x, level, message, exception, objects));
        }

        public static void LogObject(this IEnumerable<ILogger> loggers, LogLevel level, Func<string> messageFunc, params object[] objects)
        {
            loggers.ForceExecute(x => LogObject(x, level, messageFunc, objects));
        }

        public static void LogObject(this IEnumerable<ILogger> loggers, LogLevel level, Func<string> messageFunc, Exception exception, params object[] objects)
        {
            loggers.ForceExecute(x => LogObject(x, level, messageFunc, exception, objects));
        }
        #endregion
        #endregion

        #region Timed Logger
        public static TimedLogger CreateTimedLogger(this ILogger logger, LogLevel logLevel, string beginMessage, Func<TimeSpan, string> endMessageFunc)
        {
            return new StopWatchTimedLogger(logger, logLevel, () => beginMessage, endMessageFunc);
        }

        public static TimedLogger CreateTimedLogger(this IEnumerable<ILogger> loggers, LogLevel logLevel, string beginMessage, Func<TimeSpan, string> endMessageFunc)
        {
            return new StopWatchTimedLogger(loggers, logLevel, () => beginMessage, endMessageFunc);
        }

        public static TimedLogger CreateTimedLogger(this ILogger logger, LogLevel logLevel, Func<string> beginMessageFunc, Func<TimeSpan, string> endMessageFunc)
        {
            return new StopWatchTimedLogger(logger, logLevel, beginMessageFunc, endMessageFunc);
        }

        public static TimedLogger CreateTimedLogger(this IEnumerable<ILogger> loggers, LogLevel logLevel, Func<string> beginMessageFunc, Func<TimeSpan, string> endMessageFunc)
        {
            return new StopWatchTimedLogger(loggers, logLevel, beginMessageFunc, endMessageFunc);
        }
        #endregion
    }
}
