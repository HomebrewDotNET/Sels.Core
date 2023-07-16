using Sels.Core.FileSystem.Templates.FileSizes.Byte;

namespace Sels.Core.FileSizes.Byte
{
    /// <summary>
    /// Displays file size in gigabyte.
    /// </summary>
    public class GigaByte : ByteFileSize
    {
        // Constants
        /// <inheritdoc cref="Abbreviation"/>
        public const string FileSizeAbbreviation = "GB";

        // Properties
        /// <inheritdoc/>
        public override string Abbreviation => FileSizeAbbreviation;
        /// <inheritdoc/>
        protected override int SizeMultiplier => 3;
        /// <inheritdoc cref="GigaByte"/>
        public GigaByte() : base()
        {

        }
        /// <inheritdoc cref="GigaByte"/>
        /// <param name="byteSize">The file size in bytes</param>
        public GigaByte(long byteSize) : base(byteSize)
        {

        }
        /// <inheritdoc cref="GigaByte"/>
        /// <param name="size">The file size in <paramref name="size"/></param>
        public GigaByte(decimal size) : base(size)
        {

        }
    }
}
