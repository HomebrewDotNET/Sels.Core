using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.FileSystem.Templates.FileSizes.Byte
{
    /// <summary>
    /// Displays size in byte format.
    /// </summary>
    public abstract class ByteBinaryFileSize : BinaryFileSize
    {
        // Properties
        /// <inheritdoc/>
        public override bool IsByteSize => true;

        /// <inheritdoc cref="ByteBinaryFileSize"/>
        public ByteBinaryFileSize() : base()
        {

        }
        /// <inheritdoc cref="ByteBinaryFileSize"/>
        /// <param name="byteSize">The file size in bytes</param>
        public ByteBinaryFileSize(long byteSize) : base(byteSize)
        {

        }
        /// <inheritdoc cref="ByteBinaryFileSize"/>
        /// <param name="size">The file size in <paramref name="size"/></param>
        public ByteBinaryFileSize(decimal size) : base(size)
        {

        }
    }
}
