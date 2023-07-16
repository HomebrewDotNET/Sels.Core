using Sels.Core.FileSystem.Templates.FileSizes.Byte;

namespace Sels.Core.FileSizes.Byte
{
    /// <summary>
    /// Displays file size in megabyte.
    /// </summary>
    public class MegaByte : ByteFileSize
    {
        // Constants
        /// <inheritdoc cref="Abbreviation"/>
        public const string FileSizeAbbreviation = "MB";

        // Properties
        /// <inheritdoc/>
        public override string Abbreviation => FileSizeAbbreviation;
        /// <inheritdoc/>
        protected override int SizeMultiplier => 2;
        /// <inheritdoc cref="MegaByte"/>
        public MegaByte() : base()
        {

        }
        /// <inheritdoc cref="MegaByte"/>
        /// <param name="byteSize">The file size in bytes</param>
        public MegaByte(long byteSize) : base(byteSize)
        {

        }
        /// <inheritdoc cref="MegaByte"/>
        /// <param name="size">The file size in <paramref name="size"/></param>
        public MegaByte(decimal size) : base(size)
        {

        }
    }
}
