using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Templates.FileSizes.Bit
{
    /// <summary>
    /// Displays size in bit format.
    /// </summary>
    public abstract class BitBinaryFileSize : BinaryFileSize
    {
        // Properties
        public override bool IsByteSize => false;

        public BitBinaryFileSize() : base()
        {

        }

        public BitBinaryFileSize(long byteSize) : base(byteSize)
        {

        }

        public BitBinaryFileSize(decimal size) : base(size)
        {

        }
    }
}
