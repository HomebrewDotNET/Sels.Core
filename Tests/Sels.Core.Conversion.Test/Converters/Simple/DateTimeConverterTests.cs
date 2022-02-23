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
    public class DateTimeConverterTests : ITypeConverterTests
    {
        [TestCase(typeof(DateTime), typeof(string), true)]
        [TestCase(typeof(string), typeof(DateTime), true, new object[] { new char[] { 'H', 'e', 'l', 'l', 'o', '!' } })]
        [TestCase(typeof(int), typeof(DateTime), false)]
        [TestCase(typeof(bool), typeof(DateTime), false)]
        [TestCase(typeof(DateTime), typeof(int), false)]
        [TestCase(typeof(DateTime), typeof(double), false)]
        [TestCase(typeof(int), typeof(string), false)]
        [TestCase(typeof(string), typeof(DateTime?), true, new object[] { new char[] { 'H', 'e', 'l', 'l', 'o', '!' } })]
        public void DateTimeConverter_CanConvert_ReturnsTrueOnlyWhenConvertingBetweenStringAndDateTime(Type sourceType, Type targetType, bool expected, object[]? arguments = null)
        {
            // Arrange
            var instance = arguments.HasValue() ? sourceType.Construct(arguments) : sourceType.Construct();
            var converter = new DateTimeConverter();

            // Act
            var result = converter.CanConvert(instance, targetType);

            // Assert
            Assert.AreEqual(expected, result);
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
        [Test]
        public void DateTimeConverter_ConvertTo_ConvertsDateTimeToString()
        {
            // Arrange
            var date = DateTime.Now;
            var converter = new DateTimeConverter();

            // Act
            var result = converter.ConvertTo(date, typeof(string));

            // Assert
            Assert.AreEqual(result, date.ToString());
        }
        [TestCase("04/01/1998 00:13:45", "dd/MM/yyyy HH:mm:ss")]
        [TestCase("1998-01-04T00:13:45.0000000", "o")]
        [TestCase("00:13:45 04/01/1998", "HH:mm:ss dd/MM/yyyy")]
        public void DateTimeConverter_ConvertTo_ConvertsStringToDateTimeUsingCorrectFormat(string dateString, string format)
        {
            // Arrange
            var date = DateTime.ParseExact(dateString, format, null);
            var converter = new DateTimeConverter();
            var arguments = new Dictionary<string, string>();
            arguments.Add(DateTimeConverter.FormatArgument, format);

            // Act
            var result = converter.ConvertTo(date.ToString(format), typeof(DateTime), arguments);

            // Assert
            Assert.AreEqual(date, result);
        }
        [TestCase("04/01/1998 00:13:45", "dd/MM/yyyy HH:mm:ss")]
        [TestCase("1998-01-04T00:13:45.0000000", "o")]
        [TestCase("00:13:45 04/01/1998", "HH:mm:ss dd/MM/yyyy")]
        public void DateTimeConverter_ConvertTo_ConvertsDateTimeToStringUsingCorrectFormat(string dateString, string format)
        {
            // Arrange
            var date = DateTime.ParseExact(dateString, format, null);
            var converter = new DateTimeConverter();
            var arguments = new Dictionary<string, string>();
            arguments.Add(DateTimeConverter.FormatArgument, format);

            // Act
            var result = converter.ConvertTo(date, typeof(string), arguments);

            // Assert
            Assert.AreEqual(dateString, result);
        }

        protected override ITypeConverter GetTestInstance()
        {
            return new DateTimeConverter();
        }
    }
}
