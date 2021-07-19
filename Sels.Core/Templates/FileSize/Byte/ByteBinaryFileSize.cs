using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Templates.FileSize.Byte
{
    /// <summary>
    /// Displays size in byte format.
    /// </summary>
    public abstract class ByteBinaryFileSize : BinaryFileSize
    {
        // Properties
        public override bool IsByteSize => true;

        public ByteBinaryFileSize() : base()
        {

        }

        public ByteBinaryFileSize(long byteSize) : base(byteSize)
        {

        }

        public ByteBinaryFileSize(decimal size) : base(size)
        {

        }
    }
}
