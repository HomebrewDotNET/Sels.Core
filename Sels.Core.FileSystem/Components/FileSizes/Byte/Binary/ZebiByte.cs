using Sels.Core.FileSystem.Templates.FileSizes.Byte;

namespace Sels.Core.FileSizes.Byte.Binary
{
    /// <summary>
    /// Displays file size in zebibyte.
    /// </summary>
    public class ZebiByte : ByteBinaryFileSize
    {
        // Constants
        /// <inheritdoc cref="Abbreviation"/>
        public const string FileSizeAbbreviation = "ZiB";

        // Properties
        /// <inheritdoc/>
        public override string Abbreviation => FileSizeAbbreviation;
        /// <inheritdoc/>
        protected override int SizeMultiplier => 7;
        /// <inheritdoc cref="ZebiByte"/>
        public ZebiByte() : base()
        {

        }
        /// <inheritdoc cref="ZebiByte"/>
        /// <param name="byteSize">The file size in bytes</param>
        public ZebiByte(long byteSize) : base(byteSize)
        {

        }
        /// <inheritdoc cref="ZebiByte"/>
        /// <param name="size">The file size in <paramref name="size"/></param>
        public ZebiByte(decimal size) : base(size)
        {

        }
    }
}
