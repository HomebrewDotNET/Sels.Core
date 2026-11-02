using Sels.Core.Conversion.Converters;
using Sels.Core.Conversion.Converters.Simple;
using Sels.Core.Conversion.Tests.Contracts;

namespace Sels.Core.Conversion.Tests.Converters.Simple
{
    /// <summary>Contract tests for <see cref="IndexConverter"/>.</summary>
    [Trait("Category", "Unit")]
    public class IndexConverterTests : ITypeConverterTests
    {
        /// <summary>Verifies CanConvert for source and target types: returns true only between string and index.</summary>
        [Theory]
        [InlineData(typeof(string), typeof(Index), true)]
        [InlineData(typeof(Index), typeof(string), true)]
        [InlineData(typeof(string), typeof(Index?), true)]
        [InlineData(typeof(int), typeof(Index), false)]
        [InlineData(typeof(Index), typeof(int), false)]
        [InlineData(typeof(Index), typeof(TimeSpan), false)]
        [InlineData(typeof(string), typeof(int), false)]
        [InlineData(typeof(int), typeof(string), false)]
        public void CanConvert_SourceAndTargetTypes_ReturnsTrueOnlyBetweenStringAndIndex(Type sourceType, Type targetType, bool expected)
        {
            // Arrange
            var instance = sourceType == typeof(string) ? "2" : Activator.CreateInstance(sourceType)!;
            var converter = new IndexConverter();

            // Act
            var result = converter.CanConvert(instance, targetType);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>Verifies ConvertTo for an index string: returns the matching index, including the from-end notation.</summary>
        [Theory]
        [InlineData("2", 2, false)]
        [InlineData("0", 0, false)]
        [InlineData("^2", 2, true)]
        [InlineData("^0", 0, true)]
        public void ConvertTo_IndexString_ReturnsIndex(string source, int expectedValue, bool expectedFromEnd)
        {
            // Arrange
            var converter = new IndexConverter();

            // Act
            var result = converter.ConvertTo(source, typeof(Index));

            // Assert
            Assert.Equal(new Index(expectedValue, expectedFromEnd), result);
        }

        /// <summary>Verifies ConvertTo for an index: returns the matching index string, including the from-end notation.</summary>
        [Theory]
        [InlineData(2, false, "2")]
        [InlineData(0, false, "0")]
        [InlineData(2, true, "^2")]
        [InlineData(0, true, "^0")]
        public void ConvertTo_Index_ReturnsIndexString(int value, bool fromEnd, string expected)
        {
            // Arrange
            var converter = new IndexConverter();
            var index = new Index(value, fromEnd);

            // Act
            var result = converter.ConvertTo(index, typeof(string));

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>Verifies ConvertTo for a nullable index target: returns the index, since CanConvert reports the pair as convertible.</summary>
        [Fact]
        public void ConvertTo_StringToNullableIndex_ReturnsIndex()
        {
            // Arrange
            var converter = new IndexConverter();

            // Act
            var result = converter.ConvertTo("2", typeof(Index?));

            // Assert
            Assert.Equal(new Index(2), result);
        }

        /// <summary>Verifies the default converter registers the index converter for the string/index pair.</summary>
        [Fact]
        public void DefaultConverter_WithIndexTypes_ContainsIndexConverter()
        {
            // Arrange
            var converter = GenericConverter.DefaultConverter;

            // Act
            var canConvert = converter.CanConvert("2", typeof(Index));

            // Assert
            Assert.Contains(converter.Converters, x => x is IndexConverter);
            Assert.True(canConvert);
        }

        /// <inheritdoc/>
        protected override ITypeConverter GetTestInstance()
        {
            return new IndexConverter();
        }
    }
}
