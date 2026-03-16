using Sels.Core.FileSystem.Templates.FileSizes.Bit;

namespace Sels.Core.FileSizes.Bit
{
    /// <summary>
    /// Displays file size in bit.
    /// </summary>
    public class Bit : BitFileSize
    {
        // Constants
        /// <inheritdoc cref="Abbreviation"/>
        public const string FileSizeAbbreviation = "b";

        // Properties
        /// <inheritdoc/>
        public override string Abbreviation => FileSizeAbbreviation;
        /// <inheritdoc/>
        protected override int SizeMultiplier => 0;
        /// <inheritdoc cref="Bit"/>
        public Bit() : base()
        {

        }
        /// <inheritdoc cref="Bit"/>
        /// <param name="byteSize">The file size in bytes</param>
        public Bit(long byteSize) : base(byteSize)
        {

        }
        /// <inheritdoc cref="Bit"/>
        /// <param name="size">The file size in <see cref="Abbreviation"/></param>
        public Bit(decimal size) : base(size)
        {

        }
    }
}
