using Sels.Core.FileSystem.Templates.FileSizes.Byte;

namespace Sels.Core.Components.FileSizes.Byte
{
    /// <summary>
    /// Displays file size in terabyte.
    /// </summary>
    public class TeraByte : ByteFileSize
    {
        // Constants
        /// <inheritdoc cref="Abbreviation"/>
        public const string FileSizeAbbreviation = "TB";

        // Properties
        /// <inheritdoc/>
        public override string Abbreviation => FileSizeAbbreviation;
        /// <inheritdoc/>
        protected override int SizeMultiplier => 4;
        /// <inheritdoc cref="TeraByte"/>
        public TeraByte() : base()
        {

        }
        /// <inheritdoc cref="TeraByte"/>
        /// <param name="byteSize">The file size in bytes</param>
        public TeraByte(long byteSize) : base(byteSize)
        {

        }
        /// <inheritdoc cref="TeraByte"/>
        /// <param name="size">The file size in <paramref name="size"/></param>
        public TeraByte(decimal size) : base(size)
        {

        }
    }
}
