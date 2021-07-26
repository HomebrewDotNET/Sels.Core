using Sels.Core.Templates.FileSizes.Byte;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.FileSizes.Byte
{
    /// <summary>
    /// Displays file size in exabyte.
    /// </summary>
    public class ExaByte : ByteFileSize
    {
        // Constants
        public const string FileSizeAbbreviation = "EB";

        // Properties
        public override string Abbreviation => FileSizeAbbreviation;
        protected override int SizeMultiplier => 6;

        public ExaByte() : base()
        {

        }

        public ExaByte(long byteSize) : base(byteSize)
        {

        }

        public ExaByte(decimal size) : base(size)
        {

        }
    }
}
