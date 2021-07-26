using Sels.Core.Templates.FileSizes.Byte;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.FileSizes.Byte.Binary
{
    /// <summary>
    /// Displays file size in yobibyte.
    /// </summary>
    public class YobiByte : ByteBinaryFileSize
    {
        // Constants
        public const string FileSizeAbbreviation = "YiB";

        // Properties
        public override string Abbreviation => FileSizeAbbreviation;
        protected override int SizeMultiplier => 8;

        public YobiByte() : base()
        {

        }

        public YobiByte(long byteSize) : base(byteSize)
        {

        }

        public YobiByte(decimal size) : base(size)
        {

        }
    }
}
