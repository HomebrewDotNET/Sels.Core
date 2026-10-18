using Sels.Core.Entity;

namespace Sels.Core.Legacy.Tests.Entity
{
    /// <summary>Contract tests for typed metadata storage.</summary>
    [Trait("Category", "Unit")]
    public class MetadataTests
    {
        /// <summary>Metadata round-trips supported primitive, custom, and reference values.</summary>
        [Fact]
        public void SetTryGetAndUnset_SupportedValues_RoundTrip()
        {
            var values = new object[]
            {
                1, 1.5f, true, 2L, 2.5d, 3.5m, 'x', DateTime.UnixEpoch,
                TimeSpan.FromSeconds(3), Guid.NewGuid(), (byte)4, (short)5,
                new MetadataStruct(6), "reference"
            };

            foreach (var value in values)
            {
                InvokeRoundTrip(value);
            }

            Assert.All(values, Assert.NotNull);
        }

        /// <summary>Typed and untyped key overloads address the same metadata value.</summary>
        [Fact]
        public void SetAndTryGet_TypedAndUntypedKeys_AreEquivalent()
        {
            var name = $"metadata-{Guid.NewGuid():N}";
            var typedKey = MetadataKey<int>.Get(name);
            MetadataKey untypedKey = typedKey;
            var metadata = new Metadata();

            metadata.Set(typedKey, 17);
            var found = metadata.TryGetValue<int>(untypedKey, out var value);

            Assert.True(found);
            Assert.Equal(17, value);
            Assert.True(metadata.ContainsKey<int>(typedKey));
            metadata.Unset(typedKey);
            Assert.False(metadata.ContainsKey<int>(untypedKey));
        }

        private static void InvokeRoundTrip(object value)
        {
            var method = typeof(MetadataTests).GetMethod(nameof(AssertRoundTrip), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;
            var generic = method.MakeGenericMethod(value.GetType());
            generic.Invoke(null, new[] { value });
        }

        private static void AssertRoundTrip<T>(T value)
        {
            var key = MetadataKey<T>.Get($"metadata-{Guid.NewGuid():N}");
            var metadata = new Metadata();
            Assert.False(metadata.ContainsKey(key));
            metadata.Set(key, value);
            Assert.True(metadata.TryGetValue(key, out var actual));
            Assert.Equal(value, actual);
            metadata.Unset(key);
            Assert.False(metadata.ContainsKey(key));
        }

        private readonly record struct MetadataStruct(int Value);
    }
}