using Sels.Core.Command.Linux.Templates.Commands.Bash;
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
        /// <inheritdoc cref="DynamicBashCommand"/>
        /// <param name="command">The command to execute</param>
        public DynamicBashCommand(string command)
        {
            Command = command;
        }
        /// <inheritdoc cref="DynamicBashCommand"/>
        public DynamicBashCommand()
        {

        }
    }
}
