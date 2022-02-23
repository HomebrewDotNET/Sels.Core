using Sels.Core.Command.Linux.Templates;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Command.Linux.Templates.Commands.Screen
{
    /// <summary>
    /// The screen command a terminal multiplexer used to manage several terminals.
    /// </summary>
    /// <typeparam name="TCommandResult">Type of result returned by this command</typeparam>
    public abstract class ScreenCommand<TCommandResult> : BaseLinuxCommand<string, TCommandResult>
    {
        /// <inheritdoc cref="ScreenCommand{TCommandResult}"/>
        public ScreenCommand() : base(LinuxCommandConstants.Commands.Screen)
        {

        }
    }
}
