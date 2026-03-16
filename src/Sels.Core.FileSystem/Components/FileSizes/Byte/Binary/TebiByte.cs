using Sels.Core.FileSystem.Templates.FileSizes.Byte;

namespace Sels.Core.FileSizes.Byte.Binary
{
    /// <summary>
    /// Displays file size in tebibyte.
    /// </summary>
    public class TebiByte : ByteBinaryFileSize
    {
        // Constants
        /// <inheritdoc cref="Abbreviation"/>
        public const string FileSizeAbbreviation = "TiB";

        // Properties
        /// <inheritdoc/>
        public override string Abbreviation => FileSizeAbbreviation;
        /// <inheritdoc/>
        protected override int SizeMultiplier => 4;
        /// <inheritdoc cref="TebiByte"/>
        public TebiByte() : base()
        {

        }
        /// <inheritdoc cref="TebiByte"/>
        /// <param name="byteSize">The file size in bytes</param>
        public TebiByte(long byteSize) : base(byteSize)
        {

        }
        /// <inheritdoc cref="TebiByte"/>
        /// <param name="size">The file size in <paramref name="size"/></param>
        public TebiByte(decimal size) : base(size)
        {

        }
    }
}
