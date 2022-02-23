using Sels.Core.FileSystem.Templates.FileSizes.Bit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.FileSizes.Bit.Binary
{
    /// <summary>
    /// Displays file size in exbibit.
    /// </summary>
    public class ExbiBit : BitBinaryFileSize
    {
        // Constants
        /// <inheritdoc cref="Abbreviation"/>
        public const string FileSizeAbbreviation = "Eib";

        // Properties
        /// <inheritdoc/>
        public override string Abbreviation => FileSizeAbbreviation;
        /// <inheritdoc/>
        protected override int SizeMultiplier => 6;
        /// <inheritdoc cref="ExbiBit"/>
        public ExbiBit() : base()
        {

        }
        /// <inheritdoc cref="ExbiBit"/>
        /// <param name="byteSize">The file size in bytes</param>
        public ExbiBit(long byteSize) : base(byteSize)
        {

        }
        /// <inheritdoc cref="ExbiBit"/>
        /// <param name="size">The file size in <see cref="Abbreviation"/></param>
        public ExbiBit(decimal size) : base(size)
        {

        }
    }
}
