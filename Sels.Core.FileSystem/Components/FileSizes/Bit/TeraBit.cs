using Sels.Core.FileSystem.Templates.FileSizes.Bit;

namespace Sels.Core.Components.FileSizes.Bit
{
    /// <summary>
    /// Displays file size in terabit.
    /// </summary>
    public class TeraBit : BitFileSize
    {
        // Constants
        /// <inheritdoc cref="Abbreviation"/>
        public const string FileSizeAbbreviation = "Tb";

        // Properties
        /// <inheritdoc/>
        public override string Abbreviation => FileSizeAbbreviation;
        /// <inheritdoc/>
        protected override int SizeMultiplier => 4;
        /// <inheritdoc cref="TeraBit"/>
        public TeraBit() : base()
        {

        }
        /// <inheritdoc cref="TeraBit"/>
        /// <param name="byteSize">The file size in bytes</param>
        public TeraBit(long byteSize) : base(byteSize)
        {

        }
        /// <inheritdoc cref="TeraBit"/>
        /// <param name="size">The file size in <see cref="Abbreviation"/></param>
        public TeraBit(decimal size) : base(size)
        {

        }
    }
}
