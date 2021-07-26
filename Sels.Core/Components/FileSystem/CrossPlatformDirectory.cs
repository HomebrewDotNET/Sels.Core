using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Sels.Core.Components.FileSizes.Byte;
using Sels.Core.Extensions;
using Sels.Core.Templates.FileSizes;

namespace Sels.Core.Components.FileSystem
{
    /// <summary>
    /// Wrapper around DirectoryInfo that uses delegates to fetch some information that's fetched differently on other operating systems.
    /// </summary>
    public class CrossPlatformDirectory
    {
        // Fields
        private readonly Func<DirectoryInfo, FileSize> _getFreeSpaceFunc;

        public CrossPlatformDirectory(string path, Func<DirectoryInfo, FileSize> getFreeSpaceFunc) : this(new DirectoryInfo(path.ValidateArgumentNotNullOrWhitespace(nameof(path))), getFreeSpaceFunc)
        {

        }

        public CrossPlatformDirectory(DirectoryInfo info, Func<DirectoryInfo, FileSize> getFreeSpaceFunc)
        {
            Directory = info.ValidateArgument(nameof(info));
            _getFreeSpaceFunc = getFreeSpaceFunc.ValidateArgument(nameof(getFreeSpaceFunc));
        }

        // Properties
        /// <summary>
        /// Info about a directory on the filesystem.
        /// </summary>
        public DirectoryInfo Directory { get; }
        /// <summary>
        /// Amount of free space on this directory
        /// </summary>
        public FileSize FreeSpace => Directory.HasValue() && Directory.Exists ? _getFreeSpaceFunc(Directory) : new SingleByte(0);
    }
}
