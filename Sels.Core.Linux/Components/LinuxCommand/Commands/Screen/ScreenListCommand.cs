using Sels.Core.Extensions;
using Sels.Core.Linux.Components.LinuxCommand;
using Sels.Core.Linux.Templates.LinuxCommand.Commands.Screen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sels.Core.Linux.Components.LinuxCommand.Commands.Screen
{
    public class ScreenListCommand : ScreenCommand<LinuxCommandResult<string[], string>>
    {
        public override LinuxCommandResult<string[], string> CreateResult(bool wasSuccesful, int exitCode, string output, string error)
        {
            if (wasSuccesful)
            {
                var lines = output.SplitStringOnNewLine();

                if(lines.Length > 1)
                {
                    // Filter away status messages
                    lines = lines.Skip(1).SkipLast(1).ToArray();
                }
                else
                {
                    // No running screens.
                    lines = new string[0];
                }

                return new LinuxCommandResult<string[], string>(lines.Select(x => x.Trim()).ToArray(), exitCode);
            }
            else
            {
                return new LinuxCommandResult<string[], string>(error, exitCode);
            }
        }

        protected override string BuildArguments()
        {
            return "-ls";
        }
    }
}
