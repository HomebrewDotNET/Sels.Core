using Sels.Core.FileSystem.Templates.FileSizes.Bit;

namespace Sels.Core.Components.FileSizes.Bit
{
    /// <summary>
    /// Displays file size in zettabit.
    /// </summary>
    public class ZettaBit : BitFileSize
    {
        // Constants
        /// <inheritdoc cref="Abbreviation"/>
        public const string FileSizeAbbreviation = "Zb";

        // Properties
        /// <inheritdoc/>
        public override string Abbreviation => FileSizeAbbreviation;
        /// <inheritdoc/>
        protected override int SizeMultiplier => 7;
        /// <inheritdoc cref="ZettaBit"/>
        public ZettaBit() : base()
        {

        }
        /// <inheritdoc cref="ZettaBit"/>
        /// <param name="byteSize">The file size in bytes</param>
        public ZettaBit(long byteSize) : base(byteSize)
        {

        }
        /// <inheritdoc cref="ZettaBit"/>
        /// <param name="size">The file size in <see cref="Abbreviation"/></param>
        public ZettaBit(decimal size) : base(size)
        {

        }
    }
}
