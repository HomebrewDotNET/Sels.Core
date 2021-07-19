using Sels.Core.Templates.FileSize.Byte;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.FileSize.Byte.Binary
{
    /// <summary>
    /// Displays file size in exbibyte.
    /// </summary>
    public class ExbiByte : ByteBinaryFileSize
    {
        // Constants
        public const string FileSizeAbbreviation = "EiB";

        // Properties
        public override string Abbreviation => FileSizeAbbreviation;
        protected override int SizeMultiplier => 6;

        public ExbiByte() : base()
        {

        }

        public ExbiByte(long byteSize) : base(byteSize)
        {

        }

        public ExbiByte(decimal size) : base(size)
        {

        }
    }
}
