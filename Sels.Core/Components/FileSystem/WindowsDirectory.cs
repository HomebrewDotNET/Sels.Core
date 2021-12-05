using Sels.Core.Templates.FileSizes;
using Sels.Core.Templates.FileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sels.Core.Components.FileSystem
{
    /// <summary>
    /// Windows specific wrapper for <see cref="CrossPlatformDirectory"/>
    /// </summary>
    public class WindowsDirectory : CrossPlatformDirectory
    {
        // Properties
        /// <inheritdoc/>
        public override FileSize FreeSpace => Source.GetFreeSpaceOnWindows();
        /// <inheritdoc/>
        public override string MountPoint => Path.IsPathRooted(Source.FullName) ? Source.GetDriveInfo().Name : string.Empty;

        /// <summary>
        /// Windows specific wrapper for <see cref="CrossPlatformDirectory"/>
        /// </summary>
        /// <param name="path">Path to the directory</param>
        public WindowsDirectory(string path) : base(path)
        {

        }
        /// <summary>
        /// Windows specific wrapper for <see cref="CrossPlatformDirectory"/>
        /// </summary>
        /// <param name="info">Directory info to wrap</param>
        public WindowsDirectory(DirectoryInfo info) : base(info)
        {

        }
    }
}
