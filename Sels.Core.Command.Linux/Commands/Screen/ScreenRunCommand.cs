using Sels.Core.Extensions;
using Sels.Core.Command.Linux.Commands.Core;
using Sels.Core.Command.Linux.Templates.Commands.Screen;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Sels.Core.Command.Linux.Attributes;
using Sels.Core.Command.Contracts.Commands;
using Sels.Core.Command.Linux.Contracts;

namespace Sels.Core.Command.Linux.Commands.Screen
{
    /// <summary>
    /// Used to launch a new screen that will execute a command.
    /// </summary>
    public class ScreenRunCommand : ScreenCommand<ILinuxCommandResult<string, string>>
    {
        // Properties
        /// <summary>
        /// Command to run in a screen session.
        /// </summary>
        [CommandArgument(order: 3, required: true)]
        public ICommand Command { get; set; }
        /// <summary>
        /// Name of the screen session.
        /// </summary>
        [TextArgument(prefix: "-S" , parsingOption: TextParsingOptions.DoubleQuotes, order: 2)]
        public string? SessionName { get; set; }
        /// <summary>
        /// If the new screen instance should run detached when created.
        /// </summary>
        [FlagArgument(flag: "-d", order: 1)]
        public bool Detached { get; set; } = true;
        /// <summary>
        /// Always create a new screen even when called from another screen.
        /// </summary>
        [FlagArgument(flag: "-m", order: 1)]
        public bool EnforcedCreation { get; set; } = true;

        public ScreenRunCommand(string command, string? sessionName = null) : this(new DynamicCommand(command.ValidateArgumentNotNullOrWhitespace(nameof(command))), sessionName)
        {

        }

        public ScreenRunCommand(ICommand command, string? sessionName = null)
        {
            Command = command.ValidateArgument(nameof(command));
            SessionName = sessionName;
        }

        public ScreenRunCommand()
        {

        }

        /// <inheritdoc/>
        public override ILinuxCommandResult<string, string> CreateResult(bool wasSuccesful, int exitCode, string? output, string? error, IEnumerable<ILogger>? loggers = null)
        {
            error = !wasSuccesful && !error.HasValue() ? output : error;

            return new LinuxCommandResult<string>(!wasSuccesful, output, error, exitCode);
        }
    }
}
