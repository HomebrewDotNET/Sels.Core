using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;
using NUnit.Framework.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.DistributedLocking.Memory.Test
{
    public class MemoryLockingProvider_LockAsync
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
            await using var provider = new MemoryLockingProvider(options);

            // Act
            var @lock = await provider.LockAsync("Resource", requester);

            // Assert
            Assert.IsNotNull(@lock);
            Assert.That(@lock.Resource, Is.EqualTo("Resource"));
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
            await using var provider = new MemoryLockingProvider(options);

            // Act
            var lockOne = await provider.LockAsync("Resource", "Requester");
            var lockTask = provider.LockAsync("Resource", requester);
            var pendingRequests = await provider.GetPendingRequestsAsync("Resource");

            // Assert
            Assert.IsNotNull(lockOne);
            Assert.That(lockOne.Resource, Is.EqualTo("Resource"));
            Assert.That(lockOne.LockedBy, Is.EqualTo("Requester"));
            Assert.That(lockOne.ExpiryDate, Is.Null);
            Assert.IsNotNull(lockTask);
            Assert.IsNotNull(pendingRequests);
            Assert.That(pendingRequests.Length, Is.EqualTo(1));
            var request = pendingRequests[0];
            Assert.IsNotNull(request);
            Assert.That(request.Resource, Is.EqualTo("Resource"));
            Assert.That(request.Requester, Is.EqualTo(requester));
            Assert.That(request.ExpiryTime, Is.Null);
            Assert.That(request.Timeout, Is.Null);
            Assert.That(request.KeepAlive, Is.False);
        }

        [Test]
        public async Task ExpectedNumberOfRequestsAreCreatedWhenResourceIsLocked()
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock();
            await using var provider = new MemoryLockingProvider(options);

            // Act
            var lockOne = await provider.LockAsync("Resource", "Requester.0");
            _ = provider.LockAsync("Resource", "Requester.1");
            _ = provider.LockAsync("Resource", "Requester.2");
            _ = provider.LockAsync("Resource", "Requester.3");
            var pendingRequests = await provider.GetPendingRequestsAsync("Resource");

            // Assert
            Assert.IsNotNull(lockOne);
            Assert.That(lockOne.Resource, Is.EqualTo("Resource"));
            Assert.That(lockOne.LockedBy, Is.EqualTo("Requester.0"));
            Assert.That(lockOne.ExpiryDate, Is.Null);
            Assert.IsNotNull(pendingRequests);
            Assert.That(pendingRequests.Length, Is.EqualTo(3));
            for(int i =  0; i < pendingRequests.Length; i++)
            {
                Assert.IsNotNull(pendingRequests[i]);
                Assert.That(pendingRequests[i].Resource, Is.EqualTo("Resource"));
                Assert.That(pendingRequests[i].Requester, Is.EqualTo($"Requester.{i+1}"));
                Assert.That(pendingRequests[i].ExpiryTime, Is.Null);
                Assert.That(pendingRequests[i].Timeout, Is.Null);
                Assert.That(pendingRequests[i].KeepAlive, Is.False);
            }
        }

        [Test]
        public async Task ResourceCanBeLockedAfterUnlocking()
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock();
            await using var provider = new MemoryLockingProvider(options);

            // Act
            var lockOne = await provider.LockAsync("Resource", "Requester.1");
            await lockOne.UnlockAsync();
            var lockTwo = await provider.LockAsync("Resource", "Requester.2");

            // Assert
            Assert.IsNotNull(lockOne);
            Assert.That(lockOne.LockedBy, Is.EqualTo("Requester.1"));
            Assert.That(lockOne.Resource, Is.EqualTo("Resource"));
            Assert.That(lockOne.ExpiryDate, Is.Null);
            Assert.IsNotNull(lockTwo);
            Assert.That(lockTwo.LockedBy, Is.EqualTo("Requester.2"));
            Assert.That(lockTwo.Resource, Is.EqualTo("Resource"));
            Assert.That(lockTwo.ExpiryDate, Is.Null);
        }

        [Test]
        public async Task ResourceCanBeLockedAfterExpiring()
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock();
            await using var provider = new MemoryLockingProvider(options);
            var expiryTime = 500;

            // Act
            var lockOne = await provider.LockAsync("Resource", "Requester.1", TimeSpan.FromMilliseconds(expiryTime));
            await Helper.Async.Sleep(expiryTime * 2);
            var lockTwo = await provider.LockAsync("Resource", "Requester.2");

            // Assert
            Assert.IsNotNull(lockOne);
            Assert.That(lockOne.LockedBy, Is.EqualTo("Requester.1"));
            Assert.IsNotNull(lockTwo);
            Assert.That(lockTwo.LockedBy, Is.EqualTo("Requester.2"));
        }

        [Test]
        public async Task LockExpiryDateIsExtendedWhenKeepAliveIsEnabled()
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock();
            await using var provider = new MemoryLockingProvider(options);
            var expiryTime = 100;

            // Act
            var @lock = await provider.LockAsync("Resource", "Requester.1", TimeSpan.FromMilliseconds(expiryTime), true);
            var initialExpiry = @lock.ExpiryDate;
            await Helper.Async.Sleep(expiryTime * 2);
            var currentExpiry = @lock.ExpiryDate;

            // Assert
            Assert.That(initialExpiry, Is.Not.EqualTo(currentExpiry));
            Assert.That(currentExpiry, Is.GreaterThan(initialExpiry));
        }

        [Test]
        public async Task LockRequestTakesPriorityOverDirectCalls()
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock(x => x.ExpiryOffset = 10000000);
            await using var provider = new MemoryLockingProvider(options);

            // Act
            var lockOne = await provider.LockAsync("Resource", "Requester.1", TimeSpan.FromMilliseconds(100), false);
            var lockResultTask = provider.LockAsync("Resource", "Requester.2");
            await Helper.Async.Sleep(100 * 2);
            var lockResultTaskTwo = provider.LockAsync("Resource", "Requester.3");

            // Assert
            Assert.IsNotNull(lockResultTask);
            Assert.IsTrue(lockResultTask.IsCompleted);
            var lockResult = await lockResultTask;
            Assert.IsNotNull(lockResult);
            Assert.That(lockResult.Resource, Is.EqualTo("Resource"));
            Assert.That(lockResult.LockedBy, Is.EqualTo("Requester.2"));
            Assert.IsNull(lockResult.ExpiryDate);
            Assert.IsNotNull(lockResultTaskTwo);
            Assert.IsFalse(lockResultTaskTwo.IsCompleted);
        }

        [Test]
        public async Task LockRequestIsProperlyTimedOut()
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock();
            await using var provider = new MemoryLockingProvider(options);
            Exception exception = null;

            // Act
            _ = await provider.LockAsync("Resource", "Requester.1");
            var lockResultTask = provider.LockAsync("Resource", "Requester.2", timeout: TimeSpan.FromMilliseconds(100));
            await Helper.Async.Sleep(100 * 2);
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
            Assert.That(timeoutException.Lock.Resource, Is.EqualTo("Resource"));
            Assert.That(timeoutException.Lock.LockedBy, Is.EqualTo("Requester.1"));
            Assert.That(timeoutException.Requester, Is.EqualTo("Requester.2"));
            Assert.That(timeoutException.Timeout, Is.EqualTo(TimeSpan.FromMilliseconds(100)));
        }

        [Test]
        public async Task CancelingRequestThrowsTaskCanceledException()
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock();
            await using var provider = new MemoryLockingProvider(options);
            Exception exception = null;
            var tokenSource = new CancellationTokenSource();

            // Act
            _ = await provider.LockAsync("Resource", "Requester.1");
            var lockResultTask = provider.LockAsync("Resource", "Requester.2", token: tokenSource.Token);
            tokenSource.Cancel();
            await Helper.Async.Sleep(100);
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
        }

        [Test]
        public async Task LockThatExpiresAssignsNextRequestInQueue()
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock(x => x.ExpiryOffset = 0);
            await using var provider = new MemoryLockingProvider(options);

            // Act
            _ = await provider.LockAsync("Resource", "Requester.1", TimeSpan.FromMilliseconds(100));
            var lockResultTask = provider.LockAsync("Resource", "Requester.2");
            await Helper.Async.Sleep(100 * 2);

            // Assert
            Assert.That(lockResultTask, Is.Not.Null);
            Assert.That(lockResultTask.IsCompleted, Is.True);
            var @lock = await lockResultTask;
            Assert.IsNotNull(@lock);
            Assert.That(@lock.Resource, Is.EqualTo("Resource"));
            Assert.That(@lock.LockedBy, Is.EqualTo("Requester.2"));
        }
    }
}
