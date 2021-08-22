using Sels.Core.Linux.Components.LinuxCommand.Commands;
using Sels.Core.Linux.Contracts.LinuxCommand;
using Sels.Core.Linux.Contracts.LinuxCommand.Commands;
using Sels.Core.Linux.Templates.LinuxCommand;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Linux.Templates.LinuxCommand.Commands.Bash
{
    /// <summary>
    /// Base class for bash.
    /// </summary>
    /// <typeparam name="TCommandResult">Type of command result</typeparam>
    public abstract class BashCommand<TCommandResult> : BaseLinuxCommand<string, TCommandResult>
    {
        public BashCommand() : base(LinuxConstants.Commands.Bash)
        {

        }

        public override bool RunCommand(out string output, out string error, out int exitCode)
        {
            return LinuxHelper.Program.Bash.Run(BuildArguments(), out output, out error, out exitCode, token: CancellationToken);
        }

        public override string BuildCommand()
        {
            return $"{Name} {LinuxHelper.Program.FormatStringCommand(BuildArguments())}";
        }
    }

    /// <summary>
    /// Base class for bash.
    /// </summary>
    public abstract class BashCommand : BashCommand<ILinuxCommandResult<string, string>> , ILinuxCommand
    {
        public override ILinuxCommandResult<string, string> CreateResult(bool wasSuccesful, int exitCode, string output, string error)
        {
            return new LinuxCommandResult<string, string>(!wasSuccesful, output, error, exitCode);
        }
    }
}
