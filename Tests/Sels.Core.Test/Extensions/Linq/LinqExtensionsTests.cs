using NUnit.Framework;
using Sels.Core.Extensions.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sels.Core.Test.Extensions.Linq
{
    [TestFixture]
    public class LinqExtensionsTests
    {
        [Test]
        public void ForceExecute_WithIndexAndException_ShouldIncrementCounterOnlyOnSuccess()
        {
            // Arrange
            var source = new[] { 1, 2, 3, 4, 5 };
            var processedIndexes = new List<int>();
            var processedValues = new List<int>();

            // Act
            source.ForceExecute((index, value) =>
            {
                processedIndexes.Add(index);
                processedValues.Add(value);
                if (value == 3) throw new InvalidOperationException("Test exception");
            });

            // Assert
            Assert.AreEqual(new[] { 0, 1, 3, 4 }, processedIndexes.ToArray());
            Assert.AreEqual(new[] { 1, 2, 4, 5 }, processedValues.ToArray());
        }

        [Test]
        public void ForceExecute_WithIndexAndExceptionHandler_ShouldIncrementCounterOnlyOnSuccess()
        {
            // Arrange
            var source = new[] { 1, 2, 3, 4, 5 };
            var processedIndexes = new List<int>();
            var processedValues = new List<int>();
            var exceptionIndexes = new List<int>();
            var exceptionValues = new List<int>();

            // Act
            source.ForceExecute((index, value) =>
            {
                processedIndexes.Add(index);
                processedValues.Add(value);
                if (value == 3) throw new InvalidOperationException("Test exception");
            }, (index, value, ex) =>
            {
                exceptionIndexes.Add(index);
                exceptionValues.Add(value);
            });

            // Assert
            Assert.AreEqual(new[] { 0, 1, 2, 3, 4 }, processedIndexes.ToArray());
            Assert.AreEqual(new[] { 1, 2, 3, 4, 5 }, processedValues.ToArray());
            Assert.AreEqual(new[] { 2 }, exceptionIndexes.ToArray());
            Assert.AreEqual(new[] { 3 }, exceptionValues.ToArray());
        }

        [Test]
        public void ForceSelect_WithExceptionHandler_ShouldContinueProcessingAfterException()
        {
            // Arrange
            var source = new[] { 1, 2, 3, 4, 5 };
            var exceptionItems = new List<int>();

            // Act
            var result = source.ForceSelect(x =>
            {
                if (x == 3) throw new InvalidOperationException("Test exception");
                return x * 2;
            }, (item, ex) => exceptionItems.Add(item)).ToArray();

            // Assert
            Assert.AreEqual(new[] { 2, 4, 8, 10 }, result);
            Assert.AreEqual(new[] { 3 }, exceptionItems.ToArray());
        }

        [Test]
        public void ForceSelect_WithNullSource_ShouldReturnEmpty()
        {
            // Arrange
            IEnumerable<int> source = null;

            // Act
            var result = source.ForceSelect(x => x * 2).ToArray();

            // Assert
            Assert.AreEqual(new int[0], result);
        }

        [Test]
        public void ForceExecute_WithNullSource_ShouldNotThrow()
        {
            // Arrange
            IEnumerable<int> source = null;

            // Act & Assert
            Assert.DoesNotThrow(() => source.ForceExecute(x => { }));
        }

        [Test]
        public void Execute_WithExceptionHandler_ShouldRethrowAfterHandling()
        {
            // Arrange
            var source = new[] { 1, 2, 3 };
            var handledExceptions = new List<Exception>();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
            {
                source.Execute((index, value) =>
                {
                    if (value == 2) throw new InvalidOperationException("Test exception");
                }, (index, value, ex) =>
                {
                    handledExceptions.Add(ex);
                });
            });

            Assert.AreEqual(1, handledExceptions.Count);
        }

        [Test]
        public void TryGetFirst_WithMatchingCondition_ShouldReturnTrueAndItem()
        {
            // Arrange
            var source = new[] { 1, 2, 3, 4, 5 };

            // Act
            var result = source.TryGetFirst(x => x > 3, out var first);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(4, first);
        }

        [Test]
        public void TryGetFirst_WithNullSource_ShouldReturnFalse()
        {
            // Arrange
            IEnumerable<int> source = null;

            // Act
            var result = source.TryGetFirst(x => x > 3, out var first);

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual(0, first);
        }

        [Test]
        public void GetCount_WithDifferentCollectionTypes_ShouldReturnCorrectCount()
        {
            // Arrange
            var array = new[] { 1, 2, 3 };
            var list = new List<int> { 1, 2, 3, 4 };
            var enumerable = Enumerable.Range(1, 5);

            // Act & Assert
            Assert.AreEqual(3, array.GetCount());
            Assert.AreEqual(4, list.GetCount());
            Assert.AreEqual(5, enumerable.GetCount());
        }

        [Test]
        public void Modify_WithMatchingCondition_ShouldModifyElements()
        {
            // Arrange
            var source = new[] { 1, 2, 3, 4, 5 };

            // Act
            var result = source.Modify(x => x % 2 == 0, x => x * 10);

            // Assert
            Assert.AreEqual(new[] { 1, 20, 3, 40, 5 }, result);
        }

        [Test]
        public void Modify_WithNullSource_ShouldReturnSameArray()
        {
            // Arrange
            int[] source = null;

            // Act
            var result = source.Modify(x => true, x => x * 10);

            // Assert
            Assert.IsNull(result);
        }
    }
}