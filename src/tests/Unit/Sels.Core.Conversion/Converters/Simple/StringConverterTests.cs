using Sels.Core.Conversion.Converters;
using Sels.Core.Conversion.Converters.Simple;
using Sels.Core.Conversion.Tests.Contracts;

namespace Sels.Core.Conversion.Tests.Converters.Simple
{
    /// <summary>Contract tests for <see cref="StringConverter"/>.</summary>
    [Trait("Category", "Unit")]
    public class StringConverterTests : ITypeConverterTests
    {
        /// <summary>Verifies CanConvert for target type: returns true only for string.</summary>
        [Theory]
        [InlineData("56", typeof(int))]
        [InlineData(1998, typeof(string))]
        [InlineData("4", typeof(double))]
        [InlineData(6.5, typeof(decimal))]
        [InlineData(6.5f, typeof(string))]
        [InlineData("8", typeof(long))]
        [InlineData(new bool[] { true, false }, typeof(long))]
        [InlineData(new string[] { "Hi!" }, typeof(string))]
        [InlineData(new byte[] { 0, 0, 0, 0, 0, 0, 0, 1 }, typeof(int))]
        public void CanConvert_TargetType_ReturnsTrueOnlyForString<T>(T source, Type targetType)
        {
            // Arrange
            var converter = new StringConverter();

            // Act
            var result = converter.CanConvert(source, targetType);

            // Assert
            Assert.Equal(targetType.Is<string>(), result);
        }

        /// <summary>Verifies ConvertTo for object: returns to string value.</summary>
        [Theory]
        [InlineData("56")]
        [InlineData(1998)]
        [InlineData("4")]
        [InlineData(6.5)]
        [InlineData(6.5f)]
        [InlineData("8")]
        [InlineData(new bool[] { true, false })]
        [InlineData(new object[] { "Hi!" })]
        [InlineData(new byte[] { 0, 0, 0, 0, 0, 0, 0, 1 })]
        public void ConvertTo_Object_ReturnsToStringValue<T>(T source)
        {
            // Arrange
            var converter = new StringConverter();

            // Act
            var result = converter.ConvertTo(source, typeof(string));

            // Assert
            Assert.Equal(source.ToString(), result);
        }

        /// <inheritdoc/>
        protected override ITypeConverter GetTestInstance()
        {
            return new StringConverter();
        }
    }
}
