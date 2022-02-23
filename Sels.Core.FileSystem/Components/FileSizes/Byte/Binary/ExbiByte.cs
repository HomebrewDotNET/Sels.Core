using Sels.Core.FileSystem.Templates.FileSizes.Byte;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.FileSizes.Byte.Binary
{
    /// <summary>
    /// Displays file size in exbibyte.
    /// </summary>
    public class ExbiByte : ByteBinaryFileSize
    {
        // Constants
        /// <inheritdoc cref="Abbreviation"/>
        public const string FileSizeAbbreviation = "EiB";

        // Properties
        /// <inheritdoc/>
        public override string Abbreviation => FileSizeAbbreviation;
        /// <inheritdoc/>
        protected override int SizeMultiplier => 6;
        /// <inheritdoc cref="ExbiByte"/>
        public ExbiByte() : base()
        {

        }
        /// <inheritdoc cref="ExbiByte"/>
        /// <param name="byteSize">The file size in bytes</param>
        public ExbiByte(long byteSize) : base(byteSize)
        {

        }
        /// <inheritdoc cref="ExbiByte"/>
        /// <param name="size">The file size in <paramref name="size"/></param>
        public ExbiByte(decimal size) : base(size)
        {

        }
    }
}
