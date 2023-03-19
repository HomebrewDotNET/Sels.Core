using Sels.Core.FileSystem.Templates.FileSizes.Bit;

namespace Sels.Core.Components.FileSizes.Bit.Binary
{
    /// <summary>
    /// Displays file size in kibibit.
    /// </summary>
    public class KibiBit : BitBinaryFileSize
    {
        // Constants
        /// <inheritdoc cref="Abbreviation"/>
        public const string FileSizeAbbreviation = "Kib";

        // Properties
        /// <inheritdoc/>
        public override string Abbreviation => FileSizeAbbreviation;
        /// <inheritdoc/>
        protected override int SizeMultiplier => 1;
        /// <inheritdoc cref="KibiBit"/>
        public KibiBit() : base()
        {

        }
        /// <inheritdoc cref="KibiBit"/>
        /// <param name="byteSize">The file size in bytes</param>
        public KibiBit(long byteSize) : base(byteSize)
        {

        }
        /// <inheritdoc cref="KibiBit"/>
        /// <param name="size">The file size in <see cref="Abbreviation"/></param>
        public KibiBit(decimal size) : base(size)
        {

        }
    }
}
