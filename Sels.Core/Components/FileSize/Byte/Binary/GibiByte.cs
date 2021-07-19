using Sels.Core.Templates.FileSize.Byte;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.FileSize.Byte.Binary
{
    /// <summary>
    /// Displays file size in gibibyte.
    /// </summary>
    public class GibiByte : ByteBinaryFileSize
    {
        // Constants
        public const string FileSizeAbbreviation = "GiB";

        // Properties
        public override string Abbreviation => FileSizeAbbreviation;
        protected override int SizeMultiplier => 3;

        public GibiByte() : base()
        {

        }

        public GibiByte(long byteSize) : base(byteSize)
        {

        }

        public GibiByte(decimal size) : base(size)
        {

        }
    }
}
