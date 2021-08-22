using Sels.Core.Contracts.Commands;
using Sels.Core.Extensions;
using Sels.Core.Linux.Components.LinuxCommand.Commands;
using Sels.Core.Linux.Components.LinuxCommand.Commands.Core;
using Sels.Core.Linux.Contracts.LinuxCommand;
using Sels.Core.Linux.Contracts.LinuxCommand.Commands;
using Sels.Core.Linux.Templates.LinuxCommand;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Sels.Core.Linux.Templates.LinuxCommand
{
    /// <summary>
    /// Template for creating commands that consists of other commands.
    /// </summary>
    /// <typeparam name="TCommandResult">Result from executing the command</typeparam>
    public abstract class MultiCommand<TCommandResult> : ShellCommand<TCommandResult>
    {
        public MultiCommand() 
        {

        }

        protected override string BuildArguments()
        {
            var commandChain = BuildCommandChain(new MultiCommandBuilder());

            commandChain.ValidateArgument(x => x.HasValue(), x => new NotSupportedException($"{nameof(BuildCommandChain)} cannot return null"));

            return LinuxHelper.Command.BuildLinuxCommandString(commandChain.StartCommand, commandChain.IntermediateCommands, commandChain.FinalChain, commandChain.FinalCommand);
        }

        // Abstractions
        /// <summary>
        /// Used to build the chain that will be executed by this multi command.
        /// </summary>
        /// <param name="chainSetup">Object to start building the chain</param>
        /// <returns>Chain of command to execute</returns>
        protected abstract IMultiCommandChain BuildCommandChain(IMultiCommandStartSetup chainSetup);
    }

    /// <summary>
    /// Template for creating commands that consists of other commands.
    /// </summary>
    public abstract class MultiCommand : MultiCommand<ILinuxCommandResult<string, string>>, ILinuxCommand
    {
        public override ILinuxCommandResult<string, string> CreateResult(bool wasSuccesful, int exitCode, string output, string error)
        {
            return new LinuxCommandResult<string, string>(!wasSuccesful, output, error, exitCode);
        }
    }

    internal class MultiCommandBuilder : IMultiCommandStartSetup, IMultiCommandSetup, IMultiCommandChain
    {
        // Fields
        private readonly List<(CommandChainer Chain, ICommand Command)> _intermediateCommands = new List<(CommandChainer Chain, ICommand Command)>();

        // Properties
        public ICommand StartCommand { get; private set; }

        public ReadOnlyCollection<(CommandChainer Chain, ICommand Command)> IntermediateCommands => new ReadOnlyCollection<(CommandChainer Chain, ICommand Command)>(_intermediateCommands);

        public CommandChainer FinalChain { get; private set; }

        public ICommand FinalCommand { get; private set; }

        public IMultiCommandSetup StartWith(ICommand startCommand)
        {
            startCommand.ValidateArgument(nameof(startCommand));

            StartCommand = startCommand;

            return this;
        }

        public IMultiCommandSetup ContinueWith(CommandChainer chain, ICommand command)
        {
            command.ValidateArgument(nameof(command));

            _intermediateCommands.Add((chain, command));

            return this;
        }

        public IMultiCommandChain EndWith(CommandChainer finalChain, ICommand finalCommand)
        {
            finalCommand.ValidateArgument(nameof(finalCommand));

            FinalChain = finalChain;
            FinalCommand = finalCommand;

            return this;
        }


    }
}
