using System.Reflection;
using Sels.Core.Extensions.Conversion;
using Sels.Core.FileSizes.Bit;
using Sels.Core.FileSizes.Bit.Binary;
using Sels.Core.FileSizes.Byte;
using Sels.Core.FileSizes.Byte.Binary;
using Sels.Core.FileSystem.Templates.FileSizes;

namespace Sels.Core.FileSystem.Tests.Templates.FileSizes
{
    /// <summary>Contract tests for shared file-size values.</summary>
    [Trait("Category", "Unit")]
    public class FileSizeTests
    {
        /// <summary>Empty is a readonly zero-byte file-size instance.</summary>
        [Fact]
        public void Empty_Field_IsReadonlyAndZeroBytes()
        {
            var field = typeof(FileSize).GetField(nameof(FileSize.Empty), BindingFlags.Public | BindingFlags.Static);

            var empty = FileSize.Empty;

            Assert.NotNull(field);
            Assert.True(field!.IsInitOnly);
            Assert.NotNull(empty);
            Assert.Equal(0, empty.ByteSize);
            Assert.Equal("SingleByte", empty.GetType().Name);
        }

        /// <summary>Verifies Equals for other file size: returns expected comparison.</summary>
        [Theory]
        [InlineData(125987465632L, typeof(SingleByte), 125987465632L, typeof(MebiBit), true)]
        [InlineData(4672831921838425L, typeof(ExbiByte), 4672831921838424L, typeof(KibiByte), false)]
        [InlineData(98563201474122L, typeof(GigaBit), 98563201474122L, typeof(Bit), true)]
        [InlineData(4586312L, typeof(PetaBit), 5000000L, typeof(TeraByte), false)]
        [InlineData(7854120388L, typeof(KiloByte), 1L, typeof(GibiByte), false)]
        public void Equals_OtherFileSize_ReturnsExpectedComparison(long bytes, Type fileSizeType, long otherBytes, Type otherFileSizeType, bool expected)
        {
            // Arrange
            var fileSize = FileSize.CreateFromBytes(bytes, fileSizeType);
            var otherFileSize = FileSize.CreateFromBytes(otherBytes, otherFileSizeType);

            // Act
            var result = fileSize.Equals(otherFileSize);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>Verifies ToSize for created from size: sets expected byte size.</summary>
        [Theory]
        [InlineData(8, typeof(Bit), 1)]
        [InlineData(1, typeof(SingleByte), 1)]
        [InlineData(50, typeof(KiloByte), 50000)]
        [InlineData(4.5, typeof(MebiByte), 4718592)]
        [InlineData(0.1, typeof(GibiByte), 107374182)]
        public void ToSize_CreatedFromSize_SetsExpectedByteSize(double size, Type fileSizeType, long expected)
        {
            // Arrange
            var decimalSize = size.ChangeType<decimal>();

            // Act
            var fileSize = FileSize.CreateFromSize(decimalSize, fileSizeType);

            // Assert
            Assert.Equal(expected, fileSize.ByteSize);
        }

        /// <summary>Verifies ToSize for other file size type: keeps byte size.</summary>
        [Theory]
        [InlineData(125987465632L, typeof(SingleByte), typeof(MebiBit))]
        [InlineData(4672831921838425L, typeof(ExbiByte), typeof(KibiByte))]
        [InlineData(98563201474122L, typeof(GigaBit), typeof(Bit))]
        [InlineData(4586312L, typeof(PetaBit), typeof(TeraByte))]
        [InlineData(7854120388L, typeof(KiloByte), typeof(GibiByte))]
        public void ToSize_OtherFileSizeType_KeepsByteSize(long bytes, Type sourceFileSizeType, Type targetFileSizeType)
        {
            // Arrange
            var sourceFileSize = FileSize.CreateFromBytes(bytes, sourceFileSizeType);

            // Act
            var targetFileSize = sourceFileSize.ToSize(targetFileSizeType);

            /// Assert
            Assert.Equal(sourceFileSize.ByteSize, targetFileSize.ByteSize);
        }

        /// <summary>Verifies ToSize for round trip to source type: keeps byte size.</summary>
        [Theory]
        [InlineData(125987465632L, typeof(SingleByte), typeof(MebiBit))]
        [InlineData(4672831921838425L, typeof(ExbiByte), typeof(KibiByte))]
        [InlineData(98563201474122L, typeof(GigaBit), typeof(Bit))]
        [InlineData(4586312L, typeof(PetaBit), typeof(TeraByte))]
        [InlineData(7854120388L, typeof(KiloByte), typeof(GibiByte))]
        public void ToSize_RoundTripToSourceType_KeepsByteSize(long bytes, Type sourceFileSizeType, Type targetFileSizeType)
        {
            // Arrange
            var sourceFileSize = FileSize.CreateFromBytes(bytes, sourceFileSizeType);

            // Act
            var targetFileSize = sourceFileSize.ToSize(targetFileSizeType);
            var convertedSourceFileSize = targetFileSize.ToSize(sourceFileSizeType);

            /// Assert
            Assert.Equal(sourceFileSize.ByteSize, convertedSourceFileSize.ByteSize);
        }

        /// <summary>Verifies ToSize for other file size type: changes size.</summary>
        [Theory]
        [InlineData(125987465632d, typeof(SingleByte), typeof(MebiBit))]
        [InlineData(1.2, typeof(ExbiByte), typeof(KibiByte))]
        [InlineData(369.487, typeof(GigaBit), typeof(Bit))]
        [InlineData(8.5, typeof(PetaBit), typeof(TeraByte))]
        [InlineData(8000.6, typeof(KiloByte), typeof(GibiByte))]
        public void ToSize_OtherFileSizeType_ChangesSize(double size, Type sourceFileSizeType, Type targetFileSizeType)
        {
            // Arrange
            var decimalSize = size.ChangeType<decimal>();
            var sourceFileSize = FileSize.CreateFromSize(decimalSize, sourceFileSizeType);

            // Act
            var targetFileSize = sourceFileSize.ToSize(targetFileSizeType);

            /// Assert
            Assert.NotEqual(sourceFileSize.Size, targetFileSize.Size);
        }

        /// <summary>Verifies ToSize for round trip to source type: keeps size.</summary>
        [Theory]
        [InlineData(125987465632d, typeof(SingleByte), typeof(MebiBit))]
        [InlineData(1.2, typeof(ExbiByte), typeof(KibiByte))]
        [InlineData(369.487, typeof(GigaBit), typeof(Bit))]
        [InlineData(8.5, typeof(PetaBit), typeof(TeraByte))]
        [InlineData(8000.6, typeof(KiloByte), typeof(GibiByte))]
        public void ToSize_RoundTripToSourceType_KeepsSize(double size, Type sourceFileSizeType, Type targetFileSizeType)
        {
            // Arrange
            var decimalSize = size.ChangeType<decimal>();
            var sourceFileSize = FileSize.CreateFromSize(decimalSize, sourceFileSizeType);

            // Act
            var targetFileSize = sourceFileSize.ToSize(targetFileSizeType);
            var convertedSourceFileSize = targetFileSize.ToSize(sourceFileSizeType);

            /// Assert
            Assert.Equal(sourceFileSize.Size, convertedSourceFileSize.Size);
        }

        /// <summary>Verifies EqualOperator for other file size: returns expected comparison.</summary>
        [Theory]
        [InlineData(125987465632L, typeof(SingleByte), 125987465632L, typeof(MebiBit), true)]
        [InlineData(4672831921838425L, typeof(ExbiByte), 4672831921838424L, typeof(KibiByte), false)]
        [InlineData(98563201474122L, typeof(GigaBit), 98563201474122L, typeof(Bit), true)]
        [InlineData(4586312L, typeof(PetaBit), 5000000L, typeof(TeraByte), false)]
        [InlineData(7854120388L, typeof(KiloByte), 1L, typeof(GibiByte), false)]
        public void EqualOperator_OtherFileSize_ReturnsExpectedComparison(long bytes, Type fileSizeType, long otherBytes, Type otherFileSizeType, bool expected)
        {
            // Arrange
            var fileSize = FileSize.CreateFromBytes(bytes, fileSizeType);
            var otherFileSize = FileSize.CreateFromBytes(otherBytes, otherFileSizeType);

            // Act
            var result = fileSize == otherFileSize;

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>Verifies EqualOperator for null operand: returns expected comparison.</summary>
        [Fact]
        public void EqualOperator_NullOperand_ReturnsExpectedComparison()
        {
            // Arrange
            FileSize? size = null;

            // Act
            var result = size == null;

            // Assert
            Assert.True(result);
        }

        /// <summary>Verifies NotEqualOperator for other file size: returns expected comparison.</summary>
        [Theory]
        [InlineData(125987465632L, typeof(SingleByte), 125987465632L, typeof(MebiBit), false)]
        [InlineData(4672831921838425L, typeof(ExbiByte), 4672831921838424L, typeof(KibiByte), true)]
        [InlineData(98563201474122L, typeof(GigaBit), 98563201474122L, typeof(Bit), false)]
        [InlineData(4586312L, typeof(PetaBit), 5000000L, typeof(TeraByte), true)]
        [InlineData(7854120388L, typeof(KiloByte), 1L, typeof(GibiByte), true)]
        public void NotEqualOperator_OtherFileSize_ReturnsExpectedComparison(long bytes, Type fileSizeType, long otherBytes, Type otherFileSizeType, bool expected)
        {
            // Arrange
            var fileSize = FileSize.CreateFromBytes(bytes, fileSizeType);
            var otherFileSize = FileSize.CreateFromBytes(otherBytes, otherFileSizeType);

            // Act
            var result = fileSize != otherFileSize;

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>Verifies NotEqualOperator for null operand: returns expected comparison.</summary>
        [Fact]
        public void NotEqualOperator_NullOperand_ReturnsExpectedComparison()
        {
            // Arrange
            FileSize? size = null;

            // Act
            var result = size != null;

            // Assert
            Assert.False(result);
        }

        /// <summary>Verifies SmallerOperator for other file size: returns expected comparison.</summary>
        [Theory]
        [InlineData(125987465632L, typeof(SingleByte), 125987465632L, typeof(MebiBit), false)]
        [InlineData(4672831921838425L, typeof(ExbiByte), 4672831921838424L, typeof(KibiByte), false)]
        [InlineData(98563201474122L, typeof(GigaBit), 98563201474122L, typeof(Bit), false)]
        [InlineData(4586312L, typeof(PetaBit), 5000000L, typeof(TeraByte), true)]
        [InlineData(7854120388L, typeof(KiloByte), 1L, typeof(GibiByte), false)]
        public void SmallerOperator_OtherFileSize_ReturnsExpectedComparison(long bytes, Type fileSizeType, long otherBytes, Type otherFileSizeType, bool expected)
        {
            // Arrange
            var fileSize = FileSize.CreateFromBytes(bytes, fileSizeType);
            var otherFileSize = FileSize.CreateFromBytes(otherBytes, otherFileSizeType);

            // Act
            var result = fileSize < otherFileSize;

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>Verifies LargerOperator for other file size: returns expected comparison.</summary>
        [Theory]
        [InlineData(125987465632L, typeof(SingleByte), 125987465632L, typeof(MebiBit), false)]
        [InlineData(4672831921838425L, typeof(ExbiByte), 4672831921838424L, typeof(KibiByte), true)]
        [InlineData(98563201474122L, typeof(GigaBit), 98563201474122L, typeof(Bit), false)]
        [InlineData(4586312L, typeof(PetaBit), 5000000L, typeof(TeraByte), false)]
        [InlineData(7854120388L, typeof(KiloByte), 1L, typeof(GibiByte), true)]
        public void LargerOperator_OtherFileSize_ReturnsExpectedComparison(long bytes, Type fileSizeType, long otherBytes, Type otherFileSizeType, bool expected)
        {
            // Arrange
            var fileSize = FileSize.CreateFromBytes(bytes, fileSizeType);
            var otherFileSize = FileSize.CreateFromBytes(otherBytes, otherFileSizeType);

            // Act
            var result = fileSize > otherFileSize;

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>Verifies SmallerOrEqualOperator for other file size: returns expected comparison.</summary>
        [Theory]
        [InlineData(125987465632L, typeof(SingleByte), 125987465632L, typeof(MebiBit), true)]
        [InlineData(4672831921838425L, typeof(ExbiByte), 4672831921838424L, typeof(KibiByte), false)]
        [InlineData(98563201474122L, typeof(GigaBit), 98563201474122L, typeof(Bit), true)]
        [InlineData(4586312L, typeof(PetaBit), 5000000L, typeof(TeraByte), true)]
        [InlineData(7854120388L, typeof(KiloByte), 1L, typeof(GibiByte), false)]
        public void SmallerOrEqualOperator_OtherFileSize_ReturnsExpectedComparison(long bytes, Type fileSizeType, long otherBytes, Type otherFileSizeType, bool expected)
        {
            // Arrange
            var fileSize = FileSize.CreateFromBytes(bytes, fileSizeType);
            var otherFileSize = FileSize.CreateFromBytes(otherBytes, otherFileSizeType);

            // Act
            var result = fileSize <= otherFileSize;

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>Verifies LargerOrEqualOperator for other file size: returns expected comparison.</summary>
        [Theory]
        [InlineData(125987465632L, typeof(SingleByte), 125987465632L, typeof(MebiBit), true)]
        [InlineData(4672831921838425L, typeof(ExbiByte), 4672831921838424L, typeof(KibiByte), true)]
        [InlineData(98563201474122L, typeof(GigaBit), 98563201474122L, typeof(Bit), true)]
        [InlineData(4586312L, typeof(PetaBit), 5000000L, typeof(TeraByte), false)]
        [InlineData(7854120388L, typeof(KiloByte), 1L, typeof(GibiByte), true)]
        public void LargerOrEqualOperator_OtherFileSize_ReturnsExpectedComparison(long bytes, Type fileSizeType, long otherBytes, Type otherFileSizeType, bool expected)
        {
            // Arrange
            var fileSize = FileSize.CreateFromBytes(bytes, fileSizeType);
            var otherFileSize = FileSize.CreateFromBytes(otherBytes, otherFileSizeType);

            // Act
            var result = fileSize >= otherFileSize;

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>Verifies SumOperator for two file sizes: adds byte sizes.</summary>
        [Theory]
        [InlineData(6000L, typeof(SingleByte), 5000L, typeof(MebiBit))]
        [InlineData(10000L, typeof(ExbiByte), 52L, typeof(KibiByte))]
        [InlineData(98563201474122L, typeof(GigaBit), 257963L, typeof(Bit))]
        [InlineData(7500000L, typeof(PetaBit), 5000000L, typeof(TeraByte))]
        [InlineData(7854120388L, typeof(KiloByte), 1L, typeof(GibiByte))]
        public void SumOperator_TwoFileSizes_AddsByteSizes(long bytes, Type fileSizeType, long otherBytes, Type otherFileSizeType)
        {
            // Arrange
            var fileSize = FileSize.CreateFromBytes(bytes, fileSizeType);
            var otherFileSize = FileSize.CreateFromBytes(otherBytes, otherFileSizeType);

            // Act
            var newFileSize = fileSize + otherFileSize;

            // Assert
            Assert.Equal(bytes+otherBytes, newFileSize.ByteSize);
        }

        /// <summary>Verifies SubtractOperator for two file sizes: subtracts byte sizes.</summary>
        [Theory]
        [InlineData(6000L, typeof(SingleByte), 5000L, typeof(MebiBit))]
        [InlineData(10000L, typeof(ExbiByte), 52L, typeof(KibiByte))]
        [InlineData(98563201474122L, typeof(GigaBit), 257963L, typeof(Bit))]
        [InlineData(7500000L, typeof(PetaBit), 5000000L, typeof(TeraByte))]
        [InlineData(7854120388L, typeof(KiloByte), 1L, typeof(GibiByte))]
        public void SubtractOperator_TwoFileSizes_SubtractsByteSizes(long bytes, Type fileSizeType, long otherBytes, Type otherFileSizeType)
        {
            // Arrange
            var fileSize = FileSize.CreateFromBytes(bytes, fileSizeType);
            var otherFileSize = FileSize.CreateFromBytes(otherBytes, otherFileSizeType);

            // Act
            var newFileSize = fileSize - otherFileSize;

            // Assert
            Assert.Equal(bytes - otherBytes, newFileSize.ByteSize);
        }

        /// <summary>Verifies SubtractOperator for negative result: returns zero byte size.</summary>
        [Theory]
        [InlineData(typeof(SingleByte), typeof(MebiBit))]
        [InlineData(typeof(ExbiByte), typeof(KibiByte))]
        [InlineData(typeof(GigaBit), typeof(Bit))]
        [InlineData(typeof(PetaBit), typeof(TeraByte))]
        [InlineData(typeof(KiloByte), typeof(GibiByte))]
        public void SubtractOperator_NegativeResult_ReturnsZeroByteSize(Type fileSizeType, Type otherFileSizeType)
        {
            // Arrange
            var fileSize = FileSize.CreateFromBytes(5000, fileSizeType);
            var otherFileSize = FileSize.CreateFromBytes(10000, otherFileSizeType);

            // Act
            var newFileSize = fileSize - otherFileSize;

            // Assert
            Assert.Equal(0, newFileSize.ByteSize);
        }

        /// <summary>Verifies ImplicitOperator for byte count: sets byte size.</summary>
        [Theory]
        [InlineData(51234587L)]
        [InlineData(8946135468486L)]
        [InlineData(10000000L)]
        [InlineData(8L)]
        [InlineData(0L)]
        public void ImplicitOperator_ByteCount_SetsByteSize(long bytes)
        {
            // Arrange

            // Act
            FileSize fileSize = bytes;

            // Assert
            Assert.Equal(bytes, fileSize.ByteSize);
        }
    }
}