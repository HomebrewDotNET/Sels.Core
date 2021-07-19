using Sels.Core.Templates.FileSize.Bit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.FileSize.Bit
{
    /// <summary>
    /// Displays file size in yottabit.
    /// </summary>
    public class YottaBit : BitFileSize
    {
        // Constants
        public const string FileSizeAbbreviation = "Yb";

        // Properties
        public override string Abbreviation => FileSizeAbbreviation;
        protected override int SizeMultiplier => 8;

        public YottaBit() : base()
        {

        }

        public YottaBit(long byteSize) : base(byteSize)
        {

        }

        public YottaBit(decimal size) : base(size)
        {

        }
    }
}
