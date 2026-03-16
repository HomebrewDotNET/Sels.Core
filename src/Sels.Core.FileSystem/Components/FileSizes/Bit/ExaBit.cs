using Sels.Core.FileSystem.Templates.FileSizes.Bit;

namespace Sels.Core.FileSizes.Bit
{
    /// <summary>
    /// Displays file size in exabit.
    /// </summary>
    public class ExaBit : BitFileSize
    {
        // Constants
        /// <inheritdoc cref="Abbreviation"/>
        public const string FileSizeAbbreviation = "Eb";

        // Properties
        /// <inheritdoc/>
        public override string Abbreviation => FileSizeAbbreviation;
        /// <inheritdoc/>
        protected override int SizeMultiplier => 6;
        /// <inheritdoc cref="ExaBit"/>
        public ExaBit() : base()
        {

        }
        /// <inheritdoc cref="ExaBit"/>
        /// <param name="byteSize">The file size in bytes</param>
        public ExaBit(long byteSize) : base(byteSize)
        {

        }
        /// <inheritdoc cref="ExaBit"/>
        /// <param name="size">The file size in <see cref="Abbreviation"/></param>
        public ExaBit(decimal size) : base(size)
        {

        }
    }
}
