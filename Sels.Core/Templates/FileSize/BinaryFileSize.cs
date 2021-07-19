using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Templates.FileSize
{
    /// <summary>
    /// File size that displays in binary. Uses a unit size of <see cref="BinaryUnitSize"/> instead of the default <see cref="FileSize.DefaultUnitSize"/>.
    /// </summary>
    public abstract class BinaryFileSize : FileSize
    {
        // Constants
        public const int BinaryUnitSize = 1024;

        // Properties
        public override int UnitSize => BinaryUnitSize;

        public BinaryFileSize() : base()
        {

        }

        public BinaryFileSize(long byteSize) : base(byteSize)
        {

        }

        public BinaryFileSize(decimal size) : base(size)
        {

        }
    }
}
