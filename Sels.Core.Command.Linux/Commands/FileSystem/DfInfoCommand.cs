using Microsoft.Extensions.Logging;
using Sels.Core.Command.Linux.Templates.Commands.FileSystem;
using Sels.Core.Templates.FileSizes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Sels.Core.Command.Linux.Attributes;
using Sels.Core.Command.Linux.Contracts;
using Sels.Core.Conversion.Converters;
using Sels.Core.Conversion.Contracts;

namespace Sels.Core.Command.Linux.Commands.FileSystem
{
    /// <summary>
    /// Gets info about a file system member. 
    /// </summary>
    public class DfInfoCommand : DfCommand<ILinuxCommandResult<DiskFreeInfo, string>>
    {
        // Properties
        /// <summary>
        /// File system member to get info about.
        /// </summary>
        [ObjectArgument(Selector.Property, nameof(FileSystemInfo.FullName), parsingOption: TextParsingOptions.Quotes, order: 2, required: true)]
        public FileSystemInfo Member { get; set; }

        public override ILinuxCommandResult<DiskFreeInfo, string> CreateResult(bool wasSuccesful, int exitCode, string? output, string? error, IEnumerable<ILogger>? loggers = null)
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


        // Statics
        protected static ITypeConverter _converter = GenericConverter.DefaultConverter;
        protected static TableSerializer _serializer = new TableSerializer(_converter, Environment.NewLine, skipDataRow: 1);
    }

    
}
