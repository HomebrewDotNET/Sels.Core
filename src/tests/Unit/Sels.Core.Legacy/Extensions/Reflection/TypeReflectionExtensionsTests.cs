using Sels.Core.Extensions.Reflection;

namespace Sels.Core.Legacy.Tests.Extensions.Reflection
{
    /// <summary>Contract tests for reflection helpers.</summary>
    [Trait("Category", "Unit")]
    public class TypeReflectionExtensionsTests
    {
        /// <summary>Inheritance distance is zero for equal types and negative for unrelated types.</summary>
        [Fact]
        public void GetInheritanceDistance_EqualOrUnrelatedTypes_ReturnsExpectedDistance()
        {
            var same = typeof(MiddleType).GetInheritanceDistance(typeof(MiddleType));
            var unrelated = typeof(FirstType).GetInheritanceDistance(typeof(SecondType));

            Assert.Equal(0, same);
            Assert.Equal(-1, unrelated);
        }

        /// <summary>Related class types report the number of inheritance edges between them.</summary>
        [Theory]
        [InlineData(typeof(MiddleType), typeof(LeafType))]
        [InlineData(typeof(RootType), typeof(MiddleType))]
        public void GetInheritanceDistance_RelatedTypes_ReturnsOne(Type first, Type second)
        {
            var distance = first.GetInheritanceDistance(second);

            Assert.Equal(1, distance);
        }

        /// <summary>Inheritance distance rejects null type inputs by parameter name.</summary>
        [Fact]
        public void GetInheritanceDistance_NullSourceOrTarget_ThrowsArgumentNullException()
        {
            Type? source = null;
            Type? target = null;

            var sourceException = Assert.Throws<ArgumentNullException>(() => source!.GetInheritanceDistance(typeof(object)));
            var targetException = Assert.Throws<ArgumentNullException>(() => typeof(object).GetInheritanceDistance(target!));

            Assert.Equal("source", sourceException.ParamName);
            Assert.Equal("target", targetException.ParamName);
        }

        private class RootType
        {
        }

        private class MiddleType : RootType
        {
        }

        private sealed class LeafType : MiddleType
        {
        }

        private sealed class FirstType
        {
        }

        private sealed class SecondType
        {
        }
    }
}