using Sels.Core.Components.Serialization.Table.Attributes;
using Sels.Core.Templates.FileSizes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sels.Core.Linux.Commands.FileSystem
{
    public class DiskFreeInfo
    {
        /// <summary>
        /// Mounted file system.
        /// </summary>
        [TableColumn(0)]
        public string FileSystem { get; set; }
        /// <summary>
        /// Amount of 1k blocks.
        /// </summary>
        [TableColumn(1)]
        public long Blocks { get; set; }
        /// <summary>
        /// Total used file size.
        /// </summary>
        [TableColumn(2, typeof(DfFileSizeConverter))]
        public FileSize UsedSpace { get; set; }
        /// <summary>
        /// Total amount of free space.
        /// </summary>
        [TableColumn(3, typeof(DfFileSizeConverter))]
        public FileSize FreeSpace { get; set; }
        /// <summary>
        /// Total file size.
        /// </summary>
        public FileSize TotalSize => UsedSpace + FreeSpace;
        /// <summary>
        /// Directory that file system is mounted on.
        /// </summary>
        [TableColumn(5)]
        public DirectoryInfo MountPoint { get; set; }
    }
}
