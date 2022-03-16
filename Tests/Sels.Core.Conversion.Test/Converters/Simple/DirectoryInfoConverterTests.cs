using Sels.Core.Conversion;
using Sels.Core.Conversion.Converters;
using Sels.Core.Conversion.Converters.Simple;
using Sels.Core.Conversion.Test.TestTemplates.Converter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Conversion.Test.Converters.Simple
{
    public class DirectoryInfoConverterTests : ITypeConverterTests
    {
        [TestCase(typeof(DirectoryInfo), typeof(string), true, new object[] { "SomeFolder" })]
        [TestCase(typeof(string), typeof(DirectoryInfo), true, new object[] { new char[] { 'C', ':', '/' } })]
        [TestCase(typeof(int), typeof(DirectoryInfo), false)]
        [TestCase(typeof(bool), typeof(DirectoryInfo), false)]
        [TestCase(typeof(DirectoryInfo), typeof(int), false, new object[] { "SomeFolder" })]
        [TestCase(typeof(DirectoryInfo), typeof(double), false, new object[] { "SomeFolder" })]
        [TestCase(typeof(int), typeof(string), false)]
        public void DirectoryInfoConverter_CanConvert_ReturnsTrueOnlyWhenConvertingBetweenStringAndDirectoryInfo(Type sourceType, Type targetType, bool expected, object[]? arguments = null)
        {
            // Arrange
            var instance = arguments.HasValue() ? sourceType.Construct(arguments) : sourceType.Construct();
            var converter = new DirectoryInfoConverter();

            // Act
            var result = converter.CanConvert(instance, targetType);

            // Assert
            Assert.AreEqual(expected, result);
        }
        [Test]
        public void DirectoryInfoConverter_ConvertTo_ConvertsStringToDirectoryInfo()
        {
            // Arrange
            const string Folder = "SomeFolder";
            var converter = new DirectoryInfoConverter();

            // Act
            var result = converter.ConvertTo(Folder, typeof(DirectoryInfo)).Cast<DirectoryInfo>();

            // Assert
            Assert.AreEqual(Folder, result.Name);
        }
        [Test]
        public void DirectoryInfoConverter_ConvertTo_ConvertsDirectoryInfoToString()
        {
            // Arrange
            const string Folder = "SomeFolder";
            var directory = new DirectoryInfo(Folder);
            var converter = new DirectoryInfoConverter();

            // Act
            var result = converter.ConvertTo(directory, typeof(string)).Cast<string>();

            // Assert
            Assert.AreEqual(directory.FullName, result);
        }

        protected override ITypeConverter GetTestInstance()
        {
            return new DirectoryInfoConverter();
        }
    }
}
