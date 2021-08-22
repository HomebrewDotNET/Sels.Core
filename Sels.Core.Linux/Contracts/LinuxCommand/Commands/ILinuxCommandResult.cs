using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Linux.Contracts.LinuxCommand.Commands
{
    /// <summary>
    /// Simple bash command result containing the result and/or error
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
        TResult Output { get; }
        /// <summary>
        /// Error output of executed command.
        /// </summary>
        TError Error { get; }
    }
}
