using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Templates.FileSize.Bit
{
    /// <summary>
    /// Displays size in bit format.
    /// </summary>
    public abstract class BitFileSize : FileSize
    {
        // Properties
        public override bool IsByteSize => false;

        public BitFileSize() : base()
        {

        }

        public BitFileSize(long byteSize) : base(byteSize)
        {

        }

        public BitFileSize(decimal size) : base(size)
        {

        }
    }
}
