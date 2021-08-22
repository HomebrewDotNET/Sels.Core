using Sels.Core.Contracts.Commands;
using Sels.Core.Extensions;
using Sels.Core.Linux.Components.LinuxCommand.Attributes;
using Sels.Core.Linux.Contracts.LinuxCommand.Commands;
using Sels.Core.Linux.Templates.LinuxCommand;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Linux.Components.LinuxCommand.Commands.Core
{
    /// <summary>
    /// Execute string commands with shell.
    /// </summary>
    public class DynamicCommand : ShellCommand
    {
        /// <summary>
        /// Command to run.
        /// </summary>
        [TextArgument(required: true)]
        public string Command { get; set; }

        public DynamicCommand() 
        {

        }

        public DynamicCommand(string command) 
        {
            Command = command;
        }

        public DynamicCommand(ICommand command) 
        {
            Command = command.ValidateArgument(nameof(command)).BuildCommand();
        }        
    }
}
