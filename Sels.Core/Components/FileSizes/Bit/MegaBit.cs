using Sels.Core.Templates.FileSizes.Bit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.FileSizes.Bit
{
    /// <summary>
    /// Displays file size in megabit.
    /// </summary>
    public class MegaBit : BitFileSize
    {
        // Constants
        public const string FileSizeAbbreviation = "Mb";

        // Properties
        public override string Abbreviation => FileSizeAbbreviation;
        protected override int SizeMultiplier => 2;

        public MegaBit() : base()
        {

        }

        public MegaBit(long byteSize) : base(byteSize)
        {

        }

        public MegaBit(decimal size) : base(size)
        {

        }
    }
}
