using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.FileSystem.Templates.FileSizes.Bit
{
    /// <summary>
    /// Displays size in bit format.
    /// </summary>
    public abstract class BitFileSize : FileSize
    {        
        // Properties
        /// <inheritdoc/>
        public override bool IsByteSize => false;

        /// <inheritdoc cref="BitFileSize"/>
        public BitFileSize() : base()
        {

        }
        /// <inheritdoc cref="BitFileSize"/>
        /// <param name="byteSize">The file size in bytes</param>
        public BitFileSize(long byteSize) : base(byteSize)
        {

        }
        /// <inheritdoc cref="BitFileSize"/>
        /// <param name="size">The file size in <paramref name="size"/></param>
        public BitFileSize(decimal size) : base(size)
        {

        }
    }
}
