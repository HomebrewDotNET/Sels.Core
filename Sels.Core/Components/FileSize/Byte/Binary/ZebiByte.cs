using Sels.Core.Templates.FileSize.Byte;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.FileSize.Byte.Binary
{
    /// <summary>
    /// Displays file size in zebibyte.
    /// </summary>
    public class ZebiByte : ByteBinaryFileSize
    {
        // Constants
        public const string FileSizeAbbreviation = "ZiB";

        // Properties
        public override string Abbreviation => FileSizeAbbreviation;
        protected override int SizeMultiplier => 7;

        public ZebiByte() : base()
        {

        }

        public ZebiByte(long byteSize) : base(byteSize)
        {

        }

        public ZebiByte(decimal size) : base(size)
        {

        }
    }
}
