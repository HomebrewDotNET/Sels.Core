using Sels.Core.Extensions.Conversion;
using Sels.DistributedLocking.Provider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.DistributedLocking.Memory.Test
{
    public class MemoryLockingProvider_ForceUnlockAsync
    {
        [Test, Timeout(10000)]
        public async Task LockIsRemovedOnResource()
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock();
            await using var provider = new MemoryLockingProvider(options);

            // Act
            var lockResult = await provider.TryLockAsync("Resource", "Me", TimeSpan.FromDays(1));
            await provider.ForceUnlockAsync("Resource", false);
            var result = await provider.GetAsync("Resource");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Resource, Is.EqualTo("Resource"));
            Assert.That(result.LockedBy, Is.Null);
            Assert.That(result.LockedAt, Is.Null);
            Assert.That(result.ExpiryDate, Is.Null);
        }

        [Test, Timeout(10000)]
        public async Task LockRequestsAreRemoved()
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock();
            await using var provider = new MemoryLockingProvider(options);
            _ = await provider.LockAsync("Resource", "Requester.0");
            _ = provider.LockAsync("Resource", "Requester.1");
            _ = provider.LockAsync("Resource", "Requester.2");
            _ = provider.LockAsync("Resource", "Requester.3");

            // Act
            await provider.ForceUnlockAsync("Resource", true);
            var result = await provider.GetAsync("Resource");
            var pendingRequests = await provider.GetPendingRequestsAsync("Resource");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Resource, Is.EqualTo("Resource"));
            Assert.That(result.LockedBy, Is.Null);
            Assert.That(result.LockedAt, Is.Null);
            Assert.That(result.ExpiryDate, Is.Null);
            Assert.That(pendingRequests, Is.Empty);
        }
    }
}
