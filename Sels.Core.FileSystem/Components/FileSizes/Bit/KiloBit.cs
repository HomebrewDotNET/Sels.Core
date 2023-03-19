using Sels.Core.FileSystem.Templates.FileSizes.Bit;

namespace Sels.Core.Components.FileSizes.Bit
{
    /// <summary>
    /// Displays file size in kilobit.
    /// </summary>
    public class KiloBit : BitFileSize
    {
        // Constants
        /// <inheritdoc cref="Abbreviation"/>
        public const string FileSizeAbbreviation = "Kb";

        // Properties
        /// <inheritdoc/>
        public override string Abbreviation => FileSizeAbbreviation;
        /// <inheritdoc/>
        protected override int SizeMultiplier => 1;
        /// <inheritdoc cref="KiloBit"/>
        public KiloBit() : base()
        {

        }
        /// <inheritdoc cref="KiloBit"/>
        /// <param name="byteSize">The file size in bytes</param>
        public KiloBit(long byteSize) : base(byteSize)
        {

        }
        /// <inheritdoc cref="KiloBit"/>
        /// <param name="size">The file size in <see cref="Abbreviation"/></param>
        public KiloBit(decimal size) : base(size)
        {

        }
    }
}
