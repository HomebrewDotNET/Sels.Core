using Sels.Core.FileSystem.Templates.FileSizes.Byte;

namespace Sels.Core.Components.FileSizes.Byte.Binary
{
    /// <summary>
    /// Displays file size in gibibyte.
    /// </summary>
    public class GibiByte : ByteBinaryFileSize
    {
        // Constants
        /// <inheritdoc cref="Abbreviation"/>
        public const string FileSizeAbbreviation = "GiB";

        // Properties
        /// <inheritdoc/>
        public override string Abbreviation => FileSizeAbbreviation;
        /// <inheritdoc/>
        protected override int SizeMultiplier => 3;
        /// <inheritdoc cref="GibiByte"/>
        public GibiByte() : base()
        {

        }
        /// <inheritdoc cref="GibiByte"/>
        /// <param name="byteSize">The file size in bytes</param>
        public GibiByte(long byteSize) : base(byteSize)
        {

        }
        /// <inheritdoc cref="GibiByte"/>
        /// <param name="size">The file size in <paramref name="size"/></param>
        public GibiByte(decimal size) : base(size)
        {

        }
    }
}
