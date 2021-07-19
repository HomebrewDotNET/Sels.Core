using Sels.Core.Templates.FileSize.Byte;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.FileSize.Byte.Binary
{
    /// <summary>
    /// Displays file size in pebibyte.
    /// </summary>
    public class PebiByte : ByteBinaryFileSize
    {
        // Constants
        public const string FileSizeAbbreviation = "PiB";

        // Properties
        public override string Abbreviation => FileSizeAbbreviation;
        protected override int SizeMultiplier => 5;

        public PebiByte() : base()
        {

        }

        public PebiByte(long byteSize) : base(byteSize)
        {

        }

        public PebiByte(decimal size) : base(size)
        {

        }
    }
}
