using Sels.Core.Conversion.Converters;

namespace Sels.Core.Conversion.Test.Converters
{
    public class DynamicConverterTests
    {
        [Test]
        public void DynamicConverter_CanConvert_DelegateIsUsed()
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
            Assert.IsNotNull(result);
            Assert.IsTrue(result);
        }

        [Test]
        public void DynamicConverter_ConvertTo_DelegateIsUsed()
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
            Assert.IsNotNull(result);
            Assert.IsTrue(result);
        }
    }
}
