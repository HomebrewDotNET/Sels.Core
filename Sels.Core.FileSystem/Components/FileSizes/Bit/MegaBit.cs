using Sels.Core.FileSystem.Templates.FileSizes.Bit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.FileSizes.Bit
{
    /// <summary>
    /// Displays file size in megabit.
    /// </summary>
    public class MegaBit : BitFileSize
    {
        // Constants
        /// <inheritdoc cref="Abbreviation"/>
        public const string FileSizeAbbreviation = "Mb";

        // Properties
        /// <inheritdoc/>
        public override string Abbreviation => FileSizeAbbreviation;
        /// <inheritdoc/>
        protected override int SizeMultiplier => 2;
        /// <inheritdoc cref="MegaBit"/>
        public MegaBit() : base()
        {

        }
        /// <inheritdoc cref="MegaBit"/>
        /// <param name="byteSize">The file size in bytes</param>
        public MegaBit(long byteSize) : base(byteSize)
        {

        }
        /// <inheritdoc cref="MegaBit"/>
        /// <param name="size">The file size in <see cref="Abbreviation"/></param>
        public MegaBit(decimal size) : base(size)
        {

        }
    }
}
