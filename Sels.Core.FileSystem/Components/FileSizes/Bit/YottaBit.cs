using Sels.Core.FileSystem.Templates.FileSizes.Bit;

namespace Sels.Core.FileSizes.Bit
{
    /// <summary>
    /// Displays file size in yottabit.
    /// </summary>
    public class YottaBit : BitFileSize
    {
        // Constants
        /// <inheritdoc cref="Abbreviation"/>
        public const string FileSizeAbbreviation = "Yb";

        // Properties
        /// <inheritdoc/>
        public override string Abbreviation => FileSizeAbbreviation;
        /// <inheritdoc/>
        protected override int SizeMultiplier => 8;
        /// <inheritdoc cref="YottaBit"/>
        public YottaBit() : base()
        {

        }
        /// <inheritdoc cref="YottaBit"/>
        /// <param name="byteSize">The file size in bytes</param>
        public YottaBit(long byteSize) : base(byteSize)
        {

        }
        /// <inheritdoc cref="YottaBit"/>
        /// <param name="size">The file size in <see cref="Abbreviation"/></param>
        public YottaBit(decimal size) : base(size)
        {

        }
    }
}
