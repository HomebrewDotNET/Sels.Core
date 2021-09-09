using Microsoft.Extensions.Logging;
using Sels.Core.Components.Commands;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Logging;
using Sels.Core.Linux.Components.LinuxCommand.Commands;
using Sels.Core.Linux.Contracts.LinuxCommand;
using Sels.Core.Linux.Contracts.LinuxCommand.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Sels.Core.Linux.Templates.LinuxCommand
{
    /// <summary>
    /// Template for executing command using the default cli shell /bin/sh.
    /// </summary>
    /// <typeparam name="TCommandResult">Type of result that the command returns</typeparam>
    public abstract class ShellCommand<TCommandResult> : BaseLinuxCommand<string, TCommandResult>
    {
        public ShellCommand() : base(LinuxConstants.Commands.Shell)
        {

        }

        public override bool RunCommand(out string output, out string error, out int exitCode, CommandExecutionOptions options = null)
        {
            using var loggers = (options.HasValue() ? options.Loggers : null).CreateTimedLogger(LogLevel.Debug, $"Running command {LoggerName}", x => $"Ran command {LoggerName} in {x.PrintTotalMs()}");
            var optionsDefined = options.HasValue();
            var succesExitCode = optionsDefined && options.SuccessExitCode.HasValue ? options.SuccessExitCode.Value : SuccessExitCode;
            CancellationToken cancellationToken = optionsDefined ? options.Token : default;

            var result = LinuxHelper.Program.Run(BuildArguments(), out output, out error, out exitCode, succesExitCode, cancellationToken, options?.Loggers);

            if (optionsDefined && options.FailOnErrorOutput && error.HasValue())
            {
                loggers.Log((time, logger) => logger.LogMessage(LogLevel.Trace, $"Command {LoggerName} could not be run succesfully because error output contained value ({time.PrintTotalMs()})"));
                return false;
            }

            return result;
        }

        public override string BuildCommand()
        {
            return BuildArguments();
        }
    }

    /// <summary>
    /// Template for executing command using the default cli shell /bin/sh.
    /// </summary>
    public abstract class ShellCommand : ShellCommand<ILinuxCommandResult<string, string>>, ILinuxCommand
    {
        public override ILinuxCommandResult<string, string> CreateResult(bool wasSuccesful, int exitCode, string output, string error, IEnumerable<ILogger> loggers = null)
        {
            return new LinuxCommandResult<string, string>(!wasSuccesful, output, error, exitCode);
        }
    }
}
