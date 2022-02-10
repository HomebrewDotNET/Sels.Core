using Sels.Core.Extensions;
using Sels.Core.Extensions.Object;
using Sels.Core.Command.Linux.Contracts;
using Sels.Core.Command.Linux.Templates.Commands.Bash;
using Sels.Core.Command.Linux.Templates;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using Sels.Core.Command.Contracts.Commands;

namespace Sels.Core.Command.Linux.Commands.Core
{
    /// <summary>
    /// Command that chains together multiple commands.
    /// </summary>
    /// <typeparam name="TCommandResult">Result of final command</typeparam>
    public class ChainCommand<TCommandResult> : ShellCommand<TCommandResult>
    {
        // Properties
        /// <summary>
        /// First command in the chain that will be executed first.
        /// </summary>
        public ICommand StartCommand { get; }
        /// <summary>
        /// List of ordered commands that will be executed in order after <see cref="StartCommand"/>.
        /// </summary>
        public ReadOnlyCollection<(CommandChainer Chain, ICommand Command)> IntermediateCommands { get; }
        /// <summary>
        /// How <see cref="StartCommand"/> or the last command in <see cref="IntermediateCommands"/> should be linked to <see cref="FinalCommand"/>.
        /// </summary>
        public CommandChainer FinalChain { get; }
        /// <summary>
        /// Final command in the chain that will be executed and will parse the result for this command.
        /// </summary>
        public ICommand<TCommandResult> FinalCommand { get; protected set; }

        public ChainCommand(ICommand startCommand, CommandChainer finalChain, ICommand<TCommandResult> finalCommand) : base()
        {
            StartCommand = startCommand.ValidateArgument(nameof(startCommand));
            FinalChain = finalChain;
            FinalCommand = finalCommand;
        }

        public ChainCommand(ICommand startCommand, IEnumerable<(CommandChainer Chain, ICommand Command)> intermediateCommands, CommandChainer finalChain, ICommand<TCommandResult> finalCommand) : this(startCommand, finalChain, finalCommand)
        {
            intermediateCommands.ValidateArgument(nameof(intermediateCommands));
            IntermediateCommands = new ReadOnlyCollection<(CommandChainer Chain, ICommand Command)>(intermediateCommands.ValidateArgument(x => !x.Any(c => c.Command == null), $"Command cannot be null").ToList());
        }

        public ChainCommand(ICommand startCommand, CommandChainer firstChain, ICommand firstCommand, CommandChainer finalChain, ICommand<TCommandResult> finalCommand) : this(startCommand, Helper.Lists.Combine((firstChain, firstCommand)), finalChain, finalCommand)
        {

        }

        public ChainCommand(ICommand startCommand, CommandChainer firstChain, ICommand firstCommand, CommandChainer secondChain, ICommand secondCommand, CommandChainer finalChain, ICommand<TCommandResult> finalCommand) : this(startCommand, Helper.Lists.Combine((firstChain, firstCommand), (secondChain, secondCommand)), finalChain, finalCommand)
        {

        }

        public ChainCommand(ICommand startCommand, CommandChainer firstChain, ICommand firstCommand, CommandChainer secondChain, ICommand secondCommand, CommandChainer thirdChain, ICommand thirdCommand, CommandChainer finalChain, ICommand<TCommandResult> finalCommand) : this(startCommand, Helper.Lists.Combine((firstChain, firstCommand), (secondChain, secondCommand), (thirdChain, thirdCommand)), finalChain, finalCommand)
        {

        }

        public ChainCommand(ICommand startCommand, CommandChainer firstChain, ICommand firstCommand, CommandChainer secondChain, ICommand secondCommand, CommandChainer thirdChain, ICommand thirdCommand, CommandChainer fourthChain, ICommand fourthCommand, CommandChainer finalChain, ICommand<TCommandResult> finalCommand) : this(startCommand, Helper.Lists.Combine((firstChain, firstCommand), (secondChain, secondCommand), (thirdChain, thirdCommand), (fourthChain, fourthCommand)), finalChain, finalCommand)
        {

        }

        public ChainCommand(ICommand startCommand, CommandChainer firstChain, ICommand firstCommand, CommandChainer secondChain, ICommand secondCommand, CommandChainer thirdChain, ICommand thirdCommand, CommandChainer fourthChain, ICommand fourthCommand, CommandChainer fifthChain, ICommand fifthCommand, CommandChainer finalChain, ICommand<TCommandResult> finalCommand) : this(startCommand, Helper.Lists.Combine((firstChain, firstCommand), (secondChain, secondCommand), (thirdChain, thirdCommand), (fourthChain, fourthCommand), (fifthChain, fifthCommand)), finalChain, finalCommand)
        {

        }

        /// <inheritdoc/>
        protected override string BuildArguments(IEnumerable<ILogger>? loggers = null)
        {
            return LinuxCommandHelper.Command.BuildLinuxCommandString(StartCommand, IntermediateCommands, FinalChain, FinalCommand);
        }
        /// <inheritdoc/>
        public override TCommandResult CreateResult(bool wasSuccesful, int exitCode, string? output, string? error, IEnumerable<ILogger>? loggers = null)
        {
            return FinalCommand.CreateResult(wasSuccesful, exitCode, output, error);
        }
    }

    /// <summary>
    /// Command that chains together multiple commands.
    /// </summary>
    public class ChainCommand : ChainCommand<ILinuxCommandResult<string, string>>, ILinuxCommand
    {

        public ChainCommand(ICommand startCommand, CommandChainer finalChain, ILinuxCommand finalCommand) : base(startCommand, finalChain, finalCommand)
        {

        }

        public ChainCommand(ICommand startCommand, IEnumerable<(CommandChainer Chain, ICommand Command)> intermediateCommands, CommandChainer finalChain, ILinuxCommand finalCommand) : base(startCommand, intermediateCommands, finalChain, finalCommand)
        {
           
        }

        public ChainCommand(ICommand startCommand, CommandChainer firstChain, ICommand firstCommand, CommandChainer finalChain, ILinuxCommand finalCommand) : base(startCommand, firstChain, firstCommand, finalChain, finalCommand)
        {

        }

        public ChainCommand(ICommand startCommand, CommandChainer firstChain, ICommand firstCommand, CommandChainer secondChain, ICommand secondCommand, CommandChainer finalChain, ILinuxCommand finalCommand) : base(startCommand, firstChain, firstCommand, secondChain, secondCommand, finalChain, finalCommand)
        {

        }

        public ChainCommand(ICommand startCommand, CommandChainer firstChain, ICommand firstCommand, CommandChainer secondChain, ICommand secondCommand, CommandChainer thirdChain, ICommand thirdCommand, CommandChainer finalChain, ILinuxCommand finalCommand) : base(startCommand, firstChain, firstCommand, secondChain, secondCommand, thirdChain, thirdCommand, finalChain, finalCommand)
        {

        }

        public ChainCommand(ICommand startCommand, CommandChainer firstChain, ICommand firstCommand, CommandChainer secondChain, ICommand secondCommand, CommandChainer thirdChain, ICommand thirdCommand, CommandChainer fourthChain, ICommand fourthCommand, CommandChainer finalChain, ILinuxCommand finalCommand) : base(startCommand, firstChain, firstCommand, secondChain, secondCommand, thirdChain, thirdCommand, fourthChain, fourthCommand, finalChain, finalCommand)
        {

        }

        public ChainCommand(ICommand startCommand, CommandChainer firstChain, ICommand firstCommand, CommandChainer secondChain, ICommand secondCommand, CommandChainer thirdChain, ICommand thirdCommand, CommandChainer fourthChain, ICommand fourthCommand, CommandChainer fifthChain, ICommand fifthCommand, CommandChainer finalChain, ILinuxCommand finalCommand) : base(startCommand, firstChain, firstCommand, secondChain, secondCommand, thirdChain, thirdCommand, fourthChain, fourthCommand, fifthChain, fifthCommand, finalChain, finalCommand)
        {

        }
    }
}
