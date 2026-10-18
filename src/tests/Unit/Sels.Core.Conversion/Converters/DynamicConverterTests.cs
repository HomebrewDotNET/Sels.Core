using Sels.Core.Conversion.Converters;

namespace Sels.Core.Conversion.Tests.Converters
{
    /// <summary>Contract tests for <see cref="DynamicConverter"/>.</summary>
    [Trait("Category", "Unit")]
    public class DynamicConverterTests
    {
        /// <summary>Verifies CanConvert for delegate configured: uses delegate.</summary>
        [Fact]
        public void CanConvert_DelegateConfigured_UsesDelegate()
        {
            // Arrange
            bool? result = null;
            var converter = new DynamicConverter((value, type, arguments) => null, (value, type, arguments) =>
            {
                result = true;
                return true;
            });

            // Act
            converter.CanConvert(string.Empty, typeof(string));

            // Assert
            Assert.NotNull(result);
            Assert.True(result);
        }

        /// <summary>Verifies ConvertTo for delegate configured: uses delegate.</summary>
        [Fact]
        public void ConvertTo_DelegateConfigured_UsesDelegate()
        {
            // Arrange
            bool? result = null;
            var converter = new DynamicConverter((value, type, arguments) =>
            {
                result = true;
                return string.Empty;
            }, (value, type, arguments) => true);

            // Act
            converter.ConvertTo(string.Empty, typeof(string));

            // Assert
            Assert.NotNull(result);
            Assert.True(result);
        }
    }
}
