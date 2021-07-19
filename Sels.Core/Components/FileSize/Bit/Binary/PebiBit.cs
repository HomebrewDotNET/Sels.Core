using Sels.Core.Templates.FileSize.Bit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.FileSize.Bit.Binary
{
    /// <summary>
    /// Displays file size in pebibit.
    /// </summary>
    public class PebiBit : BitBinaryFileSize
    {
        // Constants
        public const string FileSizeAbbreviation = "Pib";

        // Properties
        public override string Abbreviation => FileSizeAbbreviation;
        protected override int SizeMultiplier => 5;

        public PebiBit() : base()
        {

        }

        public PebiBit(long byteSize) : base(byteSize)
        {

        }

        public PebiBit(decimal size) : base(size)
        {

        }
    }
}
