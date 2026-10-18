using Sels.Core.Extensions.Text;

namespace Sels.Core.Legacy.Tests.Extensions.Object
{
    /// <summary>Contract tests for <see cref="StringExtensions"/>.</summary>
    [Trait("Category", "Unit")]
    public class StringExtensionsTests
    {
        /// <summary>Verifies ExtractFromFormat for value matching format: returns parameter value.</summary>
        [Theory]
        [InlineData("{Key}={Value}", "{Key}", "Action=Build", new string[] { "{Value}" }, "Action")]
        [InlineData("{Command}::{Action}", "{Action}", "Config::Deploy", new string[] { "{Command}" }, "Deploy")]
        [InlineData("Hello from {0}!", "{0}", "Hello from Jens Sels!", null, "Jens Sels")]
        [InlineData("[{Index}]", "{Index}", "[]", null, null)]
        public void ExtractFromFormat_ValueMatchingFormat_ReturnsParameterValue(string format, string parameter, string value, string[] otherParameters, string expected)
        {
            // Arrange

            // Act
            var actual = format.ExtractFromFormat(parameter, value, otherParameters);

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}
