using Microsoft.Extensions.Logging;
using Sels.Core.Command.Linux.Templates.Commands.Screen;
using Sels.Core.Command.Linux.Contracts;
using Sels.Core.Extensions.Text;

namespace Sels.Core.Command.Linux.Commands.Screen
{
    /// <summary>
    /// Used to list running screens.
    /// </summary>
    public class ScreenListCommand : ScreenCommand<ILinuxCommandResult<string[], string>>
    {
        /// <inheritdoc/>
        public override ILinuxCommandResult<string[], string> CreateResult(bool wasSuccesful, int exitCode, string? output, string? error, IEnumerable<ILogger>? loggers = null)
        {
            if (wasSuccesful)
            {
                var lines = output.SplitOnNewLine();

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
        /// <inheritdoc/>
        protected override string BuildArguments(IEnumerable<ILogger>? loggers = null)
        {
            return "-ls";
        }
    }
}
