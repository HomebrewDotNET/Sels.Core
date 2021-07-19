using Sels.Core.Templates.FileSize.Bit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.FileSize.Bit
{
    /// <summary>
    /// Displays file size in exabit.
    /// </summary>
    public class ExaBit : BitFileSize
    {
        // Constants
        public const string FileSizeAbbreviation = "Eb";

        // Properties
        public override string Abbreviation => FileSizeAbbreviation;
        protected override int SizeMultiplier => 6;

        public ExaBit() : base()
        {

        }

        public ExaBit(long byteSize) : base(byteSize)
        {

        }

        public ExaBit(decimal size) : base(size)
        {

        }
    }
}
