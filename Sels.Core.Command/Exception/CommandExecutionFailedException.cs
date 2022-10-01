using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Linux.Exceptions.LinuxCommand
{
    /// <summary>
    /// Indicates that the execution of a command resulted in errors.
    /// </summary>
    public class CommandExecutionFailedException : Exception
    {
        // Properties
        /// <summary>
        /// Exit code returned by the command. 
        /// </summary>
        public int ExitCode { get; }
        /// <summary>
        /// Error output of command.
        /// </summary>
        public string Error { get; }

        /// <summary>
        /// Indicates that the execution of a command resulted in errors.
        /// </summary>
        /// <param name="exitcode">The exit code returned from the command</param>
        /// <param name="error">Error output of the command</param>
        public CommandExecutionFailedException(int exitcode, string error) : base(CreateErrorMessage(exitcode, error))
        {
            ExitCode = exitcode;
            Error = error;
        }

        /// <summary>
        /// Indicates that the execution of a command resulted in errors.
        /// </summary>
        /// <param name="exitcode">The exit code returned from the command</param>
        /// <param name="error">Error output of the command</param>
        /// <param name="message">The exception message</param>
        public CommandExecutionFailedException(int exitcode, string error, string message) : base(message)
        {
            ExitCode = exitcode;
            Error = error;
        }

        /// <summary>
        /// Indicates that the execution of a command resulted in errors.
        /// </summary>
        /// <param name="exitcode">The exit code returned from the command</param>
        /// <param name="error">Error output of the command</param>
        /// <param name="message">The exception message</param>
        /// <param name="innerException">The exception that caused this exception</param>
        public CommandExecutionFailedException(int exitcode, string error, string message, Exception innerException) : base(message, innerException)
        {
            ExitCode = exitcode;
            Error = error;
        }
        /// <summary>
        /// Indicates that the execution of a command resulted in errors.
        /// </summary>
        /// <param name="exitcode">The exit code returned from the command</param>
        /// <param name="error">Error output of the command</param>
        /// <param name="innerException">The exception that caused this exception</param>
        public CommandExecutionFailedException(int exitcode, string error, Exception innerException) : base(CreateErrorMessage(exitcode, error), innerException)
        {
            ExitCode = exitcode;
            Error = error;
        }

        private static string CreateErrorMessage(int exitcode, string error)
        {
            return $"Command exited with code {exitcode} and error: {(error.HasValue() ? error : "No error output")}";
        }
    }
}
