using Sels.Core.Conversion.Converters;
using Sels.Core.Conversion.Converters.Simple;
using Sels.Core.Conversion.Tests.Contracts;

namespace Sels.Core.Conversion.Tests.Converters.Simple
{
    /// <summary>Contract tests for <see cref="DateTimeConverter"/>.</summary>
    [Trait("Category", "Unit")]
    public class DateTimeConverterTests : ITypeConverterTests
    {
        /// <summary>Verifies CanConvert for source and target types: returns true only between string and date time.</summary>
        [Theory]
        [InlineData(typeof(DateTime), typeof(string), true)]
        [InlineData(typeof(string), typeof(DateTime), true, new object[] { new char[] { 'H', 'e', 'l', 'l', 'o', '!' } })]
        [InlineData(typeof(int), typeof(DateTime), false)]
        [InlineData(typeof(bool), typeof(DateTime), false)]
        [InlineData(typeof(DateTime), typeof(int), false)]
        [InlineData(typeof(DateTime), typeof(double), false)]
        [InlineData(typeof(int), typeof(string), false)]
        [InlineData(typeof(string), typeof(DateTime?), true, new object[] { new char[] { 'H', 'e', 'l', 'l', 'o', '!' } })]
        public void CanConvert_SourceAndTargetTypes_ReturnsTrueOnlyBetweenStringAndDateTime(Type sourceType, Type targetType, bool expected, object[]? arguments = null)
        {
            // Arrange
            var instance = arguments.HasValue() ? sourceType.Construct(arguments) : sourceType.Construct();
            var converter = new DateTimeConverter();

            // Act
            var result = converter.CanConvert(instance, targetType);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>Verifies ConvertTo for date time string: returns date time.</summary>
        [Fact]
        public void ConvertTo_DateTimeString_ReturnsDateTime()
        {
            // Arrange
            var date = DateTime.Parse("04/01/1998 00:13:45");
            var converter = new DateTimeConverter();

            // Act
            var result = converter.ConvertTo(date.ToString(), typeof(DateTime));

            // Assert
            Assert.Equal(date, result);
        }

        /// <summary>Verifies ConvertTo for date time: returns string.</summary>
        [Fact]
        public void ConvertTo_DateTime_ReturnsString()
        {
            // Arrange
            var date = DateTime.Now;
            var converter = new DateTimeConverter();

            // Act
            var result = converter.ConvertTo(date, typeof(string));

            // Assert
            Assert.Equal(result, date.ToString());
        }

        /// <summary>Verifies ConvertTo for string with format argument: returns parsed date time.</summary>
        [Theory]
        [InlineData("04/01/1998 00:13:45", "dd/MM/yyyy HH:mm:ss")]
        [InlineData("1998-01-04T00:13:45.0000000", "o")]
        [InlineData("00:13:45 04/01/1998", "HH:mm:ss dd/MM/yyyy")]
        public void ConvertTo_StringWithFormatArgument_ReturnsParsedDateTime(string dateString, string format)
        {
            // Arrange
            var date = DateTime.ParseExact(dateString, format, null);
            var converter = new DateTimeConverter();
            var arguments = new Dictionary<string, object>();
            arguments.Add(DateTimeConverter.FormatArgument, format);

            // Act
            var result = converter.ConvertTo(date.ToString(format), typeof(DateTime), arguments);

            // Assert
            Assert.Equal(date, result);
        }

        /// <summary>Verifies ConvertTo for date time with format argument: returns formatted string.</summary>
        [Theory]
        [InlineData("04/01/1998 00:13:45", "dd/MM/yyyy HH:mm:ss")]
        [InlineData("1998-01-04T00:13:45.0000000", "o")]
        [InlineData("00:13:45 04/01/1998", "HH:mm:ss dd/MM/yyyy")]
        public void ConvertTo_DateTimeWithFormatArgument_ReturnsFormattedString(string dateString, string format)
        {
            // Arrange
            var date = DateTime.ParseExact(dateString, format, null);
            var converter = new DateTimeConverter();
            var arguments = new Dictionary<string, object>();
            arguments.Add(DateTimeConverter.FormatArgument, format);

            // Act
            var result = converter.ConvertTo(date, typeof(string), arguments);

            // Assert
            Assert.Equal(dateString, result);
        }

        /// <inheritdoc/>
        protected override ITypeConverter GetTestInstance()
        {
            return new DateTimeConverter();
        }
    }
}
