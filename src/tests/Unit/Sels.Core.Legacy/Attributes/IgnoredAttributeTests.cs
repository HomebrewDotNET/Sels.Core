using Sels.Core.Attributes;
using Sels.Core.Entity;

namespace Sels.Core.Legacy.Tests.Attributes
{
    /// <summary>Contract tests for ignored-member metadata.</summary>
    [Trait("Category", "Unit")]
    public class IgnoredAttributeTests
    {
        /// <summary>IgnoredAttribute applies to types and properties and is inherited once.</summary>
        [Fact]
        public void IgnoredAttribute_UsageMetadata_MatchesContract()
        {
            var usage = typeof(IgnoredAttribute).GetCustomAttributes(typeof(AttributeUsageAttribute), true)
                .Cast<AttributeUsageAttribute>().Single();

            var targets = usage.ValidOn;

            Assert.Equal(AttributeTargets.Property | AttributeTargets.Class, targets);
            Assert.False(usage.AllowMultiple);
            Assert.True(usage.Inherited);
        }

        /// <summary>The property comparer ignores properties marked with IgnoredAttribute.</summary>
        [Fact]
        public void GeneratePropertyComparer_IgnoredPropertyDiffers_ReturnsEquivalent()
        {
            var compare = Helper.Expressions.Comparing.GeneratePropertyComparer<IgnoredModel>(false);

            var result = compare(new IgnoredModel { Value = 5, Ignored = "left" }, new IgnoredModel { Value = 5, Ignored = "right" });

            Assert.True(result);
        }

        private sealed class IgnoredModel
        {
            public int Value { get; set; }

            [Ignored]
            public string Ignored { get; set; } = string.Empty;
        }
    }
}