using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Sels.Core.Conversion.Converters;
using Sels.Core.Conversion.Converters.Simple;
using Sels.Core.Conversion.Test.TestTemplates.Converter;

namespace Sels.Core.Conversion.Test.Converters.Simple
{
    public class ArrayConverterTests : ITypeConverterTests
    {
        [TestCase(typeof(List<string>), typeof(string[]), true)]
        [TestCase(typeof(List<double>), typeof(string[]), false)]
        [TestCase(typeof(List<int>), typeof(string[]), false)]
        [TestCase(typeof(HashSet<int>), typeof(int[]), true)]
        [TestCase(typeof(Collection<bool>), typeof(bool[]), true)]
        [TestCase(typeof(int), typeof(string), false)]
        public void ArrayConverter_CanConvert_ReturnsTrueOnlyWhenElementTypeIsAssignableToArrayType(Type collectionType, Type arrayType, bool expected)
        {
            // Arrange
            var collection = collectionType.Construct();
            var converter = new ArrayConverter();

            // Act
            var result = converter.CanConvert(collection, arrayType);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestCase(typeof(List<string>), new string[] { "I'm a test string", "SomeString", "Hello World!" })]
        [TestCase(typeof(Collection<int>), new int[] { 4, 1998, 54853, 29, 98 })]
        [TestCase(typeof(HashSet<double>), new double[] { 4.3, 1998.1, 54853.9876, 29.23422, 98.99 })]
        [TestCase(typeof(List<bool>), new bool[] { true, false, true, false, false })]
        [TestCase(typeof(ReadOnlyCollection<string>), new string[] { "I'm a test string", "SomeString", "Hello World!" })]
        public void ArrayConverter_ConvertTo_ConvertsCollectionToArrayWithSameElements<T>(Type collectionType, T[] elements)
        {
            // Arrange
            var collection = collectionType.Construct(elements).CastTo<IEnumerable>();
            var converter = new ArrayConverter();

            // Act
            var array = converter.ConvertTo(collection, collectionType.GetElementTypeFromCollection().MakeArrayType()).CastToOrDefault<IEnumerable>();

            // Assert
            Assert.IsNotNull(array);
            CollectionAssert.AreEqual(collection, array);
        }

        protected override ITypeConverter GetTestInstance()
        {
            return new ArrayConverter();
        }
    }
}
