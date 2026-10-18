using Sels.Core.Conversion.Converters;
using Sels.Core.Conversion.Converters.Simple;
using Sels.Core.Conversion.Tests.Contracts;

namespace Sels.Core.Conversion.Tests.Converters.Simple
{
    /// <summary>Contract tests for <see cref="GuidConverter"/>.</summary>
    [Trait("Category", "Unit")]
    public class GuidConverterTests : ITypeConverterTests
    {
        /// <summary>Verifies CanConvert for source and target types: returns true only between string and guid.</summary>
        [Theory]
        [InlineData(typeof(Guid), typeof(string), true)]
        [InlineData(typeof(string), typeof(Guid), true, new object[] { new char[] { 'H', 'e', 'l', 'l', 'o', '!' } })]
        [InlineData(typeof(int), typeof(Guid), false)]
        [InlineData(typeof(bool), typeof(Guid), false)]
        [InlineData(typeof(Guid), typeof(int), false)]
        [InlineData(typeof(Guid), typeof(double), false)]
        [InlineData(typeof(int), typeof(string), false)]
        [InlineData(typeof(string), typeof(Guid?), true, new object[] { new char[] { 'H', 'e', 'l', 'l', 'o', '!' } })]
        public void CanConvert_SourceAndTargetTypes_ReturnsTrueOnlyBetweenStringAndGuid(Type sourceType, Type targetType, bool expected, object[]? arguments = null)
        {
            // Arrange
            var instance = arguments.HasValue() ? sourceType.Construct(arguments) : sourceType.Construct();
            var converter = new GuidConverter();

            // Act
            var result = converter.CanConvert(instance, targetType);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>Verifies ConvertTo for guid: returns string.</summary>
        [Fact]
        public void ConvertTo_Guid_ReturnsString()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var converter = new GuidConverter();

            // Act
            var result = converter.ConvertTo(guid, typeof(string));

            // Assert
            Assert.Equal(guid.ToString(), result);
        }

        /// <summary>Verifies ConvertTo for guid string: returns guid.</summary>
        [Fact]
        public void ConvertTo_GuidString_ReturnsGuid()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var converter = new GuidConverter();

            // Act
            var result = converter.ConvertTo(guid.ToString(), typeof(Guid));

            // Assert
            Assert.Equal(guid, result);
        }

        /// <summary>Verifies ConvertTo for guid with format argument: returns formatted string.</summary>
        [Theory]
        [InlineData("N")]
        [InlineData("D")]
        [InlineData("B")]
        [InlineData("P")]
        [InlineData("X")]
        public void ConvertTo_GuidWithFormatArgument_ReturnsFormattedString(string format)
        {
            // Arrange
            var guid = Guid.NewGuid();
            var converter = new GuidConverter();
            var arguments = new Dictionary<string, object>();
            arguments.Add(GuidConverter.FormatArgument, format);

            // Act
            var result = converter.ConvertTo(guid, typeof(string), arguments);

            // Assert
            Assert.Equal(guid.ToString(format), result);
        }

        /// <inheritdoc/>
        protected override ITypeConverter GetTestInstance()
        {
            return new GuidConverter();
        }
    }
}
