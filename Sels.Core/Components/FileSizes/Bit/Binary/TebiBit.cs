using Sels.Core.Templates.FileSizes.Bit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.FileSizes.Bit.Binary
{
    /// <summary>
    /// Displays file size in tebibit.
    /// </summary>
    public class TebiBit : BitBinaryFileSize
    {
        // Constants
        public const string FileSizeAbbreviation = "Tib";

        // Properties
        public override string Abbreviation => FileSizeAbbreviation;
        protected override int SizeMultiplier => 4;

        public TebiBit() : base()
        {

        }

        public TebiBit(long byteSize) : base(byteSize)
        {

        }

        public TebiBit(decimal size) : base(size)
        {

        }
    }
}
