using Sels.Core.Conversion.Converters;
using Sels.Core.Conversion.Converters.Simple;
using Sels.Core.Conversion.Tests.Contracts;

namespace Sels.Core.Conversion.Tests.Converters.Simple
{
    /// <summary>Contract tests for <see cref="GeneralConverter"/>.</summary>
    [Trait("Category", "Unit")]
    public class GeneralConverterTests : ITypeConverterTests
    {
        /// <summary>Verifies CanConvert for source and target types: returns true only between convertible types.</summary>
        [Theory]
        [InlineData("56", typeof(int), true)]
        [InlineData(1998, typeof(string), true)]
        [InlineData("4", typeof(double), true)]
        [InlineData("4", typeof(StringComparison?), true)]
        [InlineData(6.5, typeof(decimal), true)]
        [InlineData(6.5f, typeof(string), true)]
        [InlineData("8", typeof(long), true)]
        [InlineData(new bool[] {true, false }, typeof(long), false)]
        [InlineData(new string[] { "Hi!" }, typeof(string), false)]
        [InlineData(new byte[] { 0,0,0,0,0,0,0,1 }, typeof(int), false)]
        public void CanConvert_SourceAndTargetTypes_ReturnsTrueOnlyBetweenConvertibleTypes<T>(T source, Type targetType, bool expected)
        {
            // Arrange
            var converter = new GeneralConverter();

            // Act
            var result = converter.CanConvert(source, targetType);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>Verifies ConvertTo for convertible source: returns expected value.</summary>
        [Theory]
        [InlineData("56", 56)]
        [InlineData(1998, "1998")]
        [InlineData("4", 4.0)]
        [InlineData(6.5f, "6.5")]
        [InlineData("8", 8L)]
        public void ConvertTo_ConvertibleSource_ReturnsExpectedValue<TSource, TExpected>(TSource source, TExpected expected)
        {
            // Arrange
            var converter = new GeneralConverter();

            // Act
            var result = converter.ConvertTo(source, typeof(TExpected));

            // Assert
            Assert.Equal(expected, result);
        }

        /// <inheritdoc/>
        protected override ITypeConverter GetTestInstance()
        {
            return new GeneralConverter();
        }
    }
}
