using Sels.Core.FileSystem.Templates.FileSizes.Byte;

namespace Sels.Core.FileSizes.Byte
{
    /// <summary>
    /// Displays file size in exabyte.
    /// </summary>
    public class ExaByte : ByteFileSize
    {
        // Constants
        /// <inheritdoc cref="Abbreviation"/>
        public const string FileSizeAbbreviation = "EB";

        // Properties
        /// <inheritdoc/>
        public override string Abbreviation => FileSizeAbbreviation;
        /// <inheritdoc/>
        protected override int SizeMultiplier => 6;
        /// <inheritdoc cref="ExaByte"/>
        public ExaByte() : base()
        {

        }
        /// <inheritdoc cref="ExaByte"/>
        /// <param name="byteSize">The file size in bytes</param>
        public ExaByte(long byteSize) : base(byteSize)
        {

        }
        /// <inheritdoc cref="ExaByte"/>
        /// <param name="size">The file size in <paramref name="size"/></param>
        public ExaByte(decimal size) : base(size)
        {

        }
    }
}
