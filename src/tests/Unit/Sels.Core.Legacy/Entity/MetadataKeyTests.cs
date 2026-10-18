using Sels.Core.Entity;

namespace Sels.Core.Legacy.Tests.Entity
{
    /// <summary>Contract tests for metadata key identity and validation.</summary>
    [Trait("Category", "Unit")]
    public class MetadataKeyTests
    {
        /// <summary>Metadata key names use case-insensitive identity.</summary>
        [Fact]
        public void Get_NameDiffersOnlyByCase_ReturnsSameKey()
        {
            var name = $"metadata-{Guid.NewGuid():N}";

            var first = MetadataKey.Get(name);
            var same = MetadataKey.Get(name.ToUpperInvariant());

            Assert.Same(first, same);
        }

        /// <summary>Typed keys reject reuse of the same name for another value type.</summary>
        [Fact]
        public void Get_TypedNameConflicts_ThrowsInvalidOperationException()
        {
            var name = $"metadata-{Guid.NewGuid():N}";
            _ = MetadataKey<int>.Get(name);

            var exception = Assert.Throws<InvalidOperationException>(() => MetadataKey<string>.Get(name));

            Assert.Contains(name, exception.Message);
        }

        /// <summary>Metadata key names reject null, empty, and whitespace inputs.</summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void Get_InvalidName_ThrowsArgumentException(string? name)
        {
            var exception = Assert.Throws<ArgumentException>(() => MetadataKey.Get(name!));

            Assert.Contains("name", exception.Message, StringComparison.OrdinalIgnoreCase);
        }
    }
}