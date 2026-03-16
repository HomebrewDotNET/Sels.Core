using Sels.Core.Extensions;
using Sels.Core.Command.Linux.Templates;
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
        /// Command to execute.
        /// </summary>
        [TextArgument(required: true)]
        public string Command { get; set; }
        /// <inheritdoc cref="DynamicCommand"/>
        public DynamicCommand() 
        {

        }
        /// <inheritdoc cref="DynamicCommand"/>
        /// <param name="command">The command to execute</param>
        public DynamicCommand(string command) 
        {
            Command = command;
        }
        /// <inheritdoc cref="DynamicCommand"/>
        /// <param name="command">The command to execute</param>
        public DynamicCommand(ICommand command) 
        {
            Command = command.ValidateArgument(nameof(command)).BuildCommand();
        }        
    }
}
