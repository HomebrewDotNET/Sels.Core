using Sels.Core.Components.FileSystem;
using Sels.Core.Linux.Components.LinuxCommand.Commands.FileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Sels.Core.Linux.Extensions;
using Sels.Core.Templates.FileSystem;
using Sels.Core.Templates.FileSizes;
using Sels.Core.Components.FileSizes.Byte;
using Sels.Core.Components.FileSizes.Byte.Binary;

namespace Sels.Core.Linux.Components.FileSystem
{
    /// <summary>
    /// Linux specific implementation for <see cref="CrossPlatformDirectory"/>.
    /// </summary>
    public class LinuxDirectory : CrossPlatformDirectory
    {
        // Properties
        /// <inheritdoc/>
        public override FileSize FreeSpace => new DfFreeSpaceCommand<KibiByte>() { Member = Source }.Execute().GetResult();
        /// <inheritdoc/>
        public override string MountPoint => new DfMountPointCommand() { Member = Source }.Execute().GetResult();

        /// <summary>
        /// Linux specific implementation for <see cref="CrossPlatformDirectory"/>.
        /// </summary>
        /// <param name="path">Path to the directory</param>
        public LinuxDirectory(string path) : base(path)
        {

        }
        /// <summary>
        /// Linux specific implementation for <see cref="CrossPlatformDirectory"/>.
        /// </summary>
        /// <param name="info">Directory info to wrap</param>
        public LinuxDirectory(DirectoryInfo info) : base(info)
        {

        }
    }
}
