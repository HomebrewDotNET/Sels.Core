using Sels.Core.Templates.FileSizes.Byte;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.FileSizes.Byte
{
    /// <summary>
    /// Displays file size in zettabyte.
    /// </summary>
    public class ZettaByte : ByteFileSize
    {
        // Constants
        public const string FileSizeAbbreviation = "ZB";

        // Properties
        public override string Abbreviation => FileSizeAbbreviation;
        protected override int SizeMultiplier => 7;

        public ZettaByte() : base()
        {

        }

        public ZettaByte(long byteSize) : base(byteSize)
        {

        }

        public ZettaByte(decimal size) : base(size)
        {

        }
    }
}
