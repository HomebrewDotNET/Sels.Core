using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Sels.Core.Components.FileSizes.Byte;
using Sels.Core.Extensions;
using Sels.Core.Templates.FileSizes;

namespace Sels.Core.Templates.FileSystem
{
    /// <summary>
    /// Wrapper around DirectoryInfo that uses delegates to fetch some information that's fetched differently on other operating systems.
    /// </summary>
    public abstract class CrossPlatformDirectory
    {
        // Properties
        /// <summary>
        /// Info about a directory on the filesystem.
        /// </summary>
        public DirectoryInfo Source { get; }

        public CrossPlatformDirectory(string path) : this(new DirectoryInfo(path.ValidateArgumentNotNullOrWhitespace(nameof(path))))
        {

        }

        public CrossPlatformDirectory(DirectoryInfo info)
        {
            Source = info.ValidateArgument(nameof(info));
        }



        // Abstractions
        /// <summary>
        /// Amount of free space on this directory
        /// </summary>
        public abstract FileSize FreeSpace { get; }
    }
}
