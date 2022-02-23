using Moq;
using Sels.Core.Conversion.Contracts;
using Sels.Core.Conversion.Converters;
using Sels.Core.Conversion.Test.TestTemplates.Converter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Conversion.Test.Converters
{
    public class GenericConverterTests : ITypeConverterTests
    {
        [TestCase(4, typeof(string))]
        [TestCase("Hello", typeof(char[]))]
        [TestCase(StringComparison.OrdinalIgnoreCase, typeof(char[]))]
        [TestCase(5.6, typeof(int))]
        [TestCase(new string[] { "Hello again!" }, typeof(string))]
        public void GenericConverter_CanConvert_UsesSubConverterToDetermineIfTypeCanBeConverted<T>(T source, Type targetType)
        {
            // Arrange
            var mockedConverter = new Mock<ITypeConverter>();
            var converter = new GenericConverter().AddConverter(mockedConverter.Object);

            // Act
            converter.CanConvert(source, targetType);

            // Assert
            mockedConverter.Verify(x => x.CanConvert(source, targetType, null));
        }

        [TestCase(4, typeof(string))]
        [TestCase("Hello", typeof(char[]))]
        [TestCase(StringComparison.OrdinalIgnoreCase, typeof(char[]))]
        [TestCase(5.6, typeof(int))]
        [TestCase(new string[] { "Hello again!" }, typeof(string))]
        public void GenericConverter_ConvertTo_UsesSubConverterToConvert<T>(T source, Type targetType)
        {
            // Arrange
            var mockedConverter = new Mock<ITypeConverter>();
            mockedConverter.Setup(x => x.CanConvert(source, targetType, null)).Returns(true);
            var converter = new GenericConverter().AddConverter(mockedConverter.Object);

            // Act
            converter.ConvertTo(source, targetType);

            // Assert
            mockedConverter.Verify(x => x.ConvertTo(source, targetType, null));
        }

        [Test]
        public void GenericConverter_ConvertTo_DoesNotThrowExceptionWhenIgnoreUnconvertableIsEnabled()
        {
            // Arrange
            var mockedConverter = new Mock<ITypeConverter>();
            mockedConverter.Setup(x => x.CanConvert(It.IsAny<object>(), It.IsAny<Type>(), null)).Returns(true);
            mockedConverter.Setup(x => x.ConvertTo(It.IsAny<object>(), It.IsAny<Type>(), null)).Throws<Exception>();
            var converter = new GenericConverter(GenericConverterSettings.IgnoreUnconvertable).AddConverter(mockedConverter.Object);
            bool result = false;

            // Act
            try
            {
                converter.ConvertTo(new { }, typeof(string));
            }
            catch (Exception)
            {
                result = true;
            }

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void GenericConverter_ConvertTo_DoesNotCallSubConverterWhenConversionToSameTypeIsRequestedAndReturnsSameValue()
        {
            // Arrange
            var mockedConverter = new Mock<ITypeConverter>();
            var converter = new GenericConverter().AddConverter(mockedConverter.Object);
            var source = string.Empty;

            // Act
            var result = converter.ConvertTo(source, typeof(string));

            // Assert
            mockedConverter.Verify(x => x.CanConvert(It.IsAny<object>(), It.IsAny<Type>(), null), Times.Never);
            mockedConverter.Verify(x => x.ConvertTo(It.IsAny<object>(), It.IsAny<Type>(), null), Times.Never);
            Assert.AreEqual(source, result);
        }

        [Test]
        public void GenericConverter_ConvertTo_CallsSubConverterWhenAlwaysAttemptConversionIsEnabledAndConversionToSameTypeIsRequested()
        {
            // Arrange
            var mockedConverter = new Mock<ITypeConverter>();
            mockedConverter.Setup(x => x.CanConvert(It.IsAny<object>(), It.IsAny<Type>(), null)).Returns(true);
            var converter = new GenericConverter(GenericConverterSettings.AlwaysAttemptConversion).AddConverter(mockedConverter.Object);

            // Act
            converter.ConvertTo(string.Empty, typeof(string));

            // Assert
            mockedConverter.Verify(x => x.ConvertTo(It.IsAny<object>(), It.IsAny<Type>(), null));
        }        

        protected override ITypeConverter GetTestInstance()
        {
            return new GenericConverter();
        }
    }
}
