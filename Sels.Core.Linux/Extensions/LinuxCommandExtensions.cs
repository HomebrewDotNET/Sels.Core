using Sels.Core.Extensions;
using Sels.Core.Linux.Exceptions.LinuxCommand;
using Sels.Core.Linux.Contracts.LinuxCommand;
using System;
using System.Collections.Generic;
using System.Text;
using Sels.Core.Linux.Components.LinuxCommand.Commands.Core;
using Sels.Core.Linux.Contracts.LinuxCommand.Commands;
using Sels.Core.Linux.Components.LinuxCommand.Commands;
using Sels.Core.Contracts.Commands;

namespace Sels.Core.Linux.Extensions
{
    public static class LinuxCommandExtensions
    {
        #region Result
        /// <summary>
        /// Returns the result if the command was executed successfully. Throws <see cref="LinuxCommandExecutionFailedException"/> if the command failed containing the error object.
        /// </summary>
        public static TOutput GetResult<TOutput, TError>(this ILinuxCommandResult<TOutput, TError> commandResult)
        {
            commandResult.ValidateArgument(nameof(commandResult));

            return commandResult.Failed ? throw new LinuxCommandExecutionFailedException(commandResult.ExitCode, commandResult.Error) : commandResult.Output;
        }
        #endregion

        #region Execution
        /// <summary>
        /// Executes <paramref name="command"/> as super user.
        /// </summary>
        /// <typeparam name="TResult">Type of command result</typeparam>
        /// <param name="command">Command to execute</param>
        /// <param name="exitCode">Exit code returned from executing the command</param>
        /// <returns>Command result</returns>
        public static TResult ExecuteAsSuperUser<TResult>(this ICommand<TResult> command, out int exitCode)
        {
            command.ValidateArgument(nameof(command));
            return new SudoCommand<TResult>(command).Execute(out exitCode);
        }

        /// <summary>
        /// Executes <paramref name="command"/> as super user.
        /// </summary>
        /// <typeparam name="TResult">Type of command result</typeparam>
        /// <param name="command">Command to execute</param>
        /// <returns>Command result</returns>
        public static TResult ExecuteAsSuperUser<TResult>(this ICommand<TResult> command)
        {
            return command.ExecuteAsSuperUser(out _);
        }

        /// <summary>
        /// Executes <paramref name="command"/> as super user with <paramref name="password"/>.
        /// </summary>
        /// <typeparam name="TResult">Type of command result</typeparam>
        /// <param name="command">Command to execute</param>
        /// <param name="password">Password for sudo</param>
        /// <param name="exitCode">Exit code returned from executing the command</param>
        /// <returns>Command result</returns>
        public static TResult ExecuteAsSuperUser<TResult>(this ICommand<TResult> command, string password, out int exitCode)
        {
            command.ValidateArgument(nameof(command));
            password.ValidateArgumentNotNullOrWhitespace(nameof(password));

            var echo = new EchoCommand()
            {
                Message = password
            };

            var sudo = new SudoCommand<TResult>(command);

            return new ChainCommand<TResult>(echo, CommandChainer.Pipe, sudo).Execute(out exitCode);
        }

        /// <summary>
        /// Executes <paramref name="command"/> as super user with <paramref name="password"/>.
        /// </summary>
        /// <typeparam name="TResult">Type of command result</typeparam>
        /// <param name="password">Password for sudo</param>
        /// <param name="command">Command to execute</param>
        /// <returns>Command result</returns>
        public static TResult ExecuteAsSuperUser<TResult>(this ICommand<TResult> command, string password)
        {
            return command.ExecuteAsSuperUser(password, out _);
        }
        #endregion
    }
}
