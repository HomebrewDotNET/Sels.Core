using Sels.Core.Components.FileSizes.Byte;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Linq;
using Sels.Core.Templates.FileSizes;
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
    public static class FileSystemInfoExtensions
    {
        [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage"), SuppressUnmanagedCodeSecurity]
        [DllImport("Kernel32", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]

        private static extern bool GetDiskFreeSpaceEx(string lpszPath, ref long lpFreeBytesAvailable, ref long lpTotalNumberOfBytes, ref long lpTotalNumberOfFreeBytes);

        /// <summary>
        /// Returns the amount of free space on the drive that <paramref name="info"/> is located on. 
        /// </summary>
        /// <param name="info">Location to check</param>
        /// <returns>Free size in <see cref="SingleByte"/></returns>
        public static FileSize GetFreeSpace(this FileSystemInfo info)
        {
            info.ValidateArgument(nameof(info));

            var drive = info.GetDriveInfo();

            return FileSize.CreateFromBytes<SingleByte>(drive.AvailableFreeSpace);
        }

        /// <summary>
        /// Returns the amount of free space on the drive that <paramref name="info"/> is located on. Only works on windows as it relies on a windows dll.
        /// </summary>
        /// <param name="info">Location to check</param>
        /// <returns>Free size in <see cref="SingleByte"/></returns>
        public static FileSize GetFreeSpaceOnWindows(this FileSystemInfo info)
        {
            info.ValidateArgument(nameof(info));

            var path = Path.GetDirectoryName(info.FullName);

            long freeBytes = 0;
            long totalBytes = 0;
            long totalNumberFreeBytes = 0;

            if (GetDiskFreeSpaceEx(path, ref freeBytes, ref totalBytes, ref totalNumberFreeBytes))
            {
                return FileSize.CreateFromBytes<SingleByte>(freeBytes);
            }

            return FileSize.CreateFromBytes<SingleByte>(0);
        }

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
