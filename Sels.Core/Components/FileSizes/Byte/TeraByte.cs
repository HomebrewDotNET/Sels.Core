using Sels.Core.Templates.FileSizes.Byte;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.FileSizes.Byte
{
    /// <summary>
    /// Displays file size in terabyte.
    /// </summary>
    public class TeraByte : ByteFileSize
    {
        // Constants
        public const string FileSizeAbbreviation = "TB";

        // Properties
        public override string Abbreviation => FileSizeAbbreviation;
        protected override int SizeMultiplier => 4;

        public TeraByte() : base()
        {

        }

        public TeraByte(long byteSize) : base(byteSize)
        {

        }

        public TeraByte(decimal size) : base(size)
        {

        }
    }
}
