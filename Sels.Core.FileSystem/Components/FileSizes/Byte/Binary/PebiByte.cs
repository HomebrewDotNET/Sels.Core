using Sels.Core.FileSystem.Templates.FileSizes.Byte;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.FileSizes.Byte.Binary
{
    /// <summary>
    /// Displays file size in pebibyte.
    /// </summary>
    public class PebiByte : ByteBinaryFileSize
    {
        // Constants
        /// <inheritdoc cref="Abbreviation"/>
        public const string FileSizeAbbreviation = "PiB";

        // Properties
        /// <inheritdoc/>
        public override string Abbreviation => FileSizeAbbreviation;
        /// <inheritdoc/>
        protected override int SizeMultiplier => 5;
        /// <inheritdoc cref="PebiByte"/>
        public PebiByte() : base()
        {

        }
        /// <inheritdoc cref="PebiByte"/>
        /// <param name="byteSize">The file size in bytes</param>
        public PebiByte(long byteSize) : base(byteSize)
        {

        }
        /// <inheritdoc cref="PebiByte"/>
        /// <param name="size">The file size in <paramref name="size"/></param>
        public PebiByte(decimal size) : base(size)
        {

        }
    }
}
