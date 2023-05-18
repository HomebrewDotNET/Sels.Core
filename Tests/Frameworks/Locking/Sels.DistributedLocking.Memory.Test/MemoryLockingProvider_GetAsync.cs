using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.DistributedLocking.Memory.Test
{
    public class MemoryLockingProvider_GetAsync
    {
        [Test]
        public async Task ReturnsCorrectStateOfLock()
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock();
            await using var provider = new MemoryLockingProvider(options);

            // Act
            var (wasLocked, @lock) = await provider.TryLockAsync("Resource", "Me", TimeSpan.FromMinutes(5));
            var result = await provider.GetAsync("Resource");

            // Assert
            Assert.That(@lock, Is.Not.Null);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Resource, Is.EqualTo(@lock.Resource));
            Assert.That(result.LockedBy, Is.EqualTo(@lock.LockedBy));
            Assert.That(result.LockedAt, Is.EqualTo(@lock.LockedAt));
            Assert.That(result.ExpiryDate, Is.EqualTo(@lock.ExpiryDate));
            Assert.That(result.LastLockDate, Is.EqualTo(@lock.LastLockDate));
            Assert.That(result.PendingRequests, Is.EqualTo(@lock.PendingRequests));
        }

        [Test]
        public async Task NonExistantLockReturnsFreeLock()
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock();
            await using var provider = new MemoryLockingProvider(options);

            // Act
            var result = await provider.GetAsync("Resource");

            // Assert

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Resource, Is.EqualTo("Resource"));
            Assert.That(result.LockedBy, Is.Null);
            Assert.That(result.LockedAt, Is.Null);
            Assert.That(result.ExpiryDate, Is.Null);
            Assert.That(result.LastLockDate, Is.Null);
            Assert.That(result.PendingRequests, Is.EqualTo(0));
        }
    }
}
