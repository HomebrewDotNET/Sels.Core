using Sels.Core.Extensions.IO;
using Sels.Core.FileSystem.Extensions.FileSizes;
using Sels.Core.FileSystem.Templates.FileSystem;

namespace Sels.Core.FileSystem
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
            if (!OperatingSystem.IsWindows()) throw new NotSupportedException($"{GetType()} is not supported on a non windows system");
        }
        /// <summary>
        /// Windows specific wrapper for <see cref="CrossPlatformDirectory"/>
        /// </summary>
        /// <param name="info">Directory info to wrap</param>
        public WindowsDirectory(DirectoryInfo info) : base(info)
        {
            if (!OperatingSystem.IsWindows()) throw new NotSupportedException($"{GetType()} is not supported on a non windows system");
        }
    }
}
