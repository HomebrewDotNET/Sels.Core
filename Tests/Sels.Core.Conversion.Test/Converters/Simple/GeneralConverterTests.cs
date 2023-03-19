using Sels.Core.Conversion.Converters;
using Sels.Core.Conversion.Converters.Simple;
using Sels.Core.Conversion.Test.TestTemplates.Converter;
using System;

namespace Sels.Core.Conversion.Test.Converters.Simple
{
    public class GeneralConverterTests : ITypeConverterTests
    {
        [TestCase("56", typeof(int), true)]
        [TestCase(1998, typeof(string), true)]
        [TestCase("4", typeof(double), true)]
        [TestCase("4", typeof(StringComparison?), true)]
        [TestCase(6.5, typeof(decimal), true)]
        [TestCase(6.5f, typeof(string), true)]
        [TestCase("8", typeof(long), true)]
        [TestCase(new bool[] {true, false }, typeof(long), false)]
        [TestCase(new string[] { "Hi!" }, typeof(string), false)]
        [TestCase(new byte[] { 0,0,0,0,0,0,0,1 }, typeof(int), false)]
        public void GeneralConverter_CanConvert_OnlyReturnsTrueWhenConvertingBetweenIConvertibleTypes<T>(T source, Type targetType, bool expected)
        {
            // Arrange
            var converter = new GeneralConverter();

            // Act
            var result = converter.CanConvert(source, targetType);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestCase("56", 56)]
        [TestCase(1998, "1998")]
        [TestCase("4", 4.0)]
        [TestCase(6.5f, "6.5")]
        [TestCase("8", 8L)]
        public void GeneralConverter_CanConvert_ConvertsToCorrectValue<TSource, TExpected>(TSource source, TExpected expected)
        {
            // Arrange
            var converter = new GeneralConverter();

            // Act
            var result = converter.ConvertTo(source, typeof(TExpected));

            // Assert
            Assert.AreEqual(expected, result);
        }

        protected override ITypeConverter GetTestInstance()
        {
            return new GeneralConverter();
        }
    }
}
