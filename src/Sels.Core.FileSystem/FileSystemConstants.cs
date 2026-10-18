using Sels.Core.FileSizes.Byte;
using Sels.Core.FileSizes.Byte.Binary;
using Sels.Core.FileSystem.Templates.FileSizes;

namespace Sels.Core.FileSystem
{
    /// <summary>Contains file-system constants and file-size suffix tables.</summary>
    public static class FileSystemConstants
    {
        /// <summary>Contains constants and factories for file-size units.</summary>
        public static class Sizes
        {
            /// <summary>The decimal unit boundary.</summary>
            public const int DecimalBoundary = 1000;

            /// <summary>The binary unit boundary.</summary>
            public const int BinaryBoundary = 1024;

            /// <summary>Maps decimal unit abbreviations to their file-size factories.</summary>
            public static readonly IReadOnlyDictionary<string, Func<decimal, FileSize>> DecimalFactories = new Dictionary<string, Func<decimal, FileSize>>(StringComparer.OrdinalIgnoreCase)
            {
                { SingleByte.FileSizeAbbreviation, value => new SingleByte(value) },
                { KiloByte.FileSizeAbbreviation, value => new KiloByte(value) },
                { MegaByte.FileSizeAbbreviation, value => new MegaByte(value) },
                { GigaByte.FileSizeAbbreviation, value => new GigaByte(value) },
                { TeraByte.FileSizeAbbreviation, value => new TeraByte(value) },
                { PetaByte.FileSizeAbbreviation, value => new PetaByte(value) },
                { ExaByte.FileSizeAbbreviation, value => new ExaByte(value) },
                { ZettaByte.FileSizeAbbreviation, value => new ZettaByte(value) },
                { YottaByte.FileSizeAbbreviation, value => new YottaByte(value) }
            };

            /// <summary>Lists decimal unit abbreviations from smallest to largest.</summary>
            public static readonly IReadOnlyList<string> DecimalOrder = new[]
            {
                SingleByte.FileSizeAbbreviation,
                KiloByte.FileSizeAbbreviation,
                MegaByte.FileSizeAbbreviation,
                GigaByte.FileSizeAbbreviation,
                TeraByte.FileSizeAbbreviation,
                PetaByte.FileSizeAbbreviation,
                ExaByte.FileSizeAbbreviation,
                ZettaByte.FileSizeAbbreviation,
                YottaByte.FileSizeAbbreviation
            };

            /// <summary>Maps binary unit abbreviations to their file-size factories.</summary>
            public static readonly IReadOnlyDictionary<string, Func<decimal, FileSize>> BinaryFactories = new Dictionary<string, Func<decimal, FileSize>>(StringComparer.OrdinalIgnoreCase)
            {
                { SingleByte.FileSizeAbbreviation, value => new SingleByte(value) },
                { KibiByte.FileSizeAbbreviation, value => new KibiByte(value) },
                { MebiByte.FileSizeAbbreviation, value => new MebiByte(value) },
                { GibiByte.FileSizeAbbreviation, value => new GibiByte(value) },
                { TebiByte.FileSizeAbbreviation, value => new TebiByte(value) },
                { PebiByte.FileSizeAbbreviation, value => new PebiByte(value) },
                { ExbiByte.FileSizeAbbreviation, value => new ExbiByte(value) },
                { ZebiByte.FileSizeAbbreviation, value => new ZebiByte(value) },
                { YobiByte.FileSizeAbbreviation, value => new YobiByte(value) }
            };

            /// <summary>Lists binary unit abbreviations from smallest to largest.</summary>
            public static readonly IReadOnlyList<string> BinaryOrder = new[]
            {
                SingleByte.FileSizeAbbreviation,
                KibiByte.FileSizeAbbreviation,
                MebiByte.FileSizeAbbreviation,
                GibiByte.FileSizeAbbreviation,
                TebiByte.FileSizeAbbreviation,
                PebiByte.FileSizeAbbreviation,
                ExbiByte.FileSizeAbbreviation,
                ZebiByte.FileSizeAbbreviation,
                YobiByte.FileSizeAbbreviation
            };
        }
    }
}