using Sels.Core.FileSizes.Byte;
using Sels.Core.Extensions;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Security;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Reflection;
using Sels.Core.Extensions.IO;

namespace Sels.Core.FileSystem.Extensions.FileSizes
{
    /// <summary>
    /// Contains extension methods for working with <see cref="FileSize"/>.
    /// </summary>
    public static class FileSizeExtensions
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
            if (!OperatingSystem.IsWindows()) throw new NotSupportedException($"{nameof(GetFreeSpaceOnWindows)} is not supported on a non windows system");
            info.ValidateArgument(nameof(info));

            var path = Path.GetDirectoryName(info.FullName);

            long freeBytes = 0;
            long totalBytes = 0;
            long totalNumberFreeBytes = 0;

            if (GetDiskFreeSpaceEx(path ?? String.Empty, ref freeBytes, ref totalBytes, ref totalNumberFreeBytes))
            {
                return FileSize.CreateFromBytes<SingleByte>(freeBytes);
            }

            return FileSize.CreateFromBytes<SingleByte>(0);
        }
        /// <summary>
        /// Creates a file size object from <paramref name="bytes"/>.
        /// </summary>
        /// <typeparam name="TSize">File size format</typeparam>
        /// <param name="bytes">File size in bytes</param>
        /// <returns>New file size object with <see cref="FileSize.ByteSize"/> equal to <paramref name="bytes"/></returns>
        public static TSize ToFileSize<TSize>(this long bytes) where TSize : FileSize, new()
        {
            return FileSize.CreateFromBytes<TSize>(bytes);
        }

        /// <summary>
        /// Creates a file size object from <paramref name="bytes"/>.
        /// </summary>
        /// <param name="bytes">File size in bytes</param>
        /// <returns>New file size object with <see cref="FileSize.ByteSize"/> equal to <paramref name="bytes"/></returns>
        public static SingleByte ToFileSize(this long bytes)
        {
            return bytes.ToFileSize<SingleByte>();
        }

        /// <summary>
        /// Creates a file size object from <paramref name="size"/>.
        /// </summary>
        /// <typeparam name="TSize">File size format</typeparam>
        /// <param name="size">File size</param>
        /// <returns>New file size object with <see cref="FileSize.Size"/> equal to <paramref name="size"/></returns>
        public static TSize ToFileSize<TSize>(this decimal size) where TSize : FileSize, new()
        {
            return FileSize.CreateFromSize<TSize>(size);
        }

        /// <summary>
        /// Returns the file size for <paramref name="file"/>.
        /// </summary>
        /// <typeparam name="TSize">The filesize to return</typeparam>
        /// <param name="file">The file to get the file size for</param>
        /// <returns>The file size for <paramref name="file"/></returns>
        public static TSize GetFileSize<TSize>(this FileInfo file) where TSize : FileSize
        {
            file.ValidateArgumentExists(nameof(file));

            var sizeType = typeof(TSize).Is<FileSize>() ? typeof(SingleByte) : typeof(TSize);

            return FileSize.CreateFromBytes(file.Length, sizeType).CastTo<TSize>();
        }
    }
}
