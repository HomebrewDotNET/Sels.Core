using Sels.Core.Conversion.Converters;
using Sels.Core.Conversion.Converters.Simple;
using Sels.Core.Conversion.Tests.Contracts;
using System.Dynamic;

namespace Sels.Core.Conversion.Tests.Converters.Simple
{
    /// <summary>Contract tests for <see cref="JsonConverter"/>.</summary>
    [Trait("Category", "Unit")]
    public class JsonConverterTests : ITypeConverterTests
    {
        /// <summary>Verifies CanConvert for from string to class: returns true only for json string.</summary>
        [Theory]
        [InlineData("{ \"Name\": \"Jens\" }", true)]
        [InlineData("[ 1, 2, 3, 4 ]", true)]
        [InlineData("", false)]
        [InlineData(9, false)]
        [InlineData(StringComparison.OrdinalIgnoreCase, false)]
        [InlineData(true, false)]
        public void CanConvert_FromStringToClass_ReturnsTrueOnlyForJsonString<T>(T source, bool expected)
        {
            // Arrange
            var converter = new JsonConverter();

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
            var converter = new JsonConverter();
            var source = new int[] { 1, 2, 3, 4, 5 };

            // Act
            var result = converter.CanConvert(source, typeof(string));

            // Assert
            Assert.True(result);
        }

        /// <summary>Verifies ConvertTo for object to json and back: returns equivalent object.</summary>
        [Fact]
        public void ConvertTo_ObjectToJsonAndBack_ReturnsEquivalentObject()
        {
            // Arrange
            var converter = new JsonConverter();
            var source = new int[] { 1, 2, 3, 4, 5 };

            // Act
            var json = converter.ConvertTo(source, typeof(string)).CastTo<string>();
            var result = converter.ConvertTo(json, typeof(int[])).CastTo<int[]>();

            // Assert
            Assert.NotNull(result);
            Assert.True(json.StartsWith('[') && json.EndsWith(']'));
            Assert.Equal(source, result);
        }

        /// <inheritdoc/>
        protected override ITypeConverter GetTestInstance()
        {
            return new JsonConverter();
        }
    }
}
