using Sels.Core.Linux.Templates.LinuxCommand;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Linux.Templates.Commands.Screen
{
    /// <summary>
    /// The screen command a terminal multiplexer used to manage several terminals.
    /// </summary>
    /// <typeparam name="TCommandResult">Type of result returned by this command</typeparam>
    public abstract class ScreenCommand<TCommandResult> : BaseLinuxCommand<string, TCommandResult>
    {
        //// Constants
        //public const int SuccessScreenExitCode = 1;

        //// Properties
        //public override int SuccessExitCode => SuccessScreenExitCode;

        public ScreenCommand() : base(LinuxConstants.Commands.Screen)
        {

        }
    }
}
