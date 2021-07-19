using Sels.Core.Templates.FileSize.Byte;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.FileSize.Byte.Binary
{
    /// <summary>
    /// Displays file size in mebibyte.
    /// </summary>
    public class MebiByte : ByteBinaryFileSize
    {
        // Constants
        public const string FileSizeAbbreviation = "MiB";

        // Properties
        public override string Abbreviation => FileSizeAbbreviation;
        protected override int SizeMultiplier => 2;

        public MebiByte() : base()
        {

        }

        public MebiByte(long byteSize) : base(byteSize)
        {

        }

        public MebiByte(decimal size) : base(size)
        {

        }
    }
}
