using Sels.Core.Conversion.Contracts;
using Sels.Core.Conversion.Converters.Simple;
using Sels.Core.Conversion.Test.TestTemplates.Converter;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Conversion.Test.Converters.Simple
{
    public class CollectionConverterTests : ITypeConverterTests
    {
        [TestCase(typeof(HashSet<string>), typeof(List<string>), true)]
        [TestCase(typeof(List<double>), typeof(List<string>), false)]
        [TestCase(typeof(double[]), typeof(Collection<double>), true, new object[] { 0 })]
        [TestCase(typeof(Collection<int>), typeof(List<int>), true)]
        [TestCase(typeof(Collection<bool>), typeof(HashSet<bool>), true)]
        [TestCase(typeof(List<bool>), typeof(ReadOnlyCollection<bool>), true)]
        [TestCase(typeof(HashSet<int>), typeof(ReadOnlyCollection<int>), true)]
        [TestCase(typeof(HashSet<decimal>), typeof(ReadOnlyCollection<decimal>), true)]
        [TestCase(typeof(int), typeof(string), false)]
        public void CollectionConverter_CanConvert_ReturnsTrueOnlyWhenTargetTypeCanBeConstructedWithTheSourceCollectionOrWithAListOfTheSameElementType(Type sourceType, Type targetType, bool expected, object[]? constructArgs = null)
        {
            // Arrange
            var collection = constructArgs.HasValue() ? sourceType.Construct(constructArgs) : sourceType.Construct();
            var converter = new CollectionConverter();

            // Act
            var result = converter.CanConvert(collection, targetType);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestCase(typeof(List<string>), typeof(Collection<string>), new string[] { "I'm a test string", "SomeString", "Hello World!" })]
        [TestCase(typeof(Collection<int>), typeof(List<int>), new int[] { 4, 1998, 54853, 29, 98 })]
        [TestCase(typeof(HashSet<double>), typeof(List<double>), new double[] { 4.3, 1998.1, 54853.9876, 29.23422, 98.99 })]
        [TestCase(typeof(List<bool>), typeof(ReadOnlyCollection<bool>),new bool[] { true, false, true, false, false })]
        [TestCase(typeof(ReadOnlyCollection<string>), typeof(HashSet<string>), new string[] { "I'm a test string", "SomeString", "Hello World!" })]
        [TestCase(typeof(HashSet<double>), typeof(Collection<double>), new double[] { 4.3, 1998.1, 54853.9876, 29.23422, 98.99 })]
        [TestCase(typeof(HashSet<int>), typeof(ReadOnlyCollection<int>), new int[] { 4, 1998, 54853, 29, 98 })]
        public void CollectionConverter_ConvertTo_ConvertsCollectionToTargetCollectionWithSameElements<T>(Type collectionType, Type targetType, T[] elements)
        {
            // Arrange
            var collection = collectionType.Construct(elements).Cast<IEnumerable>();
            var converter = new CollectionConverter();

            // Act
            var result = converter.ConvertTo(collection, targetType).CastOrDefault<IEnumerable>();

            // Assert
            Assert.IsNotNull(result);
            CollectionAssert.AreEqual(collection, result);
        }

        protected override ITypeConverter GetTestInstance()
        {
            return new CollectionConverter();
        }
    }
}
