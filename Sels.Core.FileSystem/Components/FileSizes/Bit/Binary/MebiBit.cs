using Sels.Core.FileSystem.Templates.FileSizes.Bit;

namespace Sels.Core.Components.FileSizes.Bit.Binary
{
    /// <summary>
    /// Displays file size in mebibit.
    /// </summary>
    public class MebiBit : BitBinaryFileSize
    {
        // Constants
        /// <inheritdoc cref="Abbreviation"/>
        public const string FileSizeAbbreviation = "Mib";

        // Properties
        /// <inheritdoc/>
        public override string Abbreviation => FileSizeAbbreviation;
        /// <inheritdoc/>
        protected override int SizeMultiplier => 2;
        /// <inheritdoc cref="MebiBit"/>
        public MebiBit() : base()
        {

        }
        /// <inheritdoc cref="MebiBit"/>
        /// <param name="byteSize">The file size in bytes</param>
        public MebiBit(long byteSize) : base(byteSize)
        {

        }
        /// <inheritdoc cref="MebiBit"/>
        /// <param name="size">The file size in <see cref="Abbreviation"/></param>
        public MebiBit(decimal size) : base(size)
        {

        }
    }
}
