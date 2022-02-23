using Sels.Core.FileSystem.Templates.FileSizes.Byte;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.FileSizes.Byte
{
    /// <summary>
    /// Displays file size in kilobyte.
    /// </summary>
    public class KiloByte : ByteFileSize
    {
        // Constants
        /// <inheritdoc cref="Abbreviation"/>
        public const string FileSizeAbbreviation = "KB";

        // Properties
        /// <inheritdoc/>
        public override string Abbreviation => FileSizeAbbreviation;
        /// <inheritdoc/>
        protected override int SizeMultiplier => 1;
        /// <inheritdoc cref="KiloByte"/>
        public KiloByte() : base()
        {

        }
        /// <inheritdoc cref="KiloByte"/>
        /// <param name="byteSize">The file size in bytes</param>
        public KiloByte(long byteSize) : base(byteSize)
        {

        }
        /// <inheritdoc cref="KiloByte"/>
        /// <param name="size">The file size in <paramref name="size"/></param>
        public KiloByte(decimal size) : base(size)
        {

        }
    }
}
