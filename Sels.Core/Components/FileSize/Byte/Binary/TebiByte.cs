using Sels.Core.Templates.FileSize.Byte;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.FileSize.Byte.Binary
{
    /// <summary>
    /// Displays file size in tebibyte.
    /// </summary>
    public class TebiByte : ByteBinaryFileSize
    {
        // Constants
        public const string FileSizeAbbreviation = "TiB";

        // Properties
        public override string Abbreviation => FileSizeAbbreviation;
        protected override int SizeMultiplier => 4;

        public TebiByte() : base()
        {

        }

        public TebiByte(long byteSize) : base(byteSize)
        {

        }

        public TebiByte(decimal size) : base(size)
        {

        }
    }
}
