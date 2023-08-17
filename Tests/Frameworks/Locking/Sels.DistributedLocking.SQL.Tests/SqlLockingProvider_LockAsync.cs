using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;
using NUnit.Framework.Internal;
using Sels.Core;
using Sels.Core.Extensions.Conversion;
using Sels.DistributedLocking.Provider;
using Sels.DistributedLocking.SQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.DistributedLocking.SQL.Test
{
    public class SqlLockingProvider_LockAsync
    {
        [TestCase("Jens")]
        [TestCase("Thread.1")]
        [TestCase("Database.Deployer")]
        [TestCase("BackgroundJob.56")]
        [TestCase("sels.homebrewit.csautomatron")]
        public async Task ResourceIsLockedWhenItIsFreeByExpectedRequester(string requester)
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock();
            var resource = "Resource";
            var sqlLock = new SqlLock()
            {
                Resource = resource,
                LockedBy = requester,
                LockedAt = DateTime.Now,
                LastLockDate = DateTime.Now
            };
            var repositoryMock = TestHelper.GetRepositoryMock(x =>
            {
                x.Setup(x => x.TryAssignLockToAsync(It.IsAny<IRepositoryTransaction>(), resource, requester, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>())).ReturnsAsync(sqlLock);
            });
            await using var provider = new SqlLockingProvider(TestHelper.GetNotifierMock().Object, TestHelper.GetSubscriberMock().Object, repositoryMock.Object, options);
            var lockingProvider = provider.CastTo<ILockingProvider>();

            // Act
            var @lock = await lockingProvider.LockAndWaitAsync(resource, requester);

            // Assert
            Assert.IsNotNull(@lock);
            Assert.That(@lock.Resource, Is.EqualTo(resource));
            Assert.That(@lock.LockedBy, Is.EqualTo(requester));
            Assert.That(@lock.ExpiryDate, Is.Null);
        }

        [TestCase("Jens")]
        [TestCase("Thread.1")]
        [TestCase("Database.Deployer")]
        [TestCase("BackgroundJob.56")]
        [TestCase("sels.homebrewit.csautomatron")]
        public async Task RequestIsPlacedWhenResourceIsLocked(string requester)
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock();
            var resource = "Resource";
            var sqlLock = new SqlLock()
            {
                Resource = resource,
                LockedBy = "Requester",
                LockedAt = DateTime.Now,
                LastLockDate = DateTime.Now
            };
            var sqlRequest = new SqlLockRequest()
            {
                Id = 1,
                Resource = resource,
                Requester = requester,
                CreatedAt = DateTime.Now,
                Timeout = DateTime.Now
            };
            var repositoryMock = TestHelper.GetRepositoryMock(x =>
            {
                x.Setup(x => x.TryAssignLockToAsync(It.IsAny<IRepositoryTransaction>(), resource, requester, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>())).ReturnsAsync(sqlLock);
                x.Setup(x => x.CreateRequestAsync(It.IsAny<IRepositoryTransaction>(), It.IsAny<SqlLockRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(sqlRequest);
            });
            await using var provider = new SqlLockingProvider(TestHelper.GetNotifierMock().Object, TestHelper.GetSubscriberMock().Object, repositoryMock.Object, options);
            var lockingProvider = provider.CastTo<ILockingProvider>();

            // Act
            var lockTask = lockingProvider.LockAndWaitAsync(resource, requester);

            // Assert
            Assert.That(lockTask, Is.Not.Null);
            Assert.That(lockTask.IsCompleted, Is.False);
            repositoryMock.Verify(x => x.CreateRequestAsync(It.IsAny<IRepositoryTransaction>(), It.Is<SqlLockRequest>(x => x.Resource.Equals(resource) && x.Requester.Equals(requester)), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test, Timeout(10000)]
        [MaxTime(2000)]
        public async Task LockRequestIsProperlyTimedOut()
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock(x => x.RequestPollingRate = 50);
            var resource = "Resource";
            var holder = "Holder";
            var requester = "Requester";
            var sqlLock = new SqlLock()
            {
                Resource = resource,
                LockedBy = holder,
                LockedAt = DateTime.Now,
                LastLockDate = DateTime.Now,
                ExpiryDate = null,
            };
            var sqlRequest = new SqlLockRequest()
            {
                Id = 1,
                Resource = resource,
                Requester = requester,
                CreatedAt = DateTime.Now,
                Timeout = DateTime.Now
            };
            var repositoryMock = TestHelper.GetRepositoryMock(x =>
            {
                x.Setup(x => x.TryAssignLockToAsync(It.IsAny<IRepositoryTransaction>(), resource, requester, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>())).ReturnsAsync(sqlLock);
                x.Setup(x => x.GetLockByResourceAsync(It.IsAny<IRepositoryTransaction>(), resource, false, true, It.IsAny<CancellationToken>())).ReturnsAsync(sqlLock);
                x.Setup(x => x.CreateRequestAsync(It.IsAny<IRepositoryTransaction>(), It.IsAny<SqlLockRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(sqlRequest);
                x.Setup(x => x.DeleteAllRequestsById(It.IsAny<IRepositoryTransaction>(), It.IsAny<long[]>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            });
            await using var provider = new SqlLockingProvider(TestHelper.GetNotifierMock().Object, TestHelper.GetSubscriberMock().Object, repositoryMock.Object, options);
            var lockingProvider = provider.CastTo<ILockingProvider>();
            Exception exception = null;

            // Act
            var lockResultTask = lockingProvider.LockAndWaitAsync(resource, requester, timeout: TimeSpan.FromMilliseconds(0));
            try
            {
                await lockResultTask;
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.That(exception, Is.Not.Null);
            Assert.That(exception, Is.InstanceOf<LockTimeoutException>());
            var timeoutException = exception as LockTimeoutException;
            Assert.That(timeoutException.Lock, Is.Not.Null);
            Assert.That(timeoutException.Lock.Resource, Is.EqualTo(sqlLock.Resource));
            Assert.That(timeoutException.Lock.LockedBy, Is.EqualTo(sqlLock.LockedBy));
            Assert.That(timeoutException.Requester, Is.EqualTo(sqlRequest.Requester));
            Assert.That(timeoutException.Timeout, Is.EqualTo(TimeSpan.FromMilliseconds(0)));
            repositoryMock.Verify(x => x.DeleteAllRequestsById(It.IsAny<IRepositoryTransaction>(), It.Is<long[]>(x => x.Contains(sqlRequest.Id)), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test, Timeout(10000)]
        public async Task CancelingRequestThrowsOperationCanceledException()
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock(x => x.RequestPollingRate = 50);
            var resource = "Resource";
            var holder = "Holder";
            var requester = "Requester";
            var sqlLock = new SqlLock()
            {
                Resource = resource,
                LockedBy = holder,
                LockedAt = DateTime.Now,
                LastLockDate = DateTime.Now,
                ExpiryDate = null,
            };
            var sqlRequest = new SqlLockRequest()
            {
                Id = 1,
                Resource = resource,
                Requester = requester,
                CreatedAt = DateTime.Now,
                Timeout = null
            };
            var repositoryMock = TestHelper.GetRepositoryMock(x =>
            {
                x.Setup(x => x.TryAssignLockToAsync(It.IsAny<IRepositoryTransaction>(), resource, requester, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>())).ReturnsAsync(sqlLock);
                x.Setup(x => x.GetLockByResourceAsync(It.IsAny<IRepositoryTransaction>(), resource, false, true, It.IsAny<CancellationToken>())).ReturnsAsync(sqlLock);
                x.Setup(x => x.CreateRequestAsync(It.IsAny<IRepositoryTransaction>(), It.IsAny<SqlLockRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(sqlRequest);
                x.Setup(x => x.DeleteAllRequestsById(It.IsAny<IRepositoryTransaction>(), It.IsAny<long[]>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            });
            await using var provider = new SqlLockingProvider(TestHelper.GetNotifierMock().Object, TestHelper.GetSubscriberMock().Object, repositoryMock.Object, options, null);
            var lockingProvider = provider.CastTo<ILockingProvider>();
            Exception exception = null;
            var tokenSource = new CancellationTokenSource();

            // Act
            var lockResultTask = lockingProvider.LockAndWaitAsync(sqlRequest.Resource, sqlRequest.Requester, token: tokenSource.Token);
            tokenSource.Cancel();
            await Helper.Async.Sleep(100 * 2);
            if (lockResultTask.IsCompleted)
            {
                try
                {
                    await lockResultTask;
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
            }

            // Assert
            Assert.That(exception, Is.Not.Null);
            Assert.That(exception, Is.InstanceOf<OperationCanceledException>());
            repositoryMock.Verify(x => x.DeleteAllRequestsById(It.IsAny<IRepositoryTransaction>(), It.Is<long[]>(x => x.Contains(sqlRequest.Id)), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
