using Sels.Core.Conversion.Converters;
using Sels.Core.Conversion.Converters.Simple;
using Sels.Core.Conversion.Tests.Contracts;

namespace Sels.Core.Conversion.Tests.Converters.Simple
{
    /// <summary>Contract tests for <see cref="EnumConverter"/>.</summary>
    [Trait("Category", "Unit")]
    public class EnumConverterTests : ITypeConverterTests
    {
        /// <summary>Verifies CanConvert for source and target types: returns true only between int or string and enum.</summary>
        [Theory]
        [InlineData(1, typeof(StringComparison), true)]
        [InlineData("OrdinalIgnoreCase", typeof(StringComparison), true)]
        [InlineData(StringComparison.Ordinal, typeof(int), true)]
        [InlineData(StringComparison.OrdinalIgnoreCase, typeof(string), true)]
        [InlineData(5, typeof(string), false)]
        [InlineData(5.6, typeof(StringComparison), false)]
        [InlineData(StringComparison.InvariantCultureIgnoreCase, typeof(StringComparison), false)]
        [InlineData(StringComparison.CurrentCulture, typeof(bool), false)]
        public void CanConvert_SourceAndTargetTypes_ReturnsTrueOnlyBetweenIntOrStringAndEnum<T>(T source, Type targetType, bool expected)
        {
            // Arrange
            var converter = new EnumConverter();

            // Act
            var result = converter.CanConvert(source, targetType);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>Verifies CanConvert for int or string to nullable enum: returns true.</summary>
        [Theory]
        [InlineData(0)]
        [InlineData(3)]
        [InlineData("Ordinal")]
        [InlineData("CurrentCultureIgnoreCase")]
        public void CanConvert_IntOrStringToNullableEnum_ReturnsTrue<T>(T source)
        {
            // Arrange
            var converter = new EnumConverter();

            // Act
            var result = converter.CanConvert(source, typeof(StringComparison?));

            // Assert
            Assert.True(result);
        }

        /// <summary>Verifies DateTimeConverterConvertTo for date time string: returns date time.</summary>
        [Fact]
        public void DateTimeConverterConvertTo_DateTimeString_ReturnsDateTime()
        {
            // Arrange
            var date = DateTime.Parse("04/01/1998 00:13:45");
            var converter = new DateTimeConverter();

            // Act
            var result = converter.ConvertTo(date.ToString(), typeof(DateTime));

            // Assert
            Assert.Equal(date, result);
        }

        /// <summary>Verifies ConvertTo for int or string: returns matching enum.</summary>
        [Theory]
        [InlineData(0, StringComparison.CurrentCulture)]
        [InlineData(5, StringComparison.OrdinalIgnoreCase)]
        [InlineData(1, StringComparison.CurrentCultureIgnoreCase)]
        [InlineData("Ordinal", StringComparison.Ordinal)]
        [InlineData("CurrentCultureIgnoreCase", StringComparison.CurrentCultureIgnoreCase)]
        [InlineData("InvariantCulture", StringComparison.InvariantCulture)]
        public void ConvertTo_IntOrString_ReturnsMatchingEnum<T>(T source, StringComparison expected)
        {
            // Arrange
            var converter = new EnumConverter();

            // Act
            var result = converter.ConvertTo(source, typeof(StringComparison)).CastTo<StringComparison>();

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>Verifies ConvertTo for enum to int: returns underlying value.</summary>
        [Theory]
        [InlineData(StringComparison.CurrentCulture)]
        [InlineData(StringComparison.OrdinalIgnoreCase)]
        [InlineData(StringComparison.CurrentCultureIgnoreCase)]
        [InlineData(StringComparison.Ordinal)]
        [InlineData(StringComparison.CurrentCultureIgnoreCase)]
        [InlineData(StringComparison.InvariantCulture)]
        public void ConvertTo_EnumToInt_ReturnsUnderlyingValue(StringComparison source)
        {
            // Arrange
            var converter = new EnumConverter();

            // Act
            var result = converter.ConvertTo(source, typeof(int)).CastTo<int>();

            // Assert
            Assert.Equal(source.CastTo<int>(), result);
        }

        /// <summary>Verifies ConvertTo for enum to string: returns enum name.</summary>
        [Theory]
        [InlineData(StringComparison.CurrentCulture)]
        [InlineData(StringComparison.OrdinalIgnoreCase)]
        [InlineData(StringComparison.CurrentCultureIgnoreCase)]
        [InlineData(StringComparison.Ordinal)]
        [InlineData(StringComparison.CurrentCultureIgnoreCase)]
        [InlineData(StringComparison.InvariantCulture)]
        public void ConvertTo_EnumToString_ReturnsEnumName(StringComparison source)
        {
            // Arrange
            var converter = new EnumConverter();

            // Act
            var result = converter.ConvertTo(source, typeof(string)).CastTo<string>();

            // Assert
            Assert.Equal(source.ToString(), result);
        }

        /// <inheritdoc/>
        protected override ITypeConverter GetTestInstance()
        {
            return new EnumConverter();
        }
    }
}
