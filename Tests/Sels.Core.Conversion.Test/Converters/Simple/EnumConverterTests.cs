using Sels.Core.Conversion.Contracts;
using Sels.Core.Conversion.Converters.Simple;
using Sels.Core.Conversion.Test.TestTemplates.Converter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Conversion.Test.Converters.Simple
{
    public class EnumConverterTests : ITypeConverterTests
    {
        [TestCase(1, typeof(StringComparison), true)]
        [TestCase("OrdinalIgnoreCase", typeof(StringComparison), true)]
        [TestCase(StringComparison.Ordinal, typeof(int), true)]
        [TestCase(StringComparison.OrdinalIgnoreCase, typeof(string), true)]
        [TestCase(5, typeof(string), false)]
        [TestCase(5.6, typeof(StringComparison), false)]
        [TestCase(StringComparison.InvariantCultureIgnoreCase, typeof(StringComparison), false)]
        [TestCase(StringComparison.CurrentCulture, typeof(bool), false)]
        public void EnumConverter_CanConvert_OnlyReturnsTrueWhenConvertingBetweenIntOrStringAndAnEnum<T>(T source, Type targetType, bool expected)
        {
            // Arrange
            var converter = new EnumConverter();

            // Act
            var result = converter.CanConvert(source, targetType);

            // Assert
            Assert.AreEqual(expected, result);
        }
        [TestCase(0)]
        [TestCase(3)]
        [TestCase("Ordinal")]
        [TestCase("CurrentCultureIgnoreCase")]
        public void EnumConverter_CanConvert_ReturnsTrueWhenConvertingFromIntOrStringToANullableEnum<T>(T source)
        {
            // Arrange
            var converter = new EnumConverter();

            // Act
            var result = converter.CanConvert(source, typeof(StringComparison?));

            // Assert
            Assert.IsTrue(result);
        }
        [Test]
        public void DateTimeConverter_ConvertTo_ConvertsStringToDateTime()
        {
            // Arrange
            var date = DateTime.Parse("04/01/1998 00:13:45");
            var converter = new DateTimeConverter();

            // Act
            var result = converter.ConvertTo(date.ToString(), typeof(DateTime));

            // Assert
            Assert.AreEqual(date, result);
        }
        [TestCase(0, StringComparison.CurrentCulture)]
        [TestCase(5, StringComparison.OrdinalIgnoreCase)]
        [TestCase(1, StringComparison.CurrentCultureIgnoreCase)]
        [TestCase("Ordinal", StringComparison.Ordinal)]
        [TestCase("CurrentCultureIgnoreCase", StringComparison.CurrentCultureIgnoreCase)]
        [TestCase("InvariantCulture", StringComparison.InvariantCulture)]
        public void EnumConverter_ConvertTo_ConvertsIntAndStringToCorrectEnum<T>(T source, StringComparison expected)
        {
            // Arrange
            var converter = new EnumConverter();

            // Act
            var result = converter.ConvertTo(source, typeof(StringComparison)).Cast<StringComparison>();

            // Assert
            Assert.AreEqual(expected, result);
        }
        [TestCase(StringComparison.CurrentCulture)]
        [TestCase(StringComparison.OrdinalIgnoreCase)]
        [TestCase(StringComparison.CurrentCultureIgnoreCase)]
        [TestCase(StringComparison.Ordinal)]
        [TestCase(StringComparison.CurrentCultureIgnoreCase)]
        [TestCase(StringComparison.InvariantCulture)]
        public void EnumConverter_ConvertTo_ConvertsEnumToCorrectIntValue(StringComparison source)
        {
            // Arrange
            var converter = new EnumConverter();

            // Act
            var result = converter.ConvertTo(source, typeof(int)).Cast<int>();

            // Assert
            Assert.AreEqual(source.Cast<int>(), result);
        }

        [TestCase(StringComparison.CurrentCulture)]
        [TestCase(StringComparison.OrdinalIgnoreCase)]
        [TestCase(StringComparison.CurrentCultureIgnoreCase)]
        [TestCase(StringComparison.Ordinal)]
        [TestCase(StringComparison.CurrentCultureIgnoreCase)]
        [TestCase(StringComparison.InvariantCulture)]
        public void EnumConverter_ConvertTo_ConvertsEnumToCorrectStringValue(StringComparison source)
        {
            // Arrange
            var converter = new EnumConverter();

            // Act
            var result = converter.ConvertTo(source, typeof(string)).Cast<string>();

            // Assert
            Assert.AreEqual(source.ToString(), result);
        }

        protected override ITypeConverter GetTestInstance()
        {
            return new EnumConverter();
        }
    }
}
