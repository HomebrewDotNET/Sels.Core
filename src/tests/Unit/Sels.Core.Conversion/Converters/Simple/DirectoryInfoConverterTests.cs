using Sels.Core.Conversion.Converters;
using Sels.Core.Conversion.Converters.Simple;
using Sels.Core.Conversion.Tests.Contracts;

namespace Sels.Core.Conversion.Tests.Converters.Simple
{
    /// <summary>Contract tests for <see cref="DirectoryInfoConverter"/>.</summary>
    [Trait("Category", "Unit")]
    public class DirectoryInfoConverterTests : ITypeConverterTests
    {
        /// <summary>Verifies CanConvert for source and target types: returns true only between string and directory info.</summary>
        [Theory]
        [InlineData(typeof(DirectoryInfo), typeof(string), true, new object[] { "SomeFolder" })]
        [InlineData(typeof(string), typeof(DirectoryInfo), true, new object[] { new char[] { 'C', ':', '/' } })]
        [InlineData(typeof(int), typeof(DirectoryInfo), false)]
        [InlineData(typeof(bool), typeof(DirectoryInfo), false)]
        [InlineData(typeof(DirectoryInfo), typeof(int), false, new object[] { "SomeFolder" })]
        [InlineData(typeof(DirectoryInfo), typeof(double), false, new object[] { "SomeFolder" })]
        [InlineData(typeof(int), typeof(string), false)]
        public void CanConvert_SourceAndTargetTypes_ReturnsTrueOnlyBetweenStringAndDirectoryInfo(Type sourceType, Type targetType, bool expected, object[]? arguments = null)
        {
            // Arrange
            var instance = arguments.HasValue() ? sourceType.Construct(arguments) : sourceType.Construct();
            var converter = new DirectoryInfoConverter();

            // Act
            var result = converter.CanConvert(instance, targetType);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>Verifies ConvertTo for directory path string: returns directory info.</summary>
        [Fact]
        public void ConvertTo_DirectoryPathString_ReturnsDirectoryInfo()
        {
            // Arrange
            const string Folder = "SomeFolder";
            var converter = new DirectoryInfoConverter();

            // Act
            var result = converter.ConvertTo(Folder, typeof(DirectoryInfo)).CastTo<DirectoryInfo>();

            // Assert
            Assert.Equal(Folder, result.Name);
        }

        /// <summary>Verifies ConvertTo for directory info: returns path string.</summary>
        [Fact]
        public void ConvertTo_DirectoryInfo_ReturnsPathString()
        {
            // Arrange
            const string Folder = "SomeFolder";
            var directory = new DirectoryInfo(Folder);
            var converter = new DirectoryInfoConverter();

            // Act
            var result = converter.ConvertTo(directory, typeof(string)).CastTo<string>();

            // Assert
            Assert.Equal(directory.FullName, result);
        }

        /// <inheritdoc/>
        protected override ITypeConverter GetTestInstance()
        {
            return new DirectoryInfoConverter();
        }
    }
}
