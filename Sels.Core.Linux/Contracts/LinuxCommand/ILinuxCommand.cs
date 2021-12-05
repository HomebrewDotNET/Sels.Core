using Microsoft.Extensions.Logging;
using Sels.Core.Components.Commands;
using Sels.Core.Contracts.Commands;
using Sels.Core.Linux.Contracts.LinuxCommand.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Linux.Contracts.LinuxCommand
{
    /// <summary>
    /// Used to run linux commands or build linux command strings.
    /// </summary>
    public interface ILinuxCommand : ICommand<ILinuxCommandResult<string, string>>
    {

    }


}
