using Sels.Core.Conversion;
using Sels.Core.Conversion.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Conversion.Test.TestTemplates.Converter
{
    /// <summary>
    /// Template with common tests for <see cref="ITypeConverter"/>.
    /// </summary>
    public abstract class ITypeConverterTests
    {
        [Test]
        public void ITypeConverter_CanConvert_ReturnsFalseWhenValueIsNull()
        {
            // Arrange
            var converter = GetTestInstance();
            bool result = false;

            // Act
            result = converter.CanConvert(null, typeof(string));

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void ITypeConverter_CanConvert_ThrowsArgumentExceptionWhenConvertTypeIsNull()
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
            Assert.IsTrue(result);
        }

        [Test]
        public void ITypeConverter_ConvertTo_ThrowsArgumentExceptionWhenValueIsNull()
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
            Assert.IsTrue(result);
        }

        [Test]
        public void ITypeConverter_ConvertTo_ThrowsArgumentExceptionWhenConvertTypeIsNull()
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
            Assert.IsTrue(result);
        }

        // Abstractions
        protected abstract ITypeConverter GetTestInstance();
    }
}
