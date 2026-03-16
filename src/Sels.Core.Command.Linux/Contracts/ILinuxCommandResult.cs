using Sels.Core.Extensions;
using Sels.Core.Linux.Exceptions.LinuxCommand;

namespace Sels.Core.Command.Linux.Contracts
{
    /// <summary>
    /// Simple linux command result containing the result and/or error.
    /// </summary>
    /// <typeparam name="TResult">Type of result</typeparam>
    /// <typeparam name="TError">Type of error</typeparam>
    public interface ILinuxCommandResult<TResult, TError>
    {
        /// <summary>
        /// Indicates if the command failed.
        /// </summary>
        bool Failed { get; }
        /// <summary>
        /// Exit code of the executed command.
        /// </summary>
        int ExitCode { get; }
        /// <summary>
        /// Standard output of executed command.
        /// </summary>
        TResult? Output { get; }
        /// <summary>
        /// Error output of executed command.
        /// </summary>
        TError? Error { get; }
    }

    /// <summary>
    /// Contains extension methods for <see cref="ILinuxCommandResult{TResult, TError}"/>.
    /// </summary>
    public static class LinuxCommandResultExtensions
    {
        /// <summary>
        /// Returns the result if the command was executed successfully. Throws <see cref="CommandExecutionFailedException"/> if the command failed containing the error object.
        /// </summary>
        /// <typeparam name="TOutput">Type of the output</typeparam>
        /// <typeparam name="TError">Type of the error output</typeparam>
        /// <param name="commandResult">The command result to get the output from</param>
        /// <returns>The output from <paramref name="commandResult"/> if it didn't fail</returns>
        /// <exception cref="CommandExecutionFailedException">Thrown when <paramref name="commandResult"/> is from a failed command execution</exception>
        public static TOutput? GetResult<TOutput, TError>(this ILinuxCommandResult<TOutput, TError> commandResult)
        {
            commandResult.ValidateArgument(nameof(commandResult));

            return commandResult.Failed ? throw new CommandExecutionFailedException(commandResult.ExitCode, commandResult?.Error?.ToString() ?? "Unknown error") : commandResult.Output;
        }
    }
}
