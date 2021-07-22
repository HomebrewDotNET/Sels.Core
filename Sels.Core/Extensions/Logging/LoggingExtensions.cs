using Microsoft.Extensions.Logging;
using Sels.Core.Components.Logging;
using Sels.Core.Components.Serialization;
using Sels.Core.Components.Serialization.Providers;
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
    public static class LoggingExtensions
    {
        #region Logger
        #region Message
        public static void LogMessage(this ILogger logger, LogLevel level, string message, params object[] args)
        {
            _ = Task.Run(() =>
            {
                try
                {
                    if (logger.HasValue() && logger.IsEnabled(level))
                    {
                        logger.Log(level, message, args);
                    }
                }
                catch { }
            });
        }

        public static void LogMessage(this ILogger logger, LogLevel level, Func<string> messageFunc, params object[] args)
        {
            _ = Task.Run(() =>
            {
                try
                {
                    if (logger.HasValue() && logger.IsEnabled(level))
                    {
                        logger.Log(level, messageFunc(), args);
                    }
                }
                catch { }
            });
        }
        #endregion

        #region Exception
        public static void LogException(this ILogger logger, LogLevel level, Exception exception)
        {
            _ = Task.Run(() =>
            {
                try
                {
                    if (logger.HasValue() && logger.IsEnabled(level))
                    {
                        logger.Log(level, exception, null);
                    }
                }
                catch { }
            });
        }

        public static void LogException(this ILogger logger, LogLevel level, string message, Exception exception, params object[] args)
        {
            _ = Task.Run(() =>
            {
                try
                {
                    if (logger.HasValue() && logger.IsEnabled(level))
                    {
                        logger.Log(level, exception, message, args);
                    }
                }
                catch { }
            });
        }

        public static void LogException(this ILogger logger, LogLevel level, Func<string> messageFunc, Exception exception, params object[] args)
        {
            _ = Task.Run(() =>
            {
                try
                {
                    if (logger.HasValue() && logger.IsEnabled(level))
                    {
                        logger.Log(level, exception, messageFunc(), args);
                    }
                }
                catch { }
            });
        }
        #endregion

        #region Object
        public static void LogObject<TSerializer>(this ILogger logger, LogLevel level, params object[] objects) where TSerializer : ISerializationProvider, new()
        {
            var serializer = new TSerializer();
            logger.LogObject(level, serializer, objects);
        }

        public static void LogObject<TSerializer>(this ILogger logger, LogLevel level, string message, params object[] objects) where TSerializer : ISerializationProvider, new()
        {
            var serializer = new TSerializer();
            logger.LogObject(level, serializer, message, objects);
        }

        public static void LogObject<TSerializer>(this ILogger logger, LogLevel level, string message, Exception exception, params object[] objects) where TSerializer : ISerializationProvider, new()
        {
            var serializer = new TSerializer();
            logger.LogObject(level, serializer, message, exception, objects);
        }

        public static void LogObject<TSerializer>(this ILogger logger, LogLevel level, Func<string> messageFunc, params object[] objects) where TSerializer : ISerializationProvider, new()
        {
            var serializer = new TSerializer();
            logger.LogObject(level, serializer, messageFunc, objects);
        }

        public static void LogObject<TSerializer>(this ILogger logger, LogLevel level, Func<string> messageFunc, Exception exception, params object[] objects) where TSerializer : ISerializationProvider, new()
        {
            var serializer = new TSerializer();
            logger.LogObject(level, serializer, messageFunc, exception, objects);
        }

        public static void LogObject(this ILogger logger, LogLevel level, SerializationProvider provider, params object[] objects)
        {
            var serializer = provider.CreateProvider();
            logger.LogObject(level, serializer, objects);
        }

        public static void LogObject(this ILogger logger, LogLevel level, SerializationProvider provider, string message, params object[] objects)
        {
            var serializer = provider.CreateProvider();
            logger.LogObject(level, serializer, message, objects);
        }

        public static void LogObject(this ILogger logger, LogLevel level, SerializationProvider provider, string message, Exception exception, params object[] objects)
        {
            var serializer = provider.CreateProvider();
            logger.LogObject(level, serializer, message, exception, objects);
        }

        public static void LogObject(this ILogger logger, LogLevel level, SerializationProvider provider, Func<string> messageFunc, params object[] objects)
        {
            var serializer = provider.CreateProvider();
            logger.LogObject(level, serializer, messageFunc(), objects);
        }

        public static void LogObject(this ILogger logger, LogLevel level, SerializationProvider provider, Func<string> messageFunc, Exception exception, params object[] objects)
        {
            var serializer = provider.CreateProvider();
            logger.LogObject(level, serializer, messageFunc(), exception, objects);
        }

        public static void LogObject(this ILogger logger, LogLevel level, ISerializationProvider serializer, params object[] objects)
        {
            _ = Task.Run(() =>
            {
                try
                {
                    if (logger.HasValue() && logger.IsEnabled(level) & objects.HasValue() && serializer.HasValue())
                    {
                        logger.Log(level, objects.Where(x => x.HasValue()).Select(x => serializer.Serialize(x)).JoinStringNewLine());
                    }
                }
                catch { }
            });
        }

        public static void LogObject(this ILogger logger, LogLevel level, ISerializationProvider serializer, string message, params object[] objects)
        {
            _ = Task.Run(() =>
            {
                try
                {
                    if (logger.HasValue() && logger.IsEnabled(level))
                    {
                        logger.Log(level, Helper.Strings.JoinStringsNewLine(message, objects.Where(x => x.HasValue()).Select(x => serializer.Serialize(x)).JoinStringNewLine()));
                    }
                }
                catch { }
            });
        }

        public static void LogObject(this ILogger logger, LogLevel level, ISerializationProvider serializer, string message, Exception exception, params object[] objects)
        {
            _ = Task.Run(() =>
            {
                try
                {
                    if (logger.HasValue() && logger.IsEnabled(level))
                    {
                        logger.Log(level, exception, Helper.Strings.JoinStringsNewLine(message, objects.Where(x => x.HasValue()).Select(x => serializer.Serialize(x)).JoinStringNewLine()));
                    }
                }
                catch { }
            });
        }

        public static void LogObject(this ILogger logger, LogLevel level, ISerializationProvider serializer, Func<string> messageFunc, params object[] objects)
        {
            _ = Task.Run(() =>
            {
                try
                {
                    if (logger.HasValue() && logger.IsEnabled(level))
                    {
                        logger.Log(level, Helper.Strings.JoinStringsNewLine(messageFunc(), objects.Where(x => x.HasValue()).Select(x => serializer.Serialize(x)).JoinStringNewLine()));
                    }
                }
                catch { }
            });
        }

        public static void LogObject(this ILogger logger, LogLevel level, ISerializationProvider serializer, Func<string> messageFunc, Exception exception, params object[] objects)
        {
            _ = Task.Run(() =>
            {
                try
                {
                    if (logger.HasValue() && logger.IsEnabled(level))
                    {
                        logger.Log(level, exception, Helper.Strings.JoinStringsNewLine(messageFunc(), objects.Where(x => x.HasValue()).Select(x => serializer.Serialize(x)).JoinStringNewLine()));
                    }
                }
                catch { }
            });
        }
        #endregion
        #endregion

        #region Loggers
        #region Message
        public static void LogMessage(this IEnumerable<ILogger> loggers, LogLevel level, string message, params object[] args)
        {
            loggers.ForceExecute(x => x.LogMessage(level, message, args));
        }

        public static void LogMessage(this IEnumerable<ILogger> loggers, LogLevel level, Func<string> messageFunc, params object[] args)
        {
            loggers.ForceExecute(x => x.LogMessage(level, messageFunc, args));
        }
        #endregion

        #region Exception
        public static void LogException(this IEnumerable<ILogger> loggers, LogLevel level, Exception exception)
        {
            loggers.ForceExecute(x => x.LogException(level, exception));
        }

        public static void LogException(this IEnumerable<ILogger> loggers, LogLevel level, string message, Exception exception, params object[] args)
        {
            loggers.ForceExecute(x => x.LogException(level, message, exception, args));
        }

        public static void LogException(this IEnumerable<ILogger> loggers, LogLevel level, Func<string> messageFunc, Exception exception, params object[] args)
        {
            loggers.ForceExecute(x => x.LogException(level, messageFunc, exception, args));
        }
        #endregion

        #region Object
        public static void LogObject<TSerializer>(this IEnumerable<ILogger> loggers, LogLevel level, params object[] objects) where TSerializer : ISerializationProvider, new()
        {
            var serializer = new TSerializer();
            loggers.LogObject(level, serializer, objects);
        }

        public static void LogObject<TSerializer>(this IEnumerable<ILogger> loggers, LogLevel level, string message, params object[] objects) where TSerializer : ISerializationProvider, new()
        {
            var serializer = new TSerializer();
            loggers.LogObject(level, serializer, message, objects);
        }

        public static void LogObject<TSerializer>(this IEnumerable<ILogger> loggers, LogLevel level, string message, Exception exception, params object[] objects) where TSerializer : ISerializationProvider, new()
        {
            var serializer = new TSerializer();
            loggers.LogObject(level, serializer, message, exception, objects);
        }

        public static void LogObject<TSerializer>(this IEnumerable<ILogger> loggers, LogLevel level, Func<string> messageFunc, params object[] objects) where TSerializer : ISerializationProvider, new()
        {
            var serializer = new TSerializer();
            loggers.LogObject(level, serializer, messageFunc, objects);
        }

        public static void LogObject<TSerializer>(this IEnumerable<ILogger> loggers, LogLevel level, Func<string> messageFunc, Exception exception, params object[] objects) where TSerializer : ISerializationProvider, new()
        {
            var serializer = new TSerializer();
            loggers.LogObject(level, serializer, messageFunc, exception, objects);
        }

        public static void LogObject(this IEnumerable<ILogger> loggers, LogLevel level, SerializationProvider provider, params object[] objects)
        {
            var serializer = provider.CreateProvider();
            loggers.LogObject(level, serializer, objects);
        }

        public static void LogObject(this IEnumerable<ILogger> loggers, LogLevel level, SerializationProvider provider, string message, params object[] objects)
        {
            var serializer = provider.CreateProvider();
            loggers.LogObject(level, serializer, message, objects);
        }

        public static void LogObject(this IEnumerable<ILogger> loggers, LogLevel level, SerializationProvider provider, string message, Exception exception, params object[] objects)
        {
            var serializer = provider.CreateProvider();
            loggers.LogObject(level, serializer, message, exception, objects);
        }

        public static void LogObject(this IEnumerable<ILogger> loggers, LogLevel level, SerializationProvider provider, Func<string> messageFunc, params object[] objects)
        {
            var serializer = provider.CreateProvider();
            loggers.LogObject(level, serializer, messageFunc(), objects);
        }

        public static void LogObject(this IEnumerable<ILogger> loggers, LogLevel level, SerializationProvider provider, Func<string> messageFunc, Exception exception, params object[] objects)
        {
            var serializer = provider.CreateProvider();
            loggers.LogObject(level, serializer, messageFunc(), exception, objects);
        }

        public static void LogObject(this IEnumerable<ILogger> loggers, LogLevel level, ISerializationProvider serializer, params object[] objects)
        {
            loggers.ForceExecute(x => x.LogObject(level, serializer, objects));
        }

        public static void LogObject(this IEnumerable<ILogger> loggers, LogLevel level, ISerializationProvider serializer, string message, params object[] objects)
        {
            loggers.ForceExecute(x => x.LogObject(level, serializer, message, objects));
        }

        public static void LogObject(this IEnumerable<ILogger> loggers, LogLevel level, ISerializationProvider serializer, string message, Exception exception, params object[] objects)
        {
            loggers.ForceExecute(x => x.LogObject(level, serializer, message, exception, objects));
        }

        public static void LogObject(this IEnumerable<ILogger> loggers, LogLevel level, ISerializationProvider serializer, Func<string> messageFunc, params object[] objects)
        {
            loggers.ForceExecute(x => x.LogObject(level, serializer, messageFunc, objects));
        }

        public static void LogObject(this IEnumerable<ILogger> loggers, LogLevel level, ISerializationProvider serializer, Func<string> messageFunc, Exception exception, params object[] objects)
        {
            loggers.ForceExecute(x => x.LogObject(level, serializer, messageFunc, exception, objects));
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
