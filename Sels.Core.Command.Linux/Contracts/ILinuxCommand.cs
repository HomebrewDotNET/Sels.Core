﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using Sels.Core.Command.Contracts.Commands;

namespace Sels.Core.Command.Linux.Contracts
{
    /// <summary>
    /// Used to run linux commands or build linux command strings.
    /// </summary>
    public interface ILinuxCommand : ICommand<ILinuxCommandResult<string, string>>
    {

    }


}
