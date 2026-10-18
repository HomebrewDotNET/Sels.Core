using Sels.Core.Entity;
using Sels.Core.Entity.Components;

namespace Sels.Core.Legacy.Tests.Entity.Components
{
    /// <summary>Contract tests for hash-bucket deduplication.</summary>
    [Trait("Category", "Unit")]
    public class DeduplicatorTests
    {
        /// <summary>Deduplicator finds equal selected keys and ignores non-equivalent values.</summary>
        [Fact]
        public void HasDuplicate_SelectedKeyAndEquivalence_ReturnsExpectedItems()
        {
            var sut = new Deduplicator<Item>(by: item => item.Key);
            var first = new Item("same", 7);
            var duplicate = new Item("same", 7);
            var different = new Item("different", 7);

            sut.Add(first);
            var hasDuplicate = sut.HasDuplicate(duplicate);
            var duplicates = sut.GetDuplicates(duplicate).ToArray();
            var hasDifferent = sut.HasDuplicate(different);

            Assert.True(hasDuplicate);
            Assert.Equal(new[] { first }, duplicates);
            Assert.False(hasDifferent);
        }

        /// <summary>Update and remove move items between selected-key buckets.</summary>
        [Fact]
        public void UpdateAndRemove_ItemChangesKey_RefreshesLookup()
        {
            var sut = new Deduplicator<Item>(by: item => item.Key);
            var item = new Item("old", 1);
            var oldBucket = sut.Add(item);

            item.Key = "new";
            sut.Update(oldBucket, item);
            var oldLookup = sut.HasDuplicate(new Item("old", 2));
            var newLookup = sut.HasDuplicate(new Item("new", 3));
            sut.Remove(item);

            Assert.False(oldLookup);
            Assert.True(newLookup);
            Assert.False(sut.HasDuplicate(new Item("new", 4)));
        }

        /// <summary>Constructor seeds use the configured selector when detecting duplicates.</summary>
        [Fact]
        public void Constructor_InitialItemAndSelectedKey_FindsDuplicate()
        {
            var initial = new Item("same", 91);
            var sut = new Deduplicator<Item>(new[] { initial }, item => new DedupKey(17, item.Key));

            var result = sut.HasDuplicate(new Item("same", 92));

            Assert.True(result);
        }

        /// <summary>Required item arguments are rejected with their parameter names.</summary>
        [Fact]
        public void AddAndHasDuplicate_NullItem_ThrowArgumentNullException()
        {
            var sut = new Deduplicator<string>();

            var addException = Assert.Throws<ArgumentNullException>(() => sut.Add(null!));
            var lookupException = Assert.Throws<ArgumentNullException>(() => sut.HasDuplicate(null!));

            Assert.Equal("item", addException.ParamName);
            Assert.Equal("item", lookupException.ParamName);
        }

        /// <summary>GetDuplicates validates its item when deferred enumeration begins.</summary>
        [Fact]
        public void GetDuplicates_NullItem_ThrowsWhenEnumerated()
        {
            var sut = new Deduplicator<string>();
            var duplicates = sut.GetDuplicates(null!);

            var exception = Assert.Throws<ArgumentNullException>(() => duplicates.ToArray());

            Assert.Equal("item", exception.ParamName);
        }

        /// <summary>Deduplication interfaces preserve their public members and generic contract.</summary>
        [Fact]
        public void EntityInterfaces_Reflection_ExposeDocumentedMembers()
        {
            var deduplicatable = typeof(IDeduplicatable);
            var identifiable = typeof(IIdentifiable<Guid>);

            var methods = deduplicatable.GetMethods().Select(method => method.Name).ToArray();
            var idProperty = identifiable.GetProperty(nameof(IIdentifiable<Guid>.Id));

            Assert.Contains(nameof(IDeduplicatable.IsEquivalentTo), methods);
            Assert.Contains(nameof(IDeduplicatable.GetBucket), methods);
            Assert.NotNull(idProperty);
            Assert.Equal(typeof(Guid), idProperty!.PropertyType);
        }

        private sealed class Item
        {
            public string Key { get; set; }

            private readonly int _hashCode;

            public Item(string key, int hashCode)
            {
                Key = key;
                _hashCode = hashCode;
            }

            public override int GetHashCode() => _hashCode;
        }

        private sealed class DedupKey : IDeduplicatable
        {
            private readonly string _value;

            public DedupKey(int bucket, string value)
            {
                Bucket = bucket;
                _value = value;
            }

            public int Bucket { get; }

            public bool IsEquivalentTo(IDeduplicatable other) => other is DedupKey key && _value == key._value;

            public int GetBucket() => Bucket;
        }
    }
}