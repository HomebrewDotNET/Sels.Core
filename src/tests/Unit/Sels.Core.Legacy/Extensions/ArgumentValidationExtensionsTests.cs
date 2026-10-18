using System.Collections;
using Sels.Core.Extensions;

namespace Sels.Core.Legacy.Tests.Extensions
{
    /// <summary>Contract tests for legacy argument validation extensions.</summary>
    [Trait("Category", "Unit")]
    public class ArgumentValidationExtensionsTests
    {
        /// <summary>Passing generic validation returns the original argument.</summary>
        [Fact]
        public void ValidateArgument_ConditionPasses_ReturnsArgument()
        {
            var value = "valid";

            var result = value.ValidateArgument(x => x.Length > 0, _ => new InvalidOperationException());

            Assert.Same(value, result);
        }

        /// <summary>Failing generic validation throws the supplied exception.</summary>
        [Fact]
        public void ValidateArgument_ConditionFails_ThrowsRequestedException()
        {
            var expected = new InvalidOperationException("invalid");

            var actual = Assert.Throws<InvalidOperationException>(() => 0.ValidateArgument(x => x > 0, _ => expected));

            Assert.Same(expected, actual);
        }

        /// <summary>The message-based condition overload returns valid values and preserves its message on failure.</summary>
        [Fact]
        public void ValidateArgument_MessageOverload_ValidatesCondition()
        {
            const string value = "valid";

            var valid = value.ValidateArgument(text => text.Length > 0, "must not be empty");
            var exception = Assert.Throws<ArgumentException>(() => string.Empty.ValidateArgument(text => text.Length > 0, "must not be empty"));

            Assert.Same(value, valid);
            Assert.Contains("must not be empty", exception.Message);
        }

        /// <summary>Required argument validation preserves the parameter name.</summary>
        [Fact]
        public void ValidateArgument_NullValue_ThrowsArgumentNullException()
        {
            string? value = null;

            var exception = Assert.Throws<ArgumentNullException>(() => value!.ValidateArgument("value"));

            Assert.Equal("value", exception.ParamName);
        }

        /// <summary>Text validation rejects null, empty, and whitespace values.</summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void ValidateArgumentNotNullOrWhitespace_InvalidText_ThrowsArgumentException(string? value)
        {
            var exception = Assert.Throws<ArgumentException>(() => value!.ValidateArgumentNotNullOrWhitespace("value"));

            Assert.Contains("value", exception.Message);
        }

        /// <summary>Nonempty string validation returns the original value and rejects empty input.</summary>
        [Fact]
        public void ValidateArgumentNotNullOrEmpty_String_ValidatesInput()
        {
            const string value = "content";

            var valid = value.ValidateArgumentNotNullOrEmpty("value");
            var exception = Assert.Throws<ArgumentException>(() => string.Empty.ValidateArgumentNotNullOrEmpty("value"));

            Assert.Same(value, valid);
            Assert.Contains("value", exception.Message);
        }

        /// <summary>String prefix and suffix checks use the configured comparison mode.</summary>
        [Fact]
        public void ValidateArgumentStringComparisons_ValidValues_ReturnOriginalString()
        {
            const string value = "Alpha.txt";

            var suffix = value.ValidateArgumentEndsWith("value", ".TXT");
            var prefix = value.ValidateArgumentStartsWith("value", "alpha");
            var strictSuffix = value.ValidateArgumentEndsWith("value", ".txt", StringComparison.OrdinalIgnoreCase);
            var strictPrefix = value.ValidateArgumentStartsWith("value", "Alpha", StringComparison.Ordinal);
            var notSuffix = value.ValidateArgumentDoesNotEndWith("value", ".log");
            var notPrefix = value.ValidateArgumentDoesNotStartWith("value", "Beta");

            Assert.Same(value, suffix);
            Assert.Same(value, prefix);
            Assert.Same(value, strictSuffix);
            Assert.Same(value, strictPrefix);
            Assert.Same(value, notSuffix);
            Assert.Same(value, notPrefix);
            Assert.Throws<ArgumentException>(() => value.ValidateArgumentEndsWith("value", ".TXT", StringComparison.Ordinal));
            Assert.Throws<ArgumentException>(() => value.ValidateArgumentStartsWith("value", "alpha", StringComparison.Ordinal));
            Assert.Throws<ArgumentException>(() => value.ValidateArgumentDoesNotEndWith("value", ".TXT"));
            Assert.Throws<ArgumentException>(() => value.ValidateArgumentDoesNotStartWith("value", "Alpha"));
        }

        /// <summary>Numeric validators preserve inclusive and exclusive boundaries.</summary>
        [Fact]
        public void ValidateArgumentNumericRanges_ValidBoundaries_ReturnArgument()
        {
            var larger = 2.ValidateArgumentLarger("value", 1);
            var largerOrEqual = 1.ValidateArgumentLargerOrEqual("value", 1);
            var smaller = 1.ValidateArgumentSmaller("value", 2);
            var smallerOrEqual = 2.ValidateArgumentSmallerOrEqual("value", 2);
            var rangeStart = 1.ValidateArgumentInRange("value", 1, 2);
            var rangeEnd = 2.ValidateArgumentInRange("value", 1, 2);

            Assert.Equal(2, larger);
            Assert.Equal(1, largerOrEqual);
            Assert.Equal(1, smaller);
            Assert.Equal(2, smallerOrEqual);
            Assert.Equal(1, rangeStart);
            Assert.Equal(2, rangeEnd);
            Assert.Throws<ArgumentException>(() => 1.ValidateArgumentLarger("value", 1));
            Assert.Throws<ArgumentException>(() => 3.ValidateArgumentInRange("value", 1, 2));
        }

        /// <summary>Collection validators reject empty collections and retain valid instances.</summary>
        [Fact]
        public void ValidateArgumentNotNullOrEmpty_Collections_ValidatesItems()
        {
            var list = new List<int> { 1 };
            IEnumerable<int> enumerable = list;
            var array = new[] { 1 };

            var listResult = list.ValidateArgumentNotNullOrEmpty<List<int>, int>("items");
            var enumerableResult = enumerable.ValidateArgumentNotNullOrEmpty("items");
            var arrayResult = array.ValidateArgumentNotNullOrEmpty("items");

            Assert.Same(list, listResult);
            Assert.Same(enumerable, enumerableResult);
            Assert.Same(array, arrayResult);
            Assert.Throws<ArgumentException>(() => Array.Empty<int>().ValidateArgumentNotNullOrEmpty("items"));
        }

        /// <summary>Type validators preserve assignability direction and constructability checks.</summary>
        [Fact]
        public void ValidateArgumentTypeChecks_ValidTypes_ReturnOriginalType()
        {
            object value = new DerivedType();
            var derivedType = typeof(DerivedType);

            var objectAssignableFrom = typeof(object).ValidateArgumentAssignableFrom("type", derivedType);
            var derivedAssignableTo = typeof(DerivedType).ValidateArgumentAssignableTo("type", typeof(object));
            var instanceAssignableFrom = value.ValidateArgumentAssignableFrom("value", typeof(DerivedType));
            var instanceAssignableTo = value.ValidateArgumentAssignableTo("value", typeof(object));
            var concrete = derivedType.ValidateArgumentNotInterface("type");
            var instanceable = derivedType.ValidateArgumentInstanceable("type");
            var constructible = derivedType.ValidateArgumentCanBeContructedWith("type");
            var constructibleWithArguments = typeof(ArgumentValidationExtensionsTests)
                .ValidateArgumentCanBeContructedWithArguments("type", new object[] { });

            Assert.Same(typeof(object), objectAssignableFrom);
            Assert.Same(derivedType, derivedAssignableTo);
            Assert.Same(value, instanceAssignableFrom);
            Assert.Same(value, instanceAssignableTo);
            Assert.Same(derivedType, concrete);
            Assert.Same(derivedType, instanceable);
            Assert.Same(derivedType, constructible);
            Assert.Same(typeof(ArgumentValidationExtensionsTests), constructibleWithArguments);
            Assert.Throws<ArgumentException>(() => typeof(IDisposable).ValidateArgumentNotInterface("type"));
            Assert.Throws<ArgumentException>(() => typeof(AbstractType).ValidateArgumentInstanceable("type"));
            Assert.Throws<ArgumentException>(() => typeof(string).ValidateArgumentAssignableTo("type", typeof(DerivedType)));
        }

        /// <summary>Filesystem existence validation has an observable invalid-path contract.</summary>
        [Fact]
        public void ValidateArgumentExists_MissingFile_ThrowsArgumentException()
        {
            var file = new FileInfo(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")));

            var exception = Assert.Throws<ArgumentException>(() => file.ValidateArgumentExists("file missing"));

            Assert.Contains("file missing", exception.Message);
        }

        private abstract class AbstractType
        {
        }

        private sealed class DerivedType
        {
            public DerivedType()
            {
            }
        }
    }
}