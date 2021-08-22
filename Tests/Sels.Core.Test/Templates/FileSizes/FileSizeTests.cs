using NUnit.Framework;
using Sels.Core.Components.FileSizes.Bit;
using Sels.Core.Components.FileSizes.Bit.Binary;
using Sels.Core.Components.FileSizes.Byte;
using Sels.Core.Components.FileSizes.Byte.Binary;
using Sels.Core.Templates.FileSizes;
using Sels.Core.Extensions.Conversion;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Test.Templates.FileSizes
{
    public class FileSizeTests
    {

        #region ToSize

        [TestCase(8, typeof(Bit), 1)]
        [TestCase(1, typeof(SingleByte), 1)]
        [TestCase(50, typeof(KiloByte), 50000)]
        [TestCase(4.5, typeof(MebiByte), 4718592)]
        [TestCase(0.1, typeof(GibiByte), 107374182)]
        public void FileSize_ToSize_ByteSizeIsCorrectlySetWhenCreatingFromSize(double size, Type fileSizeType, long expected)
        {
            // Arrange
            var decimalSize = size.ConvertTo<decimal>();

            // Act
            var fileSize = FileSize.CreateFromSize(decimalSize, fileSizeType);

            // Assert
            Assert.AreEqual(expected, fileSize.ByteSize);
        }

        [TestCase(125987465632L, typeof(SingleByte), typeof(MebiBit))]
        [TestCase(4672831921838425L, typeof(ExbiByte), typeof(KibiByte))]
        [TestCase(98563201474122L, typeof(GigaBit), typeof(Bit))]
        [TestCase(4586312L, typeof(PetaBit), typeof(TeraByte))]
        [TestCase(7854120388L, typeof(KiloByte), typeof(GibiByte))]
        public void FileSize_ToSize_ByteSizeRemainsTheSameWhenConvertingToOtherFileSize(long bytes, Type sourceFileSizeType, Type targetFileSizeType)
        {
            // Arrange
            var sourceFileSize = FileSize.CreateFromBytes(bytes, sourceFileSizeType);

            // Act
            var targetFileSize = sourceFileSize.ToSize(targetFileSizeType);

            /// Assert
            Assert.AreEqual(sourceFileSize.ByteSize, targetFileSize.ByteSize);
        }

        [TestCase(125987465632L, typeof(SingleByte), typeof(MebiBit))]
        [TestCase(4672831921838425L, typeof(ExbiByte), typeof(KibiByte))]
        [TestCase(98563201474122L, typeof(GigaBit), typeof(Bit))]
        [TestCase(4586312L, typeof(PetaBit), typeof(TeraByte))]
        [TestCase(7854120388L, typeof(KiloByte), typeof(GibiByte))]
        public void FileSize_ToSize_ByteSizeRemainsTheSameWhenConvertingToOtherFileSizeAndBackToSourceFileSize(long bytes, Type sourceFileSizeType, Type targetFileSizeType)
        {
            // Arrange
            var sourceFileSize = FileSize.CreateFromBytes(bytes, sourceFileSizeType);

            // Act
            var targetFileSize = sourceFileSize.ToSize(targetFileSizeType);
            var convertedSourceFileSize = targetFileSize.ToSize(sourceFileSizeType);

            /// Assert
            Assert.AreEqual(sourceFileSize.ByteSize, convertedSourceFileSize.ByteSize);
        }

        [TestCase(125987465632d, typeof(SingleByte), typeof(MebiBit))]
        [TestCase(1.2, typeof(ExbiByte), typeof(KibiByte))]
        [TestCase(369.487, typeof(GigaBit), typeof(Bit))]
        [TestCase(8.5, typeof(PetaBit), typeof(TeraByte))]
        [TestCase(8000.6, typeof(KiloByte), typeof(GibiByte))]
        public void FileSize_ToSize_SizeChangesWhenConvertingToOtherFileSize(double size, Type sourceFileSizeType, Type targetFileSizeType)
        {
            // Arrange
            var decimalSize = size.ConvertTo<decimal>();
            var sourceFileSize = FileSize.CreateFromSize(decimalSize, sourceFileSizeType);

            // Act
            var targetFileSize = sourceFileSize.ToSize(targetFileSizeType);

            /// Assert
            Assert.AreNotEqual(sourceFileSize.Size, targetFileSize.Size);
        }

        [TestCase(125987465632d, typeof(SingleByte), typeof(MebiBit))]
        [TestCase(1.2, typeof(ExbiByte), typeof(KibiByte))]
        [TestCase(369.487, typeof(GigaBit), typeof(Bit))]
        [TestCase(8.5, typeof(PetaBit), typeof(TeraByte))]
        [TestCase(8000.6, typeof(KiloByte), typeof(GibiByte))]
        public void FileSize_ToSize_SizeRemainsTheSameWhenConvertingToOtherFileSizeAndBackToSourceFileSize(double size, Type sourceFileSizeType, Type targetFileSizeType)
        {
            // Arrange
            var decimalSize = size.ConvertTo<decimal>();
            var sourceFileSize = FileSize.CreateFromSize(decimalSize, sourceFileSizeType);

            // Act
            var targetFileSize = sourceFileSize.ToSize(targetFileSizeType);
            var convertedSourceFileSize = targetFileSize.ToSize(sourceFileSizeType);

            /// Assert
            Assert.AreEqual(sourceFileSize.Size, convertedSourceFileSize.Size);
        }
        #endregion

        #region Operations
        [TestCase(125987465632L, typeof(SingleByte), 125987465632L, typeof(MebiBit), true)]
        [TestCase(4672831921838425L, typeof(ExbiByte), 4672831921838424L, typeof(KibiByte), false)]
        [TestCase(98563201474122L, typeof(GigaBit), 98563201474122L, typeof(Bit), true)]
        [TestCase(4586312L, typeof(PetaBit), 5000000L, typeof(TeraByte), false)]
        [TestCase(7854120388L, typeof(KiloByte), 1L, typeof(GibiByte), false)]
        public void FileSize_EqualOperator_CorrectComparisonResultIsReturned(long bytes, Type fileSizeType, long otherBytes, Type otherFileSizeType, bool expected)
        {
            // Arrange
            var fileSize = FileSize.CreateFromBytes(bytes, fileSizeType);
            var otherFileSize = FileSize.CreateFromBytes(otherBytes, otherFileSizeType);

            // Act
            var result = fileSize == otherFileSize;

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestCase(125987465632L, typeof(SingleByte), 125987465632L, typeof(MebiBit), false)]
        [TestCase(4672831921838425L, typeof(ExbiByte), 4672831921838424L, typeof(KibiByte), true)]
        [TestCase(98563201474122L, typeof(GigaBit), 98563201474122L, typeof(Bit), false)]
        [TestCase(4586312L, typeof(PetaBit), 5000000L, typeof(TeraByte), true)]
        [TestCase(7854120388L, typeof(KiloByte), 1L, typeof(GibiByte), true)]
        public void FileSize_NotEqualOperator_CorrectComparisonResultIsReturned(long bytes, Type fileSizeType, long otherBytes, Type otherFileSizeType, bool expected)
        {
            // Arrange
            var fileSize = FileSize.CreateFromBytes(bytes, fileSizeType);
            var otherFileSize = FileSize.CreateFromBytes(otherBytes, otherFileSizeType);

            // Act
            var result = fileSize != otherFileSize;

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestCase(125987465632L, typeof(SingleByte), 125987465632L, typeof(MebiBit), false)]
        [TestCase(4672831921838425L, typeof(ExbiByte), 4672831921838424L, typeof(KibiByte), false)]
        [TestCase(98563201474122L, typeof(GigaBit), 98563201474122L, typeof(Bit), false)]
        [TestCase(4586312L, typeof(PetaBit), 5000000L, typeof(TeraByte), true)]
        [TestCase(7854120388L, typeof(KiloByte), 1L, typeof(GibiByte), false)]
        public void FileSize_SmallerOperator_CorrectComparisonResultIsReturned(long bytes, Type fileSizeType, long otherBytes, Type otherFileSizeType, bool expected)
        {
            // Arrange
            var fileSize = FileSize.CreateFromBytes(bytes, fileSizeType);
            var otherFileSize = FileSize.CreateFromBytes(otherBytes, otherFileSizeType);

            // Act
            var result = fileSize < otherFileSize;

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestCase(125987465632L, typeof(SingleByte), 125987465632L, typeof(MebiBit), false)]
        [TestCase(4672831921838425L, typeof(ExbiByte), 4672831921838424L, typeof(KibiByte), true)]
        [TestCase(98563201474122L, typeof(GigaBit), 98563201474122L, typeof(Bit), false)]
        [TestCase(4586312L, typeof(PetaBit), 5000000L, typeof(TeraByte), false)]
        [TestCase(7854120388L, typeof(KiloByte), 1L, typeof(GibiByte), true)]
        public void FileSize_LargerOperator_CorrectComparisonResultIsReturned(long bytes, Type fileSizeType, long otherBytes, Type otherFileSizeType, bool expected)
        {
            // Arrange
            var fileSize = FileSize.CreateFromBytes(bytes, fileSizeType);
            var otherFileSize = FileSize.CreateFromBytes(otherBytes, otherFileSizeType);

            // Act
            var result = fileSize > otherFileSize;

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestCase(125987465632L, typeof(SingleByte), 125987465632L, typeof(MebiBit), true)]
        [TestCase(4672831921838425L, typeof(ExbiByte), 4672831921838424L, typeof(KibiByte), false)]
        [TestCase(98563201474122L, typeof(GigaBit), 98563201474122L, typeof(Bit), true)]
        [TestCase(4586312L, typeof(PetaBit), 5000000L, typeof(TeraByte), true)]
        [TestCase(7854120388L, typeof(KiloByte), 1L, typeof(GibiByte), false)]
        public void FileSize_SmallerOrEqualOperator_CorrectComparisonResultIsReturned(long bytes, Type fileSizeType, long otherBytes, Type otherFileSizeType, bool expected)
        {
            // Arrange
            var fileSize = FileSize.CreateFromBytes(bytes, fileSizeType);
            var otherFileSize = FileSize.CreateFromBytes(otherBytes, otherFileSizeType);

            // Act
            var result = fileSize <= otherFileSize;

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestCase(125987465632L, typeof(SingleByte), 125987465632L, typeof(MebiBit), true)]
        [TestCase(4672831921838425L, typeof(ExbiByte), 4672831921838424L, typeof(KibiByte), true)]
        [TestCase(98563201474122L, typeof(GigaBit), 98563201474122L, typeof(Bit), true)]
        [TestCase(4586312L, typeof(PetaBit), 5000000L, typeof(TeraByte), false)]
        [TestCase(7854120388L, typeof(KiloByte), 1L, typeof(GibiByte), true)]
        public void FileSize_LargerOrEqualOperator_CorrectComparisonResultIsReturned(long bytes, Type fileSizeType, long otherBytes, Type otherFileSizeType, bool expected)
        {
            // Arrange
            var fileSize = FileSize.CreateFromBytes(bytes, fileSizeType);
            var otherFileSize = FileSize.CreateFromBytes(otherBytes, otherFileSizeType);

            // Act
            var result = fileSize >= otherFileSize;

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestCase(6000L, typeof(SingleByte), 5000L, typeof(MebiBit))]
        [TestCase(10000L, typeof(ExbiByte), 52L, typeof(KibiByte))]
        [TestCase(98563201474122L, typeof(GigaBit), 257963L, typeof(Bit))]
        [TestCase(7500000L, typeof(PetaBit), 5000000L, typeof(TeraByte))]
        [TestCase(7854120388L, typeof(KiloByte), 1L, typeof(GibiByte))]
        public void FileSize_SumOperator_ByteSizeIsCorrectlyAddedUp(long bytes, Type fileSizeType, long otherBytes, Type otherFileSizeType)
        {
            // Arrange
            var fileSize = FileSize.CreateFromBytes(bytes, fileSizeType);
            var otherFileSize = FileSize.CreateFromBytes(otherBytes, otherFileSizeType);

            // Act
            var newFileSize = fileSize + otherFileSize;

            // Assert
            Assert.AreEqual(bytes+otherBytes, newFileSize.ByteSize);
        }

        [TestCase(6000L, typeof(SingleByte), 5000L, typeof(MebiBit))]
        [TestCase(10000L, typeof(ExbiByte), 52L, typeof(KibiByte))]
        [TestCase(98563201474122L, typeof(GigaBit), 257963L, typeof(Bit))]
        [TestCase(7500000L, typeof(PetaBit), 5000000L, typeof(TeraByte))]
        [TestCase(7854120388L, typeof(KiloByte), 1L, typeof(GibiByte))]
        public void FileSize_SubtractOperator_ByteSizeIsCorrectlySubtracted(long bytes, Type fileSizeType, long otherBytes, Type otherFileSizeType)
        {
            // Arrange
            var fileSize = FileSize.CreateFromBytes(bytes, fileSizeType);
            var otherFileSize = FileSize.CreateFromBytes(otherBytes, otherFileSizeType);

            // Act
            var newFileSize = fileSize - otherFileSize;

            // Assert
            Assert.AreEqual(bytes - otherBytes, newFileSize.ByteSize);
        }

        [TestCase(typeof(SingleByte), typeof(MebiBit))]
        [TestCase(typeof(ExbiByte), typeof(KibiByte))]
        [TestCase(typeof(GigaBit), typeof(Bit))]
        [TestCase(typeof(PetaBit), typeof(TeraByte))]
        [TestCase(typeof(KiloByte), typeof(GibiByte))]
        public void FileSize_SubtractOperator_NegativeByteSizeDefaultsToZero(Type fileSizeType, Type otherFileSizeType)
        {
            // Arrange
            var fileSize = FileSize.CreateFromBytes(5000, fileSizeType);
            var otherFileSize = FileSize.CreateFromBytes(10000, otherFileSizeType);

            // Act
            var newFileSize = fileSize - otherFileSize;

            // Assert
            Assert.AreEqual(0, newFileSize.ByteSize);
        }

        [TestCase(51234587L)]
        [TestCase(8946135468486L)]
        [TestCase(10000000L)]
        [TestCase(8L)]
        [TestCase(0L)]
        public void FileSize_ImpliticOperator_CorrectByteSizeIsSet(long bytes)
        {
            // Arrange

            // Act
            FileSize fileSize = bytes;

            // Assert
            Assert.AreEqual(bytes, fileSize.ByteSize);
        }
        #endregion
    }
}
