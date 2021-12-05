﻿using Microsoft.Extensions.Logging;
using Sels.Core.Components.Commands;
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
        /// <summary>
        /// Template for creating commands that consists of other commands.
        /// </summary>
        public MultiCommand() 
        {

        }

        /// <inheritdoc/>
        protected override string BuildArguments(IEnumerable<ILogger> loggers = null)
        {
            var commandChain = BuildCommandChain(new MultiCommandBuilder<CommandChainer>());

            commandChain.ValidateArgument(x => x.HasValue(), x => new NotSupportedException($"{nameof(BuildCommandChain)} cannot return null"));

            return LinuxHelper.Command.BuildLinuxCommandString(commandChain.StartCommand, commandChain.IntermediateCommands, commandChain.FinalChain, commandChain.FinalCommand);
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
        public override ILinuxCommandResult<string, string> CreateResult(bool wasSuccesful, int exitCode, string output, string error, IEnumerable<ILogger> loggers = null)
        {
            return new LinuxCommandResult<string, string>(!wasSuccesful, output, error, exitCode);
        }
    }


}
