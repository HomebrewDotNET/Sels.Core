using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;
using NUnit.Framework.Internal;
using Sels.Core;
using Sels.DistributedLocking.SQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.DistributedLocking.Memory.Test
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
                LockedAt = DateTimeOffset.Now,
                LastLockDate = DateTimeOffset.Now
            };
            var repositoryMock = TestHelper.GetRepositoryMock(x =>
            {
                x.Setup(x => x.TryAssignLockToAsync(It.IsAny<IRepositoryTransaction>(), resource, requester, It.IsAny<DateTimeOffset?>(), It.IsAny<CancellationToken>())).ReturnsAsync(sqlLock);
            });
            await using var provider = new SqlLockingProvider(repositoryMock.Object, options);

            // Act
            var @lock = await provider.LockAsync(resource, requester);

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
                LockedAt = DateTimeOffset.Now,
                LastLockDate = DateTimeOffset.Now
            };
            var sqlRequest = new SqlLockRequest()
            {
                Id = 1,
                Resource = resource,
                Requester = requester,
                CreatedAt = DateTimeOffset.Now,
                Timeout = DateTimeOffset.Now
            };
            var repositoryMock = TestHelper.GetRepositoryMock(x =>
            {
                x.Setup(x => x.TryAssignLockToAsync(It.IsAny<IRepositoryTransaction>(), resource, requester, It.IsAny<DateTimeOffset?>(), It.IsAny<CancellationToken>())).ReturnsAsync(sqlLock);
                x.Setup(x => x.CreateRequestAsync(It.IsAny<IRepositoryTransaction>(), It.IsAny<SqlLockRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(sqlRequest);
            });
            await using var provider = new SqlLockingProvider(repositoryMock.Object, options);

            // Act
            var lockTask = provider.LockAsync(resource, requester);

            // Assert
            Assert.That(lockTask, Is.Not.Null);
            Assert.That(lockTask.IsCompleted, Is.False);
            repositoryMock.Verify(x => x.CreateRequestAsync(It.IsAny<IRepositoryTransaction>(), It.Is<SqlLockRequest>(x => x.Resource.Equals(resource) && x.Requester.Equals(requester)), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
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
                LockedAt = DateTimeOffset.Now,
                LastLockDate = DateTimeOffset.Now,
                ExpiryDate = null,
            };
            var sqlRequest = new SqlLockRequest()
            {
                Id = 1,
                Resource = resource,
                Requester = requester,
                CreatedAt = DateTimeOffset.Now,
                Timeout = DateTimeOffset.Now
            };
            var repositoryMock = TestHelper.GetRepositoryMock(x =>
            {
                x.Setup(x => x.TryAssignLockToAsync(It.IsAny<IRepositoryTransaction>(), resource, requester, It.IsAny<DateTimeOffset?>(), It.IsAny<CancellationToken>())).ReturnsAsync(sqlLock);
                x.Setup(x => x.GetLockByResourceAsync(It.IsAny<IRepositoryTransaction>(), resource, false, true, It.IsAny<CancellationToken>())).ReturnsAsync(sqlLock);
                x.Setup(x => x.CreateRequestAsync(It.IsAny<IRepositoryTransaction>(), It.IsAny<SqlLockRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(sqlRequest);
                x.Setup(x => x.DeleteAllRequestsById(It.IsAny<IRepositoryTransaction>(), It.IsAny<long[]>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            });
            await using var provider = new SqlLockingProvider(repositoryMock.Object, options);
            Exception exception = null;

            // Act
            var lockResultTask = provider.LockAsync(resource, requester, timeout: TimeSpan.FromMilliseconds(0));
            await Helper.Async.Sleep(250);
            if (lockResultTask.IsCompleted)
            {
                try
                {
                    await lockResultTask;
                }
                catch(Exception ex) {
                    exception = ex;
                }
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

        [Test]
        public async Task CancelingRequestThrowsTaskCanceledException()
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
                LockedAt = DateTimeOffset.Now,
                LastLockDate = DateTimeOffset.Now,
                ExpiryDate = null,
            };
            var sqlRequest = new SqlLockRequest()
            {
                Id = 1,
                Resource = resource,
                Requester = requester,
                CreatedAt = DateTimeOffset.Now,
                Timeout = null
            };
            var repositoryMock = TestHelper.GetRepositoryMock(x =>
            {
                x.Setup(x => x.TryAssignLockToAsync(It.IsAny<IRepositoryTransaction>(), resource, requester, It.IsAny<DateTimeOffset?>(), It.IsAny<CancellationToken>())).ReturnsAsync(sqlLock);
                x.Setup(x => x.GetLockByResourceAsync(It.IsAny<IRepositoryTransaction>(), resource, false, true, It.IsAny<CancellationToken>())).ReturnsAsync(sqlLock);
                x.Setup(x => x.CreateRequestAsync(It.IsAny<IRepositoryTransaction>(), It.IsAny<SqlLockRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(sqlRequest);
                x.Setup(x => x.DeleteAllRequestsById(It.IsAny<IRepositoryTransaction>(), It.IsAny<long[]>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            });
            await using var provider = new SqlLockingProvider(repositoryMock.Object, options, TestHelper.GetDebugLogger<SqlLockingProvider>());
            Exception exception = null;
            var tokenSource = new CancellationTokenSource();

            // Act
            var lockResultTask = provider.LockAsync(sqlRequest.Resource, sqlRequest.Requester, token: tokenSource.Token);
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
            Assert.That(exception, Is.InstanceOf<TaskCanceledException>());
            repositoryMock.Verify(x => x.DeleteAllRequestsById(It.IsAny<IRepositoryTransaction>(), It.Is<long[]>(x => x.Contains(sqlRequest.Id)), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
