namespace Sels.Core.FileSystem.Templates.FileSizes.Byte
{
    /// <summary>
    /// Displays size in byte format.
    /// </summary>
    public abstract class ByteFileSize : FileSize
    {
        // Properties
        /// <inheritdoc/>
        public override bool IsByteSize => true;

        /// <inheritdoc cref="ByteFileSize"/>
        public ByteFileSize() : base()
        {

        }
        /// <inheritdoc cref="ByteFileSize"/>
        /// <param name="byteSize">The file size in bytes</param>
        public ByteFileSize(long byteSize) : base(byteSize)
        {

        }
        /// <inheritdoc cref="ByteFileSize"/>
        /// <param name="size">The file size in <paramref name="size"/></param>
        public ByteFileSize(decimal size) : base(size)
        {

        }
    }
}
