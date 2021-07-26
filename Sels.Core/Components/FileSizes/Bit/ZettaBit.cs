using Sels.Core.Templates.FileSizes.Bit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.FileSizes.Bit
{
    /// <summary>
    /// Displays file size in zettabit.
    /// </summary>
    public class ZettaBit : BitFileSize
    {
        // Constants
        public const string FileSizeAbbreviation = "Zb";

        // Properties
        public override string Abbreviation => FileSizeAbbreviation;
        protected override int SizeMultiplier => 7;

        public ZettaBit() : base()
        {

        }

        public ZettaBit(long byteSize) : base(byteSize)
        {

        }

        public ZettaBit(decimal size) : base(size)
        {

        }
    }
}
