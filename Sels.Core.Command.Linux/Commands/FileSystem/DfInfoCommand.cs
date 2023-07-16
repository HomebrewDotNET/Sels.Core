using Microsoft.Extensions.Logging;
using Sels.Core.Command.Linux.Templates.Commands.FileSystem;
using Sels.Core.Command.Linux.Attributes;
using Sels.Core.Command.Linux.Contracts;
using Sels.Core.Conversion.Converters;
using Sels.Core.Conversion.Serializers.Table;
using Sels.Core.Extensions.Text;

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
        /// <inheritdoc/>
        public override ILinuxCommandResult<DiskFreeInfo, string> CreateResult(bool wasSuccesful, int exitCode, string? output, string? error, IEnumerable<ILogger>? loggers = null)
        {
            if (wasSuccesful)
            {
                var info = _serializer.Deserialize<DiskFreeInfo>(output);
                return new LinuxCommandResult<DiskFreeInfo, string>(info, exitCode);
            }
            else
            {
                return new LinuxCommandResult<DiskFreeInfo, string>(error, exitCode);
            }
        }

        // Statics
        /// <summary>
        /// The serializer to use to deserialize the command output.
        /// </summary>
        protected static TableSerializer _serializer = new TableSerializer(x => x.UseConverters(GenericConverter.DefaultConverter).SplitAndJoinColummsUsing(x => x.Split(), x => x.JoinString(Constants.Strings.Tab)).SplitAndJoinRowsUsing(Environment.NewLine));
    }

    
}
