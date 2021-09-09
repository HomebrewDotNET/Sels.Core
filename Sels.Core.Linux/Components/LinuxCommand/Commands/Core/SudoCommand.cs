using Microsoft.Extensions.Logging;
using Sels.Core.Contracts.Commands;
using Sels.Core.Extensions;
using Sels.Core.Linux.Components.LinuxCommand.Attributes;
using Sels.Core.Linux.Contracts.LinuxCommand;
using Sels.Core.Linux.Contracts.LinuxCommand.Commands;
using Sels.Core.Linux.Templates.LinuxCommand;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Linux.Components.LinuxCommand.Commands.Core
{
    /// <summary>
    /// Executes other commands as super user.
    /// </summary>
    /// <typeparam name="TResult">Type of command result</typeparam>
    public class SudoCommand<TResult> : BaseLinuxCommand<string, TResult>
    {
        // Properties
        /// <summary>
        /// Command to run as super user.
        /// </summary>
        [CommandArgument(required: true)]
        public ILinuxCommand<TResult> Command { get; set; }

        /// <summary>
        /// Forces sudo to ask for a password.
        /// </summary>
        [FlagArgument("-k" ,order: 1)]
        public bool ForceAskForPassword { get; set; }

        /// <summary>
        /// Tells sudo to read password from stdin.
        /// </summary>
        [FlagArgument("-S" ,order: 1)]
        public bool ReadPasswordFromInput { get; set; }

        public SudoCommand() : base(LinuxConstants.Commands.Sudo)
        {

        }

        public SudoCommand(ILinuxCommand<TResult> command) : this()
        {
            Command = command.ValidateArgument(nameof(command));
        }

        public override TResult CreateResult(bool wasSuccesful, int exitCode, string output, string error, IEnumerable<ILogger> loggers = null)
        {
            return Command.CreateResult(wasSuccesful, exitCode, output, error);
        }
    }

    /// <summary>
    /// Executes other commands as super user.
    /// </summary>
    public class SudoCommand: SudoCommand<ILinuxCommandResult<string, string>>, ILinuxCommand
    {
        public SudoCommand() : base()
        {

        }

        public SudoCommand(string command) : base(new DynamicCommand(command.ValidateArgumentNotNullOrWhitespace(nameof(command))))
        {

        }

        public SudoCommand(ILinuxCommand<ILinuxCommandResult<string, string>> command) : base(command)
        {

        }

        public SudoCommand(ILinuxCommand command) : base(command)
        {

        }
    }
}
