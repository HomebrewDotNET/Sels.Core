using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.DistributedLocking.Memory.Test
{
    public class MemoryLockingProvider_GetPendingRequestsAsync
    {
        [Test, Timeout(60000)]
        public async Task ReturnsCorrectAmountOfPendingRequests()
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock();
            await using var provider = new MemoryLockingProvider(options);

            // Act
            _ = await provider.LockAsync("Resource", "Requester.0");
            _ = provider.LockAsync("Resource", "Requester.1");
            _ = provider.LockAsync("Resource", "Requester.2");
            _ = provider.LockAsync("Resource", "Requester.3");
            var pendingRequests = await provider.GetPendingRequestsAsync("Resource");

            // Assert
            Assert.IsNotNull(pendingRequests);
            Assert.That(pendingRequests.Length, Is.EqualTo(3));
            for (int i = 0; i < pendingRequests.Length; i++)
            {
                Assert.IsNotNull(pendingRequests[i]);
                Assert.That(pendingRequests[i].Resource, Is.EqualTo("Resource"));
                Assert.That(pendingRequests[i].Requester, Is.EqualTo($"Requester.{i + 1}"));
                Assert.That(pendingRequests[i].ExpiryTime, Is.Null);
                Assert.That(pendingRequests[i].Timeout, Is.Null);
                Assert.That(pendingRequests[i].KeepAlive, Is.False);
            }
        }

        [Test, Timeout(60000)]
        public async Task ReturnsEmptyArrayWhenThereAreNoPendingRequests()
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock();
            await using var provider = new MemoryLockingProvider(options);

            // Act
            _ = await provider.LockAsync("Resource", "Requester.0");
            var pendingRequests = await provider.GetPendingRequestsAsync("Resource");

            // Assert
            Assert.IsNotNull(pendingRequests);
            Assert.That(pendingRequests.Length, Is.EqualTo(0));
        }

        [Test, Timeout(60000)]
        public async Task ReturnsEmptyArrayWhenNoLocksExistForTheResource()
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock();
            await using var provider = new MemoryLockingProvider(options);

            // Act
            var pendingRequests = await provider.GetPendingRequestsAsync("NonExistant");

            // Assert
            Assert.IsNotNull(pendingRequests);
            Assert.That(pendingRequests.Length, Is.EqualTo(0));
        }
    }
}
