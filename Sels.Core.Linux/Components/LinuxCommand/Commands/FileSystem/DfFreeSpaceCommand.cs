using Sels.Core.Extensions.Conversion;
using Sels.Core.Linux.Components.LinuxCommand.Attributes;
using Sels.Core.Linux.Components.LinuxCommand.Commands.Awk;
using Sels.Core.Linux.Contracts.LinuxCommand;
using Sels.Core.Linux.Contracts.LinuxCommand.Commands;
using Sels.Core.Linux.Templates.LinuxCommand;
using Sels.Core.Templates.FileSizes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Sels.Core.Linux.Components.LinuxCommand.Commands.FileSystem
{
    /// <summary>
    /// Command that checks how much free disk space a file system member has.
    /// </summary>
    /// <typeparam name="TFileSize">Type of filesize format to display the free disk space in</typeparam>
    public class DfFreeSpaceCommand<TFileSize> : MultiCommand<ILinuxCommandResult<TFileSize, string>> where TFileSize : FileSize
    {
        // Constants
        private const string _awkScript = "{print $4}";

        // Fields
        private readonly DfFileSizeConverter _converter = new DfFileSizeConverter();
        private readonly DfInfoCommand _infoCommand = new DfInfoCommand();
        private readonly DynamicAwkCommand _awkCommand = new DynamicAwkCommand(_awkScript);

        // Properties
        /// <summary>
        /// File system member to get info about.
        /// </summary>
        public FileSystemInfo Member { get { return _infoCommand.Member; } set { _infoCommand.Member = value; } }

        public override ILinuxCommandResult<TFileSize, string> CreateResult(bool wasSuccesful, int exitCode, string output, string error)
        {
            if (wasSuccesful)
            {
                var fileSize = _converter.ConvertTo(typeof(string), typeof(TFileSize), output.SplitStringOnNewLine().Skip(1).First()).As<TFileSize>();
                return new LinuxCommandResult<TFileSize, string>(fileSize, exitCode);
            }

            return new LinuxCommandResult<TFileSize, string>(error, exitCode);
        }

        protected override IMultiCommandChain BuildCommandChain(IMultiCommandStartSetup chainSetup)
        {
            return chainSetup.StartWith(_infoCommand).EndWith(CommandChainer.Pipe, _awkCommand);
        }
    }
}
