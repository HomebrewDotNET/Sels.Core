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
        public override FileSize FreeSpace => Source.GetFreeSpaceOnWindows();

        public WindowsDirectory(string path) : base(path)
        {

        }

        public WindowsDirectory(DirectoryInfo info) : base(info)
        {

        }
    }
}
