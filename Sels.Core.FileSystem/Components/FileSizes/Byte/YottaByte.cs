using Sels.Core.FileSystem.Templates.FileSizes.Byte;

namespace Sels.Core.Components.FileSizes.Byte
{
    /// <summary>
    /// Displays file size in yottabyte.
    /// </summary>
    public class YottaByte : ByteFileSize
    {
        // Constants
        /// <inheritdoc cref="Abbreviation"/>
        public const string FileSizeAbbreviation = "YB";

        // Properties
        /// <inheritdoc/>
        public override string Abbreviation => FileSizeAbbreviation;
        /// <inheritdoc/>
        protected override int SizeMultiplier => 8;
        /// <inheritdoc cref="YottaByte"/>
        public YottaByte() : base()
        {

        }
        /// <inheritdoc cref="YottaByte"/>
        /// <param name="byteSize">The file size in bytes</param>
        public YottaByte(long byteSize) : base(byteSize)
        {

        }
        /// <inheritdoc cref="YottaByte"/>
        /// <param name="size">The file size in <paramref name="size"/></param>
        public YottaByte(decimal size) : base(size)
        {

        }
    }
}
