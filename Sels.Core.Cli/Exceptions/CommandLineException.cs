using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Cli
{
    /// <summary>
    /// Thrown from a cli. Exposes the exit code to use.
    /// </summary>
    public class CommandLineException : Exception
    {
        /// <summary>
        /// The exit code to return.
        /// </summary>
        public int ExitCode { get; } = 1;

        /// <inheritdoc cref="CommandLineException"/>
        /// <param name="exitcode">The exit code for this exception</param>
        /// <param name="message">The message for this exception</param>
        public CommandLineException(int exitcode, string message) :base(message)
        {
            ExitCode = exitcode;
        }
        /// <inheritdoc cref="CommandLineException"/>
        /// <param name="exitcode">The exit code for this exception</param>
        /// <param name="message">The message for this exception</param>
        /// <param name="innerException">The exception that caused this exception</param>
        public CommandLineException(int exitcode, string message, Exception innerException) : base(message, innerException)
        {
            ExitCode = exitcode;
        }
    }
}
