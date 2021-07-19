using Sels.Core.Templates.FileSize.Byte;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.FileSize.Byte
{
    /// <summary>
    /// Displays file size in petabyte.
    /// </summary>
    public class PetaByte : ByteFileSize
    {
        // Constants
        public const string FileSizeAbbreviation = "PB";

        // Properties
        public override string Abbreviation => FileSizeAbbreviation;
        protected override int SizeMultiplier => 5;

        public PetaByte() : base()
        {

        }

        public PetaByte(long byteSize) : base(byteSize)
        {

        }

        public PetaByte(decimal size) : base(size)
        {

        }
    }
}
