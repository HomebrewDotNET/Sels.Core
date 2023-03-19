using Sels.Core.FileSystem.Templates.FileSizes.Bit;

namespace Sels.Core.Components.FileSizes.Bit
{
    /// <summary>
    /// Displays file size in petabyte.
    /// </summary>
    public class PetaBit : BitFileSize
    {
        // Constants
        /// <inheritdoc cref="Abbreviation"/>
        public const string FileSizeAbbreviation = "Pb";

        // Properties
        /// <inheritdoc/>
        public override string Abbreviation => FileSizeAbbreviation;
        /// <inheritdoc/>
        protected override int SizeMultiplier => 5;
        /// <inheritdoc cref="PetaBit"/>
        public PetaBit() : base()
        {

        }
        /// <inheritdoc cref="PetaBit"/>
        /// <param name="byteSize">The file size in bytes</param>
        public PetaBit(long byteSize) : base(byteSize)
        {

        }
        /// <inheritdoc cref="PetaBit"/>
        /// <param name="size">The file size in <see cref="Abbreviation"/></param>
        public PetaBit(decimal size) : base(size)
        {

        }
    }
}
