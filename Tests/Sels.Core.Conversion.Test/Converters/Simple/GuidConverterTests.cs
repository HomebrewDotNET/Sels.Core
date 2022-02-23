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
    public class GuidConverterTests : ITypeConverterTests
    {
        [TestCase(typeof(Guid), typeof(string), true)]
        [TestCase(typeof(string), typeof(Guid), true, new object[] { new char[] { 'H', 'e', 'l', 'l', 'o', '!' } })]
        [TestCase(typeof(int), typeof(Guid), false)]
        [TestCase(typeof(bool), typeof(Guid), false)]
        [TestCase(typeof(Guid), typeof(int), false)]
        [TestCase(typeof(Guid), typeof(double), false)]
        [TestCase(typeof(int), typeof(string), false)]
        [TestCase(typeof(string), typeof(Guid?), true, new object[] { new char[] { 'H', 'e', 'l', 'l', 'o', '!' } })]
        public void GuidConverter_CanConvert_ReturnsTrueOnlyWhenConvertingBetweenStringAndGuid(Type sourceType, Type targetType, bool expected, object[]? arguments = null)
        {
            // Arrange
            var instance = arguments.HasValue() ? sourceType.Construct(arguments) : sourceType.Construct();
            var converter = new GuidConverter();

            // Act
            var result = converter.CanConvert(instance, targetType);

            // Assert
            Assert.AreEqual(expected, result);
        }
        [Test]
        public void GuidConverter_ConvertTo_ConvertsGuidToString()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var converter = new GuidConverter();

            // Act
            var result = converter.ConvertTo(guid, typeof(string));

            // Assert
            Assert.AreEqual(guid.ToString(), result);
        }
        [Test]
        public void GuidConverter_ConvertTo_ConvertsStringToGuid()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var converter = new GuidConverter();

            // Act
            var result = converter.ConvertTo(guid.ToString(), typeof(Guid));

            // Assert
            Assert.AreEqual(guid, result);
        }
        [TestCase("N")]
        [TestCase("D")]
        [TestCase("B")]
        [TestCase("P")]
        [TestCase("X")]
        public void GuidConverter_ConvertTo_ConvertsGuidToStringUsingCorrectFormat(string format)
        {
            // Arrange
            var guid = Guid.NewGuid();
            var converter = new GuidConverter();
            var arguments = new Dictionary<string, string>();
            arguments.Add(GuidConverter.FormatArgument, format);

            // Act
            var result = converter.ConvertTo(guid, typeof(string), arguments);

            // Assert
            Assert.AreEqual(guid.ToString(format), result);
        }
        protected override ITypeConverter GetTestInstance()
        {
            return new GuidConverter();
        }
    }
}
