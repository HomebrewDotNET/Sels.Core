using Microsoft.Extensions.Logging;
using Sels.Core.Extensions;
using Sels.Core.Command.Components.Commands;
using Sels.Core.Command.Linux.Commands;
using Sels.Core.Command.Contracts.Commands;
using Sels.Core.Command.Linux.Contracts;

namespace Sels.Core.Command.Linux.Templates
{
    /// <summary>
    /// Template for creating commands that consists of other commands.
    /// </summary>
    /// <typeparam name="TCommandResult">Result from executing the command</typeparam>
    public abstract class MultiCommand<TCommandResult> : ShellCommand<TCommandResult>
    {
        /// <summary>
        /// Template for creating commands that consists of other commands.
        /// </summary>
        public MultiCommand() 
        {

        }

        /// <inheritdoc/>
        protected override string BuildArguments(IEnumerable<ILogger>? loggers = null)
        {
            var commandChain = BuildCommandChain(new MultiCommandBuilder<CommandChainer>());

            commandChain.ValidateArgument(x => x.HasValue(), x => new NotSupportedException($"{nameof(BuildCommandChain)} cannot return null"));

            return LinuxCommandHelper.Command.BuildLinuxCommandString(commandChain.StartCommand, commandChain.IntermediateCommands, commandChain.FinalChain, commandChain.FinalCommand);
        }

        // Abstractions
        /// <summary>
        /// Used to build the chain that will be executed by this multi command.
        /// </summary>
        /// <param name="chainSetup">Object to start building the chain</param>
        /// <returns>Chain of command to execute</returns>
        protected abstract IMultiCommandChain<CommandChainer> BuildCommandChain(IMultiCommandStartSetup<CommandChainer> chainSetup);
    }

    /// <summary>
    /// Template for creating commands that consists of other commands.
    /// </summary>
    public abstract class MultiCommand : MultiCommand<ILinuxCommandResult<string, string>>, ILinuxCommand
    {
        /// <inheritdoc/>
        public override ILinuxCommandResult<string, string> CreateResult(bool wasSuccesful, int exitCode, string? output, string? error, IEnumerable<ILogger>? loggers = null)
        {
            return new LinuxCommandResult<string, string>(!wasSuccesful, output, error, exitCode);
        }
    }


}
