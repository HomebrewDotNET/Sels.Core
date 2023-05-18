using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;
using NuGet.Frameworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.DistributedLocking.Memory.Test
{
    public class SqlLockingProvider_TryLockAsync
    {
        [TestCase("Jens")]
        [TestCase("Thread.1")]
        [TestCase("Database.Deployer")]
        [TestCase("BackgroundJob.56")]
        [TestCase("sels.homebrewit.csautomatron")]
        public async Task NewResourceIsLockedByExpectedRequester(string requester)
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock();
            var resource = "Resource";
            var sqlLock = new SqlLock()
            {
                Resource = resource,
                LockedBy = requester,
                LockedAt = DateTimeOffset.Now,
                LastLockDate = DateTimeOffset.Now
            };
            var repositoryMock = TestHelper.GetRepositoryMock(x =>
            {
                x.Setup(x => x.TryAssignLockToAsync(It.IsAny<IRepositoryTransaction>(), resource, requester, It.IsAny<DateTimeOffset?>(), It.IsAny<CancellationToken>())).ReturnsAsync(sqlLock);
            });
            await using var provider = new SqlLockingProvider(repositoryMock.Object, options);

            // Act
            var (wasLocked, @lock) = await provider.TryLockAsync(resource, requester);

            // Assert
            Assert.IsTrue(wasLocked);
            Assert.IsNotNull(requester);
            Assert.IsNotNull(@lock);
            Assert.That(@lock.Resource, Is.EqualTo(resource));
            Assert.That(@lock.LockedBy, Is.EqualTo(requester));
            Assert.That(@lock.ExpiryDate, Is.Null);
        }

        [Test]
        public async Task ShouldNotAcquireLockTwice()
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock();
            var resource = "Resource";
            var holder = "Holder";
            var requester = "Requester";
            var sqlLock = new SqlLock()
            {
                Resource = resource,
                LockedBy = holder,
                LockedAt = DateTimeOffset.Now,
                LastLockDate = DateTimeOffset.Now
            };
            var repositoryMock = TestHelper.GetRepositoryMock(x =>
            {
                x.Setup(x => x.TryAssignLockToAsync(It.IsAny<IRepositoryTransaction>(), resource, It.IsAny<string>(), It.IsAny<DateTimeOffset?>(), It.IsAny<CancellationToken>())).ReturnsAsync(sqlLock);
            });
            await using var provider = new SqlLockingProvider(repositoryMock.Object, options);

            // Act
            var (wasLockedOne, lockOne) = await provider.TryLockAsync(resource, holder);
            var (wasLockedTwo, lockTwo) = await provider.TryLockAsync(resource, requester);

            // Assert
            Assert.IsTrue(wasLockedOne);
            Assert.IsNotNull(lockOne);
            Assert.IsFalse(wasLockedTwo);
            Assert.IsNull(lockTwo);
        }

        [Test]
        public async Task TrueIsReturnedWhenRequesterLocksResourceItAlreadyHolds()
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock();
            var resource = "Resource";
            var requester = "Requester";
            var sqlLock = new SqlLock()
            {
                Resource = resource,
                LockedBy = requester,
                LockedAt = DateTimeOffset.Now,
                LastLockDate = DateTimeOffset.Now
            };
            var repositoryMock = TestHelper.GetRepositoryMock(x =>
            {
                x.Setup(x => x.TryAssignLockToAsync(It.IsAny<IRepositoryTransaction>(), resource, requester, It.IsAny<DateTimeOffset?>(), It.IsAny<CancellationToken>())).ReturnsAsync(sqlLock);
            });
            await using var provider = new SqlLockingProvider(repositoryMock.Object, options);

            // Act
            var (wasLockedOne, lockOne) = await provider.TryLockAsync(resource, requester);
            var (wasLockedTwo, lockTwo) = await provider.TryLockAsync(resource, requester);

            // Assert
            Assert.IsTrue(wasLockedOne);
            Assert.IsNotNull(lockOne);
            Assert.IsTrue(wasLockedTwo);
            Assert.IsNotNull(lockTwo);
        }
    }
}
