using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;
using Sels.Core.Extensions.Logging;
using Sels.Core.Command.Contracts.Commands;
using Sels.Core.Command.Linux.Templates.Attributes;
using Sels.Core.Command.Linux.Commands;
using Sels.Core.Attributes.Enumeration.Value;

namespace Sels.Core.Command.Linux
{
    /// <summary>
    /// Contains helper methods for running Linux commands.
    /// </summary>
    public static class LinuxCommandHelper
    {
        /// <summary>
        /// Contains static helper methods for working with linux commands.
        /// </summary>
        public static class Command
        {
            /// <summary>
            /// Builds an argument string using the LinuxArgument attributes on the properties of the supplied bashCommand 
            /// </summary>
            /// <param name="command">Bash command to build arguments for</param>
            /// <param name="additionalArguments">Optional arguments that should also be added</param>
            /// <param name="loggers">Optional loggers for tracing</param>
            public static string BuildLinuxArguments(ICommand command, IEnumerable<(string Argument, int Order)>? additionalArguments = null, IEnumerable <ILogger>? loggers = null)
            {
                command.ValidateArgument(nameof(command));
                var commandName = command.GetType().Name;

                using var logger = loggers.CreateTimedLogger(LogLevel.Debug, $"Building linux arguments for command {commandName}", x => $"Built linux arguments for command {commandName} in {x.PrintTotalMs()}");

                // Get all properties and group properties in a dictionary and select first LinuxArgument, then filter out properties without any LinuxArgument. Last select argument value from attribute and the order. 
                var propertyArguments = command.GetProperties().ToDictionary(x => x, x => x.GetCustomAttributes().FirstOrDefault(a => a.IsAssignableTo<LinuxArgument>()).CastOrDefault<LinuxArgument>()).Where(x => x.Value.HasValue()).Select(x => (Argument: x.Value.GetArgument(x.Key.Name, x.Key.GetValue(command)), Order: x.Value.Order)).ToList();

                logger.Log((time, log) => log.LogMessage(LogLevel.Trace, $"Building {propertyArguments.Count} arguments for command {commandName} ({time.PrintTotalMs()})"));

                if (additionalArguments.HasValue())
                {
                    propertyArguments = Helper.Lists.Merge(propertyArguments, additionalArguments);
                }

                var builder = new StringBuilder();

                // Order the arguments by Order keeping the ones without a defined order last.
                foreach (var argument in propertyArguments.OrderByDescending(x => x.Order > LinuxCommandConstants.DefaultLinuxArgumentOrder).ThenBy(x => x.Order).Select(x => x.Argument))
                {
                    if (!argument.IsNullOrEmpty())
                    {
                        logger.Log((time, log) => log.LogMessage(LogLevel.Trace, $"Appending argument <{argument}> for command {commandName} ({time.PrintTotalMs()})"));
                        builder.Append(argument).AppendSpace();
                    }
                }

                return builder.ToString();
            }

            /// <summary>
            /// Builds a command string by chaining all the provided commands together using a <see cref="CommandChainer"/>.
            /// </summary>
            /// <param name="firstCommand">First command in the chain</param>
            /// <param name="chain">How to chain together <paramref name="firstCommand"/> and <paramref name="finalCommand"/></param>
            /// <param name="finalCommand">Final command to chain</param>
            /// <returns>Command string of all chained command</returns>
            public static string BuildLinuxCommandString(ICommand firstCommand, CommandChainer chain, ICommand finalCommand)
            {
                firstCommand.ValidateArgument(nameof(firstCommand));
                firstCommand.ValidateArgument(nameof(finalCommand));

                return BuildLinuxCommandString(firstCommand, null, chain, finalCommand);
            }

            /// <summary>
            /// Builds a command string by chaining all the provided commands together using a <see cref="CommandChainer"/>.
            /// </summary>
            /// <param name="firstCommand">First command in the chain</param>
            /// <param name="intermediateCommands">List of ordered commands that will be chained after <paramref name="firstCommand"/></param>
            /// <param name="chain">How to chain together <paramref name="firstCommand"/> and <paramref name="finalCommand"/></param>
            /// <param name="finalCommand">Final command to chain</param>
            /// <returns>Command string of all chained command</returns>
            public static string BuildLinuxCommandString(ICommand firstCommand, IEnumerable<(CommandChainer Chain, ICommand Command)>? intermediateCommands, CommandChainer chain, ICommand finalCommand)
            {
                firstCommand.ValidateArgument(nameof(firstCommand));
                firstCommand.ValidateArgument(nameof(finalCommand));

                var builder = new StringBuilder();
                builder.Append(firstCommand.BuildCommand()).AppendSpace();

                if (intermediateCommands != null)
                {
                    foreach (var chainedCommand in intermediateCommands)
                    {
                        builder.Append(chainedCommand.Chain.GetStringValue()).AppendSpace().Append(chainedCommand.Command.BuildCommand()).AppendSpace();
                    }
                }

                builder.Append(chain.GetStringValue()).AppendSpace().Append(finalCommand.BuildCommand());

                return builder.ToString();
            }
        }
        /// <summary>
        /// Contains static helper methods for running processes on linux.
        /// </summary>
        public static class Program
        {
            private static string EscapeString { get; } = "\\";
            private static string[] StringsToEscape { get; } = new string[] { "\"" };

            /// <summary>
            /// Runs <paramref name="program"/> with <paramref name="arguments"/>.
            /// </summary>
            /// <param name="program">Program to run</param>
            /// <param name="arguments">Arguments for <paramref name="program"/></param>
            /// <param name="output">Stout of command</param>
            /// <param name="error">Sterr of command</param>
            /// <param name="exitCode">Exit code of command</param>
            /// <param name="succesExitCode">Exit code indicating succesful execution</param>
            /// <param name="loggers">Optional loggers for tracing</param>
            /// <param name="token">Optional token for cancelling the process</param>
            /// <returns>Boolean indicating if the command was executed successfully</returns>
            public static bool Run(string program, string arguments, out string output, out string error, out int exitCode, int succesExitCode = CommandConstants.SuccessExitCode, CancellationToken token = default, IEnumerable<ILogger>? loggers = null)
            {
                program.ValidateArgument(nameof(program));

                exitCode = Helper.Program.Run(program, arguments, out output, out error, token, loggers);

                return exitCode == succesExitCode;
            }

            /// <summary>
            /// Runs <paramref name="command"/> with Shell.
            /// </summary>
            /// <param name="command">Command to run</param>
            /// <param name="output">Stout of command</param>
            /// <param name="error">Sterr of command</param>
            /// <param name="exitCode">Exit code of command</param>
            /// <param name="succesExitCode">Exit code indicating succesful execution</param>
            /// <param name="loggers">Optional loggers for tracing</param>
            /// <param name="token">Optional token for cancelling the process</param>
            /// <returns>Boolean indicating if the command was executed successfully</returns>
            public static bool Run(string command, out string output, out string error, out int exitCode, int succesExitCode = CommandConstants.SuccessExitCode, CancellationToken token = default, IEnumerable<ILogger>? loggers = null)
            {
                command.ValidateArgument(nameof(command));

                return Run(LinuxCommandConstants.Commands.Shell, FormatStringCommand(command), out output, out error, out exitCode, succesExitCode, token, loggers);
            }

            /// <summary>
            /// Format the command so it can be run with Bash/Shell.
            /// </summary>
            /// <param name="command">Command to format</param>
            /// <returns>Formatted command</returns>
            public static string FormatStringCommand(string command)
            {
                command.ValidateArgument(nameof(command));

                return $"-c \"{command.EscapeStrings(EscapeString, StringsToEscape)}\"";
            }

            #region Bash
            /// <summary>
            /// Contains static helper methods for working with bash.
            /// </summary>
            public static class Bash
            {
                /// <summary>
                /// Runs <paramref name="command"/> with Bash.
                /// </summary>
                /// <param name="command">Command to run</param>
                /// <param name="output">Stout of command</param>
                /// <param name="error">Sterr of command</param>
                /// <param name="exitCode">Exit code of command</param>
                /// <param name="succesExitCode">Exit code indicating succesful execution</param>
                /// <param name="loggers">Optional loggers for tracing</param>
                /// <param name="token">Optional token for cancelling the process</param>
                /// <returns>Boolean indicating if the command was executed successfully</returns>
                public static bool Run(string command, out string output, out string error, out int exitCode, int succesExitCode = CommandConstants.SuccessExitCode, CancellationToken token = default, IEnumerable<ILogger>? loggers = null)
                {
                    command.ValidateArgument(nameof(command));

                    return Program.Run(LinuxCommandConstants.Commands.Bash, FormatStringCommand(command), out output, out error, out exitCode, succesExitCode, token, loggers);
                }
            }
            #endregion
        }
    }
}
