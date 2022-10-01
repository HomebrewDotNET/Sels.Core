using Sels.Core.FileSystem.Templates.FileSizes;
using Sels.Core.FileSystem.Templates.FileSizes.Bit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.FileSizes.Bit.Binary
{
    /// <summary>
    /// Displays file size in gibibit.
    /// </summary>
    public class GibiBit : BitBinaryFileSize
    {
        // Constants
        /// <inheritdoc cref="Abbreviation"/>.
        public const string FileSizeAbbreviation = "Gib";

        // Properties
        /// <inheritdoc/>
        public override string Abbreviation => FileSizeAbbreviation;
        /// <inheritdoc/>
        protected override int SizeMultiplier => 3;
        /// <inheritdoc cref="GibiBit"/>
        public GibiBit() : base()
        {

        }
        /// <inheritdoc cref="GibiBit"/>
        /// <param name="byteSize">The file size in bytes</param>
        public GibiBit(long byteSize) : base(byteSize)
        {

        }
        /// <inheritdoc cref="GibiBit"/>
        /// <param name="size">The file size in <see cref="Abbreviation"/></param>
        public GibiBit(decimal size) : base(size)
        {

        }
    }
}
