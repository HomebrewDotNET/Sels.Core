using System.Linq.Expressions;

namespace Sels.Core.Tests
{
    /// <summary>Contract tests for current-tier argument guards.</summary>
    [Trait("Category", "Unit")]
    public class GuardTests
    {
        /// <summary>Expression guards accept true predicates and report false predicates.</summary>
        [Fact]
        public void Is_ExpressionCondition_PreservesResultAndMessage()
        {
            Expression<Func<bool>> valid = () => 3 > 2;
            Expression<Func<bool>> invalid = () => 2 > 3;

            Guard.Is(valid);
            var exception = Assert.Throws<ArgumentException>(() => Guard.Is(invalid));

            Assert.Contains(invalid.ToString(), exception.Message);
        }

        /// <summary>Expression guards reject a null expression with its parameter name.</summary>
        [Fact]
        public void Is_NullExpression_ThrowsArgumentNullException()
        {
            Expression<Func<bool>>? condition = null;

            var exception = Assert.Throws<ArgumentNullException>(() => Guard.Is(condition!));

            Assert.Equal("condition", exception.ParamName);
        }

        /// <summary>Exceptions raised while evaluating a guard predicate propagate unchanged.</summary>
        [Fact]
        public void Is_PredicateThrows_PropagatesOriginalException()
        {
            var expected = new InvalidOperationException("predicate failed");
            Expression<Func<bool>> condition = () => Throw(expected);

            var actual = Assert.Throws<InvalidOperationException>(() => Guard.Is(condition));

            Assert.Same(expected, actual);
        }

        /// <summary>IsDefined accepts named enum values and rejects undefined flag combinations.</summary>
        [Fact]
        public void IsDefined_EnumValues_ReturnsOrRejectsValue()
        {
            var defined = SampleFlags.First;
            var undefinedCombination = SampleFlags.First | SampleFlags.Second;

            var result = Guard.IsDefined(defined);
            var exception = Assert.Throws<ArgumentException>(() => Guard.IsDefined(undefinedCombination));

            Assert.Equal(defined, result);
            Assert.Contains(nameof(SampleFlags), exception.Message);
        }

        /// <summary>Existing guard methods preserve their documented null exception families.</summary>
        [Fact]
        public void IsNotNullAndTextGuards_InvalidInputs_ThrowDocumentedExceptions()
        {
            string? text = null;

            var nullException = Assert.Throws<ArgumentNullException>(() => Guard.IsNotNull(text));
            var emptyException = Assert.Throws<ArgumentException>(() => Guard.IsNotNullOrWhitespace(text));
            var conditionException = Assert.Throws<ArgumentException>(() => Guard.IsNotNullOrEmpty(text));

            Assert.Equal("text", nullException.ParamName);
            Assert.Contains("text", emptyException.Message);
            Assert.Contains("text", conditionException.Message);
        }

        private static bool Throw(Exception exception) => throw exception;

        [Flags]
        private enum SampleFlags
        {
            First = 1,
            Second = 2
        }
    }
}