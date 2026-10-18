using Sels.Core;

namespace Sels.Core.Legacy.Tests
{
    /// <summary>Contract tests for shared reflection metadata.</summary>
    [Trait("Category", "Unit")]
    public class ConstantsTests
    {
        /// <summary>Cached reflection members invoke their matching object operations.</summary>
        [Fact]
        public void Reflections_Methods_InvokeMatchingObjectOperations()
        {
            var value = new ReflectionProbe("value");

            var type = Constants.Reflections.ObjectGetTypeMethod.Invoke(value, null);
            var text = Constants.Reflections.ObjectToStringMethod.Invoke(value, null);
            var equals = Constants.Reflections.ObjectEqualsMethod.Invoke(value, new object[] { value });

            Assert.Equal(typeof(ReflectionProbe), type);
            Assert.Equal("value", text);
            Assert.Equal(true, equals);
            Assert.Equal("left", Constants.Expressions.LeftParameterName);
            Assert.Equal("right", Constants.Expressions.RightParameterName);
        }

        private sealed class ReflectionProbe
        {
            private readonly string _value;

            public ReflectionProbe(string value)
            {
                _value = value;
            }

            public override string ToString() => _value;
        }
    }
}