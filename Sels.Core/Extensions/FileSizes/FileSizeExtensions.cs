using Sels.Core.Components.FileSizes.Byte;
using Sels.Core.Templates.FileSizes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Extensions.FileSizes
{
    public static class FileSizeExtensions
    {
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
        /// Creates a file size object from <paramref name="bytes"/>.
        /// </summary>
        /// <typeparam name="TSize">File size format</typeparam>
        /// <param name="size">File size</param>
        /// <returns>New file size object with <see cref="FileSize.Size"/> equal to <paramref name="size"/></returns>
        public static TSize ToFileSize<TSize>(this decimal size) where TSize : FileSize, new()
        {
            return FileSize.CreateFromSize<TSize>(size);
        }
    }
}
