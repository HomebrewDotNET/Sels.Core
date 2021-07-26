using Sels.Core.Templates.FileSizes.Byte;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.FileSizes.Byte
{
    /// <summary>
    /// Displays file size in byte.
    /// </summary>
    public class SingleByte : ByteFileSize
    {
        // Constants
        public const string FileSizeAbbreviation = "B";

        // Properties
        public override string Abbreviation => FileSizeAbbreviation;
        protected override int SizeMultiplier => 0;

        public SingleByte() : base()
        {

        }

        public SingleByte(long byteSize) : base(byteSize)
        {

        }

        public SingleByte(decimal size) : base(size)
        {

        }
    }
}
