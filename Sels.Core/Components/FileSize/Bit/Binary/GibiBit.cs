using Sels.Core.Templates.FileSize.Bit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.FileSize.Bit.Binary
{
    /// <summary>
    /// Displays file size in gibibit.
    /// </summary>
    public class GibiBit : BitBinaryFileSize
    {
        // Constants
        public const string FileSizeAbbreviation = "Gib";

        // Properties
        public override string Abbreviation => FileSizeAbbreviation;
        protected override int SizeMultiplier => 3;

        public GibiBit() : base()
        {

        }

        public GibiBit(long byteSize) : base(byteSize)
        {

        }

        public GibiBit(decimal size) : base(size)
        {

        }
    }
}
