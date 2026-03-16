using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

namespace Sels.Core.Process
{
    /// <summary>
    /// Executes a process and exposes a fluent api for configuring the execution of the process.
    /// </summary>
    public interface IProcessRunner
    {
        #region Settings
        /// <summary>
        /// Runs the process as another user.
        /// </summary>
        /// <param name="username">The username of the user</param>
        /// <param name="passwordBuilder">Delegate for inputting the password for the user</param>
        /// <param name="domain">Optional domain that the user is located in</param>
        /// <returns>Current instance for method chaining</returns>
        IProcessRunner AsUser(string username, Action<SecureString> passwordBuilder, string domain = null);
        /// <summary>
        /// Runs the process as another user.
        /// </summary>
        /// <param name="username">The username of the user</param>
        /// <param name="password">Optional password for the user</param>
        /// <param name="domain">Optional domain that the user is located in</param>
        /// <returns>Current instance for method chaining</returns>
        IProcessRunner AsUser(string username, string password = null, string domain = null);
        /// <summary>
        /// Optional logger to enable tracing of the process execution.
        /// </summary>
        /// <param name="logger">Optional logger for tracing</param>
        /// <returns>Current instance for method chaining</returns>
        IProcessRunner WithLogger(ILogger logger);
        /// <summary>
        /// How long to wait for the process to exit when a kill request was sent.
        /// </summary>
        /// <param name="killTime">How long in milliseconds to wait for a process to exit after sending a kill request</param>
        /// <returns>Current instance for method chaining</returns>
        IProcessRunner WithKillTime(int killTime);
        /// <summary>
        /// How long to sleep if the process is still running. (frees up the thread during the sleep time)
        /// </summary>
        /// <param name="sleepTime">How long to sleep in milliseconds</param>
        /// <returns>Current instance for method chaining</returns>
        IProcessRunner WithSleepTime(int sleepTime);
        /// <summary>
        /// Indicates that a new window should be created when starting the process.
        /// </summary>
        /// <returns>Current instance for method chaining</returns>
        IProcessRunner WithWindow();
        /// <summary>
        /// Uses the operating system shell to start the process.
        /// </summary>
        /// <returns>Current instance for method chaining</returns>
        IProcessRunner UseShell();
        /// <summary>
        /// Sets the priority of the process.
        /// </summary>
        /// <param name="processPriority">The priority for the process</param>
        /// <returns>Current instance for method chaining</returns>
        IProcessRunner WithPriority(ProcessPriorityClass processPriority);
        /// <summary>
        /// The standard output of the process will not be kept in memory.
        /// </summary>
        /// <returns>Current instance for method chaining</returns>
        IProcessRunner WithoutStandardOutput();
        /// <summary>
        /// The error output of the process will not be kept in memory.
        /// </summary>
        /// <returns>Current instance for method chaining</returns>
        IProcessRunner WithoutErrorOutput();
        /// <summary>
        /// The standard and error output of the process will not be kept in memory.
        /// </summary>
        /// <returns>Current instance for method chaining</returns>
        IProcessRunner WithoutOutput();
        #endregion

        #region Event
        /// <summary>
        /// Triggered when the process exits.
        /// </summary>
        /// <param name="action">The action to execute</param>
        /// <returns>Current instance for method chaining</returns>
        IProcessRunner OnExit(Action action);
        /// <summary>
        /// Triggered when the process outputs a line to the standard output stream.
        /// </summary>
        /// <param name="handler">Delegate that handles the outputted line</param>
        /// <returns>Current instance for method chaining</returns>
        IProcessRunner OnStandardOutput(Action<string> handler);
        /// <summary>
        /// Triggered when the process outputs a line to the error output stream.
        /// </summary>
        /// <param name="handler">Delegate that handles the outputted line</param>
        /// <returns>Current instance for method chaining</returns>
        IProcessRunner OnErrorOutput(Action<string> handler);
        /// <summary>
        /// Triggered when the process outputs a line to the standard or error output stream.
        /// </summary>
        /// <param name="handler">Delegate that handles the outputted line</param>
        /// <returns>Current instance for method chaining</returns>
        IProcessRunner OnOutput(Action<string> handler);         
        #endregion

        /// <summary>
        /// Executes the current process and waits until it exists.
        /// </summary>
        /// <param name="arguments">Optional arguments for running the process</param>
        /// <param name="token">Optional cancellation token for cancelling the execution of the process</param>
        /// <returns>The result from executing the process</returns>
        Task<ProcessExecutionResult> ExecuteAsync(string arguments = null, CancellationToken token = default);
    }
}
