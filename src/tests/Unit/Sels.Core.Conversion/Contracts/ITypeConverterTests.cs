using Sels.Core.Conversion.Converters;

namespace Sels.Core.Conversion.Tests.Contracts
{
    /// <summary>
    /// Template with common tests for <see cref="ITypeConverter"/>.
    /// </summary>
    [Trait("Category", "Unit")]
    public abstract class ITypeConverterTests
    {
        /// <summary>Verifies CanConvert for null value: returns false.</summary>
        [Fact]
        public void CanConvert_NullValue_ReturnsFalse()
        {
            // Arrange
            var converter = GetTestInstance();
            bool result = false;

            // Act
            result = converter.CanConvert(null, typeof(string));

            // Assert
            Assert.False(result);
        }

        /// <summary>Verifies CanConvert for null convert type: throws argument null exception.</summary>
        [Fact]
        public void CanConvert_NullConvertType_ThrowsArgumentNullException()
        {
            // Arrange
            var converter = GetTestInstance();
            bool result = false;

            // Act
            try
            {
                converter.CanConvert(new object(), null);
            }
            catch (ArgumentNullException)
            {
                result = true;
            }

            // Assert
            Assert.True(result);
        }

        /// <summary>Verifies ConvertTo for null value: throws argument exception.</summary>
        [Fact]
        public void ConvertTo_NullValue_ThrowsArgumentException()
        {
            // Arrange
            var converter = GetTestInstance();
            bool result = false;

            // Act
            try
            {
                converter.ConvertTo(null, typeof(string));
            }
            catch (ArgumentNullException)
            {
                result = true;
            }
            catch (ArgumentException)
            {
                result = true;
            }

            // Assert
            Assert.True(result);
        }

        /// <summary>Verifies ConvertTo for null convert type: throws argument exception.</summary>
        [Fact]
        public void ConvertTo_NullConvertType_ThrowsArgumentException()
        {
            // Arrange
            var converter = GetTestInstance();
            bool result = false;

            // Act
            try
            {
                converter.ConvertTo(new object(), null);
            }
            catch (ArgumentNullException)
            {
                result = true;
            }
            catch (ArgumentException)
            {
                result = true;
            }

            // Assert
            Assert.True(result);
        }

        /// <summary>Creates the converter instance the template tests run against.</summary>
        protected abstract ITypeConverter GetTestInstance();
    }
}
