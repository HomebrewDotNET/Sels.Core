using Sels.Core.Templates.FileSizes.Byte;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.FileSizes.Byte.Binary
{
    /// <summary>
    /// Displays file size in kibibyte.
    /// </summary>
    public class KibiByte : ByteBinaryFileSize
    {
        // Constants
        public const string FileSizeAbbreviation = "KiB";

        // Properties
        public override string Abbreviation => FileSizeAbbreviation;
        protected override int SizeMultiplier => 1;

        public KibiByte() : base()
        {

        }

        public KibiByte(long byteSize) : base(byteSize)
        {

        }

        public KibiByte(decimal size) : base(size)
        {

        }
    }
}
