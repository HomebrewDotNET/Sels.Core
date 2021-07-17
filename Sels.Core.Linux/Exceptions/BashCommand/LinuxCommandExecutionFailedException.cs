using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Linux.Exceptions.LinuxCommand
{
    public class LinuxCommandExecutionFailedException : Exception
    {
        // Properties
        public object Error { get; set; }

        public LinuxCommandExecutionFailedException(object error) : base(error.ValidateVariable(nameof(error)).ToString())
        {
            Error = error;
        }

        public LinuxCommandExecutionFailedException(object error, string message) : base(message)
        {
            Error = error;
        }

        public LinuxCommandExecutionFailedException(object error, string message, Exception innerException) : base(message, innerException)
        {
            Error = error;
        }

        public LinuxCommandExecutionFailedException(object error, Exception innerException) : base(error.ValidateVariable(nameof(error)).ToString(), innerException)
        {
            Error = error;
        }
    }
}
