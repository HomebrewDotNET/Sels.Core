using Sels.Core.Components.FileSystem;
using Sels.Core.Linux.Commands.FileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Sels.Core.Linux.Extensions;

namespace Sels.Core.Linux.Components.FileSystem
{
    /// <summary>
    /// Windows specific wrapper for <see cref="CrossPlatformDirectory"/>
    /// </summary>
    public class LinuxDirectory : CrossPlatformDirectory
    {
        public LinuxDirectory(string path) : base(path, x => new DfInfoCommand() { Member = x }.Execute().GetResult().FreeSpace)
        {

        }

        public LinuxDirectory(DirectoryInfo info) : base(info, x => new DfInfoCommand() { Member = x }.Execute().GetResult().FreeSpace)
        {

        }
    }
}
