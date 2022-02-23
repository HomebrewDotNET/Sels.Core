using Sels.Core.FileSystem.Templates.FileSizes.Byte;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.FileSizes.Byte.Binary
{
    /// <summary>
    /// Displays file size in mebibyte.
    /// </summary>
    public class MebiByte : ByteBinaryFileSize
    {
        // Constants
        /// <inheritdoc cref="Abbreviation"/>
        public const string FileSizeAbbreviation = "MiB";

        // Properties
        /// <inheritdoc/>
        public override string Abbreviation => FileSizeAbbreviation;
        /// <inheritdoc/>
        protected override int SizeMultiplier => 2;
        /// <inheritdoc cref="MebiByte"/>
        public MebiByte() : base()
        {

        }
        /// <inheritdoc cref="MebiByte"/>
        /// <param name="byteSize">The file size in bytes</param>
        public MebiByte(long byteSize) : base(byteSize)
        {

        }
        /// <inheritdoc cref="MebiByte"/>
        /// <param name="size">The file size in <paramref name="size"/></param>
        public MebiByte(decimal size) : base(size)
        {

        }
    }
}
