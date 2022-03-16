using Sels.Core.Conversion;
using Sels.Core.Conversion.Converters;
using Sels.Core.Conversion.Converters.Simple;
using Sels.Core.Conversion.Test.TestTemplates.Converter;
using Sels.Core.Testing.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Conversion.Test.Converters.Simple
{
    public class XmlConverterTests : ITypeConverterTests
    {
        [TestCase("<Root><Person><Name>Jens Sels</Name></Person></Root>", true)]
        [TestCase("", false)]
        [TestCase(9, false)]
        [TestCase(StringComparison.OrdinalIgnoreCase, false)]
        [TestCase(true, false)]
        public void XmlConverter_CanConvert_OnlyReturnsTrueWhenConvertingFromXmlStringToClass<T>(T source, bool expected)
        {
            // Arrange
            var converter = new XmlConverter();

            // Act
            var result = converter.CanConvert(source, typeof(ExpandoObject));

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void XmlConverter_CanConvert_OnlyReturnsTrueWhenConvertingFromClassToString()
        {
            // Arrange
            var converter = new XmlConverter();
            var source = new ExamResult();

            // Act
            var result = converter.CanConvert(source, typeof(string));

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void XmlConverter_ConvertTo_ConvertingToXmlAndBackProducesSameResult()
        {
            // Arrange
            var converter = new XmlConverter();
            var source = new ExamResult() { 
                Name = "Jens",
                FamilyName = "Sels",
                ExecutionDate = DateTime.Now,
                Score = 90,
                Course = "C#"
            };

            // Act
            var xml = converter.ConvertTo(source, typeof(string)).Cast<string>();
            var result = converter.ConvertTo(xml, typeof(ExamResult)).Cast<ExamResult>();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(xml.StartsWith('<') && xml.EndsWith('>'));
            Assert.AreEqual(source.Name, result.Name);
            Assert.AreEqual(source.FamilyName, result.FamilyName);
            Assert.AreEqual(source.ExecutionDate, result.ExecutionDate);
            Assert.AreEqual(source.Score, result.Score);
            Assert.AreEqual(source.Course, result.Course);
        }

        protected override ITypeConverter GetTestInstance()
        {
            return new XmlConverter();
        }
    }
}
