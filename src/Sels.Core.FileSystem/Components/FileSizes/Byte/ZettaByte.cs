using Sels.Core.FileSystem.Templates.FileSizes.Byte;

namespace Sels.Core.FileSizes.Byte
{
    /// <summary>
    /// Displays file size in zettabyte.
    /// </summary>
    public class ZettaByte : ByteFileSize
    {
        // Constants
        /// <inheritdoc cref="Abbreviation"/>
        public const string FileSizeAbbreviation = "ZB";

        // Properties
        /// <inheritdoc/>
        public override string Abbreviation => FileSizeAbbreviation;
        /// <inheritdoc/>
        protected override int SizeMultiplier => 7;
        /// <inheritdoc cref="ZettaByte"/>
        public ZettaByte() : base()
        {

        }
        /// <inheritdoc cref="ZettaByte"/>
        /// <param name="byteSize">The file size in bytes</param>
        public ZettaByte(long byteSize) : base(byteSize)
        {

        }
        /// <inheritdoc cref="ZettaByte"/>
        /// <param name="size">The file size in <paramref name="size"/></param>
        public ZettaByte(decimal size) : base(size)
        {

        }
    }
}
