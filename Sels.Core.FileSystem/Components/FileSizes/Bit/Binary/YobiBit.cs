using Sels.Core.FileSystem.Templates.FileSizes.Bit;

namespace Sels.Core.FileSizes.Bit.Binary
{
    /// <summary>
    /// Displays file size in yobibit.
    /// </summary>
    public class YobiBit : BitBinaryFileSize
    {
        // Constants
        /// <inheritdoc cref="Abbreviation"/>
        public const string FileSizeAbbreviation = "Yib";

        // Properties
        /// <inheritdoc/>
        public override string Abbreviation => FileSizeAbbreviation;
        /// <inheritdoc/>
        protected override int SizeMultiplier => 8;
        /// <inheritdoc cref="YobiBit"/>
        public YobiBit() : base()
        {

        }
        /// <inheritdoc cref="YobiBit"/>
        /// <param name="byteSize">The file size in bytes</param>
        public YobiBit(long byteSize) : base(byteSize)
        {

        }
        /// <inheritdoc cref="YobiBit"/>
        /// <param name="size">The file size in <see cref="Abbreviation"/></param>
        public YobiBit(decimal size) : base(size)
        {

        }
    }
}
