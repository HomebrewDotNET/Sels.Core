using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;
using Sels.DistributedLocking.SQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.DistributedLocking.Memory.Test
{
    public class SqlLockingProvider_GetAsync
    {
        [Test]
        public async Task ReturnsCorrectStateOfLock()
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock();
            var date = DateTimeOffset.Now.AddMinutes(5);
            var now = DateTimeOffset.Now;
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
                x.Setup(x => x.TryAssignLockToAsync(It.IsAny<IRepositoryTransaction>(), resource, requester, It.IsAny<DateTimeOffset?>(), It.IsAny<CancellationToken>())).ReturnsAsync(sqlLock);
                x.Setup(x => x.GetLockByResourceAsync(It.IsAny<IRepositoryTransaction>(), resource, true, false, It.IsAny<CancellationToken>())).ReturnsAsync(sqlLock);
            });
            
            await using var provider = new SqlLockingProvider(repositoryMock.Object, options);

            // Act
            var (wasLocked, @lock) = await provider.TryLockAsync(resource, requester, TimeSpan.FromMinutes(5));
            var result = await provider.GetAsync(resource);

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
            var repositoryMock = TestHelper.GetRepositoryMock(x =>
            {
                x.Setup(x => x.GetLockByResourceAsync(It.IsAny<IRepositoryTransaction>(), "Resource", true, false, default)).Returns(Task.FromResult<SqlLock?>(null));
            });
            await using var provider = new SqlLockingProvider(repositoryMock.Object, options);

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
