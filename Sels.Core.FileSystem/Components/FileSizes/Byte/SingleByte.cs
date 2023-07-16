using Sels.Core.FileSystem.Templates.FileSizes.Byte;

namespace Sels.Core.FileSizes.Byte
{
    /// <summary>
    /// Displays file size in byte.
    /// </summary>
    public class SingleByte : ByteFileSize
    {
        // Constants
        /// <inheritdoc cref="DisplayName"/>
        public const string FileSizeDisplayName = "Byte";
        /// <inheritdoc cref="Abbreviation"/>
        public const string FileSizeAbbreviation = "B";

        // Properties
        /// <inheritdoc/>
        public override string Abbreviation => FileSizeAbbreviation;
        /// <inheritdoc/>
        protected override int SizeMultiplier => 0;
        /// <inheritdoc/>
        public override string DisplayName => FileSizeDisplayName;
        /// <inheritdoc cref="SingleByte"/>
        public SingleByte() : base()
        {

        }
        /// <inheritdoc cref="SingleByte"/>
        /// <param name="byteSize">The file size in bytes</param>
        public SingleByte(long byteSize) : base(byteSize)
        {

        }
        /// <inheritdoc cref="SingleByte"/>
        /// <param name="size">The file size in <paramref name="size"/></param>
        public SingleByte(decimal size) : base(size)
        {

        }
    }
}
