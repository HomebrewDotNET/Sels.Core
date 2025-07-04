using NUnit.Framework;
using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sels.Core.Test.Extensions.Validation
{
    [TestFixture]
    public class ArgumentValidationExtensionsTests
    {
        [Test]
        public void ValidateArgument_WithNullArgument_ShouldThrowArgumentNullException()
        {
            // Arrange
            object arg = null;

            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() => arg.ValidateArgument("testParam"));
            Assert.AreEqual("testParam", ex.ParamName);
        }

        [Test]
        public void ValidateArgument_WithNullParameterName_ShouldThrowArgumentException()
        {
            // Arrange
            var arg = "test";

            // Act & Assert
            Assert.Throws<ArgumentException>(() => arg.ValidateArgument(null));
            Assert.Throws<ArgumentException>(() => arg.ValidateArgument(""));
            Assert.Throws<ArgumentException>(() => arg.ValidateArgument("   "));
        }

        [Test]
        public void ValidateArgumentSmaller_WithCorrectErrorMessage_ShouldShowSmallerMessage()
        {
            // Arrange
            var value = 10;
            var comparator = 5;

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => value.ValidateArgumentSmaller("testParam", comparator));
            StringAssert.Contains("must be smaller than", ex.Message);
            StringAssert.Contains("5", ex.Message);
            StringAssert.Contains("10", ex.Message);
        }

        [Test]
        public void ValidateArgumentSmallerOrEqual_WithCorrectErrorMessage_ShouldShowSmallerOrEqualMessage()
        {
            // Arrange
            var value = 10;
            var comparator = 5;

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => value.ValidateArgumentSmallerOrEqual("testParam", comparator));
            StringAssert.Contains("must be smaller or equal to", ex.Message);
            StringAssert.Contains("5", ex.Message);
            StringAssert.Contains("10", ex.Message);
        }

        [Test]
        public void ValidateArgumentLarger_WithValidValue_ShouldReturnValue()
        {
            // Arrange
            var value = 10;
            var comparator = 5;

            // Act
            var result = value.ValidateArgumentLarger("testParam", comparator);

            // Assert
            Assert.AreEqual(10, result);
        }

        [Test]
        public void ValidateArgumentInRange_WithValueOutOfRange_ShouldThrowArgumentException()
        {
            // Arrange
            var value = 15;
            var startRange = 1;
            var endRange = 10;

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => value.ValidateArgumentInRange("testParam", startRange, endRange));
            StringAssert.Contains("must be in range", ex.Message);
            StringAssert.Contains("1", ex.Message);
            StringAssert.Contains("10", ex.Message);
            StringAssert.Contains("15", ex.Message);
        }

        [Test]
        public void ValidateArgumentNotNullOrEmpty_WithValidString_ShouldReturnString()
        {
            // Arrange
            var value = "test";

            // Act
            var result = value.ValidateArgumentNotNullOrEmpty("testParam");

            // Assert
            Assert.AreEqual("test", result);
        }

        [Test]
        public void ValidateArgumentNotNullOrEmpty_WithNullString_ShouldThrowArgumentException()
        {
            // Arrange
            string value = null;

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => value.ValidateArgumentNotNullOrEmpty("testParam"));
            StringAssert.Contains("cannot be null or empty", ex.Message);
        }

        [Test]
        public void ValidateArgumentNotNullOrWhitespace_WithWhitespaceString_ShouldThrowArgumentException()
        {
            // Arrange
            var value = "   ";

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => value.ValidateArgumentNotNullOrWhitespace("testParam"));
            StringAssert.Contains("cannot be null, empty or whitespace", ex.Message);
        }

        [Test]
        public void ValidateArgumentEndsWith_WithNonMatchingString_ShouldThrowArgumentException()
        {
            // Arrange
            var value = "test.txt";
            var comparator = ".xml";

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => value.ValidateArgumentEndsWith("testParam", comparator));
            StringAssert.Contains("must end with .xml", ex.Message);
        }

        [Test]
        public void ValidateArgumentDoesNotEndWith_WithMatchingString_ShouldThrowArgumentException()
        {
            // Arrange
            var value = "test.txt";
            var comparator = ".txt";

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => value.ValidateArgumentDoesNotEndWith("testParam", comparator));
            StringAssert.Contains("can't end with .txt", ex.Message);
        }

        [Test]
        public void ValidateArgumentStartsWith_WithNonMatchingString_ShouldThrowArgumentException()
        {
            // Arrange
            var value = "test.txt";
            var comparator = "file";

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => value.ValidateArgumentStartsWith("testParam", comparator));
            StringAssert.Contains("must start with file", ex.Message);
        }

        [Test]
        public void ValidateArgumentDoesNotStartWith_WithMatchingString_ShouldThrowArgumentException()
        {
            // Arrange
            var value = "test.txt";
            var comparator = "test";

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => value.ValidateArgumentDoesNotStartWith("testParam", comparator));
            StringAssert.Contains("can't start with test", ex.Message);
        }

        [Test]
        public void ValidateArgumentNotNullOrEmpty_WithEmptyCollection_ShouldThrowArgumentException()
        {
            // Arrange
            var value = new List<int>();

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => value.ValidateArgumentNotNullOrEmpty<List<int>, int>("testParam"));
            StringAssert.Contains("cannot be null and must contain at least 1 item", ex.Message);
        }

        [Test]
        public void ValidateArgumentNotNullOrEmpty_WithNullCollection_ShouldThrowArgumentException()
        {
            // Arrange
            IEnumerable<int> value = null;

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => value.ValidateArgumentNotNullOrEmpty("testParam"));
            StringAssert.Contains("cannot be null and must contain at least 1 item", ex.Message);
        }

        [Test]
        public void ValidateArgumentNotNullOrEmpty_WithValidArray_ShouldReturnArray()
        {
            // Arrange
            var value = new[] { 1, 2, 3 };

            // Act
            var result = value.ValidateArgumentNotNullOrEmpty("testParam");

            // Assert
            Assert.AreEqual(value, result);
        }

        [Test]
        public void ValidateArgumentAssignableFrom_WithNullArgument_ShouldThrowArgumentException()
        {
            // Arrange
            object value = null;

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => value.ValidateArgumentAssignableFrom("testParam", typeof(string)));
            StringAssert.Contains("cannot be null", ex.Message);
        }

        [Test]
        public void ValidateArgumentAssignableTo_WithValidType_ShouldReturnValue()
        {
            // Arrange
            var value = "test";

            // Act
            var result = value.ValidateArgumentAssignableTo("testParam", typeof(object));

            // Assert
            Assert.AreEqual("test", result);
        }

        [Test]
        public void ValidateArgumentNotInterface_WithInterfaceType_ShouldThrowArgumentException()
        {
            // Arrange
            var value = typeof(IDisposable);

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => value.ValidateArgumentNotInterface("testParam"));
            StringAssert.Contains("can't be an interface", ex.Message);
        }

        [Test]
        public void ValidateArgumentExists_WithNonExistentFile_ShouldThrowArgumentException()
        {
            // Arrange
            var value = new FileInfo("nonexistent.txt");

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => value.ValidateArgumentExists("File does not exist"));
            StringAssert.Contains("File does not exist", ex.Message);
        }

        [Test]
        public void ValidateArgument_WithCustomCondition_ShouldReturnValueWhenConditionMet()
        {
            // Arrange
            var value = 10;

            // Act
            var result = value.ValidateArgument(x => x > 5, "Value must be greater than 5");

            // Assert
            Assert.AreEqual(10, result);
        }

        [Test]
        public void ValidateArgument_WithCustomCondition_ShouldThrowWhenConditionNotMet()
        {
            // Arrange
            var value = 3;

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => value.ValidateArgument(x => x > 5, "Value must be greater than 5"));
            StringAssert.Contains("Value must be greater than 5", ex.Message);
        }

        [Test]
        public void ValidateArgument_WithNullCondition_ShouldThrowArgumentNullException()
        {
            // Arrange
            var value = 10;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => value.ValidateArgument(null as Predicate<int>, "message"));
        }

        [Test]
        public void ValidateArgument_WithCustomExceptionFunc_ShouldThrowCustomException()
        {
            // Arrange
            var value = 3;

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => 
                value.ValidateArgument(x => x > 5, x => new InvalidOperationException($"Custom error: {x}")));
            StringAssert.Contains("Custom error: 3", ex.Message);
        }
    }
}