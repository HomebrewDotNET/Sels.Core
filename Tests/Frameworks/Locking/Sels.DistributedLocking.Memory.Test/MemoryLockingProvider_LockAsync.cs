using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;
using NUnit.Framework.Internal;
using Sels.Core.Extensions.Conversion;
using Sels.DistributedLocking.Provider;
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
            var lockingProvider = provider.CastTo<ILockingProvider>();

            // Act
            var @lock = await lockingProvider.LockAndWaitAsync("Resource", requester);

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
            var lockingProvider = provider.CastTo<ILockingProvider>();

            // Act
            var lockOne = await lockingProvider.LockAndWaitAsync("Resource", "Requester");
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

        [Test, Timeout(60000)]
        public async Task ExpectedNumberOfRequestsAreCreatedWhenResourceIsLocked()
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock();
            await using var provider = new MemoryLockingProvider(options);
            var lockingProvider = provider.CastTo<ILockingProvider>();

            // Act
            var lockOne = await lockingProvider.LockAndWaitAsync("Resource", "Requester.0");
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

        [Test, Timeout(60000)]
        public async Task ResourceCanBeLockedAfterUnlocking()
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock();
            await using var provider = new MemoryLockingProvider(options);
            var lockingProvider = provider.CastTo<ILockingProvider>();

            // Act
            var lockOne = await lockingProvider.LockAndWaitAsync("Resource", "Requester.1");
            await lockOne.UnlockAsync();
            var lockTwo = await lockingProvider.LockAndWaitAsync("Resource", "Requester.2");

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

        [Test, Timeout(60000)]
        public async Task ResourceCanBeLockedAfterExpiring()
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock();
            await using var provider = new MemoryLockingProvider(options);
            var lockingProvider = provider.CastTo<ILockingProvider>();
            var expiryTime = 500;

            // Act
            var lockOne = await lockingProvider.LockAndWaitAsync("Resource", "Requester.1", TimeSpan.FromMilliseconds(expiryTime));
            await Helper.Async.Sleep(expiryTime * 2);
            var lockTwo = await lockingProvider.LockAndWaitAsync("Resource", "Requester.2");

            // Assert
            Assert.IsNotNull(lockOne);
            Assert.That(lockOne.LockedBy, Is.EqualTo("Requester.1"));
            Assert.IsNotNull(lockTwo);
            Assert.That(lockTwo.LockedBy, Is.EqualTo("Requester.2"));
        }

        [Test, Timeout(60000)]
        public async Task LockExpiryDateIsExtendedWhenKeepAliveIsEnabled()
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock();
            await using var provider = new MemoryLockingProvider(options);
            var lockingProvider = provider.CastTo<ILockingProvider>();
            var expiryTime = 100;

            // Act
            var @lock = await lockingProvider.LockAndWaitAsync("Resource", "Requester.1", TimeSpan.FromMilliseconds(expiryTime), true);
            var initialExpiry = @lock.ExpiryDate;
            await Helper.Async.Sleep(expiryTime * 2);
            var currentExpiry = @lock.ExpiryDate;

            // Assert
            Assert.That(initialExpiry, Is.Not.EqualTo(currentExpiry));
            Assert.That(currentExpiry, Is.GreaterThan(initialExpiry));
        }

        [Test, Timeout(60000)]
        public async Task LockRequestTakesPriorityOverDirectCalls()
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock(x => x.ExpiryOffset = 10000000);
            await using var provider = new MemoryLockingProvider(options);
            var lockingProvider = provider.CastTo<ILockingProvider>();

            // Act
            var lockOne = await lockingProvider.LockAndWaitAsync("Resource", "Requester.1");
            var lockRequest = await lockingProvider.LockAsync("Resource", "Requester.2");

            // Unlock first lock
            await lockOne.DisposeAsync();

            // Attempt to directly lock
            var lockResult = await lockingProvider.TryLockAsync("Resource", "Requester.3");

            // Assert
            Assert.IsNotNull(lockResult);
            Assert.IsFalse(lockResult.Success);
            Assert.IsNotNull(lockRequest);
            var @lock = await Helper.Async.WaitOn(lockRequest.Callback, TimeSpan.FromMinutes(1));
            Assert.IsNotNull(lockResult);
            Assert.That(@lock.Resource, Is.EqualTo("Resource"));
            Assert.That(@lock.LockedBy, Is.EqualTo("Requester.2"));
            Assert.IsNull(@lock.ExpiryDate);
        }

        [Test, Timeout(60000)]
        public async Task LockRequestIsProperlyTimedOut()
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock();
            await using var provider = new MemoryLockingProvider(options);
            var lockingProvider = provider.CastTo<ILockingProvider>();
            Exception exception = null;

            // Act
            _ = await lockingProvider.LockAndWaitAsync("Resource", "Requester.1");
            var lockResultTask = lockingProvider.LockAndWaitAsync("Resource", "Requester.2", timeout: TimeSpan.FromMilliseconds(100));
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

        [Test, Timeout(60000)]
        public async Task CancelingRequestThrowsOperationCanceledException()
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock();
            await using var provider = new MemoryLockingProvider(options);
            Exception exception = null;
            var tokenSource = new CancellationTokenSource();

            // Act
            _ = await provider.LockAsync("Resource", "Requester.1");
            var lockResultTask = await provider.LockAsync("Resource", "Requester.2", token: tokenSource.Token);
            tokenSource.Cancel();
            try
            {
                await lockResultTask.Callback;
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.That(exception, Is.Not.Null);
            Assert.That(exception, Is.InstanceOf<OperationCanceledException>());
        }

        [Test, Timeout(60000)]
        public async Task LockThatExpiresAssignsNextRequestInQueue()
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock(x => x.ExpiryOffset = 0);
            await using var provider = new MemoryLockingProvider(options);
            var lockingProvider = provider.CastTo<ILockingProvider>();

            // Act
            _ = await lockingProvider.LockAndWaitAsync("Resource", "Requester.1", TimeSpan.FromMilliseconds(100));
            var lockRequest = await lockingProvider.LockAsync("Resource", "Requester.2");
            
            // Assert
            Assert.That(lockRequest, Is.Not.Null);
            var @lock = await Helper.Async.WaitOn(lockRequest.Callback, TimeSpan.FromMinutes(1));
            Assert.IsNotNull(@lock);
            Assert.That(@lock.Resource, Is.EqualTo("Resource"));
            Assert.That(@lock.LockedBy, Is.EqualTo("Requester.2"));
        }
    }
}
