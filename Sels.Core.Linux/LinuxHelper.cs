using Sels.Core.Contracts.Commands;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Object;
using Sels.Core.Extensions.Reflection;
using Sels.Core.Linux.Components.LinuxCommand.Commands;
using Sels.Core.Linux.Components.LinuxCommand.Attributes;
using Sels.Core.Linux.Contracts.LinuxCommand;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Sels.Core.Linux
{
    public static class LinuxHelper
    {
        public static class Command
        {
            /// <summary>
            /// Builds an argument string using the LinuxArgument attributes on the properties of the supplied bashCommand 
            /// </summary>
            /// <param name="bashCommand">Bash command to build arguments for</param>
            /// <param name="additionalArguments">Optiona arguments that should also be added</param>
            /// <typeparam name="TName">Type of object that represents the command name</typeparam>
            public static string BuildLinuxArguments(ICommand bashCommand, IEnumerable<(string Argument, int Order)> additionalArguments = null)
            {
                if (bashCommand.HasValue())
                {
                    // Get all properties and group properties in a dictionary and select first LinuxArgument, then filter out properties without any LinuxArgument. Last select argument value from attribute and the order. 
                    var propertyArguments = bashCommand.GetProperties().ToDictionary(x => x, x => x.GetCustomAttributes().FirstOrDefault(a => a.IsAssignableTo<LinuxArgument>()).AsOrDefault<LinuxArgument>()).Where(x => x.Value.HasValue()).Select(x => (Argument: x.Value.GetArgument(x.Key.Name, x.Key.GetValue(bashCommand)), Order: x.Value.Order));

                    if (additionalArguments.HasValue())
                    {
                        propertyArguments = Helper.Lists.Merge(propertyArguments, additionalArguments);
                    }

                    var builder = new StringBuilder();

                    // Order the arguments by Order keeping the ones without a defined order last.
                    foreach (var argument in propertyArguments.OrderByDescending(x => x.Order > LinuxConstants.DefaultLinuxArgumentOrder).ThenBy(x => x.Order).Select(x => x.Argument))
                    {
                        if (!argument.IsNullOrEmpty())
                        {
                            builder.Append(argument).AppendSpace();
                        }
                    }

                    return builder.ToString();
                }

                return string.Empty;
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
            public static string BuildLinuxCommandString(ICommand firstCommand, IEnumerable<(CommandChainer Chain, ICommand Command)> intermediateCommands, CommandChainer chain, ICommand finalCommand)
            {
                firstCommand.ValidateArgument(nameof(firstCommand));
                firstCommand.ValidateArgument(nameof(finalCommand));

                var builder = new StringBuilder();
                builder.Append(firstCommand.BuildCommand()).AppendSpace();

                if (intermediateCommands.HasValue())
                {
                    foreach (var chainedCommand in intermediateCommands)
                    {
                        builder.Append(chainedCommand.Chain.GetValue()).AppendSpace().Append(chainedCommand.Command.BuildCommand()).AppendSpace();
                    }
                }

                builder.Append(chain.GetValue()).AppendSpace().Append(finalCommand.BuildCommand());

                return builder.ToString();
            }
        }

        public static class Program
        {
            public static string EscapeString { get; } = "\\";
            public static string[] StringsToEscape { get; } = new string[] { "\"" };

            /// <summary>
            /// Runs <paramref name="program"/> with <paramref name="arguments"/>.
            /// </summary>
            /// <param name="program">Program to run</param>
            /// <param name="arguments">Arguments for <paramref name="program"/></param>
            /// <param name="output">Stout of command</param>
            /// <param name="error">Sterr of command</param>
            /// <param name="exitCode">Exit code of command</param>
            /// <param name="succesExitCode">Exit code indicating succesful execution</param>
            /// <returns>Boolean indicating if the command was executed successfully</returns>
            public static bool Run(string program, string arguments, out string output, out string error, out int exitCode, int succesExitCode = LinuxConstants.SuccessExitCode, CancellationToken token = default)
            {
                program.ValidateArgument(nameof(program));

                exitCode = Helper.Program.Run(program, arguments, out output, out error, token);

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
            /// <returns>Boolean indicating if the command was executed successfully</returns>
            public static bool Run(string command, out string output, out string error, out int exitCode, int succesExitCode = LinuxConstants.SuccessExitCode, CancellationToken token = default)
            {
                command.ValidateArgument(nameof(command));

                return Run(LinuxConstants.Commands.Shell, FormatStringCommand(command), out output, out error, out exitCode, succesExitCode, token);
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
                /// <returns>Boolean indicating if the command was executed successfully</returns>
                public static bool Run(string command, out string output, out string error, out int exitCode, int succesExitCode = LinuxConstants.SuccessExitCode, CancellationToken token = default)
                {
                    command.ValidateArgument(nameof(command));

                    return Program.Run(LinuxConstants.Commands.Bash, FormatStringCommand(command), out output, out error, out exitCode, succesExitCode, token);
                }
            }
            #endregion
        }
    }
}
