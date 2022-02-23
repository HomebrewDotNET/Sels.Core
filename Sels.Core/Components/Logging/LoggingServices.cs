using Microsoft.Extensions.Logging;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Linq;
using Sels.Core.Extensions.Logging;
using Sels.Core.Extensions.Logging.Advanced;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace Sels.Core.Components.Logging
{
    /// <summary>
    /// Static class that makes it easier to log in projects. Only required to be setup during startup.
    /// </summary>
    public static class LoggingServices
    {
        // Fields
        private static object _threadLock = new object();
        private static readonly List<ILogger> _loggers = new List<ILogger>();

        // Properties
        /// <summary>
        /// Registered loggers used by the <see cref="LoggingServices"/>.
        /// </summary>
        public static IReadOnlyCollection<ILogger> Loggers => new ReadOnlyCollection<ILogger>(_loggers);

        #region Setup
        /// <summary>
        /// Registers the loggers to be used.
        /// </summary>
        /// <param name="loggers">The loggers to use</param>
        public static void RegisterLoggers(IEnumerable<ILogger> loggers)
        {
            loggers.ValidateArgumentNotNullOrEmpty(nameof(loggers));

            lock (_threadLock)
            {
                _loggers.AddRange(loggers);
            }
        }
        /// <summary>
        /// Registers a logger to be used.
        /// </summary>
        /// <param name="logger">The logger to use</param>
        public static void RegisterLogger(ILogger logger)
        {
            logger.ValidateArgument(nameof(logger));

            lock (_threadLock)
            {
                _loggers.Add(logger);
            }
        }
        #endregion

        #region Logging
        #region Trace levels
        /// <summary>
        /// Logs a message using severity <see cref="LogLevel.Debug"/>.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="args">Optional logging parameters</param>
        public static void Debug(string message, params object[] args) => _loggers.Debug(message, args);

        /// <summary>
        /// Logs a message using severity <see cref="LogLevel.Warning"/>.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="args">Optional logging parameters</param>
        public static void Warning(string message, params object[] args) => _loggers.Warning(message, args);
        #endregion

        #region Log
        /// <summary>
        /// Perform a logging action using the registered loggers.
        /// </summary>
        /// <param name="logAction">Action to perform on loggers</param>
        public static void Log(Action<IEnumerable<ILogger>> logAction)
        {
            if (_loggers.HasValue() && logAction.HasValue())
            {
                try
                {
                    logAction(_loggers);
                }
                catch { }
            }
        }
        /// <summary>
        /// Logs a message using severity <see cref="LogLevel.Information"/>.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="args">Optional logging parameters</param>
        public static void Log(string message, params object[] args) => _loggers.Log(message, args);
        /// <summary>
        /// Logs a message.
        /// </summary>
        /// <param name="level">Severity level for log</param>
        /// <param name="message">Message to log</param>
        /// <param name="args">Optional logging parameters</param>
        public static void Log(LogLevel level, string message, params object[] args) => Log(x => x.LogMessage(level, message, args));
        /// <summary>
        /// Logs an exception with severity <see cref="LogLevel.Error"/>.
        /// </summary>
        /// <param name="exception">Exception to log</param>
        public static void Log(Exception exception) => _loggers.Log(exception);
        /// <summary>
        /// Logs an exception.
        /// </summary>
        /// <param name="level">Severity level for log</param>
        /// <param name="exception">Exception to log</param>
        public static void Log(LogLevel level, Exception exception) => Log(x => x.LogException(level, exception));

        /// <summary>
        /// Logs a exception with an extra message with severity <see cref="LogLevel.Error"/>.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="exception">Exception to log</param>
        /// <param name="args">Optional logging parameters</param>
        public static void Log(string message, Exception exception, params object[] args) => _loggers.Log(message, exception, args);
        /// <summary>
        /// Logs a exception with an extra message.
        /// </summary>
        /// <param name="level">Severity level for log</param>
        /// <param name="message">Message to log</param>
        /// <param name="exception">Exception to log</param>
        /// <param name="args">Optional logging parameters</param>
        public static void Log(LogLevel level, string message, Exception exception, params object[] args) => Log(x => x.LogException(level, message, exception, args));
        #endregion

        #region Trace
        /// <summary>
        /// Logs a message using severity <see cref="LogLevel.Trace"/>.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="args">Optional logging parameters</param>
        public static void Trace(string message, params object[] args) => Log(LogLevel.Trace, message, args);
        /// <summary>
        /// Traces an object to the logs serialized to json with severity <see cref="LogLevel.Trace"/>.
        /// </summary>
        /// <param name="objectToTrace">Object to serialize and log</param>
        public static void TraceObject(object objectToTrace) => _loggers.TraceObject(objectToTrace);
        /// <summary>
        /// Traces an object to the logs serialized to json.
        /// </summary>
        /// <param name="level">Severity level for log</param>
        /// <param name="objectToTrace">Object to serialize and log</param>
        public static void TraceObject(LogLevel level, object objectToTrace) => _loggers.LogObject(level, objectToTrace);
        /// <summary>
        /// Traces an object to the logs serialized to json with an extra message with severity <see cref="LogLevel.Trace"/>.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="objectToTrace">Object to serialize and log</param>
        public static void TraceObject(string message, object objectToTrace) => _loggers.TraceObject(message, objectToTrace);
        /// <summary>
        /// Traces an object to the logs serialized to json with an extra message.
        /// </summary>
        /// <param name="level">Severity level for log</param>
        /// <param name="message">Message to log</param>
        /// <param name="objectToTrace">Object to serialize and log</param>
        public static void TraceObject(LogLevel level, string message, object objectToTrace) => _loggers.LogObject(level, message, objectToTrace);
        /// <summary>
        /// Traces how long an action took to execute with severity <see cref="LogLevel.Information"/>. Timer starts when calling method and stops when return value is disposed.
        /// </summary>
        /// <param name="action">Action to trace</param>
        /// <returns>Timing scope</returns>
        public static IDisposable TraceAction(string action) => _loggers.TraceAction(action);
        /// <summary>
        /// Traces how long an action took to execute with severity <see cref="LogLevel.Information"/>. Timer starts when calling method and stops when return value is disposed.
        /// </summary>
        /// <param name="actionStartMessage">Log message when action starts</param>
        /// <param name="actionFinishedMessage">Log message when action is finished</param>
        /// <returns>Timing scope</returns>
        public static IDisposable TraceAction(string actionStartMessage, Func<TimeSpan, string> actionFinishedMessage) => _loggers.TraceAction(actionStartMessage, actionFinishedMessage);
        /// <summary>
        /// Traces how long an action took to execute. Timer starts when calling method and stops when return value is disposed.
        /// </summary>
        /// <param name="level">Severity level for log</param>
        /// <param name="actionStartMessage">Log message when action starts</param>
        /// <param name="actionFinishedMessage">Log message when action is finished</param>
        /// <returns>Timing scope</returns>
        public static IDisposable TraceAction(LogLevel level, string actionStartMessage, Func<TimeSpan, string> actionFinishedMessage) => _loggers.TraceAction(level, actionStartMessage, actionFinishedMessage);
        /// <summary>
        /// Traces how long an action took to execute. Timer starts when calling method and stops when return value is disposed.
        /// </summary>
        /// <param name="level">Severity level for log</param>
        /// <param name="action">Action to trace</param>
        /// <returns>Timing scope</returns>
        public static IDisposable TraceAction(LogLevel level, string action) => _loggers.TraceAction(level, action);
        /// <summary>
        /// Traces how long a method took to execute with severity <see cref="LogLevel.Trace"/>. Timer starts when calling method and stops when return value is disposed.
        /// </summary>
        /// <param name="caller">Object that wants it's method execution traced</param>
        /// <param name="method">Name of method to trace. If not provider the calling method name will be used</param>
        /// <returns>Timing scope</returns>
        public static IDisposable TraceMethod(object caller, [CallerMemberName] string method = null) => _loggers.TraceMethod(caller, method);
        /// <summary>
        /// Traces how long a method took to execute. Timer starts when calling method and stops when return value is disposed.
        /// </summary>
        /// <param name="level">Severity level for log</param>
        /// <param name="caller">Object that wants it's method execution traced</param>
        /// <param name="method">Name of method to trace. If not provider the calling method name will be used</param>
        /// <returns>Timing scope</returns>
        public static IDisposable TraceMethod(LogLevel level, object caller, [CallerMemberName] string method = null) => _loggers.TraceMethod(level, caller, method);

        /// <summary>
        /// Traces how long a method took to execute with severity <see cref="LogLevel.Trace"/>. Timer starts when calling method and stops when return value is disposed.
        /// </summary>
        /// <param name="caller">Type of object that wants it's method execution traced</param>
        /// <param name="method">Name of method to trace. If not provider the calling method name will be used</param>
        /// <returns>Timing scope</returns>
        public static IDisposable TraceMethod(Type caller, [CallerMemberName] string method = null) => _loggers.TraceMethod(caller, method);
        /// <summary>
        /// Traces how long a method took to execute. Timer starts when calling method and stops when return value is disposed.
        /// </summary>
        /// <param name="level">Severity level for log</param>
        /// <param name="caller">Type of object that wants it's method execution traced</param>
        /// <param name="method">Name of method to trace. If not provider the calling method name will be used</param>
        /// <returns>Timing scope</returns>
        public static IDisposable TraceMethod(LogLevel level, Type caller, [CallerMemberName] string method = null) => _loggers.TraceMethod(level, caller, method);
        #endregion

        /// <summary>
        /// Creates a logger that logs a message when created and a message when disposed using the elapsed time between creating and disposing the logger.
        /// </summary>
        /// <param name="logLevel">Severity level for log</param>
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
