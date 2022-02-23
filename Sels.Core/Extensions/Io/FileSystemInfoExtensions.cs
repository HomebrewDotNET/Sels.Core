using Sels.Core.Extensions;
using Sels.Core.Extensions.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace System.IO
{
    /// <summary>
    /// Contains extension methods for working with <see cref="FileSystemInfo"/>
    /// </summary>
    public static class FileSystemInfoExtensions
    {
        /// <summary>
        /// Returns the drive info for the <paramref name="info"/> object.
        /// </summary>
        /// <param name="info">File system object to get drive info from</param>
        /// <returns>Drive info for <paramref name="info"/></returns>
        public static DriveInfo GetDriveInfo(this FileSystemInfo info)
        {
            info.ValidateArgument(nameof(info));

            return new DriveInfo(info.FullName);
        }
    }
}
