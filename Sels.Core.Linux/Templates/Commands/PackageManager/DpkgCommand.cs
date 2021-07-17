using Sels.Core.Linux.Templates.LinuxCommand;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Linux.Templates.Commands.PackageManager
{
    /// <summary>
    /// Base class for the dpkg command.
    /// </summary>
    /// <typeparam name="TCommandResult">Type of command result</typeparam>
    public abstract class DpkgCommand<TCommandResult> : BaseLinuxCommand<string, TCommandResult>
    {
        public DpkgCommand() : base(LinuxConstants.Commands.Dpkg)
        {

        }
    }
}
