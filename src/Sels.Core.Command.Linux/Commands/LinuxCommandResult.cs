using Sels.Core.Command.Linux.Contracts;

namespace Sels.Core.Command.Linux.Commands
{
    internal class LinuxCommandResult<TResult, TError> : ILinuxCommandResult<TResult, TError>
    {
        // Properties
        public bool Failed { get; }
        public int ExitCode { get; }
        public TResult? Output { get; }
        public TError? Error { get; }

        public LinuxCommandResult()
        {

        }

        public LinuxCommandResult(bool failed, TResult? output, TError? error, int exitCode)
        {
            Failed = failed;
            Output = output;
            Error = error;
            ExitCode = exitCode;
        }

        public LinuxCommandResult(TResult? output, int exitCode = CommandConstants.SuccessExitCode)
        {
            ExitCode = exitCode;
            Output = output;
        }

        public LinuxCommandResult(TError? error, int exitCode)
        {
            ExitCode = exitCode;
            Error = error;
            Failed = true;
        }
    }

    internal class LinuxCommandResult<TResult> : LinuxCommandResult<TResult, TResult>
    {
        public LinuxCommandResult() : base()
        {

        }

        public LinuxCommandResult(bool failed, TResult? output, TResult? error, int exitCode) : base(failed, output, error, exitCode)
        {

        }
    }
}
