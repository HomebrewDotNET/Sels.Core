using Sels.Core.Templates.FileSizes.Bit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.FileSizes.Bit.Binary
{
    /// <summary>
    /// Displays file size in mebibit.
    /// </summary>
    public class MebiBit : BitBinaryFileSize
    {
        // Constants
        public const string FileSizeAbbreviation = "Mib";

        // Properties
        public override string Abbreviation => FileSizeAbbreviation;
        protected override int SizeMultiplier => 2;

        public MebiBit() : base()
        {

        }

        public MebiBit(long byteSize) : base(byteSize)
        {

        }

        public MebiBit(decimal size) : base(size)
        {

        }
    }
}
