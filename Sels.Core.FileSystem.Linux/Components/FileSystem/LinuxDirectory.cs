using Sels.Core.Components.FileSizes.Byte.Binary;
using Sels.Core.FileSystem.Templates.FileSystem;
using Sels.Core.FileSystem.Templates.FileSizes;
using Sels.Core.Command.Linux.Commands.FileSystem;
using Sels.Core.Command.Linux.Contracts;

namespace Sels.Core.Linux.Components.FileSystem
{
    /// <summary>
    /// Linux specific implementation for <see cref="CrossPlatformDirectory"/>.
    /// </summary>
    public class LinuxDirectory : CrossPlatformDirectory
    {
        // Properties
        /// <inheritdoc/>
        public override FileSize FreeSpace => new DfFreeSpaceCommand<KibiByte>() { Member = Source }.Execute().GetResult() ?? FileSize.Empty;

        /// <inheritdoc/>
        public override string MountPoint => new DfMountPointCommand() { Member = Source }.Execute().GetResult() ?? String.Empty;

        /// <summary>
        /// Linux specific implementation for <see cref="CrossPlatformDirectory"/>.
        /// </summary>
        /// <param name="path">Path to the directory</param>
        public LinuxDirectory(string path) : base(path)
        {
            if (!OperatingSystem.IsLinux()) throw new NotSupportedException($"{GetType()} is not supported on a non linux system");
        }
        /// <summary>
        /// Linux specific implementation for <see cref="CrossPlatformDirectory"/>.
        /// </summary>
        /// <param name="info">Directory info to wrap</param>
        public LinuxDirectory(DirectoryInfo info) : base(info)
        {
            if (!OperatingSystem.IsLinux()) throw new NotSupportedException($"{GetType()} is not supported on a non linux system");
        }
    }
}
