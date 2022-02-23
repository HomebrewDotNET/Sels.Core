using Sels.Core.FileSystem.Templates.FileSizes.Byte;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.FileSizes.Byte
{
    /// <summary>
    /// Displays file size in petabyte.
    /// </summary>
    public class PetaByte : ByteFileSize
    {
        // Constants
        /// <inheritdoc cref="Abbreviation"/>
        public const string FileSizeAbbreviation = "PB";

        // Properties
        /// <inheritdoc/>
        public override string Abbreviation => FileSizeAbbreviation;
        /// <inheritdoc/>
        protected override int SizeMultiplier => 5;
        /// <inheritdoc cref="PetaByte"/>
        public PetaByte() : base()
        {

        }
        /// <inheritdoc cref="PetaByte"/>
        /// <param name="byteSize">The file size in bytes</param>
        public PetaByte(long byteSize) : base(byteSize)
        {

        }
        /// <inheritdoc cref="PetaByte"/>
        /// <param name="size">The file size in <paramref name="size"/></param>
        public PetaByte(decimal size) : base(size)
        {

        }
    }
}
