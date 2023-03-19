using Sels.Core.FileSystem.Templates.FileSizes.Bit;

namespace Sels.Core.Components.FileSizes.Bit
{
    /// <summary>
    /// Displays file size in gigabit.
    /// </summary>
    public class GigaBit : BitFileSize
    {
        // Constants
        /// <inheritdoc cref="Abbreviation"/>
        public const string FileSizeAbbreviation = "Gb";

        // Properties
        /// <inheritdoc/>
        public override string Abbreviation => FileSizeAbbreviation;
        /// <inheritdoc/>
        protected override int SizeMultiplier => 3;
        /// <inheritdoc cref="GigaBit"/>
        public GigaBit() : base()
        {

        }
        /// <inheritdoc cref="GigaBit"/>
        /// <param name="byteSize">The file size in bytes</param>
        public GigaBit(long byteSize) : base(byteSize)
        {

        }
        /// <inheritdoc cref="GigaBit"/>
        /// <param name="size">The file size in <see cref="Abbreviation"/></param>
        public GigaBit(decimal size) : base(size)
        {

        }
    }
}
