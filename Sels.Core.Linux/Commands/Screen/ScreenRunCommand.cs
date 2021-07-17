using Sels.Core.Contracts.Commands;
using Sels.Core.Extensions;
using Sels.Core.Linux.Commands.Core;
using Sels.Core.Linux.Components.LinuxCommand;
using Sels.Core.Linux.Components.LinuxCommand.Attributes;
using Sels.Core.Linux.Templates.Commands.Screen;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Linux.Commands.Screen
{
    public class ScreenRunCommand : ScreenCommand<LinuxCommandResult<string>>
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
        public string SessionName { get; set; }
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

        public ScreenRunCommand(string command, string sessionName = null) : this(new DynamicCommand(command.ValidateArgumentNotNullOrWhitespace(nameof(command))), sessionName)
        {

        }

        public ScreenRunCommand(ICommand command, string sessionName = null)
        {
            Command = command.ValidateArgument(nameof(command));
            SessionName = sessionName;
        }

        public ScreenRunCommand()
        {

        }

        public override LinuxCommandResult<string> CreateResult(bool wasSuccesful, int exitCode, string output, string error)
        {
            error = !wasSuccesful && !error.HasValue() ? output : error;

            return new LinuxCommandResult<string>(!wasSuccesful, output, error, exitCode);
        }
    }
}
