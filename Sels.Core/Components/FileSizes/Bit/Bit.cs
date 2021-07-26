using Sels.Core.Templates.FileSizes.Bit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.FileSizes.Bit
{
    /// <summary>
    /// Displays file size in bit.
    /// </summary>
    public class Bit : BitFileSize
    {
        // Constants
        public const string FileSizeAbbreviation = "b";

        // Properties
        public override string Abbreviation => FileSizeAbbreviation;
        protected override int SizeMultiplier => 0;

        public Bit() : base()
        {

        }

        public Bit(long byteSize) : base(byteSize)
        {

        }

        public Bit(decimal size) : base(size)
        {

        }
    }
}
