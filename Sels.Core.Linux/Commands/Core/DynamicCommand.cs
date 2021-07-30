using Sels.Core.Contracts.Commands;
using Sels.Core.Extensions;
using Sels.Core.Linux.Components.LinuxCommand.Attributes;
using Sels.Core.Linux.Templates.LinuxCommand;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Linux.Commands.Core
{
    /// <summary>
    /// Execute string commands with shell.
    /// </summary>
    public class DynamicCommand : BaseLinuxCommand<string>
    {
        /// <summary>
        /// Command to run.
        /// </summary>
        [TextArgument(required: true)]
        public string Command { get; set; }

        public DynamicCommand() : base(LinuxConstants.Commands.Shell)
        {

        }

        public DynamicCommand(string command) : this()
        {
            Command = command;
        }

        public DynamicCommand(ICommand command) : this()
        {
            Command = command.ValidateArgument(nameof(command)).BuildCommand();
        }

        public override bool RunCommand(out string output, out string error, out int exitCode)
        {
            return LinuxHelper.Program.Run(BuildArguments(), out output, out error, out exitCode, token: CancellationToken);
        }

        public override string BuildCommand()
        {
            return Command;
        }
    }
}
