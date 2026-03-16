using Microsoft.Extensions.Logging;
using Sels.Core.Command.Linux.Commands.Awk;
using Sels.Core.Command.Linux.Contracts;
using Sels.Core.Command.Linux.Templates;
using Sels.Core.Command.Contracts.Commands;
using Sels.Core.Extensions.Text;

namespace Sels.Core.Command.Linux.Commands.FileSystem
{
    /// <summary>
    /// Gets the mount point for a file system member.
    /// </summary>
    public class DfMountPointCommand : MultiCommand<ILinuxCommandResult<string, string>>
    {
        // Constants
        private const string _awkScript = "{print $6}";

        // Fields
        private readonly DfInfoCommand _infoCommand = new DfInfoCommand();
        private readonly DynamicAwkCommand _awkCommand = new DynamicAwkCommand(_awkScript);

        // Properties
        /// <summary>
        /// File system member to get info about.
        /// </summary>
        public FileSystemInfo Member { get { return _infoCommand.Member; } set { _infoCommand.Member = value; } }
        /// <inheritdoc/>
        public override ILinuxCommandResult<string, string> CreateResult(bool wasSuccesful, int exitCode, string? output, string? error, IEnumerable<ILogger>? loggers = null)
        {
            if (wasSuccesful)
            {
                output = output.SplitOnNewLine().Skip(1).First();
            }

            return new LinuxCommandResult<string, string>(!wasSuccesful, output, error, exitCode);
        }
        /// <inheritdoc/>
        protected override IMultiCommandChain<CommandChainer> BuildCommandChain(IMultiCommandStartSetup<CommandChainer> chainSetup)
        {
            return chainSetup.StartWith(_infoCommand).EndWith(CommandChainer.Pipe, _awkCommand);
        }
    }
}
