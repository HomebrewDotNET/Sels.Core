using Sels.Core.Templates.FileSizes.Bit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.FileSizes.Bit.Binary
{
    /// <summary>
    /// Displays file size in zebibit.
    /// </summary>
    public class ZebiBit : BitBinaryFileSize
    {
        // Constants
        public const string FileSizeAbbreviation = "Zib";

        // Properties
        public override string Abbreviation => FileSizeAbbreviation;
        protected override int SizeMultiplier => 7;

        public ZebiBit() : base()
        {

        }

        public ZebiBit(long byteSize) : base(byteSize)
        {

        }

        public ZebiBit(decimal size) : base(size)
        {

        }
    }
}
