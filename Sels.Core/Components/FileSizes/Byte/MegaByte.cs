using Sels.Core.Templates.FileSizes.Byte;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.FileSizes.Byte
{
    /// <summary>
    /// Displays file size in megabyte.
    /// </summary>
    public class MegaByte : ByteFileSize
    {
        // Constants
        public const string FileSizeAbbreviation = "MB";

        // Properties
        public override string Abbreviation => FileSizeAbbreviation;
        protected override int SizeMultiplier => 2;

        public MegaByte() : base()
        {

        }

        public MegaByte(long byteSize) : base(byteSize)
        {

        }

        public MegaByte(decimal size) : base(size)
        {

        }
    }
}
