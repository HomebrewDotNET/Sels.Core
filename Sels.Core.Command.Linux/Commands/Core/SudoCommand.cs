using Microsoft.Extensions.Logging;
using Sels.Core.Extensions;
using Sels.Core.Command.Linux.Contracts;
using Sels.Core.Command.Linux.Templates;
using Sels.Core.Command.Linux.Attributes;
using Sels.Core.Command.Contracts.Commands;

namespace Sels.Core.Command.Linux.Commands.Core
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
        public ICommand<TResult> Command { get; set; }

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
        /// <summary>
        /// Executes other commands as super user.
        /// </summary>
        public SudoCommand() : base(LinuxCommandConstants.Commands.Sudo)
        {

        }
        /// <summary>
        /// Executes other commands as super user.
        /// </summary>
        /// <param name="command">The command to execute as sudo</param>
        public SudoCommand(ICommand<TResult> command) : this()
        {
            Command = command.ValidateArgument(nameof(command));
        }

        /// <inheritdoc/>
        public override TResult CreateResult(bool wasSuccesful, int exitCode, string? output, string? error, IEnumerable<ILogger>? loggers = null)
        {
            return Command.CreateResult(wasSuccesful, exitCode, output, error);
        }
    }

    /// <summary>
    /// Executes other commands as super user.
    /// </summary>
    public class SudoCommand: SudoCommand<ILinuxCommandResult<string, string>>, ILinuxCommand
    {
        /// <summary>
        /// Executes other commands as super user.
        /// </summary>
        public SudoCommand() : base()
        {

        }
        /// <summary>
        /// Executes other commands as super user.
        /// </summary>
        /// <param name="command">The command to execute as sudo</param>
        public SudoCommand(string command) : base(new DynamicCommand(command.ValidateArgumentNotNullOrWhitespace(nameof(command))))
        {

        }
        /// <summary>
        /// Executes other commands as super user.
        /// </summary>
        /// <param name="command">The command to execute as sudo</param>
        public SudoCommand(ICommand<ILinuxCommandResult<string, string>> command) : base(command)
        {

        }
        /// <summary>
        /// Executes other commands as super user.
        /// </summary>
        /// <param name="command">The command to execute as sudo</param>
        public SudoCommand(ILinuxCommand command) : base(command)
        {

        }
    }
}
