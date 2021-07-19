using Sels.Core.Templates.FileSize.Bit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.FileSize.Bit
{
    /// <summary>
    /// Displays file size in kilobit.
    /// </summary>
    public class KiloBit : BitFileSize
    {
        // Constants
        public const string FileSizeAbbreviation = "Kb";

        // Properties
        public override string Abbreviation => FileSizeAbbreviation;
        protected override int SizeMultiplier => 1;

        public KiloBit() : base()
        {

        }

        public KiloBit(long byteSize) : base(byteSize)
        {

        }

        public KiloBit(decimal size) : base(size)
        {

        }
    }
}
