using Sels.Core.Contracts.Commands;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Reflection;
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
        #region Arguments
        public static class Arguments
        {
            /// <summary>
            /// Builds an argument string using the LinuxArgument attributes on the properties of the supplied bashCommand 
            /// </summary>
            /// <param name="bashCommand">Bash command to build arguments for</param>
            /// <typeparam name="TName">Type of object that represents the command name</typeparam>
            public static string BuildLinuxArguments(ICommand bashCommand)
            {
                if (bashCommand.HasValue())
                {
                    // Get all properties and group properties in a dictionary and select first LinuxArgument, then filter out properties without any LinuxArgument and order by Order keeping the arguments without a defined order at the bottom
                    var argumentProperties = bashCommand.GetProperties().ToDictionary(x => x, x => x.GetCustomAttributes().FirstOrDefault(a => a.IsAssignableTo<LinuxArgument>()).AsOrDefault<LinuxArgument>()).Where(x => x.Value.HasValue()).OrderByDescending(x => x.Value.Order > LinuxConstants.DefaultLinuxArgumentOrder).ThenBy(x => x.Value.Order);

                    var builder = new StringBuilder();

                    // Loop over arguments and generate the argument using the property value
                    foreach (var argumentProperty in argumentProperties)
                    {
                        var property = argumentProperty.Key;
                        var attribute = argumentProperty.Value;

                        var argument = attribute.GetArgument(property.Name, property.GetValue(bashCommand));

                        if (!argument.IsNullOrEmpty())
                        {
                            builder.Append(argument).AppendSpace();
                        }
                    }

                    return builder.ToString();
                }

                return string.Empty;
            }
        }
        #endregion

        #region Program
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
        #endregion
    }
}
