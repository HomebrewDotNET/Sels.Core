using Sels.Core.Components.FileSystem;
using Sels.Core.Linux.Commands.FileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Sels.Core.Linux.Extensions;
using Sels.Core.Templates.FileSystem;
using Sels.Core.Templates.FileSizes;

namespace Sels.Core.Linux.Components.FileSystem
{
    /// <summary>
    /// Linux specific implementation for <see cref="CrossPlatformDirectory"/>
    /// </summary>
    public class LinuxDirectory : CrossPlatformDirectory
    {
        // Properties
        public override FileSize FreeSpace => new DfInfoCommand() { Member = Source }.Execute().GetResult().FreeSpace;

        public LinuxDirectory(string path) : base(path)
        {

        }

        public LinuxDirectory(DirectoryInfo info) : base(info)
        {

        }
    }
}
