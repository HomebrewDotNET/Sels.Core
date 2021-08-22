using Sels.Core.Linux.Components.LinuxCommand;
using Sels.Core.Linux.Components.LinuxCommand.Attributes;
using Sels.Core.Linux.Templates.LinuxCommand.Commands.Bash;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Linux.Components.LinuxCommand.Commands.Bash
{
    /// <summary>
    /// Used to execute commands with bash. 
    /// </summary>
    public class DynamicBashCommand : BashCommand
    {
        /// <summary>
        /// Command to execute with bash.
        /// </summary>
        [TextArgument]
        public string Command { get; set; }

        public DynamicBashCommand(string command)
        {
            Command = command;
        }
        public DynamicBashCommand()
        {

        }
    }
}
