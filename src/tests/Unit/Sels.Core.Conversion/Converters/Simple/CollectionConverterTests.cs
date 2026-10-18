using Sels.Core.Conversion.Converters;
using Sels.Core.Conversion.Converters.Simple;
using Sels.Core.Conversion.Tests.Contracts;
using System.Collections;
using System.Collections.ObjectModel;

namespace Sels.Core.Conversion.Tests.Converters.Simple
{
    /// <summary>Contract tests for <see cref="CollectionConverter"/>.</summary>
    [Trait("Category", "Unit")]
    public class CollectionConverterTests : ITypeConverterTests
    {
        /// <summary>Verifies CanConvert for source and target types: returns true only for constructible target collection.</summary>
        [Theory]
        [InlineData(typeof(HashSet<string>), typeof(List<string>), true)]
        [InlineData(typeof(List<double>), typeof(List<string>), false)]
        [InlineData(typeof(double[]), typeof(Collection<double>), true, new object[] { 0 })]
        [InlineData(typeof(Collection<int>), typeof(List<int>), true)]
        [InlineData(typeof(Collection<bool>), typeof(HashSet<bool>), true)]
        [InlineData(typeof(List<bool>), typeof(ReadOnlyCollection<bool>), true)]
        [InlineData(typeof(HashSet<int>), typeof(ReadOnlyCollection<int>), true)]
        [InlineData(typeof(HashSet<decimal>), typeof(ReadOnlyCollection<decimal>), true)]
        [InlineData(typeof(int), typeof(string), false)]
        public void CanConvert_SourceAndTargetTypes_ReturnsTrueOnlyForConstructibleTargetCollection(Type sourceType, Type targetType, bool expected, object[]? constructArgs = null)
        {
            // Arrange
            var collection = constructArgs.HasValue() ? sourceType.Construct(constructArgs) : sourceType.Construct();
            var converter = new CollectionConverter();

            // Act
            var result = converter.CanConvert(collection, targetType);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>Verifies ConvertTo for collection: returns target collection with same elements.</summary>
        [Theory]
        [InlineData(typeof(List<string>), typeof(Collection<string>), new string[] { "I'm a test string", "SomeString", "Hello World!" })]
        [InlineData(typeof(Collection<int>), typeof(List<int>), new int[] { 4, 1998, 54853, 29, 98 })]
        [InlineData(typeof(HashSet<double>), typeof(List<double>), new double[] { 4.3, 1998.1, 54853.9876, 29.23422, 98.99 })]
        [InlineData(typeof(List<bool>), typeof(ReadOnlyCollection<bool>),new bool[] { true, false, true, false, false })]
        [InlineData(typeof(ReadOnlyCollection<string>), typeof(HashSet<string>), new string[] { "I'm a test string", "SomeString", "Hello World!" })]
        [InlineData(typeof(HashSet<double>), typeof(Collection<double>), new double[] { 4.3, 1998.1, 54853.9876, 29.23422, 98.99 })]
        [InlineData(typeof(HashSet<int>), typeof(ReadOnlyCollection<int>), new int[] { 4, 1998, 54853, 29, 98 })]
        public void ConvertTo_Collection_ReturnsTargetCollectionWithSameElements<T>(Type collectionType, Type targetType, T[] elements)
        {
            // Arrange
            var collection = collectionType.Construct(elements).CastTo<IEnumerable>();
            var converter = new CollectionConverter();

            // Act
            var result = converter.ConvertTo(collection, targetType).CastToOrDefault<IEnumerable>();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(collection, result);
        }

        /// <inheritdoc/>
        protected override ITypeConverter GetTestInstance()
        {
            return new CollectionConverter();
        }
    }
}
