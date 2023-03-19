namespace Sels.Core.FileSystem.Templates.FileSizes.Bit
{
    /// <summary>
    /// Displays size in bit format.
    /// </summary>
    public abstract class BitBinaryFileSize : BinaryFileSize
    {
        // Properties
        /// <inheritdoc/>
        public override bool IsByteSize => false;

        /// <inheritdoc cref="BitBinaryFileSize"/>
        public BitBinaryFileSize() : base()
        {

        }
        /// <inheritdoc cref="BitBinaryFileSize"/>
        /// <param name="byteSize">The file size in bytes</param>
        public BitBinaryFileSize(long byteSize) : base(byteSize)
        {

        }
        /// <inheritdoc cref="BitBinaryFileSize"/>
        /// <param name="size">The file size in <paramref name="size"/></param>
        public BitBinaryFileSize(decimal size) : base(size)
        {

        }
    }
}
