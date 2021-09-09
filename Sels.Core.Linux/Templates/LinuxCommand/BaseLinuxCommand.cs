using Microsoft.Extensions.Logging;
using Sels.Core.Components.Commands;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Logging;
using Sels.Core.Extensions.Reflection;
using Sels.Core.Linux.Components.LinuxCommand.Commands;
using Sels.Core.Linux.Contracts.LinuxCommand;
using Sels.Core.Linux.Contracts.LinuxCommand.Commands;
using Sels.Core.Linux.Extensions.Argument;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Sels.Core.Linux.Templates.LinuxCommand
{
    /// <summary>
    /// Used to run linux commands or build linux command strings.
    /// </summary>
    public abstract class BaseLinuxCommand : BaseLinuxCommand<string>
    {
        public BaseLinuxCommand(string name) : base(name)
        {

        }
    }

    /// <summary>
    /// Used to run linux commands or build linux command strings.
    /// </summary>
    /// <typeparam name="TName">Type of object that represents the command name</typeparam>
    public abstract class BaseLinuxCommand<TName> : BaseLinuxCommand<TName, ILinuxCommandResult<string, string>>, ILinuxCommand
    {
        public BaseLinuxCommand(TName name) : base(name)
        {
        }

        public override ILinuxCommandResult<string, string> CreateResult(bool wasSuccesful, int exitCode, string output, string error, IEnumerable<ILogger> loggers = null)
        {
            loggers.LogMessage(LogLevel.Trace, $"Creating command result for {LoggerName} who {(wasSuccesful ? "executed successfully" : "failed execution")} with exit code {exitCode}, output length of {output?.Length} and error length of {error?.Length}");
            return new LinuxCommandResult<string, string>(!wasSuccesful, output, error, exitCode);
        }
    }

    /// <summary>
    /// Used to run linux commands or build linux command strings.
    /// </summary>
    /// <typeparam name="TName">Type of object that represents the command name</typeparam>
    /// <typeparam name="TCommandResult">Type of result that the command returns</typeparam>
    public abstract class BaseLinuxCommand<TName, TCommandResult> : ILinuxCommand<TCommandResult>
    {
        public TName Name { get; }

        protected string LoggerName => GetType().Name;

        public BaseLinuxCommand(TName name) 
        {
            Name = name.ValidateArgument(nameof(name));
        }

        public virtual bool RunCommand(out string output, out string error, out int exitCode, CommandExecutionOptions options = null)
        {
            using var loggers = (options.HasValue() ? options.Loggers : null).CreateTimedLogger(LogLevel.Debug, $"Running command {LoggerName}", x => $"Ran command {LoggerName} in {x.PrintTotalMs()}");
            var optionsDefined = options.HasValue();
            var succesExitCode = optionsDefined && options.SuccessExitCode.HasValue ? options.SuccessExitCode.Value : SuccessExitCode;
            CancellationToken cancellationToken = optionsDefined ? options.Token : default;

            var result = LinuxHelper.Program.Run(Name.GetArgumentValue(), BuildArguments(options.Loggers), out output, out error, out exitCode, succesExitCode, cancellationToken, options?.Loggers);

            if (optionsDefined && options.FailOnErrorOutput && error.HasValue())
            {
                loggers.Log((time, logger) => logger.LogMessage(LogLevel.Trace, $"Command {LoggerName} could not be run succesfully because error output contained value ({time.PrintTotalMs()})"));
                return false;
            }

            return result;
        }

        public virtual string BuildCommand()
        {
            return $"{Name} {BuildArguments()}";
        }

        /// <summary>
        /// Builds arguments for running the linux command.
        /// </summary>
        protected virtual string BuildArguments(IEnumerable<ILogger> loggers = null)
        {
            loggers.LogMessage(LogLevel.Debug, $"Building arguments for command {LoggerName}");
            return LinuxHelper.Command.BuildLinuxArguments(this, GetStaticArguments(loggers), loggers);
        }

        /// <summary>
        /// Optional method for providing additional arguments who aren't created from properties.
        /// </summary>
        /// <returns>List of additional properties</returns>
        protected virtual IEnumerable<(string Argument, int Order)> GetStaticArguments(IEnumerable<ILogger> loggers = null)
        {
            loggers.LogMessage(LogLevel.Debug, $"Getting static arguments for command {LoggerName}");
            return null;
        }

        public TCommandResult Execute(CommandExecutionOptions options = null)
        {
            return Execute(out _, options);
        }

        public TCommandResult Execute(out int exitCode, CommandExecutionOptions options = null)
        {
            using var loggers = (options.HasValue() ? options.Loggers : null).CreateTimedLogger(LogLevel.Debug, $"Executing command {LoggerName}", x => $"Executed command {LoggerName} in {x.PrintTotalMs()}");

            if(RunCommand(out string output, out string error, out exitCode, options))
            {
                var commandExitCode = exitCode;
                loggers.Log((time, logger) => logger.LogMessage(LogLevel.Trace, $"Command {LoggerName} succesfully executed with exit code {commandExitCode} ({time.PrintTotalMs()})"));
                return CreateResult(true, exitCode, output, error, options?.Loggers);
            }
            else
            {
                var commandExitCode = exitCode;
                loggers.Log((time, logger) => logger.LogMessage(LogLevel.Trace, $"Command {LoggerName} failed execution with exit code {commandExitCode} ({time.PrintTotalMs()})"));
                return CreateResult(false, exitCode, output, error, options?.Loggers);
            }
        }

        /// <summary>
        /// Default exit code returned from executing the command that indicates it executed succesfully.
        /// </summary>
        protected virtual int SuccessExitCode => LinuxConstants.SuccessExitCode;

        public abstract TCommandResult CreateResult(bool wasSuccesful, int exitCode, string output, string error, IEnumerable<ILogger> loggers = null);

        // Overrides
        public override string ToString()
        {
            try
            {
                return BuildCommand();
            }
            catch(Exception ex)
            {
                return "Could not build command: " + ex.Message;
            }
        }
    }
}
