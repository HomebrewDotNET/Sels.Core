using Microsoft.Extensions.Logging;
using Sels.Core.Components.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Sels.Core.Contracts.Commands
{
    /// <summary>
    /// Exposes methods to run commands (powershell, linux, ...) and build the command string.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Executed the current command and returns it's output.
        /// </summary>
        /// <param name="output">Standard output received from executing the command. Equals the Stout</param>
        /// <param name="error">Error output received from executing the command. Equals the Sterr</param>
        /// <param name="exitCode">The exit returned by the command process</param>
        /// <param name="options">Optional options for modifying the execution behaviour</param>
        /// <returns>If the command was succesfully executed</returns>
        bool RunCommand(out string output, out string error, out int exitCode, CommandExecutionOptions options = null);

        /// <summary>
        /// Builds a string that represents the command that will be executed.
        /// </summary>
        /// <returns>The command string that will be executed if <see cref="RunCommand(out string, out string, out int, CommandExecutionOptions)"/> is called</returns>
        string BuildCommand();
    }

    /// <summary>
    /// Used to execute command and return
    /// </summary>
    /// <typeparam name="TCommandResult">Type of result returned by the command</typeparam>
    public interface ICommand<TCommandResult> : ICommand
    {
        /// <summary>
        /// Executes this command and parses it's command output to <typeparamref name="TCommandResult"/>.
        /// </summary>
        /// <param name="options">Optional options for modifying the execution behaviour</param>
        /// <returns>The parsed command output from executing this command</returns>
        TCommandResult Execute(CommandExecutionOptions options = null);

        /// <summary>
        /// Executes this command and parses it's command output to <typeparamref name="TCommandResult"/>.
        /// </summary>
        /// <param name="exitCode">Exit code from executing the command</param>
        /// <param name="options">Optional options for modifying the execution behaviour</param>
        /// <returns>The parsed command output from executing this command</returns>
        TCommandResult Execute(out int exitCode, CommandExecutionOptions options = null);

        /// <summary>
        /// Parses <paramref name="exitCode"/> and <paramref name="error"/> from command execution to an object of type <typeparamref name="TCommandResult"/>.
        /// </summary>
        /// <param name="exitCode">Exit code of command execution</param>
        /// <param name="error">Sterr of command execution</param>
        /// <param name="output">Stout of command execution</param>
        /// <param name="wasSuccesful">Boolean indicating if command execution was successful</param>
        /// <param name="loggers">Optional loggers for tracing</param>
        /// <returns>Result from command execution</returns>
        TCommandResult CreateResult(bool wasSuccesful, int exitCode, string output, string error, IEnumerable<ILogger> loggers = null);
    }
}
