using Sels.Core.Templates.FileSize.Bit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.FileSize.Bit.Binary
{
    /// <summary>
    /// Displays file size in yobibit.
    /// </summary>
    public class YobiBit : BitBinaryFileSize
    {
        // Constants
        public const string FileSizeAbbreviation = "Yib";

        // Properties
        public override string Abbreviation => FileSizeAbbreviation;
        protected override int SizeMultiplier => 8;

        public YobiBit() : base()
        {

        }

        public YobiBit(long byteSize) : base(byteSize)
        {

        }

        public YobiBit(decimal size) : base(size)
        {

        }
    }
}
