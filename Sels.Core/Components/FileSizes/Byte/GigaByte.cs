using Sels.Core.Templates.FileSizes.Byte;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.FileSizes.Byte
{
    /// <summary>
    /// Displays file size in gigabyte.
    /// </summary>
    public class GigaByte : ByteFileSize
    {
        // Constants
        public const string FileSizeAbbreviation = "GB";

        // Properties
        public override string Abbreviation => FileSizeAbbreviation;
        protected override int SizeMultiplier => 3;

        public GigaByte() : base()
        {

        }

        public GigaByte(long byteSize) : base(byteSize)
        {

        }

        public GigaByte(decimal size) : base(size)
        {

        }
    }
}
