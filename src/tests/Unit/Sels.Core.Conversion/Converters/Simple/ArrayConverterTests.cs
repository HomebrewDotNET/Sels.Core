using System.Collections;
using System.Collections.ObjectModel;
using Sels.Core.Conversion.Converters;
using Sels.Core.Conversion.Converters.Simple;
using Sels.Core.Conversion.Tests.Contracts;

namespace Sels.Core.Conversion.Tests.Converters.Simple
{
    /// <summary>Contract tests for <see cref="ArrayConverter"/>.</summary>
    [Trait("Category", "Unit")]
    public class ArrayConverterTests : ITypeConverterTests
    {
        /// <summary>Verifies CanConvert for source and target types: returns true only for assignable element type.</summary>
        [Theory]
        [InlineData(typeof(List<string>), typeof(string[]), true)]
        [InlineData(typeof(List<double>), typeof(string[]), false)]
        [InlineData(typeof(List<int>), typeof(string[]), false)]
        [InlineData(typeof(HashSet<int>), typeof(int[]), true)]
        [InlineData(typeof(Collection<bool>), typeof(bool[]), true)]
        [InlineData(typeof(int), typeof(string), false)]
        public void CanConvert_SourceAndTargetTypes_ReturnsTrueOnlyForAssignableElementType(Type collectionType, Type arrayType, bool expected)
        {
            // Arrange
            var collection = collectionType.Construct();
            var converter = new ArrayConverter();

            // Act
            var result = converter.CanConvert(collection, arrayType);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>Verifies ConvertTo for collection: returns array with same elements.</summary>
        [Theory]
        [InlineData(typeof(List<string>), new string[] { "I'm a test string", "SomeString", "Hello World!" })]
        [InlineData(typeof(Collection<int>), new int[] { 4, 1998, 54853, 29, 98 })]
        [InlineData(typeof(HashSet<double>), new double[] { 4.3, 1998.1, 54853.9876, 29.23422, 98.99 })]
        [InlineData(typeof(List<bool>), new bool[] { true, false, true, false, false })]
        [InlineData(typeof(ReadOnlyCollection<string>), new string[] { "I'm a test string", "SomeString", "Hello World!" })]
        public void ConvertTo_Collection_ReturnsArrayWithSameElements<T>(Type collectionType, T[] elements)
        {
            // Arrange
            var collection = collectionType.Construct(elements).CastTo<IEnumerable>();
            var converter = new ArrayConverter();

            // Act
            var array = converter.ConvertTo(collection, collectionType.GetElementTypeFromCollection().MakeArrayType()).CastToOrDefault<IEnumerable>();

            // Assert
            Assert.NotNull(array);
            Assert.Equal(collection, array);
        }

        /// <inheritdoc/>
        protected override ITypeConverter GetTestInstance()
        {
            return new ArrayConverter();
        }
    }
}
