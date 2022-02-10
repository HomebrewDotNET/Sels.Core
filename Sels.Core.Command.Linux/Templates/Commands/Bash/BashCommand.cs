using Microsoft.Extensions.Logging;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Logging;
using Sels.Core.Command.Linux.Templates;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Sels.Core.Command.Components.Commands;
using Sels.Core.Command.Linux.Contracts;
using Sels.Core.Command.Linux.Commands;

namespace Sels.Core.Command.Linux.Templates.Commands.Bash
{
    /// <summary>
    /// Base class for bash.
    /// </summary>
    /// <typeparam name="TCommandResult">Type of command result</typeparam>
    public abstract class BashCommand<TCommandResult> : BaseLinuxCommand<string, TCommandResult>
    {
        public BashCommand() : base(LinuxCommandConstants.Commands.Bash)
        {

        }

        /// <inheritdoc/>
        public override bool RunCommand(out string output, out string error, out int exitCode, CommandExecutionOptions? options = null)
        {
            using var loggers = (options.HasValue() ? options.Loggers : null).CreateTimedLogger(LogLevel.Debug, $"Running command {LoggerName}", x => $"Ran command {LoggerName} in {x.PrintTotalMs()}");
            var optionsDefined = options.HasValue();
            var succesExitCode = optionsDefined && options.SuccessExitCode.HasValue ? options.SuccessExitCode.Value : SuccessExitCode;
            CancellationToken cancellationToken = optionsDefined ? options.Token : default;

            var result = LinuxCommandHelper.Program.Bash.Run(BuildArguments(), out output, out error, out exitCode, succesExitCode, cancellationToken, options?.Loggers);

            if (optionsDefined && options.FailOnErrorOutput && error.HasValue())
            {
                loggers.Log((time, logger) => logger.LogMessage(LogLevel.Trace, $"Command {LoggerName} could not be run succesfully because error output contained value ({time.PrintTotalMs()})"));
                return false;
            }

            return result;
        }
        /// <inheritdoc/>
        public override string BuildCommand()
        {
            return $"{Name} {LinuxCommandHelper.Program.FormatStringCommand(BuildArguments())}";
        }
    }

    /// <summary>
    /// Base class for bash.
    /// </summary>
    public abstract class BashCommand : BashCommand<ILinuxCommandResult<string, string>> , ILinuxCommand
    {
        public override ILinuxCommandResult<string, string> CreateResult(bool wasSuccesful, int exitCode, string output, string error, IEnumerable<ILogger>? loggers = null)
        {
            return new LinuxCommandResult<string, string>(!wasSuccesful, output, error, exitCode);
        }
    }
}
