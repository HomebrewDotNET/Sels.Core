using Sels.Core.Components.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Sels.Core.Contracts.Commands
{
    /// <summary>
    /// Exposes methods to run commands (powershell, linux, ...) and build the command string.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Runs the command and gets the stout, sterr and exit code.
        /// </summary>
        /// <param name="output">Stout of command</param>
        /// <param name="error">Sterr of command</param>
        /// <param name="exitCode">Exit code of command</param>
        /// <returns>Boolean indicating if the command executed succesfully</returns>
        bool RunCommand(out string output, out string error, out int exitCode, CommandExecutionOptions options = null);

        /// <summary>
        /// Builds the command string.
        /// </summary>
        string BuildCommand();
    }
}
