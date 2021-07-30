using Sels.Core.Components.Enumeration.Value;
using Sels.Core.Contracts.Commands;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Object;
using Sels.Core.Linux.Components.LinuxCommand;
using Sels.Core.Linux.Contracts.LinuxCommand;
using Sels.Core.Linux.Templates.Commands.Bash;
using Sels.Core.Linux.Templates.LinuxCommand;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Sels.Core.Linux.Commands.Core
{
    /// <summary>
    /// Command that chains together multiple commands.
    /// </summary>
    /// <typeparam name="TCommandResult">Result of final command</typeparam>
    public class ChainCommand<TCommandResult> : BashCommand<TCommandResult>
    {
        // Properties
        public ICommand StartCommand { get; }
        public ReadOnlyCollection<(CommandChain Chain, ICommand Command)> IntermediateCommands { get; }
        public CommandChain FinalChain { get; }
        public ILinuxCommand<TCommandResult> FinalCommand { get; }

        public ChainCommand(ICommand startCommand, CommandChain finalChain, ILinuxCommand<TCommandResult> finalCommand) : base()
        {
            StartCommand = startCommand.ValidateArgument(nameof(startCommand));
            FinalChain = finalChain;
            FinalCommand = finalCommand;
        }

        public ChainCommand(ICommand startCommand, IEnumerable<(CommandChain Chain, ICommand Command)> intermediateCommands, CommandChain finalChain, ILinuxCommand<TCommandResult> finalCommand) : this(startCommand, finalChain, finalCommand)
        {
            intermediateCommands.ValidateArgument(nameof(intermediateCommands));
            IntermediateCommands = new ReadOnlyCollection<(CommandChain Chain, ICommand Command)>(intermediateCommands.ValidateArgument(x => !x.Any(c => c.Command == null), $"Command cannot be null").ToList());
        }

        public ChainCommand(ICommand startCommand, CommandChain firstChain, ICommand firstCommand, CommandChain finalChain, ILinuxCommand<TCommandResult> finalCommand) : this(startCommand, Helper.Lists.Combine((firstChain, firstCommand)), finalChain, finalCommand)
        {

        }

        public ChainCommand(ICommand startCommand, CommandChain firstChain, ICommand firstCommand, CommandChain secondChain, ICommand secondCommand, CommandChain finalChain, ILinuxCommand<TCommandResult> finalCommand) : this(startCommand, Helper.Lists.Combine((firstChain, firstCommand), (secondChain, secondCommand)), finalChain, finalCommand)
        {

        }

        public ChainCommand(ICommand startCommand, CommandChain firstChain, ICommand firstCommand, CommandChain secondChain, ICommand secondCommand, CommandChain thirdChain, ICommand thirdCommand, CommandChain finalChain, ILinuxCommand<TCommandResult> finalCommand) : this(startCommand, Helper.Lists.Combine((firstChain, firstCommand), (secondChain, secondCommand), (thirdChain, thirdCommand)), finalChain, finalCommand)
        {

        }

        public ChainCommand(ICommand startCommand, CommandChain firstChain, ICommand firstCommand, CommandChain secondChain, ICommand secondCommand, CommandChain thirdChain, ICommand thirdCommand, CommandChain fourthChain, ICommand fourthCommand, CommandChain finalChain, ILinuxCommand<TCommandResult> finalCommand) : this(startCommand, Helper.Lists.Combine((firstChain, firstCommand), (secondChain, secondCommand), (thirdChain, thirdCommand), (fourthChain, fourthCommand)), finalChain, finalCommand)
        {

        }

        public ChainCommand(ICommand startCommand, CommandChain firstChain, ICommand firstCommand, CommandChain secondChain, ICommand secondCommand, CommandChain thirdChain, ICommand thirdCommand, CommandChain fourthChain, ICommand fourthCommand, CommandChain fifthChain, ICommand fifthCommand, CommandChain finalChain, ILinuxCommand<TCommandResult> finalCommand) : this(startCommand, Helper.Lists.Combine((firstChain, firstCommand), (secondChain, secondCommand), (thirdChain, thirdCommand), (fourthChain, fourthCommand), (fifthChain, fifthCommand)), finalChain, finalCommand)
        {

        }

        protected override string BuildArguments()
        {
            var builder = new StringBuilder();
            builder.Append(StartCommand.BuildCommand()).AppendSpace();

            if (IntermediateCommands.HasValue())
            {
                foreach (var chainedCommand in IntermediateCommands)
                {
                    builder.Append(chainedCommand.Chain.GetValue()).AppendSpace().Append(chainedCommand.Command.BuildCommand()).AppendSpace();
                }
            }

            builder.Append(FinalChain.GetValue()).AppendSpace().Append(FinalCommand.BuildCommand());

            return builder.ToString();
        }

        public override TCommandResult CreateResult(bool wasSuccesful, int exitCode, string output, string error)
        {
            return FinalCommand.CreateResult(wasSuccesful, exitCode, output, error);
        }
    }

    /// <summary>
    /// Command that chains together multiple commands.
    /// </summary>
    public class ChainCommand : ChainCommand<string>, ICommand
    {

        public ChainCommand(ICommand startCommand, CommandChain finalChain, ILinuxCommand finalCommand) : base(startCommand, finalChain, finalCommand)
        {

        }

        public ChainCommand(ICommand startCommand, IEnumerable<(CommandChain Chain, ICommand Command)> intermediateCommands, CommandChain finalChain, ILinuxCommand finalCommand) : base(startCommand, intermediateCommands, finalChain, finalCommand)
        {
           
        }

        public ChainCommand(ICommand startCommand, CommandChain firstChain, ICommand firstCommand, CommandChain finalChain, ILinuxCommand finalCommand) : base(startCommand, firstChain, firstCommand, finalChain, finalCommand)
        {

        }

        public ChainCommand(ICommand startCommand, CommandChain firstChain, ICommand firstCommand, CommandChain secondChain, ICommand secondCommand, CommandChain finalChain, ILinuxCommand finalCommand) : base(startCommand, firstChain, firstCommand, secondChain, secondCommand, finalChain, finalCommand)
        {

        }

        public ChainCommand(ICommand startCommand, CommandChain firstChain, ICommand firstCommand, CommandChain secondChain, ICommand secondCommand, CommandChain thirdChain, ICommand thirdCommand, CommandChain finalChain, ILinuxCommand finalCommand) : base(startCommand, firstChain, firstCommand, secondChain, secondCommand, thirdChain, thirdCommand, finalChain, finalCommand)
        {

        }

        public ChainCommand(ICommand startCommand, CommandChain firstChain, ICommand firstCommand, CommandChain secondChain, ICommand secondCommand, CommandChain thirdChain, ICommand thirdCommand, CommandChain fourthChain, ICommand fourthCommand, CommandChain finalChain, ILinuxCommand finalCommand) : base(startCommand, firstChain, firstCommand, secondChain, secondCommand, thirdChain, thirdCommand, fourthChain, fourthCommand, finalChain, finalCommand)
        {

        }

        public ChainCommand(ICommand startCommand, CommandChain firstChain, ICommand firstCommand, CommandChain secondChain, ICommand secondCommand, CommandChain thirdChain, ICommand thirdCommand, CommandChain fourthChain, ICommand fourthCommand, CommandChain fifthChain, ICommand fifthCommand, CommandChain finalChain, ILinuxCommand finalCommand) : base(startCommand, firstChain, firstCommand, secondChain, secondCommand, thirdChain, thirdCommand, fourthChain, fourthCommand, fifthChain, fifthCommand, finalChain, finalCommand)
        {

        }
    }

    public enum CommandChain
    {
        /// <summary>
        /// Always chain regardless of exit code of previous command.
        /// </summary>
        [EnumValue(";")]
        Always,
        /// <summary>
        /// Only chain if previous command was executed succesfully.
        /// </summary>
        [EnumValue("&&")]
        OnSuccess,
        /// <summary>
        /// Only chain if previous command failed to execute properly.
        /// </summary>
        [EnumValue("||")]
        OnFail,
        /// <summary>
        /// Pipe output from previous command to current command.
        /// </summary>
        [EnumValue("|")]
        Pipe,
        /// <summary>
        /// Links commands with a space
        /// </summary>
        [EnumValue(" ")]
        None
    }
}
