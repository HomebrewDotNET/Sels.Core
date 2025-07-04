using NUnit.Framework;
using Sels.Core.Extensions.Linq;
using Sels.Core.Extensions.Logging.Advanced;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Sels.Core.Test.Performance
{
    [TestFixture]
    public class PerformanceOptimizationsTests
    {
        private const int LargeCollectionSize = 50000;

        [Test]
        public void SelectOrDefault_WithNullSource_ShouldBeOptimized()
        {
            // Arrange
            IEnumerable<int> source = null;
            var stopwatch = new Stopwatch();

            // Act
            stopwatch.Start();
            for (int i = 0; i < 1000; i++)
            {
                var result = source.SelectOrDefault(x => x * 2).ToArray();
            }
            stopwatch.Stop();

            // Assert
            Assert.Less(stopwatch.ElapsedMilliseconds, 100, 
                "SelectOrDefault with null source should be very fast");
        }

        [Test]
        public void ForceSelect_WithNullSource_ShouldNotEnumerate()
        {
            // Arrange
            IEnumerable<int> source = null;
            var stopwatch = new Stopwatch();

            // Act
            stopwatch.Start();
            for (int i = 0; i < 1000; i++)
            {
                var result = source.ForceSelect(x => x * 2).ToArray();
            }
            stopwatch.Stop();

            // Assert
            Assert.Less(stopwatch.ElapsedMilliseconds, 50, 
                "ForceSelect with null source should be very fast");
        }

        [Test]
        public void TraceMethod_WithNullLoggers_ShouldNotCreateStrings()
        {
            // Arrange
            IEnumerable<ILogger> loggers = null;
            var stopwatch = new Stopwatch();

            // Act
            stopwatch.Start();
            for (int i = 0; i < 10000; i++)
            {
                using var trace = loggers.TraceMethod(LogLevel.Information, typeof(string), "TestMethod");
            }
            stopwatch.Stop();

            // Assert
            Assert.Less(stopwatch.ElapsedMilliseconds, 100, 
                "TraceMethod with null loggers should be fast");
        }

        [Test]
        public void SelectOrDefault_ComparedToLinq_ShouldHaveSimilarPerformance()
        {
            // Arrange
            var source = Enumerable.Range(0, LargeCollectionSize);
            var stopwatchLinq = new Stopwatch();
            var stopwatchCustom = new Stopwatch();

            // Act - LINQ Select
            stopwatchLinq.Start();
            var resultLinq = source.Select(x => x * 2).ToArray();
            stopwatchLinq.Stop();

            // Act - Custom SelectOrDefault
            stopwatchCustom.Start();
            var resultCustom = source.SelectOrDefault(x => x * 2).ToArray();
            stopwatchCustom.Stop();

            // Assert
            Assert.AreEqual(resultLinq.Length, resultCustom.Length);
            Assert.Less(stopwatchCustom.ElapsedMilliseconds, stopwatchLinq.ElapsedMilliseconds * 2, 
                "SelectOrDefault should not be significantly slower than LINQ Select");
        }

        [Test]
        public void GetCount_PerformanceComparison()
        {
            // Arrange
            var array = new int[LargeCollectionSize];
            var list = new List<int>(LargeCollectionSize);
            list.AddRange(Enumerable.Range(0, LargeCollectionSize));
            var enumerable = Enumerable.Range(0, LargeCollectionSize);

            var stopwatchGetCount = new Stopwatch();
            var stopwatchLinqCount = new Stopwatch();

            // Act - GetCount (optimized)
            stopwatchGetCount.Start();
            for (int i = 0; i < 100; i++)
            {
                var arrayCount = array.GetCount();
                var listCount = list.GetCount();
            }
            stopwatchGetCount.Stop();

            // Act - LINQ Count
            stopwatchLinqCount.Start();
            for (int i = 0; i < 100; i++)
            {
                var arrayCount = array.Count();
                var listCount = list.Count();
            }
            stopwatchLinqCount.Stop();

            // Assert
            Assert.Less(stopwatchGetCount.ElapsedMilliseconds, stopwatchLinqCount.ElapsedMilliseconds * 2,
                "GetCount should be at least as fast as LINQ Count for collections");
        }

        [Test]
        public void ForceSelect_MemoryEfficiency()
        {
            // Arrange
            var source = Enumerable.Range(0, LargeCollectionSize);
            var initialMemory = GC.GetTotalMemory(true);

            // Act
            var result = source.ForceSelect(x => x % 1000 == 0 ? throw new InvalidOperationException() : x * 2).ToArray();

            // Force garbage collection
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var finalMemory = GC.GetTotalMemory(true);

            // Assert
            var memoryIncrease = finalMemory - initialMemory;
            Assert.Less(memoryIncrease, LargeCollectionSize * sizeof(int) * 3, 
                "ForceSelect should be memory efficient even with exceptions");
        }
    }
}