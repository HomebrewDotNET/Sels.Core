using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Processing.BackgroundJob
{
    /// <summary>
    /// Allows a job to modify it's own state scoped to the current job.
    /// </summary>
    public interface IBackgroundJobState
    {
        #region Parameters
        /// <summary>
        /// Gets the value of parameter <paramref name="name"/> tied to the current job.
        /// </summary>
        /// <typeparam name="T">The type of the parameter value</typeparam>
        /// <param name="name">The name of the parameter to get the value from</param>
        /// <returns>The value for parameter <paramref name="name"/></returns>
        T Get<T>(string name);
        /// <summary>
        /// Sets the value of parameter <paramref name="name"/> to <paramref name="data"/>. The data can be accessed with <see cref="Get{T}(string)"/>.
        /// </summary>
        /// <typeparam name="T">The type of the parameter value</typeparam>
        /// <param name="name">The name of the parameter to set</param>
        /// <param name="data">The value to set. Value must be serializable to json</param>
        void Set<T>(string name, T data);
        #endregion

        #region ExecutedActions
        /// <summary>
        /// Checks if the current job already executed action with name <paramref name="action"/>. Useful when dealing with resubmits.
        /// </summary>
        /// <param name="action">The name of the action to check</param>
        /// <returns>True if action <paramref name="action"/> was already executed by the current background job, otherwise false</returns>
        bool IsActionExecuted(string action);
        /// <summary>
        /// Remember that the current job executed <paramref name="action"/>. Should the job fail afterwards it can check with <see cref="IsActionExecuted(string)"/> if it already executed the action before.
        /// </summary>
        /// <param name="action">The name of the action that was executed</param>
        /// <param name="message">Optional message to add as log entry</param>
        void AddExecutedAction(string action, string message = null);
        /// <summary>
        /// Remember that the current job executed <paramref name="action"/>. Should the job fail afterwards it can check with <see cref="TryGetExecutedActionState{T}(string, out T)"/> if it already executed the action before.
        /// Also saves state <paramref name="data"/> tied to <paramref name="action"/>.
        /// </summary>
        /// <typeparam name="T">The type of the action state</typeparam>
        /// <param name="action">The name of the action that was executed</param>
        /// <param name="data">The state tied to <paramref name="action"/>. Must be serializable to json</param>
        /// <param name="message">Optional message to add as log entry</param>
        void AddExecutedAction<T>(string action, T data, string message = null);
        /// <summary>
        /// Checks if <paramref name="action"/> was already executed before and fetches the state of the executed action if it was executed before.
        /// </summary>
        /// <typeparam name="T">The type of of the action state</typeparam>
        /// <param name="action">The name of the action to check</param>
        /// <param name="data">The state of the action when it was executed, will only be set when the method returns true</param>
        /// <returns>True if <paramref name="action"/> was already executed by the current background job, otherwise false</returns>
        bool TryGetExecutedActionState<T>(string action, out T data);
        #endregion

        #region Logging
        /// <summary>
        /// Creates a log entry for the current job.
        /// </summary>
        /// <param name="logLevel">The log level of message</param>
        /// <param name="message">The log message</param>
        /// <param name="exception">Optional exception tied to the log entry</param>
        void AddLogEntry(LogLevel logLevel, string message, Exception exception = null);
        /// <summary>
        /// Creates a log entry for the current job with log level <see cref="LogLevel.Trace"/>.
        /// </summary>
        /// <param name="message">The log message</param>
        /// <param name="exception">Optional exception tied to the log entry</param>
        void Trace(string message, Exception exception = null) => AddLogEntry(LogLevel.Trace, message, exception);
        /// <summary>
        /// Creates a log entry for the current job with log level <see cref="LogLevel.Debug"/>.
        /// </summary>
        /// <param name="message">The log message</param>
        /// <param name="exception">Optional exception tied to the log entry</param>
        void Debug(string message, Exception exception = null) => AddLogEntry(LogLevel.Debug, message, exception);
        /// <summary>
        /// Creates a log entry for the current job with log level <see cref="LogLevel.Information"/>.
        /// </summary>
        /// <param name="message">The log message</param>
        /// <param name="exception">Optional exception tied to the log entry</param>
        void Log(string message, Exception exception = null) => AddLogEntry(LogLevel.Information, message, exception);
        /// <summary>
        /// Creates a log entry for the current job with log level <see cref="LogLevel.Warning"/>.
        /// </summary>
        /// <param name="message">The log message</param>
        /// <param name="exception">Optional exception tied to the log entry</param>
        void Warning(string message, Exception exception = null) => AddLogEntry(LogLevel.Warning, message, exception);
        /// <summary>
        /// Creates a log entry for the current job with log level <see cref="LogLevel.Error"/>.
        /// </summary>
        /// <param name="message">The log message</param>
        /// <param name="exception">Optional exception tied to the log entry</param>
        void Error(string message, Exception exception = null) => AddLogEntry(LogLevel.Error, message, exception);
        #endregion
    }
}
