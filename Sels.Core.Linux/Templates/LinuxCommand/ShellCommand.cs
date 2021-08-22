using Sels.Core.Linux.Components.LinuxCommand.Commands;
using Sels.Core.Linux.Contracts.LinuxCommand;
using Sels.Core.Linux.Contracts.LinuxCommand.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Linux.Templates.LinuxCommand
{
    /// <summary>
    /// Template for executing command using the default cli shell /bin/sh.
    /// </summary>
    /// <typeparam name="TCommandResult">Type of result that the command returns</typeparam>
    public abstract class ShellCommand<TCommandResult> : BaseLinuxCommand<string, TCommandResult>
    {
        public ShellCommand() : base(LinuxConstants.Commands.Shell)
        {

        }

        public override bool RunCommand(out string output, out string error, out int exitCode)
        {
            return LinuxHelper.Program.Run(BuildArguments(), out output, out error, out exitCode, token: CancellationToken);
        }

        public override string BuildCommand()
        {
            return BuildArguments();
        }
    }

    /// <summary>
    /// Template for executing command using the default cli shell /bin/sh.
    /// </summary>
    public abstract class ShellCommand : ShellCommand<ILinuxCommandResult<string, string>>, ILinuxCommand
    {
        public override ILinuxCommandResult<string, string> CreateResult(bool wasSuccesful, int exitCode, string output, string error)
        {
            return new LinuxCommandResult<string, string>(!wasSuccesful, output, error, exitCode);
        }
    }
}
