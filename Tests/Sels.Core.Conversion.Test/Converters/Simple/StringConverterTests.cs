using Sels.Core.Conversion.Contracts;
using Sels.Core.Conversion.Converters.Simple;
using Sels.Core.Conversion.Test.TestTemplates.Converter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Conversion.Test.Converters.Simple
{
    public class StringConverterTests : ITypeConverterTests
    {
        [TestCase("56", typeof(int))]
        [TestCase(1998, typeof(string))]
        [TestCase("4", typeof(double))]
        [TestCase(6.5, typeof(decimal))]
        [TestCase(6.5f, typeof(string))]
        [TestCase("8", typeof(long))]
        [TestCase(new bool[] { true, false }, typeof(long))]
        [TestCase(new string[] { "Hi!" }, typeof(string))]
        [TestCase(new byte[] { 0, 0, 0, 0, 0, 0, 0, 1 }, typeof(int))]
        public void StringConverter_CanConvert_OnlyReturnsTrueWhenConvertTypeIsString<T>(T source, Type targetType)
        {
            // Arrange
            var converter = new StringConverter();

            // Act
            var result = converter.CanConvert(source, targetType);

            // Assert
            Assert.AreEqual(targetType.Is<string>(), result);
        }
        [TestCase("56")]
        [TestCase(1998)]
        [TestCase("4")]
        [TestCase(6.5)]
        [TestCase(6.5f)]
        [TestCase("8")]
        [TestCase(new bool[] { true, false })]
        [TestCase(new object[] { "Hi!" })]
        [TestCase(new byte[] { 0, 0, 0, 0, 0, 0, 0, 1 })]
        public void StringConverter_ConvertTo_ConvertsObjectsUsingToString<T>(T source)
        {
            // Arrange
            var converter = new StringConverter();

            // Act
            var result = converter.ConvertTo(source, typeof(string));

            // Assert
            Assert.AreEqual(source.ToString(), result);
        }

        protected override ITypeConverter GetTestInstance()
        {
            return new StringConverter();
        }
    }
}
