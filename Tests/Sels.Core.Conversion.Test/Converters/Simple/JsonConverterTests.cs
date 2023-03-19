using Sels.Core.Conversion.Converters;
using Sels.Core.Conversion.Converters.Simple;
using Sels.Core.Conversion.Test.TestTemplates.Converter;
using System;
using System.Dynamic;
using System.Linq;

namespace Sels.Core.Conversion.Test.Converters.Simple
{
    public class JsonConverterTests : ITypeConverterTests
    {
        [TestCase("{ \"Name\": \"Jens\" }", true)]
        [TestCase("[ 1, 2, 3, 4 ]", true)]
        [TestCase("", false)]
        [TestCase(9, false)]
        [TestCase(StringComparison.OrdinalIgnoreCase, false)]
        [TestCase(true, false)]
        public void JsonConverter_CanConvert_OnlyReturnsTrueWhenConvertingFromJsonStringToClass<T>(T source, bool expected)
        {
            // Arrange
            var converter = new JsonConverter();

            // Act
            var result = converter.CanConvert(source, typeof(ExpandoObject));

            // Assert
            Assert.AreEqual(expected, result);
        }
        [Test]
        public void JsonConverter_CanConvert_OnlyReturnsTrueWhenConvertingFromClassToString()
        {
            // Arrange
            var converter = new JsonConverter();
            var source = new int[] { 1, 2, 3, 4, 5 };

            // Act
            var result = converter.CanConvert(source, typeof(string));

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void JsonConverter_ConvertTo_ConvertingToJsonAndBackProducesSameResult()
        {
            // Arrange
            var converter = new JsonConverter();
            var source = new int[] { 1, 2, 3, 4, 5 };

            // Act
            var json = converter.ConvertTo(source, typeof(string)).Cast<string>();
            var result = converter.ConvertTo(json, typeof(int[])).Cast<int[]>();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(json.StartsWith('[') && json.EndsWith(']'));
            CollectionAssert.AreEqual(source, result);
        }

        protected override ITypeConverter GetTestInstance()
        {
            return new JsonConverter();
        }
    }
}
