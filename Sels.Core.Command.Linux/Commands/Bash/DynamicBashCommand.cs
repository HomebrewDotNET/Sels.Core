using Sels.Core.Command.Linux.Templates.Commands.Bash;
using System;
using System.Collections.Generic;
using System.Text;
using Sels.Core.Command.Linux.Attributes;

namespace Sels.Core.Command.Linux.Commands.Bash
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
