using Sels.Core.FileSystem.Templates.FileSizes.Byte;

namespace Sels.Core.Components.FileSizes.Byte.Binary
{
    /// <summary>
    /// Displays file size in yobibyte.
    /// </summary>
    public class YobiByte : ByteBinaryFileSize
    {
        // Constants
        /// <inheritdoc cref="Abbreviation"/>
        public const string FileSizeAbbreviation = "YiB";

        // Properties
        /// <inheritdoc/>
        public override string Abbreviation => FileSizeAbbreviation;
        /// <inheritdoc/>
        protected override int SizeMultiplier => 8;
        /// <inheritdoc cref="YobiByte"/>
        public YobiByte() : base()
        {

        }
        /// <inheritdoc cref="YobiByte"/>
        /// <param name="byteSize">The file size in bytes</param>
        public YobiByte(long byteSize) : base(byteSize)
        {

        }
        /// <inheritdoc cref="YobiByte"/>
        /// <param name="size">The file size in <paramref name="size"/></param>
        public YobiByte(decimal size) : base(size)
        {

        }
    }
}
