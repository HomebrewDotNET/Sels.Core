using Sels.Core.Entity;

namespace Sels.Core.Legacy.Tests.Entity.Components
{
    /// <summary>Contract tests for collection equivalence wrappers.</summary>
    [Trait("Category", "Unit")]
    public class EquivalentCollectionTests
    {
        /// <summary>Wrapped collection exposes the supplied items through list members.</summary>
        [Fact]
        public void CollectionAndEnumeration_WrappedValues_MirrorValues()
        {
            IReadOnlyList<int> values = new[] { 2, 4, 6 };
            var sut = new EquivalentCollection<IReadOnlyList<int>, int>(values);

            var enumerated = sut.ToArray();

            Assert.Same(values, sut.Collection);
            Assert.Equal(3, sut.Count);
            Assert.Equal(4, sut[1]);
            Assert.Equal(values, enumerated);
        }

        /// <summary>Strict comparison preserves sequence order.</summary>
        [Fact]
        public void IsEquivalentTo_StrictOrderAndReorderedItems_ReturnsFalse()
        {
            var left = new EquivalentCollection<IReadOnlyList<int>, int>(new[] { 1, 2 }, true);
            var right = new EquivalentCollection<IReadOnlyList<int>, int>(new[] { 2, 1 }, true);

            var result = left.IsEquivalentTo(right);

            Assert.False(result);
        }

        /// <summary>Unordered comparison accepts reordered equal content and rejects different content.</summary>
        [Fact]
        public void IsEquivalentTo_UnorderedContent_MatchesItemsRegardlessOfOrder()
        {
            var left = new EquivalentCollection<IReadOnlyList<int>, int>(new[] { 1, 2 });
            var reordered = new EquivalentCollection<IReadOnlyList<int>, int>(new[] { 2, 1 });
            var different = new EquivalentCollection<IReadOnlyList<int>, int>(new[] { 1, 3 });
            var missing = new EquivalentCollection<IReadOnlyList<int>, int>(new[] { 1 });

            var equalContent = left.IsEquivalentTo(reordered);
            var differentContent = left.IsEquivalentTo(different);
            var missingContent = left.IsEquivalentTo(missing);

            Assert.True(equalContent);
            Assert.False(differentContent);
            Assert.False(missingContent);
        }
    }
}