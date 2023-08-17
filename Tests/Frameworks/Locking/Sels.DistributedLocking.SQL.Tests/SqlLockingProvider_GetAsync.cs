using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;
using Sels.DistributedLocking.SQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.DistributedLocking.SQL.Test
{
    public class SqlLockingProvider_GetAsync
    {
        [Test, Timeout(10000)]
        public async Task ReturnsCorrectStateOfLock()
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock();
            var date = DateTime.Now.AddMinutes(5);
            var now = DateTime.Now;
            var resource = "Resource";
            var requester = "Me";
            var sqlLock = new SqlLock()
            {
                Resource = resource,
                LockedBy = requester,
                ExpiryDate = date,
                LastLockDate = now,
                LockedAt = now,
                PendingRequests = 0
            };
            var repositoryMock = TestHelper.GetRepositoryMock(x =>
            {
                x.Setup(x => x.TryAssignLockToAsync(It.IsAny<IRepositoryTransaction>(), resource, requester, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>())).ReturnsAsync(sqlLock);
                x.Setup(x => x.GetLockByResourceAsync(It.IsAny<IRepositoryTransaction>(), resource, true, false, It.IsAny<CancellationToken>())).ReturnsAsync(sqlLock);
            });
            
            await using var provider = new SqlLockingProvider(TestHelper.GetNotifierMock().Object, TestHelper.GetSubscriberMock().Object, repositoryMock.Object, options);

            // Act
            var lockResult = await provider.TryLockAsync(resource, requester, TimeSpan.FromMinutes(5));
            var result = await provider.GetAsync(resource);

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

        [Test, Timeout(10000)]
        public async Task NonExistantLockReturnsFreeLock()
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock();
            var repositoryMock = TestHelper.GetRepositoryMock(x =>
            {
                x.Setup(x => x.GetLockByResourceAsync(It.IsAny<IRepositoryTransaction>(), "Resource", true, false, default)).Returns(Task.FromResult<SqlLock?>(null));
            });
            await using var provider = new SqlLockingProvider(TestHelper.GetNotifierMock().Object, TestHelper.GetSubscriberMock().Object, repositoryMock.Object, options);

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
