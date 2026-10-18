using Sels.Core.Conversion.Converters;
using Sels.Core.Conversion.Converters.Simple;
using Sels.Core.Conversion.Tests.Contracts;

namespace Sels.Core.Conversion.Tests.Converters.Simple
{
    /// <summary>Contract tests for <see cref="FileInfoConverter"/>.</summary>
    [Trait("Category", "Unit")]
    public class FileInfoConverterTests : ITypeConverterTests
    {
        /// <summary>Verifies CanConvert for source and target types: returns true only between string and file info.</summary>
        [Theory]
        [InlineData(typeof(FileInfo), typeof(string), true, new object[] { "SomeFile.pdf" })]
        [InlineData(typeof(string), typeof(FileInfo), true, new object[] { new char[] { 'C', ':', '/' } })]
        [InlineData(typeof(int), typeof(FileInfo), false)]
        [InlineData(typeof(bool), typeof(FileInfo), false)]
        [InlineData(typeof(FileInfo), typeof(int), false, new object[] { "SomeFile.txt" })]
        [InlineData(typeof(FileInfo), typeof(double), false, new object[] { "SomeFile.json" })]
        [InlineData(typeof(int), typeof(string), false)]
        public void CanConvert_SourceAndTargetTypes_ReturnsTrueOnlyBetweenStringAndFileInfo(Type sourceType, Type targetType, bool expected, object[]? arguments = null)
        {
            // Arrange
            var instance = arguments.HasValue() ? sourceType.Construct(arguments) : sourceType.Construct();
            var converter = new FileInfoConverter();

            // Act
            var result = converter.CanConvert(instance, targetType);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>Verifies ConvertTo for file path string: returns file info.</summary>
        [Fact]
        public void ConvertTo_FilePathString_ReturnsFileInfo()
        {
            // Arrange
            const string File = "SomeFile.txt";
            var converter = new FileInfoConverter();

            // Act
            var result = converter.ConvertTo(File, typeof(FileInfo)).CastTo<FileInfo>();

            // Assert
            Assert.Equal(File, result.Name);
        }

        /// <summary>Verifies ConvertTo for file info: returns path string.</summary>
        [Fact]
        public void ConvertTo_FileInfo_ReturnsPathString()
        {
            // Arrange
            const string File = "SomeFile.txt";
            var file = new FileInfo(File);
            var converter = new FileInfoConverter();

            // Act
            var result = converter.ConvertTo(file, typeof(string)).CastTo<string>();

            // Assert
            Assert.Equal(file.FullName, result);
        }

        /// <inheritdoc/>
        protected override ITypeConverter GetTestInstance()
        {
            return new FileInfoConverter();
        }
    }
}
