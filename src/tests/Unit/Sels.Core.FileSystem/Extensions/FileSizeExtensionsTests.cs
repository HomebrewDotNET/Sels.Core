using Sels.Core.FileSystem.Extensions.FileSizes;
using Sels.Core.FileSystem;

namespace Sels.Core.FileSystem.Tests.Extensions
{
    /// <summary>Contract tests for file-size suffix and closest-unit conversion.</summary>
    [Trait("Category", "Unit")]
    public class FileSizeExtensionsTests
    {
        /// <summary>Suffix tables preserve factory values, case-insensitive lookup, and ordering.</summary>
        [Fact]
        public void SuffixTables_FactoriesAndOrder_Agree()
        {
            var decimalFiles = FileSizeExtensions.OrderedSizeSuffixes
                .Select((suffix, index) => (suffix, File: FileSizeExtensions.SizeSuffixes[suffix](1m / (decimal)Math.Pow(1000, index))))
                .ToArray();
            var binaryFiles = FileSizeExtensions.OrderedBinarySizeSuffixes
                .Select((suffix, index) => (suffix, File: FileSizeExtensions.BinarySizeSuffixes[suffix](1m / (decimal)Math.Pow(1024, index))))
                .ToArray();
            var decimalLookup = FileSizeExtensions.SizeSuffixes[FileSizeExtensions.OrderedSizeSuffixes[1].ToLowerInvariant()];
            var binaryLookup = FileSizeExtensions.BinarySizeSuffixes[FileSizeExtensions.OrderedBinarySizeSuffixes[1].ToLowerInvariant()];

            Assert.All(decimalFiles, entry =>
            {
                Assert.Equal(entry.suffix, entry.File.Abbreviation);
                Assert.Equal(1, entry.File.ByteSize);
            });
            Assert.All(binaryFiles, entry =>
            {
                Assert.Equal(entry.suffix, entry.File.Abbreviation);
                Assert.Equal(1, entry.File.ByteSize);
            });
            Assert.Equal(FileSizeExtensions.SizeSuffixes.Keys.OrderBy(value => value), FileSizeExtensions.OrderedSizeSuffixes.OrderBy(value => value));
            Assert.Equal(FileSizeExtensions.BinarySizeSuffixes.Keys.OrderBy(value => value), FileSizeExtensions.OrderedBinarySizeSuffixes.OrderBy(value => value));
            Assert.Equal(FileSizeExtensions.OrderedSizeSuffixes[1], decimalLookup(0).Abbreviation);
            Assert.Equal(FileSizeExtensions.OrderedBinarySizeSuffixes[1], binaryLookup(0).Abbreviation);
            Assert.Same(FileSystemConstants.Sizes.DecimalFactories, FileSizeExtensions.SizeSuffixes);
            Assert.Same(FileSystemConstants.Sizes.DecimalOrder, FileSizeExtensions.OrderedSizeSuffixes);
            Assert.Same(FileSystemConstants.Sizes.BinaryFactories, FileSizeExtensions.BinarySizeSuffixes);
            Assert.Same(FileSystemConstants.Sizes.BinaryOrder, FileSizeExtensions.OrderedBinarySizeSuffixes);
            Assert.Equal(1000, FileSystemConstants.Sizes.DecimalBoundary);
            Assert.Equal(1024, FileSystemConstants.Sizes.BinaryBoundary);
        }

        /// <summary>Closest-size conversion preserves byte values and fractional unit amounts.</summary>
        [Fact]
        public void ToClosestSize_ZeroAndAboveByteBoundary_PreservesBytes()
        {
            var zero = 0L.ToClosestSize();
            var decimalUnit = 1001L.ToClosestSize(useBinary: false);
            var binaryUnit = 1025L.ToClosestSize(useBinary: true);

            Assert.Equal("SingleByte", zero.GetType().Name);
            Assert.Equal(0, zero.ByteSize);
            Assert.Equal("KiloByte", decimalUnit.GetType().Name);
            Assert.Equal(1.001m, decimalUnit.Size);
            Assert.Equal(1001, decimalUnit.ByteSize);
            Assert.Equal("KibiByte", binaryUnit.GetType().Name);
            Assert.Equal(1025, binaryUnit.ByteSize);
            Assert.Equal(1025m / 1024m, binaryUnit.Size);
        }

        /// <summary>Exact positive unit boundaries promote consistently in decimal and binary systems.</summary>
        [Fact]
        public void ToClosestSize_ExactBoundaries_PromotesToNextUnit()
        {
            var binaryKilo = 1024L.ToClosestSize(useBinary: true);
            var decimalKilo = 1000L.ToClosestSize(useBinary: false);
            var binaryMega = 1048576L.ToClosestSize(useBinary: true);
            var decimalMega = 1000000L.ToClosestSize(useBinary: false);

            Assert.Equal("KibiByte", binaryKilo.GetType().Name);
            Assert.Equal(1m, binaryKilo.Size);
            Assert.Equal(1024, binaryKilo.ByteSize);
            Assert.Equal("KiB", binaryKilo.Abbreviation);
            Assert.Equal("KiloByte", decimalKilo.GetType().Name);
            Assert.Equal(1m, decimalKilo.Size);
            Assert.Equal(1000, decimalKilo.ByteSize);
            Assert.Equal("MebiByte", binaryMega.GetType().Name);
            Assert.Equal(1m, binaryMega.Size);
            Assert.Equal(1048576, binaryMega.ByteSize);
            Assert.Equal("MegaByte", decimalMega.GetType().Name);
            Assert.Equal(1m, decimalMega.Size);
            Assert.Equal(1000000, decimalMega.ByteSize);
        }
    }
}