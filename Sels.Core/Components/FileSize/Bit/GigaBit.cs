using Sels.Core.Templates.FileSize.Bit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.FileSize.Bit
{
    /// <summary>
    /// Displays file size in gigabit.
    /// </summary>
    public class GigaBit : BitFileSize
    {
        // Constants
        public const string FileSizeAbbreviation = "Gb";

        // Properties
        public override string Abbreviation => FileSizeAbbreviation;
        protected override int SizeMultiplier => 3;

        public GigaBit() : base()
        {

        }

        public GigaBit(long byteSize) : base(byteSize)
        {

        }

        public GigaBit(decimal size) : base(size)
        {

        }
    }
}
