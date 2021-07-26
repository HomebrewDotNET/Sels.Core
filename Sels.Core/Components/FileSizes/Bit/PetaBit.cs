using Sels.Core.Templates.FileSizes.Bit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.FileSizes.Bit
{
    /// <summary>
    /// Displays file size in petabyte.
    /// </summary>
    public class PetaBit : BitFileSize
    {
        // Constants
        public const string FileSizeAbbreviation = "Pb";

        // Properties
        public override string Abbreviation => FileSizeAbbreviation;
        protected override int SizeMultiplier => 5;

        public PetaBit() : base()
        {

        }

        public PetaBit(long byteSize) : base(byteSize)
        {

        }

        public PetaBit(decimal size) : base(size)
        {

        }
    }
}
