using Sels.Core.Conversion.Contracts;
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
    public class FileInfoConverterTests : ITypeConverterTests
    {
        [TestCase(typeof(FileInfo), typeof(string), true, new object[] { "SomeFile.pdf" })]
        [TestCase(typeof(string), typeof(FileInfo), true, new object[] { new char[] { 'C', ':', '/' } })]
        [TestCase(typeof(int), typeof(FileInfo), false)]
        [TestCase(typeof(bool), typeof(FileInfo), false)]
        [TestCase(typeof(FileInfo), typeof(int), false, new object[] { "SomeFile.txt" })]
        [TestCase(typeof(FileInfo), typeof(double), false, new object[] { "SomeFile.json" })]
        [TestCase(typeof(int), typeof(string), false)]
        public void FileInfoConverter_CanConvert_ReturnsTrueOnlyWhenConvertingBetweenStringAndFileInfo(Type sourceType, Type targetType, bool expected, object[]? arguments = null)
        {
            // Arrange
            var instance = arguments.HasValue() ? sourceType.Construct(arguments) : sourceType.Construct();
            var converter = new FileInfoConverter();

            // Act
            var result = converter.CanConvert(instance, targetType);

            // Assert
            Assert.AreEqual(expected, result);
        }
        [Test]
        public void FileInfoConverter_ConvertTo_ConvertsStringToFileInfo()
        {
            // Arrange
            const string File = "SomeFile.txt";
            var converter = new FileInfoConverter();

            // Act
            var result = converter.ConvertTo(File, typeof(FileInfo)).Cast<FileInfo>();

            // Assert
            Assert.AreEqual(File, result.Name);
        }
        [Test]
        public void FileInfoConverter_ConvertTo_ConvertsFileInfoToString()
        {
            // Arrange
            const string File = "SomeFile.txt";
            var file = new FileInfo(File);
            var converter = new FileInfoConverter();

            // Act
            var result = converter.ConvertTo(file, typeof(string)).Cast<string>();

            // Assert
            Assert.AreEqual(file.FullName, result);
        }
        protected override ITypeConverter GetTestInstance()
        {
            return new FileInfoConverter();
        }
    }
}
