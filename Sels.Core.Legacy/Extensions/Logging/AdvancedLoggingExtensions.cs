using Microsoft.Extensions.Logging;
using Sels.Core.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Sels.Core.Extensions.Logging.Advanced
{
    /// <summary>
    /// Exposes some more advanced logging methods.
    /// </summary>
    public static class AdvancedLoggingExtensions
    {
        #region Loggers
        #region Trace levels
        /// <summary>
        /// Logs a message using severity <see cref="LogLevel.Debug"/>.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="args">Optional logging parameters</param>
        /// <param name="loggers">Loggers to perform logging action with. If loggers is null nothing will be logged but no exceptions are thrown</param>
        public static void Debug(this IEnumerable<ILogger> loggers, string message, params object[] args)
        {
            loggers.LogMessage(LogLevel.Debug, message, args);
        }

        /// <summary>
        /// Logs a message using severity <see cref="LogLevel.Warning"/>.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="args">Optional logging parameters</param>
        /// <param name="loggers">Loggers to perform logging action with. If loggers is null nothing will be logged but no exceptions are thrown</param>
        public static void Warning(this IEnumerable<ILogger> loggers, string message, params object[] args)
        {
            loggers.LogMessage(LogLevel.Warning, message, args);
        }

        /// <summary>
        /// Logs a message using severity <see cref="LogLevel.Warning"/>.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="args">Optional logging parameters</param>
        /// <param name="exception">The exception to log</param>
        /// <param name="loggers">Loggers to perform logging action with. If loggers is null nothing will be logged but no exceptions are thrown</param>
        public static void Warning(this IEnumerable<ILogger> loggers, string message, Exception exception, params object[] args)
        {
            loggers.LogMessage(LogLevel.Warning, message, exception, args);
        }
        #endregion

        #region Log
        /// <summary>
        /// Logs a message using severity <see cref="LogLevel.Information"/>.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="args">Optional logging parameters</param>
        /// <param name="loggers">Loggers to perform logging action with. If loggers is null nothing will be logged but no exceptions are thrown</param>
        public static void Log(this IEnumerable<ILogger> loggers, string message, params object[] args)
        {
            loggers.LogMessage(LogLevel.Information, message, args);
        }
        /// <summary>
        /// Logs an exception with severity <see cref="LogLevel.Error"/>.
        /// </summary>
        /// <param name="exception">Exception to log</param>
        /// <param name="loggers">Loggers to perform logging action with. If loggers is null nothing will be logged but no exceptions are thrown</param>
        public static void Log(this IEnumerable<ILogger> loggers, Exception exception)
        {
            loggers.LogException(LogLevel.Error, exception);
        }
        /// <summary>
        /// Logs a exception with an extra message with severity <see cref="LogLevel.Error"/>.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="exception">Exception to log</param>
        /// <param name="args">Optional logging parameters</param>
        /// <param name="loggers">Loggers to perform logging action with. If loggers is null nothing will be logged but no exceptions are thrown</param>
        public static void Log(this IEnumerable<ILogger> loggers, string message, Exception exception, params object[] args)
        {
            loggers.LogException(LogLevel.Error, message, exception, args);
        }
        #endregion

        #region Trace
        /// <summary>
        /// Logs a message using severity <see cref="LogLevel.Trace"/>.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="args">Optional logging parameters</param>
        /// <param name="loggers">Loggers to perform logging action with. If loggers is null nothing will be logged but no exceptions are thrown</param>
        public static void Trace(this IEnumerable<ILogger> loggers, string message, params object[] args)
        {
            loggers.LogMessage(LogLevel.Trace, message, args);
        }
        /// <summary>
        /// Traces how long an action took to execute with severity <see cref="LogLevel.Information"/>. Timer starts when calling method and stops when return value is disposed.
        /// </summary>
        /// <param name="action">Action to trace</param>
        /// <param name="loggers">Loggers to perform logging action with. If loggers is null nothing will be logged but no exceptions are thrown</param>
        /// <returns>Timing scope</returns>
        public static IDisposable TraceAction(this IEnumerable<ILogger> loggers, string action)
        {
            return TraceAction(loggers, LogLevel.Information, action);
        }
        /// <summary>
        /// Traces how long an action took to execute with severity <see cref="LogLevel.Information"/>. Timer starts when calling method and stops when return value is disposed.
        /// </summary>
        /// <param name="actionStartMessage">Log message when action starts</param>
        /// <param name="actionFinishedMessage">Log message when action is finished</param>
        /// <param name="loggers">Loggers to perform logging action with. If loggers is null nothing will be logged but no exceptions are thrown</param>
        /// <returns>Timing scope</returns>
        public static IDisposable TraceAction(this IEnumerable<ILogger> loggers, string actionStartMessage, Func<TimeSpan, string> actionFinishedMessage)
        {
            return TraceAction(loggers, LogLevel.Information, actionStartMessage, actionFinishedMessage);
        }
        /// <summary>
        /// Traces how long an action took to execute. Timer starts when calling method and stops when return value is disposed.
        /// </summary>
        /// <param name="level">Severity level for log</param>
        /// <param name="actionStartMessage">Log message when action starts</param>
        /// <param name="actionFinishedMessage">Log message when action is finished</param>
        /// <param name="loggers">Loggers to perform logging action with. If loggers is null nothing will be logged but no exceptions are thrown</param>
        /// <returns>Timing scope</returns>
        public static IDisposable TraceAction(this IEnumerable<ILogger> loggers, LogLevel level, string actionStartMessage, Func<TimeSpan, string> actionFinishedMessage)
        {
            return loggers.CreateTimedLogger(level, () => actionStartMessage, actionFinishedMessage);
        }
        /// <summary>
        /// Traces how long an action took to execute. Timer starts when calling method and stops when return value is disposed.
        /// </summary>
        /// <param name="level">Severity level for log</param>
        /// <param name="action">Action to trace</param>
        /// <param name="loggers">Loggers to perform logging action with. If loggers is null nothing will be logged but no exceptions are thrown</param>
        /// <returns>Timing scope</returns>
        public static IDisposable TraceAction(this IEnumerable<ILogger> loggers, LogLevel level, string action)
        {
            return loggers.CreateTimedLogger(level, () => $"Executing action <{action}>", x => $"Executed action <{action}> in {x.PrintTotalMs()}");
        }
        /// <summary>
        /// Traces how long a method took to execute with severity <see cref="LogLevel.Trace"/>. Timer starts when calling method and stops when return value is disposed.
        /// </summary>
        /// <param name="caller">Object that wants it's method execution traced</param>
        /// <param name="method">Name of method to trace. If not provider the calling method name will be used</param>
        /// <param name="loggers">Loggers to perform logging action with. If loggers is null nothing will be logged but no exceptions are thrown</param>
        /// <returns>Timing scope</returns>
        public static IDisposable TraceMethod(this IEnumerable<ILogger> loggers, object caller, [CallerMemberName] string method = null)
        {
            return TraceMethod(loggers, LogLevel.Trace, caller, method);
        }
        /// <summary>
        /// Traces how long a method took to execute. Timer starts when calling method and stops when return value is disposed.
        /// </summary>
        /// <param name="level">Severity level for log</param>
        /// <param name="caller">Object that wants it's method execution traced</param>
        /// <param name="method">Name of method to trace. If not provider the calling method name will be used</param>
        /// <param name="loggers">Loggers to perform logging action with. If loggers is null nothing will be logged but no exceptions are thrown</param>
        /// <returns>Timing scope</returns>
        public static IDisposable TraceMethod(this IEnumerable<ILogger> loggers, LogLevel level, object caller, [CallerMemberName] string method = null)
        {
            return TraceMethod(loggers, level, caller?.GetType(), method);
        }

        /// <summary>
        /// Traces how long a method took to execute with severity <see cref="LogLevel.Trace"/>. Timer starts when calling method and stops when return value is disposed.
        /// </summary>
        /// <param name="caller">Type of object that wants it's method execution traced</param>
        /// <param name="method">Name of method to trace. If not provider the calling method name will be used</param>
        /// <param name="loggers">Loggers to perform logging action with. If loggers is null nothing will be logged but no exceptions are thrown</param>
        /// <returns>Timing scope</returns>
        public static IDisposable TraceMethod(this IEnumerable<ILogger> loggers, Type caller, [CallerMemberName] string method = null)
        {
            return TraceMethod(loggers, LogLevel.Trace, caller, method);
        }
        /// <summary>
        /// Traces how long a method took to execute. Timer starts when calling method and stops when return value is disposed.
        /// </summary>
        /// <param name="level">Severity level for log</param>
        /// <param name="caller">Type of object that wants it's method execution traced</param>
        /// <param name="method">Name of method to trace. If not provider the calling method name will be used</param>
        /// <param name="loggers">Loggers to perform logging action with. If loggers is null nothing will be logged but no exceptions are thrown</param>
        /// <returns>Timing scope</returns>
        public static IDisposable TraceMethod(this IEnumerable<ILogger> loggers, LogLevel level, Type caller, [CallerMemberName] string method = null)
        {
            var fullMethodName = $"{(caller.HasValue() ? caller.FullName : "Null")}.{method}";

            return loggers.CreateTimedLogger(level, () => $"Calling method <{fullMethodName}>", x => $"Called method <{fullMethodName}> in {x.PrintTotalMs()}");
        }
        /// <summary>
        /// Traces an object to the logs serialized to json with severity <see cref="LogLevel.Trace"/>.
        /// </summary>
        /// <param name="objectToTrace">Object to serialize and log</param>
        /// <param name="loggers">Loggers to perform logging action with. If loggers is null nothing will be logged but no exceptions are thrown</param>
        public static void TraceObject(this IEnumerable<ILogger> loggers, object objectToTrace)
        {
            loggers.LogObject(LogLevel.Trace, objectToTrace);
        }
        /// <summary>
        /// Traces an object to the logs serialized to json with an extra message with severity <see cref="LogLevel.Trace"/>.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="objectToTrace">Object to serialize and log</param>
        /// <param name="loggers">Loggers to perform logging action with. If loggers is null nothing will be logged but no exceptions are thrown</param>
        public static void TraceObject(this IEnumerable<ILogger> loggers, string message, object objectToTrace)
        {
            loggers.LogObject(LogLevel.Trace, message, objectToTrace);
        }
        #endregion
        #endregion

        #region Logger
        #region Trace levels
        /// <summary>
        /// Logs a message using severity <see cref="LogLevel.Debug"/>.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="args">Optional logging parameters</param>
        /// <param name="logger">Logger to perform logging action with. If logger is null nothing will be logged but no exceptions are thrown</param>
        public static void Debug(this ILogger logger, string message, params object[] args)
        {
            logger.LogMessage(LogLevel.Debug, message, args);
        }

        /// <summary>
        /// Logs a message using severity <see cref="LogLevel.Warning"/>.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="args">Optional logging parameters</param>
        /// <param name="logger">Logger to perform logging action with. If logger is null nothing will be logged but no exceptions are thrown</param>
        public static void Warning(this ILogger logger, string message, params object[] args)
        {
            logger.LogMessage(LogLevel.Warning, message, args);
        }

        /// <summary>
        /// Logs a message using severity <see cref="LogLevel.Warning"/>.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="args">Optional logging parameters</param>
        /// <param name="exception">The exception to log</param>
        /// <param name="logger">Logger to perform logging action with. If logger is null nothing will be logged but no exceptions are thrown</param>
        public static void Warning(this ILogger logger, string message, Exception exception, params object[] args)
        {
            logger.LogMessage(LogLevel.Warning, message, exception, args);
        }
        #endregion

        #region Log
        /// <summary>
        /// Logs a message using severity <see cref="LogLevel.Information"/>.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="args">Optional logging parameters</param>
        /// <param name="logger">Logger to perform logging action with. If logger is null nothing will be logged but no exceptions are thrown</param>
        public static void Log(this ILogger logger, string message, params object[] args)
        {
            logger.LogMessage(LogLevel.Information, message, args);
        }
        /// <summary>
        /// Logs an exception with severity <see cref="LogLevel.Error"/>.
        /// </summary>
        /// <param name="exception">Exception to log</param>
        /// <param name="logger">Logger to perform logging action with. If logger is null nothing will be logged but no exceptions are thrown</param>
        public static void Log(this ILogger logger, Exception exception)
        {
            logger.LogException(LogLevel.Error, exception);
        }
        /// <summary>
        /// Logs a exception with an extra message with severity <see cref="LogLevel.Error"/>.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="exception">Exception to log</param>
        /// <param name="args">Optional logging parameters</param>
        /// <param name="logger">Logger to perform logging action with. If logger is null nothing will be logged but no exceptions are thrown</param>
        public static void Log(this ILogger logger, string message, Exception exception, params object[] args)
        {
            logger.LogException(LogLevel.Error, message, exception, args);
        }
        #endregion

        #region Trace
        /// <summary>
        /// Logs a message using severity <see cref="LogLevel.Trace"/>.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="args">Optional logging parameters</param>
        /// <param name="logger">Logger to perform logging action with. If logger is null nothing will be logged but no exceptions are thrown</param>
        public static void Trace(this ILogger logger, string message, params object[] args)
        {
            logger.LogMessage(LogLevel.Trace, message, args);
        }
        /// <summary>
        /// Traces how long an action took to execute with severity <see cref="LogLevel.Information"/>. Timer starts when calling method and stops when return value is disposed.
        /// </summary>
        /// <param name="action">Action to trace</param>
        /// <param name="logger">Logger to perform logging action with. If logger is null nothing will be logged but no exceptions are thrown</param>
        /// <returns>Timing scope</returns>
        public static IDisposable TraceAction(this ILogger logger, string action)
        {
            return TraceAction(logger, LogLevel.Information, action);
        }
        /// <summary>
        /// Traces how long an action took to execute with severity <see cref="LogLevel.Information"/>. Timer starts when calling method and stops when return value is disposed.
        /// </summary>
        /// <param name="actionStartMessage">Log message when action starts</param>
        /// <param name="actionFinishedMessage">Log message when action is finished</param>
        /// <param name="logger">Logger to perform logging action with. If logger is null nothing will be logged but no exceptions are thrown</param>
        /// <returns>Timing scope</returns>
        public static IDisposable TraceAction(this ILogger logger, string actionStartMessage, Func<TimeSpan, string> actionFinishedMessage)
        {
            return TraceAction(logger, LogLevel.Information, actionStartMessage, actionFinishedMessage);
        }
        /// <summary>
        /// Traces how long an action took to execute. Timer starts when calling method and stops when return value is disposed.
        /// </summary>
        /// <param name="level">Severity level for log</param>
        /// <param name="actionStartMessage">Log message when action starts</param>
        /// <param name="actionFinishedMessage">Log message when action is finished</param>
        /// <param name="logger">Logger to perform logging action with. If logger is null nothing will be logged but no exceptions are thrown</param>
        /// <returns>Timing scope</returns>
        public static IDisposable TraceAction(this ILogger logger, LogLevel level, string actionStartMessage, Func<TimeSpan, string> actionFinishedMessage)
        {
            return logger.CreateTimedLogger(level, () => actionStartMessage, actionFinishedMessage);
        }
        /// <summary>
        /// Traces how long an action took to execute. Timer starts when calling method and stops when return value is disposed.
        /// </summary>
        /// <param name="level">Severity level for log</param>
        /// <param name="action">Action to trace</param>
        /// <param name="logger">Logger to perform logging action with. If logger is null nothing will be logged but no exceptions are thrown</param>
        /// <returns>Timing scope</returns>
        public static IDisposable TraceAction(this ILogger logger, LogLevel level, string action)
        {
            return logger.CreateTimedLogger(level, () => $"Executing action <{action}>", x => $"Executed action <{action}> in {x.PrintTotalMs()}");
        }
        /// <summary>
        /// Traces how long a method took to execute with severity <see cref="LogLevel.Trace"/>. Timer starts when calling method and stops when return value is disposed.
        /// </summary>
        /// <param name="caller">Object that wants it's method execution traced</param>
        /// <param name="method">Name of method to trace. If not provider the calling method name will be used</param>
        /// <param name="logger">Logger to perform logging action with. If logger is null nothing will be logged but no exceptions are thrown</param>
        /// <returns>Timing scope</returns>
        public static IDisposable TraceMethod(this ILogger logger, object caller, [CallerMemberName] string method = null)
        {
            return TraceMethod(logger, LogLevel.Trace, caller, method);
        }
        /// <summary>
        /// Traces how long a method took to execute. Timer starts when calling method and stops when return value is disposed.
        /// </summary>
        /// <param name="level">Severity level for log</param>
        /// <param name="caller">Object that wants it's method execution traced</param>
        /// <param name="method">Name of method to trace. If not provider the calling method name will be used</param>
        /// <param name="logger">Logger to perform logging action with. If logger is null nothing will be logged but no exceptions are thrown</param>
        /// <returns>Timing scope</returns>
        public static IDisposable TraceMethod(this ILogger logger, LogLevel level, object caller, [CallerMemberName] string method = null)
        {
            return TraceMethod(logger, level, caller?.GetType(), method);
        }

        /// <summary>
        /// Traces how long a method took to execute with severity <see cref="LogLevel.Trace"/>. Timer starts when calling method and stops when return value is disposed.
        /// </summary>
        /// <param name="caller">Type of object that wants it's method execution traced</param>
        /// <param name="method">Name of method to trace. If not provider the calling method name will be used</param>
        /// <param name="logger">Logger to perform logging action with. If logger is null nothing will be logged but no exceptions are thrown</param>
        /// <returns>Timing scope</returns>
        public static IDisposable TraceMethod(this ILogger logger, Type caller, [CallerMemberName] string method = null)
        {
            return TraceMethod(logger, LogLevel.Trace, caller, method);
        }
        /// <summary>
        /// Traces how long a method took to execute. Timer starts when calling method and stops when return value is disposed.
        /// </summary>
        /// <param name="level">Severity level for log</param>
        /// <param name="caller">Type of object that wants it's method execution traced</param>
        /// <param name="method">Name of method to trace. If not provider the calling method name will be used</param>
        /// <param name="logger">Logger to perform logging action with. If logger is null nothing will be logged but no exceptions are thrown</param>
        /// <returns>Timing scope</returns>
        public static IDisposable TraceMethod(this ILogger logger, LogLevel level, Type caller, [CallerMemberName] string method = null)
        {
            var fullMethodName = $"{(caller.HasValue() ? caller.GetDisplayName() : "Null")}.{method}";

            return logger.CreateTimedLogger(level, () => $"Calling method <{fullMethodName}>", x => $"Called method <{fullMethodName}> in {x.PrintTotalMs()}");
        }
        /// <summary>
        /// Traces an object to the logs serialized to json with severity <see cref="LogLevel.Trace"/>.
        /// </summary>
        /// <param name="objectToTrace">Object to serialize and log</param>
        /// <param name="logger">Logger to perform logging action with. If logger is null nothing will be logged but no exceptions are thrown</param>
        public static void TraceObject(this ILogger logger, object objectToTrace)
        {
            logger.LogObject(LogLevel.Trace, objectToTrace);
        }
        /// <summary>
        /// Traces an object to the logs serialized to json with an extra message with severity <see cref="LogLevel.Trace"/>.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="objectToTrace">Object to serialize and log</param>
        /// <param name="logger">Logger to perform logging action with. If logger is null nothing will be logged but no exceptions are thrown</param>
        public static void TraceObject(this ILogger logger, string message, object objectToTrace)
        {
            logger.LogObject(LogLevel.Trace, message, objectToTrace);
        }
        #endregion
        #endregion
    }
}
