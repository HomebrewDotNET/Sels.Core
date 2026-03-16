using Sels.Core.FileSystem.Templates.FileSizes.Bit;

namespace Sels.Core.FileSizes.Bit.Binary
{
    /// <summary>
    /// Displays file size in zebibit.
    /// </summary>
    public class ZebiBit : BitBinaryFileSize
    {
        // Constants
        /// <inheritdoc cref="Abbreviation"/>
        public const string FileSizeAbbreviation = "Zib";

        // Properties
        /// <inheritdoc/>
        public override string Abbreviation => FileSizeAbbreviation;
        /// <inheritdoc/>
        protected override int SizeMultiplier => 7;
        /// <inheritdoc cref="ZebiBit"/>
        public ZebiBit() : base()
        {

        }
        /// <inheritdoc cref="ZebiBit"/>
        /// <param name="byteSize">The file size in bytes</param>
        public ZebiBit(long byteSize) : base(byteSize)
        {

        }
        /// <inheritdoc cref="ZebiBit"/>
        /// <param name="size">The file size in <see cref="Abbreviation"/></param>
        public ZebiBit(decimal size) : base(size)
        {

        }
    }
}
