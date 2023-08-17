using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.DistributedLocking.Memory.Test
{
    public class MemoryLockingProvider_GetAsync
    {
        [Test, Timeout(60000)]
        public async Task ReturnsCorrectStateOfLock()
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock();
            await using var provider = new MemoryLockingProvider(options);

            // Act
            var lockResult = await provider.TryLockAsync("Resource", "Me", TimeSpan.FromMinutes(5));
            var result = await provider.GetAsync("Resource");

            // Assert
            Assert.That(lockResult, Is.Not.Null);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Resource, Is.EqualTo(lockResult.AcquiredLock.Resource));
            Assert.That(result.LockedBy, Is.EqualTo(lockResult.AcquiredLock.LockedBy));
            Assert.That(result.LockedAt, Is.EqualTo(lockResult.AcquiredLock.LockedAt));
            Assert.That(result.ExpiryDate, Is.EqualTo(lockResult.AcquiredLock.ExpiryDate));
            Assert.That(result.LastLockDate, Is.EqualTo(lockResult.AcquiredLock.LastLockDate));
            Assert.That(result.PendingRequests, Is.EqualTo(lockResult.AcquiredLock.PendingRequests));
        }

        [Test, Timeout(60000)]
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
