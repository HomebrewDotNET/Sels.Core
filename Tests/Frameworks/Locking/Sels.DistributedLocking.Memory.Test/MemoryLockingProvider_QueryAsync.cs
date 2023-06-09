using Sels.Core.Extensions.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.DistributedLocking.Memory.Test
{
    public class MemoryLockingProvider_QueryAsync
    {
        [TestCase("System.", new string[] { "Resource", "System.Monitor", "System.Heartbeat", "RecurringJob.1-1" }, 2)]
        [TestCase("RecurringJob", new string[] { "Resource", "System.Monitor", "System.Heartbeat", "RecurringJob.1-1" }, 1)]
        [TestCase("Database.Deployment", new string[] { "Resource", "System.Monitor", "System.Heartbeat", "RecurringJob.1-1" }, 0)]
        public async Task FilterIsAppliedAndCorrectNumberOfResultsAreReturned(string filter, string[] locks, int expected)
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock();
            await using var provider = new MemoryLockingProvider(options);
            foreach (var @lock in locks)
            {
                await provider.TryLockAsync(@lock, "Me");
            }

            // Act
            var result = await provider.QueryAsync(filter);

            // Assert
            Assert.That(result.Results, Is.Not.Null);
            Assert.That(result.Results.Length, Is.EqualTo(expected));
            foreach (var lockResult in result.Results)
            {
                Assert.That(lockResult, Is.Not.Null);
                Assert.Contains(lockResult.Resource, locks);
            }
        }

        [Test]
        public async Task ReturnsAllLocksWhenNoFilterApplied()
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock();
            await using var provider = new MemoryLockingProvider(options);
            var locks = new string[] { "Resource", "System.Monitor", "System.Heartbeat", "RecurringJob.1-1" };
            foreach (var @lock in locks)
            {
                await provider.TryLockAsync(@lock, "Me");
            }

            // Act
            var result = await provider.QueryAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            foreach (var lockResult in result.Results)
            {
                Assert.That(lockResult, Is.Not.Null);
                Assert.Contains(lockResult.Resource, locks);
            }
        }

        [TestCase(1, 2, new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9" }, new string[] { "1", "2" })]
        [TestCase(1, 100, new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9" }, new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9" })]
        [TestCase(0, 1, new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9" }, new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9" })]
        [TestCase(2, 5, new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9" }, new string[] { "6", "7", "8", "9" })]
        [TestCase(6, 2, new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9" }, new string[] { })]
        public async Task ReturnsCorrectLocksWhenPaginationIsApplied(int page, int pageSize, string[] locks, string[] expected)
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock();
            await using var provider = new MemoryLockingProvider(options);
            foreach (var @lock in locks)
            {
                await provider.TryLockAsync(@lock, "Me");
            }

            // Act
            var result = await provider.QueryAsync(page: page, pageSize: pageSize);

            // Assert
            Assert.That(result, Is.Not.Null);
            CollectionAssert.AreEqual(expected, result.Results.Select(x => x.Resource));
        }

        [TestCase(new string[] { "1", "2", "3", "4", "5" }, new string[] { "1","2","3","4", "5" } , false)]
        [TestCase(new string[] { "1", "2", "3", "4", "5" }, new string[] { "5", "4", "3", "2", "1" }, true)]
        public async Task CorrectSortingIsAppliedWhenSortingOnResource(string[] locks, string[] expected, bool sortDescending)
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock();
            await using var provider = new MemoryLockingProvider(options);
            foreach (var @lock in locks)
            {
                await provider.TryLockAsync(@lock, Guid.NewGuid().ToString());
            }

            // Act
            var results = await provider.QueryAsync(sortBy: x => x.Resource, sortDescending: sortDescending);

            // Assert
            Assert.That(results, Is.Not.Null);
            CollectionAssert.AreEqual(expected, results.Results.Select(x => x.Resource));
        }

        [TestCase(new string?[] { "1", "2", "3", "4", "5", null }, new string?[] { null, "1", "2", "3", "4", "5" }, false)]
        [TestCase(new string?[] { "1", "2", "3", "4", "5", null }, new string?[] { "5", "4", "3", "2", "1", null }, true)]
        public async Task CorrectSortingIsAppliedWhenSortingOnLockedBy(string?[] requesters, string?[] expected, bool sortDescending)
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock();
            await using var provider = new MemoryLockingProvider(options);
            foreach (var @lock in requesters)
            {
                // Unlock lock to simulate empty LockedBy property
                if(@lock == null)
                {
                    var lockResult = await provider.TryLockAsync(Guid.NewGuid().ToString(), "Nullable");
                    await lockResult.AcquiredLock.DisposeAsync();
                }
                else
                {
                    await provider.TryLockAsync(Guid.NewGuid().ToString(), @lock);
                }
                
            }

            // Act
            var results = await provider.QueryAsync(sortBy: x => x.LockedBy, sortDescending: sortDescending);

            // Assert
            Assert.That(results, Is.Not.Null);
            CollectionAssert.AreEqual(expected, results.Results.Select(x => x.LockedBy));
        }
    }
}
