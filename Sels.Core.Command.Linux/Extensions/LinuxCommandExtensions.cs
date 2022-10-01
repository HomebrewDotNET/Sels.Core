using Sels.Core.Command.Contracts.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sels.Core.Extensions;
using Sels.Core.Command.Linux.Commands;
using Sels.Core.Command.Linux.Commands.Core;

namespace Sels.Core.Command.Linux.Extensions
{
    /// <summary>
    /// Contains extension methods for executing linux commands.
    /// </summary>
    public static class LinuxCommandExtensions
    {
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
    }
}
