using Sels.Core.Extensions;
using Sels.Core.Linux.Contracts.LinuxCommand;
using Sels.Core.Linux.Contracts.LinuxCommand.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Linux.Components.LinuxCommand.Commands
{
    public class LinuxCommandResult<TResult, TError> : ILinuxCommandResult<TResult, TError>
    {
        // Properties
        public bool Failed { get; }
        public int ExitCode { get; }
        public TResult Output { get; }
        public TError Error { get; }

        public LinuxCommandResult()
        {

        }

        public LinuxCommandResult(bool failed, TResult output, TError error, int exitCode)
        {
            Failed = failed;
            Output = output;
            Error = error;
            ExitCode = exitCode;
        }

        public LinuxCommandResult(TResult output, int exitCode = LinuxConstants.SuccessExitCode)
        {
            ExitCode = exitCode;
            Output = output;
        }

        public LinuxCommandResult(TError error, int exitCode)
        {
            ExitCode = exitCode;
            Error = error;
            Failed = true;
        }
    }

    public class LinuxCommandResult<TResult> : LinuxCommandResult<TResult, TResult>
    {
        public LinuxCommandResult() : base()
        {

        }

        public LinuxCommandResult(bool failed, TResult output, TResult error, int exitCode) : base(failed, output, error, exitCode)
        {

        }
    }
}
