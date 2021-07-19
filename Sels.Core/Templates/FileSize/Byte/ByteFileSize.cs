using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Templates.FileSize.Byte
{
    /// <summary>
    /// Displays size in byte format.
    /// </summary>
    public abstract class ByteFileSize : FileSize
    {
        // Properties
        public override bool IsByteSize => true;

        public ByteFileSize() : base()
        {

        }

        public ByteFileSize(long byteSize) : base(byteSize)
        {

        }

        public ByteFileSize(decimal size) : base(size)
        {

        }
    }
}
