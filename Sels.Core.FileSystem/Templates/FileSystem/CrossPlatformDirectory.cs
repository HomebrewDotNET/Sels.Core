using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Sels.Core.Components.FileSizes.Byte;
using Sels.Core.Extensions;
using Sels.Core.FileSystem.Templates.FileSizes;

namespace Sels.Core.FileSystem.Templates.FileSystem
{
    /// <summary>
    /// Wrapper around DirectoryInfo that exposes additional information that is fetched differently on other platforms.
    /// </summary>
    public abstract class CrossPlatformDirectory
    {
        // Properties
        /// <summary>
        /// Info about a directory on the filesystem.
        /// </summary>
        public DirectoryInfo Source { get; }

        /// <inheritdoc cref="FileSystemInfo.Name"/>
        public string Name => Source.Name;

        /// <inheritdoc cref="FileSystemInfo.FullName"/>
        public string FullName => Source.FullName;

        /// <inheritdoc cref="FileSystemInfo.Exists"/>
        public bool Exists => Source.Exists;

        /// <inheritdoc cref="FileSystemInfo.Attributes"/>
        public FileAttributes Attributes => Source.Attributes;

        /// <inheritdoc cref="FileSystemInfo.LastWriteTime"/>
        public DateTime LastWriteTime => Source.LastWriteTime;

        /// <inheritdoc cref="FileSystemInfo.LastWriteTimeUtc"/>
        public DateTime LastWriteTimeUtc => Source.LastWriteTimeUtc;

        /// <inheritdoc cref="FileSystemInfo.LastAccessTimeUtc"/>
        public DateTime LastAccessTimeUtc => Source.LastAccessTimeUtc;

        /// <inheritdoc cref="FileSystemInfo.LastAccessTime"/>
        public DateTime LastAccessTime => Source.LastAccessTime;

        /// <inheritdoc cref="FileSystemInfo.CreationTime"/>
        public DateTime CreationTime => Source.CreationTime;

        /// <inheritdoc cref="FileSystemInfo.CreationTimeUtc"/>
        public DateTime CreationTimeUtc => Source.CreationTimeUtc;

        /// <summary>
        /// Wrapper around DirectoryInfo that exposes additional information that is fetched differently on other platforms.
        /// </summary>
        /// <param name="path">Path to the directory</param>
        public CrossPlatformDirectory(string path) : this(new DirectoryInfo(path.ValidateArgumentNotNullOrWhitespace(nameof(path))))
        {

        }
        /// <summary>
        /// Wrapper around DirectoryInfo that exposes additional information that is fetched differently on other platforms.
        /// </summary>
        /// <param name="info">Directory info to wrap</param>
        public CrossPlatformDirectory(DirectoryInfo info)
        {
            Source = info.ValidateArgument(nameof(info));
        }
      
        // Abstractions
        /// <summary>
        /// Amount of free space on this directory
        /// </summary>
        public abstract FileSize FreeSpace { get; }
        /// <summary>
        /// Mount point for this directory. 
        /// </summary>
        public abstract string MountPoint { get; }
    }
}
