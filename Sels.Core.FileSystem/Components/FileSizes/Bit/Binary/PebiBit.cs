using Sels.Core.FileSystem.Templates.FileSizes.Bit;

namespace Sels.Core.Components.FileSizes.Bit.Binary
{
    /// <summary>
    /// Displays file size in pebibit.
    /// </summary>
    public class PebiBit : BitBinaryFileSize
    {
        // Constants
        /// <inheritdoc cref="Abbreviation"/>
        public const string FileSizeAbbreviation = "Pib";

        // Properties
        /// <inheritdoc/>
        public override string Abbreviation => FileSizeAbbreviation;
        /// <inheritdoc/>
        protected override int SizeMultiplier => 5;
        /// <inheritdoc cref="PebiBit"/>
        public PebiBit() : base()
        {

        }
        /// <inheritdoc cref="PebiBit"/>
        /// <param name="byteSize">The file size in bytes</param>
        public PebiBit(long byteSize) : base(byteSize)
        {

        }
        /// <inheritdoc cref="PebiBit"/>
        /// <param name="size">The file size in <see cref="Abbreviation"/></param>
        public PebiBit(decimal size) : base(size)
        {

        }
    }
}
