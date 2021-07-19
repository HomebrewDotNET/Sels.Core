using Sels.Core.Templates.FileSize.Bit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.FileSize.Bit.Binary
{
    /// <summary>
    /// Displays file size in exbibit.
    /// </summary>
    public class ExbiBit : BitBinaryFileSize
    {
        // Constants
        public const string FileSizeAbbreviation = "Eib";

        // Properties
        public override string Abbreviation => FileSizeAbbreviation;
        protected override int SizeMultiplier => 6;

        public ExbiBit() : base()
        {

        }

        public ExbiBit(long byteSize) : base(byteSize)
        {

        }

        public ExbiBit(decimal size) : base(size)
        {

        }
    }
}
