using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Linux.Exceptions.LinuxCommand
{
    public class LinuxCommandExecutionFailedException : Exception
    {
        // Properties
        /// <summary>
        /// Exit code returned by the command. 
        /// </summary>
        public int ExitCode { get; }
        /// <summary>
        /// Error output of command.
        /// </summary>
        public object Error { get; }

        public LinuxCommandExecutionFailedException(int exitcode, object error) : base(CreateErrorMessage(exitcode, error.ValidateArgument(nameof(error)).ToString()))
        {
            ExitCode = exitcode;
            Error = error;
        }

        public LinuxCommandExecutionFailedException(int exitcode, object error, string message) : base(message)
        {
            ExitCode = exitcode;
            Error = error;
        }

        public LinuxCommandExecutionFailedException(int exitcode, object error, string message, Exception innerException) : base(message, innerException)
        {
            ExitCode = exitcode;
            Error = error;
        }

        public LinuxCommandExecutionFailedException(int exitcode, object error, Exception innerException) : base(CreateErrorMessage(exitcode, error.ValidateArgument(nameof(error)).ToString()), innerException)
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
