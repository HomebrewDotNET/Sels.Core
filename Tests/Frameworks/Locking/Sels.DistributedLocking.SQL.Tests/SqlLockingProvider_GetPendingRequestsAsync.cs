using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;
using Sels.DistributedLocking.SQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.DistributedLocking.Memory.Test
{
    public class SqlLockingProvider_GetPendingRequestsAsync
    {
        [Test, Timeout(10000)]
        public async Task ReturnsCorrectAmountOfPendingRequests()
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock();
            var resource = "Resource";
            var sqlRequests = new SqlLockRequest[]
            {
                new SqlLockRequest()
                {
                    Id = 1,
                    Resource = resource,
                    Requester = "Requester.1",
                    ExpiryTime = TimeSpan.FromMinutes(5).TotalSeconds,
                    KeepAlive = false,
                    Timeout = null,
                    CreatedAt = DateTime.Now.AddMinutes(-2)
                },
                new SqlLockRequest()
                {
                    Id = 2,
                    Resource = resource,
                    Requester = "Requester.2",
                    ExpiryTime = null,
                    KeepAlive = true,
                    Timeout = null,
                    CreatedAt = DateTime.Now.AddMinutes(-1)
                },
                new SqlLockRequest()
                {
                    Id = 3,
                    Resource = resource,
                    Requester = "Requester.3",
                    ExpiryTime = null,
                    KeepAlive = false,
                    Timeout = DateTime.Now.AddMilliseconds(1000),
                    CreatedAt = DateTime.Now
                }
            };
            var repositoryMock = TestHelper.GetRepositoryMock(x =>
            {
                x.Setup(x => x.GetAllLockRequestsByResourceAsync(It.IsAny<IRepositoryTransaction>(), resource, It.IsAny<CancellationToken>())).ReturnsAsync(sqlRequests);
            });

            await using var provider = new SqlLockingProvider(repositoryMock.Object, options);

            // Act
            var pendingRequests = await provider.GetPendingRequestsAsync(resource);

            // Assert
            Assert.IsNotNull(pendingRequests);
            Assert.That(pendingRequests.Length, Is.EqualTo(sqlRequests.Length));
            for (int i = 0; i < pendingRequests.Length; i++)
            {
                Assert.IsNotNull(pendingRequests[i]);
                Assert.That(pendingRequests[i].Resource, Is.EqualTo(sqlRequests[i].Resource));
                Assert.That(pendingRequests[i].Requester, Is.EqualTo(sqlRequests[i].Requester));
                Assert.That(pendingRequests[i].ExpiryTime, Is.EqualTo(sqlRequests[i].ExpiryTime.HasValue ? TimeSpan.FromSeconds(sqlRequests[i].ExpiryTime.Value) : null));
                Assert.That(pendingRequests[i].Timeout, Is.EqualTo(sqlRequests[i].Timeout));
                Assert.That(pendingRequests[i].KeepAlive, Is.EqualTo(sqlRequests[i].KeepAlive));
                Assert.That(pendingRequests[i].CreatedAt, Is.EqualTo(sqlRequests[i].CreatedAt));
            }
        }

        [Test, Timeout(10000)]
        public async Task ReturnsEmptyArrayWhenThereAreNoPendingRequests()
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock();
            var resource = "Resource";
            var repositoryMock = TestHelper.GetRepositoryMock(x =>
            {
                x.Setup(x => x.GetAllLockRequestsByResourceAsync(It.IsAny<IRepositoryTransaction>(), resource, It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<SqlLockRequest>());
            });

            await using var provider = new SqlLockingProvider(repositoryMock.Object, options);

            // Act
            var pendingRequests = await provider.GetPendingRequestsAsync(resource);

            // Assert
            Assert.IsNotNull(pendingRequests);
            Assert.That(pendingRequests.Length, Is.EqualTo(0));
        }

        [Test, Timeout(10000)]
        public async Task ReturnsEmptyArrayWhenNoLocksExistForTheResource()
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock();
            var repositoryMock = TestHelper.GetRepositoryMock(x =>
            {
            });
            await using var provider = new SqlLockingProvider(repositoryMock.Object, options);

            // Act
            var pendingRequests = await provider.GetPendingRequestsAsync("NonExistant");

            // Assert
            Assert.IsNotNull(pendingRequests);
            Assert.That(pendingRequests.Length, Is.EqualTo(0));
        }
    }
}
