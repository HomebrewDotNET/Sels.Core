using Microsoft.Extensions.Logging;
using Sels.Core.Command.Components.Commands;
using Sels.Core.Command.Contracts.Commands;
using Sels.Core.Command.Linux.Commands;
using Sels.Core.Command.Linux.Contracts;
using Sels.Core.Command.Linux.Templates.Attributes;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Logging;

namespace Sels.Core.Command.Linux.Templates
{
    /// <summary>
    /// Used to run linux commands or build linux command strings.
    /// </summary>
    public abstract class BaseLinuxCommand : BaseLinuxCommand<string>
    {
        /// <summary>
        /// Used to run linux commands or build linux command strings.
        /// </summary>
        /// <param name="name">The name of the command to execute</param>
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
        /// <summary>
        /// Used to run linux commands or build linux command strings.
        /// </summary>
        /// <param name="name">The name of the command to execute</param>
        public BaseLinuxCommand(TName name) : base(name)
        {
        }

        /// <inheritdoc/>
        public override ILinuxCommandResult<string, string> CreateResult(bool wasSuccesful, int exitCode, string? output, string? error, IEnumerable<ILogger>? loggers = null)
        {
            loggers.LogMessage(LogLevel.Trace, $"Creating command result for {LoggerName} who {(wasSuccesful ? "executed successfully" : "failed execution")} with exit code {exitCode}, output length of <{output?.Length}> and error length of <{error?.Length}>");
            return new LinuxCommandResult<string, string>(!wasSuccesful, output, error, exitCode);
        }
    }

    /// <summary>
    /// Used to run linux commands or build linux command strings.
    /// </summary>
    /// <typeparam name="TName">Type of object that represents the command name</typeparam>
    /// <typeparam name="TCommandResult">Type of result that the command returns</typeparam>
    public abstract class BaseLinuxCommand<TName, TCommandResult> : ICommand<TCommandResult>
    {
        /// <summary>
        /// The name of the command to execute.
        /// </summary>
        public TName Name { get; }
        /// <summary>
        /// Name that can be used for logging.
        /// </summary>
        protected string LoggerName => GetType().Name;

        /// <summary>
        /// Used to run linux commands or build linux command strings.
        /// </summary>
        /// <param name="name">The name of the command to execute</param>
        public BaseLinuxCommand(TName name) 
        {
            if (!OperatingSystem.IsLinux()) throw new NotSupportedException($"{GetType()} is not supported on a non linux system");
            Name = name.ValidateArgument(nameof(name));
        }

        /// <inheritdoc/>
        public virtual bool RunCommand(out string output, out string error, out int exitCode, CommandExecutionOptions? options = null)
        {
            var loggers = (options.HasValue() ? options?.Loggers : null);
            using var methodLogger = loggers.CreateTimedLogger(LogLevel.Debug, $"Running command {LoggerName}", x => $"Ran command {LoggerName} in {x.PrintTotalMs()}");

            var succesExitCode = options != null && options.SuccessExitCode.HasValue ? options.SuccessExitCode.Value : SuccessExitCode;
            CancellationToken cancellationToken = options != null ? options.Token : default;

            var result = LinuxCommandHelper.Program.Run(Name?.GetArgumentValue() ?? String.Empty, BuildArguments(options?.Loggers), out output, out error, out exitCode, succesExitCode, cancellationToken, options?.Loggers);

            if (options != null && options.FailOnErrorOutput && error.HasValue())
            {
                methodLogger.Log((time, logger) => logger.LogMessage(LogLevel.Trace, $"Command {LoggerName} could not be run succesfully because error output contained value ({time.PrintTotalMs()})"));
                return false;
            }

            return result;
        }

        /// <inheritdoc/>
        public virtual string BuildCommand()
        {
            return $"{Name} {BuildArguments()}";
        }

        /// <summary>
        /// Builds arguments for running the linux command.
        /// </summary>
        /// <param name="loggers">Optional loggers for tracing</param>
        protected virtual string BuildArguments(IEnumerable<ILogger>? loggers = null)
        {
            loggers.LogMessage(LogLevel.Debug, $"Building arguments for command {LoggerName}");
            return LinuxCommandHelper.Command.BuildLinuxArguments(this, GetStaticArguments(loggers), loggers);
        }

        /// <summary>
        /// Optional method for providing additional arguments who aren't created from properties.
        /// </summary>
        /// <param name="loggers">Optional loggers for tracing</param>
        /// <returns>List of additional properties</returns>
        protected virtual IEnumerable<(string Argument, int Order)>? GetStaticArguments(IEnumerable<ILogger>? loggers = null)
        {
            loggers.LogMessage(LogLevel.Debug, $"Getting static arguments for command {LoggerName}");
            return null;
        }

        /// <inheritdoc/>
        public TCommandResult Execute(CommandExecutionOptions? options = null)
        {
            return Execute(out _, options);
        }

        /// <inheritdoc/>
        public TCommandResult Execute(out int exitCode, CommandExecutionOptions? options = null)
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
        protected virtual int SuccessExitCode => CommandConstants.SuccessExitCode;
        /// <inheritdoc/>
        public abstract TCommandResult CreateResult(bool wasSuccesful, int exitCode, string? output, string? error, IEnumerable<ILogger>? loggers = null);

        // Overrides
        /// <inheritdoc/>
        public override string ToString()
        {
            return BuildCommand();
        }
    }
}
