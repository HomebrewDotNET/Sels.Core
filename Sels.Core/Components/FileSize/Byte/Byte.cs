using Sels.Core.Templates.FileSize.Byte;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.FileSize.Byte
{
    /// <summary>
    /// Displays file size in byte.
    /// </summary>
    public class Byte : ByteFileSize
    {
        // Constants
        public const string FileSizeAbbreviation = "B";

        // Properties
        public override string Abbreviation => FileSizeAbbreviation;
        protected override int SizeMultiplier => 0;

        public Byte() : base()
        {

        }

        public Byte(long byteSize) : base(byteSize)
        {

        }

        public Byte(decimal size) : base(size)
        {

        }
    }
}
