using Sels.Core.Extensions;
using Sels.Core.Command.Linux.Templates;
using System;
using System.Collections.Generic;
using System.Text;
using Sels.Core.Command.Linux.Attributes;
using Sels.Core.Command.Contracts.Commands;

namespace Sels.Core.Command.Linux.Commands.Core
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
