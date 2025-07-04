using NUnit.Framework;
using Sels.Core.Extensions.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Sels.Core.Test.Performance
{
    [TestFixture]
    public class LinqExtensionsPerformanceTests
    {
        private const int LargeCollectionSize = 100000;
        private const int TestIterations = 10;

        [Test]
        public void GetCount_WithLargeArray_ShouldBeOptimized()
        {
            // Arrange
            var array = new int[LargeCollectionSize];
            var stopwatch = new Stopwatch();

            // Act
            stopwatch.Start();
            for (int i = 0; i < TestIterations; i++)
            {
                var count = array.GetCount();
            }
            stopwatch.Stop();

            // Assert
            Assert.Less(stopwatch.ElapsedMilliseconds, 100, "GetCount should be optimized for arrays");
        }

        [Test]
        public void GetCount_WithLargeList_ShouldBeOptimized()
        {
            // Arrange
            var list = new List<int>(LargeCollectionSize);
            list.AddRange(Enumerable.Range(0, LargeCollectionSize));
            var stopwatch = new Stopwatch();

            // Act
            stopwatch.Start();
            for (int i = 0; i < TestIterations; i++)
            {
                var count = list.GetCount();
            }
            stopwatch.Stop();

            // Assert
            Assert.Less(stopwatch.ElapsedMilliseconds, 100, "GetCount should be optimized for collections");
        }

        [Test]
        public void ForceSelect_WithLargeCollection_ShouldNotLeakMemory()
        {
            // Arrange
            var source = Enumerable.Range(0, LargeCollectionSize);
            var initialMemory = GC.GetTotalMemory(true);

            // Act
            var result = source.ForceSelect(x => x * 2).ToArray();

            // Force garbage collection
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var finalMemory = GC.GetTotalMemory(true);

            // Assert
            Assert.AreEqual(LargeCollectionSize, result.Length);
            
            // Memory should not have increased significantly (allowing for some variance)
            var memoryIncrease = finalMemory - initialMemory;
            Assert.Less(memoryIncrease, LargeCollectionSize * sizeof(int) * 3, 
                "Memory usage should not be excessive");
        }

        [Test]
        public void ForceExecute_WithExceptions_ShouldNotDegradePerformance()
        {
            // Arrange
            var source = Enumerable.Range(0, 10000);
            var stopwatch = new Stopwatch();

            // Act
            stopwatch.Start();
            source.ForceExecute(x =>
            {
                if (x % 1000 == 0) throw new InvalidOperationException("Test exception");
            });
            stopwatch.Stop();

            // Assert
            Assert.Less(stopwatch.ElapsedMilliseconds, 1000, 
                "ForceExecute should handle exceptions efficiently");
        }

        [Test]
        public void Execute_WithIndexedAction_ShouldHaveConsistentPerformance()
        {
            // Arrange
            var source = Enumerable.Range(0, LargeCollectionSize);
            var processedItems = new List<int>();
            var stopwatch = new Stopwatch();

            // Act
            stopwatch.Start();
            source.Execute((index, value) =>
            {
                if (index % 1000 == 0) processedItems.Add(value);
            });
            stopwatch.Stop();

            // Assert
            Assert.Less(stopwatch.ElapsedMilliseconds, 1000, 
                "Execute with index should be performant");
            Assert.AreEqual(LargeCollectionSize / 1000, processedItems.Count);
        }

        [Test]
        public void SelectOrDefault_WithNullSource_ShouldBeFast()
        {
            // Arrange
            IEnumerable<int> source = null;
            var stopwatch = new Stopwatch();

            // Act
            stopwatch.Start();
            for (int i = 0; i < TestIterations; i++)
            {
                var result = source.SelectOrDefault(x => x * 2).ToArray();
            }
            stopwatch.Stop();

            // Assert
            Assert.Less(stopwatch.ElapsedMilliseconds, 10, 
                "SelectOrDefault with null source should be fast");
        }

        [Test]
        public void Modify_WithLargeArray_ShouldBeEfficient()
        {
            // Arrange
            var array = Enumerable.Range(0, LargeCollectionSize).ToArray();
            var stopwatch = new Stopwatch();

            // Act
            stopwatch.Start();
            array.Modify(x => x % 2 == 0, x => x * 2);
            stopwatch.Stop();

            // Assert
            Assert.Less(stopwatch.ElapsedMilliseconds, 500, 
                "Modify should be efficient for large arrays");
            
            // Verify that modification worked
            Assert.AreEqual(0, array[0]);
            Assert.AreEqual(2, array[1]);
            Assert.AreEqual(4, array[2]);
        }

        [Test]
        public void TryGetFirst_WithLargeCollection_ShouldStopEarly()
        {
            // Arrange
            var source = Enumerable.Range(0, LargeCollectionSize);
            var stopwatch = new Stopwatch();

            // Act
            stopwatch.Start();
            var found = source.TryGetFirst(x => x == 10, out var result);
            stopwatch.Stop();

            // Assert
            Assert.IsTrue(found);
            Assert.AreEqual(10, result);
            Assert.Less(stopwatch.ElapsedMilliseconds, 50, 
                "TryGetFirst should stop early when item is found");
        }

        [Test]
        public void ForceExecute_MemoryUsage_ShouldBeConstant()
        {
            // Arrange
            var source = Enumerable.Range(0, LargeCollectionSize);
            var initialMemory = GC.GetTotalMemory(true);

            // Act
            source.ForceExecute(x => { /* Do nothing */ });

            // Force garbage collection
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var finalMemory = GC.GetTotalMemory(true);

            // Assert
            var memoryIncrease = finalMemory - initialMemory;
            Assert.Less(memoryIncrease, 1024 * 1024, // Less than 1MB increase
                "ForceExecute should not cause significant memory increase");
        }
    }
}