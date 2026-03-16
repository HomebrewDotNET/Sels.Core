using Sels.Core.Conversion.Attributes.Serialization;
using Sels.Core.Conversion.Attributes.Table;
using Sels.Core.FileSystem.Templates.FileSizes;

namespace Sels.Core.Command.Linux.Commands.FileSystem
{
    /// <summary>
    /// Info about a file system member.
    /// </summary>
    public class DiskFreeInfo
    {
        /// <summary>
        /// Mounted file system.
        /// </summary>
        [ColumnIndex(0)]
        public string FileSystem { get; set; }
        /// <summary>
        /// Amount of 1k blocks.
        /// </summary>
        [ColumnIndex(1)]
        public long Blocks { get; set; }
        /// <summary>
        /// Total used file size.
        /// </summary>
        [Converter(typeof(DfFileSizeConverter))]
        [ColumnIndex(2)]
        public FileSize UsedSpace { get; set; }
        /// <summary>
        /// Total amount of free space.
        /// </summary>
        [Converter(typeof(DfFileSizeConverter))]
        [ColumnIndex(3)]
        public FileSize FreeSpace { get; set; }
        /// <summary>
        /// Total file size.
        /// </summary>
        public FileSize TotalSize => UsedSpace + FreeSpace;
        /// <summary>
        /// Directory that file system is mounted on.
        /// </summary>
        [ColumnIndex(5)]
        public DirectoryInfo MountPoint { get; set; }
    }
}
