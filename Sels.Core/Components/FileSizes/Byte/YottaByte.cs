using Sels.Core.Templates.FileSizes.Byte;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.FileSizes.Byte
{
    /// <summary>
    /// Displays file size in yottabyte.
    /// </summary>
    public class YottaByte : ByteFileSize
    {
        // Constants
        public const string FileSizeAbbreviation = "YB";

        // Properties
        public override string Abbreviation => FileSizeAbbreviation;
        protected override int SizeMultiplier => 8;

        public YottaByte() : base()
        {

        }

        public YottaByte(long byteSize) : base(byteSize)
        {

        }

        public YottaByte(decimal size) : base(size)
        {

        }
    }
}
