using Sels.Core.Conversion.Converters;
using Sels.Core.Conversion.Converters.Simple;
using Sels.Core.Conversion.Tests.Contracts;
using Sels.Core.Testing.Models;
using System.Dynamic;

namespace Sels.Core.Conversion.Tests.Converters.Simple
{
    /// <summary>Contract tests for <see cref="XmlConverter"/>.</summary>
    [Trait("Category", "Unit")]
    public class XmlConverterTests : ITypeConverterTests
    {
        /// <summary>Verifies CanConvert for from string to class: returns true only for xml string.</summary>
        [Theory]
        [InlineData("<Root><Person><Name>Jens Sels</Name></Person></Root>", true)]
        [InlineData("", false)]
        [InlineData(9, false)]
        [InlineData(StringComparison.OrdinalIgnoreCase, false)]
        [InlineData(true, false)]
        public void CanConvert_FromStringToClass_ReturnsTrueOnlyForXmlString<T>(T source, bool expected)
        {
            // Arrange
            var converter = new XmlConverter();

            // Act
            var result = converter.CanConvert(source, typeof(ExpandoObject));

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>Verifies CanConvert for from class: returns true only for string target.</summary>
        [Fact]
        public void CanConvert_FromClass_ReturnsTrueOnlyForStringTarget()
        {
            // Arrange
            var converter = new XmlConverter();
            var source = new ExamResult();

            // Act
            var result = converter.CanConvert(source, typeof(string));

            // Assert
            Assert.True(result);
        }

        /// <summary>Verifies ConvertTo for object to xml and back: returns equivalent object.</summary>
        [Fact]
        public void ConvertTo_ObjectToXmlAndBack_ReturnsEquivalentObject()
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
            var xml = converter.ConvertTo(source, typeof(string)).CastTo<string>();
            var result = converter.ConvertTo(xml, typeof(ExamResult)).CastTo<ExamResult>();

            // Assert
            Assert.NotNull(result);
            Assert.True(xml.StartsWith('<') && xml.EndsWith('>'));
            Assert.Equal(source.Name, result.Name);
            Assert.Equal(source.FamilyName, result.FamilyName);
            Assert.Equal(source.ExecutionDate, result.ExecutionDate);
            Assert.Equal(source.Score, result.Score);
            Assert.Equal(source.Course, result.Course);
        }

        /// <inheritdoc/>
        protected override ITypeConverter GetTestInstance()
        {
            return new XmlConverter();
        }
    }
}
