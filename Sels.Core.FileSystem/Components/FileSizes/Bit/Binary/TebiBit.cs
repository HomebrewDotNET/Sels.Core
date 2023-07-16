using Sels.Core.FileSystem.Templates.FileSizes.Bit;

namespace Sels.Core.FileSizes.Bit.Binary
{
    /// <summary>
    /// Displays file size in tebibit.
    /// </summary>
    public class TebiBit : BitBinaryFileSize
    {
        // Constants
        /// <inheritdoc cref="Abbreviation"/>
        public const string FileSizeAbbreviation = "Tib";

        // Properties
        /// <inheritdoc/>
        public override string Abbreviation => FileSizeAbbreviation;
        /// <inheritdoc/>
        protected override int SizeMultiplier => 4;
        /// <inheritdoc cref="TebiBit"/>
        public TebiBit() : base()
        {

        }
        /// <inheritdoc cref="TebiBit"/>
        /// <param name="byteSize">The file size in bytes</param>
        public TebiBit(long byteSize) : base(byteSize)
        {

        }
        /// <inheritdoc cref="TebiBit"/>
        /// <param name="size">The file size in <see cref="Abbreviation"/></param>
        public TebiBit(decimal size) : base(size)
        {

        }
    }
}
