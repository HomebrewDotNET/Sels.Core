using Sels.Core.Templates.FileSize.Bit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.FileSize.Bit
{
    /// <summary>
    /// Displays file size in terabit.
    /// </summary>
    public class TeraBit : BitFileSize
    {
        // Constants
        public const string FileSizeAbbreviation = "Tb";

        // Properties
        public override string Abbreviation => FileSizeAbbreviation;
        protected override int SizeMultiplier => 4;

        public TeraBit() : base()
        {

        }

        public TeraBit(long byteSize) : base(byteSize)
        {

        }

        public TeraBit(decimal size) : base(size)
        {

        }
    }
}
