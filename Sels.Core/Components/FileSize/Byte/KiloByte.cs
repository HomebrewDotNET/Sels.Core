using Sels.Core.Templates.FileSize.Byte;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.FileSize.Byte
{
    /// <summary>
    /// Displays file size in kilobyte.
    /// </summary>
    public class KiloByte : ByteFileSize
    {
        // Constants
        public const string FileSizeAbbreviation = "KB";

        // Properties
        public override string Abbreviation => FileSizeAbbreviation;
        protected override int SizeMultiplier => 1;

        public KiloByte() : base()
        {

        }

        public KiloByte(long byteSize) : base(byteSize)
        {

        }

        public KiloByte(decimal size) : base(size)
        {

        }
    }
}
