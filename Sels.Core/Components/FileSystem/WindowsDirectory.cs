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
        public WindowsDirectory(string path) : base(path, x => x.GetFreeSpace())
        {

        }

        public WindowsDirectory(DirectoryInfo info) : base(info, x => x.GetFreeSpace())
        {

        }
    }
}
