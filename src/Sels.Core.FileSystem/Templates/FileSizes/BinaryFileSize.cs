namespace Sels.Core.FileSystem.Templates.FileSizes
{
    /// <summary>
    /// File size that displays in binary. Uses a unit size of <see cref="BinaryUnitSize"/> instead of the default <see cref="FileSize.DefaultUnitSize"/>.
    /// </summary>
    public abstract class BinaryFileSize : FileSize
    {
        // Constants
        /// <inheritdoc cref="UnitSize"/>
        public const int BinaryUnitSize = 1024;

        // Properties
        /// <inheritdoc/>
        public override int UnitSize => BinaryUnitSize;
        /// <inheritdoc cref="BinaryFileSize"/>
        public BinaryFileSize() : base()
        {

        }
        /// <inheritdoc cref="BinaryFileSize"/>
        /// <param name="byteSize">The file size in bytes</param>
        public BinaryFileSize(long byteSize) : base(byteSize)
        {

        }
        /// <inheritdoc cref="BinaryFileSize"/>
        /// <param name="size">The file size in <paramref name="size"/></param>
        public BinaryFileSize(decimal size) : base(size)
        {

        }
    }
}
