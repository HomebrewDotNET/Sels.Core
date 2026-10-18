using System.Collections;
using Sels.Core.Extensions.Linq;

namespace Sels.Core.Legacy.Tests.Extensions.Linq
{
    /// <summary>Contract tests for collection count helpers.</summary>
    [Trait("Category", "Unit")]
    public class LinqExtensionsTests
    {
        /// <summary>Known collection interfaces report their counts without enumeration.</summary>
        [Fact]
        public void TryGetCount_KnownCollections_ReturnsCount()
        {
            var values = new[] { 3, 5, 8 };

            var arrayCount = values.TryGetCount();
            var listCount = ((IList<int>)values).TryGetCount();
            var readOnlyCount = ((IReadOnlyCollection<int>)values).TryGetCount();
            var nonGenericListCount = ((IList)values).TryGetCount();
            var nonGenericCollectionCount = ((ICollection)values).TryGetCount();

            Assert.Equal(3, arrayCount);
            Assert.Equal(3, listCount);
            Assert.Equal(3, readOnlyCount);
            Assert.Equal(3, nonGenericListCount);
            Assert.Equal(3, nonGenericCollectionCount);
        }

        /// <summary>Unknown enumerable implementations return no count without enumerating.</summary>
        [Fact]
        public void TryGetCount_UnknownEnumerable_DoesNotEnumerate()
        {
            var enumerations = 0;
            IEnumerable<int> values = Enumerate();

            var count = values.TryGetCount();

            Assert.Null(count);
            Assert.Equal(0, enumerations);

            IEnumerable<int> Enumerate()
            {
                enumerations++;
                yield return 1;
            }
        }

        /// <summary>GetCount enumerates unknown generic sequences and returns their item count.</summary>
        [Fact]
        public void GetCount_UnknownGenericEnumerable_ReturnsEnumeratedCount()
        {
            IEnumerable<int> values = YieldValues();

            var count = values.GetCount();

            Assert.Equal(3, count);
        }

        /// <summary>GetCount enumerates unknown non-generic sequences and returns their item count.</summary>
        [Fact]
        public void GetCount_UnknownNonGenericEnumerable_ReturnsEnumeratedCount()
        {
            IEnumerable values = YieldValues();

            var count = values.GetCount();

            Assert.Equal(3, count);
        }

        /// <summary>GetCount rejects a null generic source at entry.</summary>
        [Fact]
        public void GetCount_NullGenericSource_ThrowsArgumentNullException()
        {
            IEnumerable<int> values = null!;

            var exception = Assert.Throws<ArgumentNullException>(() => values.GetCount());

            Assert.Equal("source", exception.ParamName);
        }

        /// <summary>TryGetCount rejects a null generic source at entry.</summary>
        [Fact]
        public void TryGetCount_NullGenericSource_ThrowsArgumentNullException()
        {
            IEnumerable<int> values = null!;

            var exception = Assert.Throws<ArgumentNullException>(() => values.TryGetCount());

            Assert.Equal("source", exception.ParamName);
        }

        /// <summary>TryGetCount rejects a null non-generic source at entry.</summary>
        [Fact]
        public void TryGetCount_NullNonGenericSource_ThrowsArgumentNullException()
        {
            IEnumerable values = null!;

            var exception = Assert.Throws<ArgumentNullException>(() => LinqExtensions.TryGetCount(values));

            Assert.Equal("source", exception.ParamName);
        }

        private static IEnumerable<int> YieldValues()
        {
            yield return 1;
            yield return 2;
            yield return 3;
        }
    }
}