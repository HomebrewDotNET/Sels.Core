using Sels.Core.Templates.FileSize.Bit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.FileSize.Bit.Binary
{
    /// <summary>
    /// Displays file size in kibibit.
    /// </summary>
    public class KibiBit : BitBinaryFileSize
    {
        // Constants
        public const string FileSizeAbbreviation = "Kib";

        // Properties
        public override string Abbreviation => FileSizeAbbreviation;
        protected override int SizeMultiplier => 1;

        public KibiBit() : base()
        {

        }

        public KibiBit(long byteSize) : base(byteSize)
        {

        }

        public KibiBit(decimal size) : base(size)
        {

        }
    }
}
