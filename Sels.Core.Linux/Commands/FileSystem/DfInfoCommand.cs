using Sels.Core.Linux.Components.LinuxCommand;
using Sels.Core.Linux.Components.LinuxCommand.Attributes;
using Sels.Core.Linux.Templates.Commands.FileSystem;
using Sels.Core.Templates.FileSizes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Sels.Core.Linux.Commands.FileSystem
{
    /// <summary>
    /// Get disk free info about a file system member. 
    /// </summary>
    public class DfInfoCommand : DfCommand<LinuxCommandResult<DiskFreeInfo, string>>
    {
        // Properties
        /// <summary>
        /// File system member to get info about.
        /// </summary>
        [ObjectArgument(Selector.Property, nameof(FileSystemInfo.FullName), order: 2, required: true)]
        public FileSystemInfo Member { get; set; }

        protected override string BuildArguments()
        {
            return "-P " + base.BuildArguments();
        }

        public override LinuxCommandResult<DiskFreeInfo, string> CreateResult(bool wasSuccesful, int exitCode, string output, string error)
        {
            if (wasSuccesful)
            {
                var info = _serializer.Deserialize<DiskFreeInfo>(output).First();
                return new LinuxCommandResult<DiskFreeInfo, string>(info, exitCode);
            }
            else
            {
                return new LinuxCommandResult<DiskFreeInfo, string>(error, exitCode);
            }
        }
    }

    
}
