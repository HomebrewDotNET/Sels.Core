using Microsoft.Extensions.Logging;
using Sels.Core.Components.Serialization;
using Sels.Core.Components.Serialization.Providers;
using Sels.Core.Contracts.Serialization;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Linq;
using Sels.Core.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
            loggers.ValidateArgumentNotNullOrEmpty(nameof(loggers));

            lock (_threadLock)
            {
                _loggers.AddRange(loggers);
            }
        }

        public static void RegisterLoggers(ILogger logger)
        {
            logger.ValidateArgument(nameof(logger));

            lock (_threadLock)
            {
                _loggers.Add(logger);
            }
        }

        public static void RegisterLoggers(ILoggerFactory factory, string category)
        {
            factory.ValidateArgument(nameof(factory));
            category.ValidateArgumentNotNullOrWhitespace(nameof(category));

            lock (_threadLock)
            {
                _loggers.Add(factory.CreateLogger(category));
            }
        }

        #endregion

        #region Logging
        #region Log
        /// <summary>
        /// Perform a logging action using the registered loggers.
        /// </summary>
        /// <param name="logAction">Action to perform on loggers</param>
        public static void Log(Action<IEnumerable<ILogger>> logAction)
        {
            if (_loggers.HasValue() && logAction.HasValue())
            {
                logAction.ForceExecute(_loggers);
            }
        }
        /// <summary>
        /// Logs a message using severity <see cref="LogLevel.Information"/>.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="args">Optional logging parameters</param>
        public static void Log(string message, params object[] args)
        {
            Log(LogLevel.Information, message, args);
        }
        /// <summary>
        /// Logs a message.
        /// </summary>
        /// <param name="level">Severity level for log</param>
        /// <param name="message">Message to log</param>
        /// <param name="args">Optional logging parameters</param>
        public static void Log(LogLevel level, string message, params object[] args)
        {
            Log(x => x.LogMessage(level, message, args));
        }
        /// <summary>
        /// Logs an exception with severity <see cref="LogLevel.Error"/>.
        /// </summary>
        /// <param name="exception">Exception to log</param>
        public static void Log(Exception exception)
        {
            Log(LogLevel.Error, exception);
        }
        /// <summary>
        /// Logs an exception.
        /// </summary>
        /// <param name="level">Severity level for log</param>
        /// <param name="exception">Exception to log</param>
        public static void Log(LogLevel level, Exception exception)
        {
            Log(x => x.LogException(level, exception));
        }
        /// <summary>
        /// Logs a exception with an extra message with severity <see cref="LogLevel.Error"/>.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="exception">Exception to log</param>
        /// <param name="args">Optional logging parameters</param>
        public static void Log(string message, Exception exception, params object[] args)
        {
            Log(LogLevel.Error, message, exception, args);
        }
        /// <summary>
        /// Logs a exception with an extra message.
        /// </summary>
        /// <param name="level">Severity level for log</param>
        /// <param name="message">Message to log</param>
        /// <param name="exception">Exception to log</param>
        /// <param name="args">Optional logging parameters</param>
        public static void Log(LogLevel level, string message, Exception exception, params object[] args)
        {
            Log(x => x.LogException(level, message, exception, args));
        }
        #endregion

        #region Trace
        /// <summary>
        /// Traces an object to the logs using the <see cref="JsonProvider"/> serialization provider with severity <see cref="LogLevel.Trace"/>.
        /// </summary>
        /// <param name="objectToTrace">Object to serialize and log</param>
        public static void Trace(object objectToTrace)
        {
            Trace<JsonProvider>(LogLevel.Trace, objectToTrace);
        }
        /// <summary>
        /// Traces an object to the logs using the <see cref="JsonProvider"/> serialization provider.
        /// </summary>
        /// <param name="level">Severity level for log</param>
        /// <param name="objectToTrace">Object to serialize and log</param>
        public static void Trace(LogLevel level, object objectToTrace)
        {
            Trace<JsonProvider>(level, objectToTrace);
        }
        /// <summary>
        /// Traces an object to the logs using the <see cref="JsonProvider"/> serialization provider with an extra message with severity <see cref="LogLevel.Trace"/>.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="objectToTrace">Object to serialize and log</param>
        public static void Trace(string message, object objectToTrace)
        {
            Trace<JsonProvider>(LogLevel.Trace, message, objectToTrace);
        }
        /// <summary>
        /// Traces an object to the logs using the <see cref="JsonProvider"/> serialization provider with an extra message.
        /// </summary>
        /// <param name="level">Severity level for log</param>
        /// <param name="message">Message to log</param>
        /// <param name="objectToTrace">Object to serialize and log</param>
        public static void Trace(LogLevel level, string message, object objectToTrace)
        {
            Trace<JsonProvider>(level, message, objectToTrace);
        }
        /// <summary>
        /// Traces an object to the logs using the <typeparamref name="TProvider"/> serialization provider with severity <see cref="LogLevel.Trace"/>.
        /// </summary>
        /// <typeparam name="TProvider">Type of serialization provider</typeparam>
        /// <param name="objectToTrace">Object to serialize and log</param>
        public static void Trace<TProvider>(object objectToTrace) where TProvider : ISerializationProvider, new()
        {
            Trace<TProvider>(LogLevel.Trace, objectToTrace);
        }
        /// <summary>
        /// Traces an object to the logs using the <typeparamref name="TProvider"/> serialization provider.
        /// </summary>
        /// <typeparam name="TProvider">Type of serialization provider</typeparam>
        /// <param name="level">Severity level for log</param>
        /// <param name="objectToTrace">Object to serialize and log</param>
        public static void Trace<TProvider>(LogLevel level, object objectToTrace) where TProvider : ISerializationProvider, new()
        {
            Log(x => x.LogObject<TProvider>(level, objectToTrace));
        }
        /// <summary>
        /// Traces an object to the logs using the <typeparamref name="TProvider"/> serialization provider with an extra message with severity <see cref="LogLevel.Trace"/>.
        /// </summary>
        /// <typeparam name="TProvider">Type of serialization provider</typeparam>
        /// <param name="message">Message to log</param>
        /// <param name="objectToTrace">Object to serialize and log</param>
        public static void Trace<TProvider>(string message, object objectToTrace) where TProvider : ISerializationProvider, new()
        {
            Trace<TProvider>(LogLevel.Trace, message, objectToTrace);
        }
        /// <summary>
        /// Traces an object to the logs using the <typeparamref name="TProvider"/> serialization provider with an extra message.
        /// </summary>
        /// <typeparam name="TProvider">Type of serialization provider</typeparam>
        /// <param name="level">Severity level for log</param>
        /// <param name="message">Message to log</param>
        /// <param name="objectToTrace">Object to serialize and log</param>
        public static void Trace<TProvider>(LogLevel level, string message, object objectToTrace) where TProvider : ISerializationProvider, new()
        {
            Log(x => x.LogObject<TProvider>(level, message, objectToTrace));
        }
        /// <summary>
        /// Traces how long an action took to execute with severity <see cref="LogLevel.Information"/>. Timer starts when calling method and stops when return value is disposed.
        /// </summary>
        /// <param name="action">Action to trace</param>
        /// <returns>Timing scope</returns>
        public static IDisposable TraceAction(string action)
        {
            return TraceAction(LogLevel.Information, action);
        }
        /// <summary>
        /// Traces how long an action took to execute. Timer starts when calling method and stops when return value is disposed.
        /// </summary>
        /// <param name="level">Severity level for log</param>
        /// <param name="action">Action to trace</param>
        /// <returns>Timing scope</returns>
        public static IDisposable TraceAction(LogLevel level, string action)
        {
            return CreateTimedLogger(level, () => $"Executing action <{action}>", x => $"Executed action <{action}> in {x.PrintTotalMs()}");
        }
        /// <summary>
        /// Traces how long a method took to execute with severity <see cref="LogLevel.Debug"/>. Timer starts when calling method and stops when return value is disposed.
        /// </summary>
        /// <param name="caller">Object that wants it's method execution traced</param>
        /// <param name="method">Name of method to trace. If not provider the calling method name will be used</param>
        /// <returns>Timing scope</returns>
        public static IDisposable TraceMethod(object caller, [CallerMemberName] string method = null)
        {
            return TraceMethod(LogLevel.Debug, caller, method);
        }
        /// <summary>
        /// Traces how long a method took to execute. Timer starts when calling method and stops when return value is disposed.
        /// </summary>
        /// <param name="level">Severity level for log</param>
        /// <param name="caller">Object that wants it's method execution traced</param>
        /// <param name="method">Name of method to trace. If not provider the calling method name will be used</param>
        /// <returns>Timing scope</returns>
        public static IDisposable TraceMethod(LogLevel level, object caller, [CallerMemberName] string method = null)
        {
            var fullMethodName = $"{(caller.HasValue() ? caller.GetType().FullName : "Null")}.{method}";

            return CreateTimedLogger(level, () => $"Calling method <{fullMethodName}>", x => $"Called method <{fullMethodName}> in {x.PrintTotalMs()}");
        }
        #endregion

        /// <summary>
        /// Creates a logger that logs a message when created and a message when disposed using the elapsed time between creating and disposing the logger.
        /// </summary>
        /// <param name="level">Severity level for log</param>
        /// <param name="beginMessageFunc">Func that creates the start message</param>
        /// <param name="endMessageFunc">Func that creates the stop message</param>
        /// <returns>Logger that keeps start of elapsed time since it was created</returns>
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
