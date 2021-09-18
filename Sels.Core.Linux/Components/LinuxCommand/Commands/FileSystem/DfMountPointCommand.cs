using Microsoft.Extensions.Logging;
using Sels.Core.Linux.Components.LinuxCommand.Commands.Awk;
using Sels.Core.Linux.Contracts.LinuxCommand;
using Sels.Core.Linux.Contracts.LinuxCommand.Commands;
using Sels.Core.Linux.Templates.LinuxCommand;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Sels.Core.Linux.Components.LinuxCommand.Commands.FileSystem
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

        public override ILinuxCommandResult<string, string> CreateResult(bool wasSuccesful, int exitCode, string output, string error, IEnumerable<ILogger> loggers = null)
        {
            if (wasSuccesful)
            {
                output = output.SplitStringOnNewLine().Skip(1).First();
            }

            return new LinuxCommandResult<string, string>(!wasSuccesful, output, error, exitCode);
        }

        protected override IMultiCommandChain BuildCommandChain(IMultiCommandStartSetup chainSetup)
        {
            return chainSetup.StartWith(_infoCommand).EndWith(CommandChainer.Pipe, _awkCommand);
        }
    }
}
