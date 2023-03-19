using Sels.Core.FileSystem.Templates.FileSizes.Byte;

namespace Sels.Core.Components.FileSizes.Byte.Binary
{
    /// <summary>
    /// Displays file size in kibibyte.
    /// </summary>
    public class KibiByte : ByteBinaryFileSize
    {
        // Constants
        /// <inheritdoc cref="Abbreviation"/>
        public const string FileSizeAbbreviation = "KiB";

        // Properties
        /// <inheritdoc/>
        public override string Abbreviation => FileSizeAbbreviation;
        /// <inheritdoc/>
        protected override int SizeMultiplier => 1;
        /// <inheritdoc cref="KibiByte"/>
        public KibiByte() : base()
        {

        }
        /// <inheritdoc cref="KibiByte"/>
        /// <param name="byteSize">The file size in bytes</param>
        public KibiByte(long byteSize) : base(byteSize)
        {

        }
        /// <inheritdoc cref="KibiByte"/>
        /// <param name="size">The file size in <paramref name="size"/></param>
        public KibiByte(decimal size) : base(size)
        {

        }
    }
}
