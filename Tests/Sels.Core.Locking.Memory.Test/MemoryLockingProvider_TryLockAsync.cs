using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;
using NuGet.Frameworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Locking.Memory.Test
{
    public class MemoryLockingProvider_TryLockAsync
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
            await using  var provider = new MemoryLockingProvider(options);

            // Act
            var wasLocked = await provider.TryLockAsync("Resource", requester, out var @lock);

            // Assert
            Assert.IsTrue(wasLocked);
            Assert.IsNotNull(requester);
            Assert.IsNotNull(@lock);
            Assert.That(@lock.Resource, Is.EqualTo("Resource"));
            Assert.That(@lock.LockedBy, Is.EqualTo(requester));
            Assert.That(@lock.ExpiryDate, Is.Null);
        }

        [Test]
        public async Task ShouldNotAcquireLockTwice()
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock();
            await using  var provider = new MemoryLockingProvider(options);

            // Act
            var wasLockedOne = await provider.TryLockAsync("Resource", "Requester.1", out var lockOne);
            var wasLockedTwo = await provider.TryLockAsync("Resource", "Requester.2", out var lockTwo);

            // Assert
            Assert.IsTrue(wasLockedOne);
            Assert.IsNotNull(lockOne);
            Assert.IsFalse(wasLockedTwo);
            Assert.IsNull(lockTwo);
        }

        [Test]
        public async Task OnlyOneThreadLocksResourceWhenMultipleLocksAreCalledInParallel()
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock();
            await using  var provider = new MemoryLockingProvider(options);
            var threads = 500;
            List<Task<bool>> _tasks = new List<Task<bool>>();

            // Act
            for(int i = 0; i < threads; i++)
            {
                var currentI = i;
                _tasks.Add(Task.Run(() => provider.TryLockAsync("Resource", $"Thread.{currentI}", out _)));
            }
            var results = await Task.WhenAll(_tasks);

            // Assert
            Assert.IsTrue(results.Length == threads);
            Assert.That(results.Count(x => x), Is.EqualTo(1));
        }

        [Test]
        public async Task TrueIsReturnedWhenRequesterLocksResourceItAlreadyHolds()
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock();
            await using var provider = new MemoryLockingProvider(options);

            // Act
            var wasLockedOne = await provider.TryLockAsync("Resource", "Requester.1", out var lockOne);
            var wasLockedTwo = await provider.TryLockAsync("Resource", "Requester.1", out var lockTwo);

            // Assert
            Assert.IsTrue(wasLockedOne);
            Assert.IsNotNull(lockOne);
            Assert.IsTrue(wasLockedTwo);
            Assert.IsNotNull(lockTwo);
        }

        [Test]
        public async Task ResourceCanBeLockedAfterUnlocking()
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock();
            await using  var provider = new MemoryLockingProvider(options);

            // Act
            var wasLockedOne = await provider.TryLockAsync("Resource", "Requester.1", out var lockOne);
            await lockOne.UnlockAsync();
            var wasLockedTwo = await provider.TryLockAsync("Resource", "Requester.2", out var lockTwo);

            // Assert
            Assert.IsTrue(wasLockedOne);
            Assert.IsNotNull(lockOne);
            Assert.That(lockOne.LockedBy, Is.EqualTo("Requester.1"));
            Assert.That(lockOne.Resource, Is.EqualTo("Resource"));
            Assert.That(lockOne.ExpiryDate, Is.Null);
            Assert.IsTrue(wasLockedTwo);
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
            await using  var provider = new MemoryLockingProvider(options);
            var expiryTime = 500;

            // Act
            var wasLockedOne = await provider.TryLockAsync("Resource", "Requester.1", out var lockOne, TimeSpan.FromMilliseconds(expiryTime));
            await Helper.Async.Sleep(expiryTime * 2);
            var wasLockedTwo = await provider.TryLockAsync("Resource", "Requester.2", out var lockTwo);

            // Assert
            Assert.IsTrue(wasLockedOne);
            Assert.IsNotNull(lockOne);
            Assert.That(lockOne.LockedBy, Is.EqualTo("Requester.1"));
            Assert.IsTrue(wasLockedTwo);
            Assert.IsNotNull(lockTwo);
            Assert.That(lockTwo.LockedBy, Is.EqualTo("Requester.2"));
        }

        [Test]
        public async Task LockExpiryDateIsExtendedWhenKeepAliveIsEnabled()
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock();
            await using  var provider = new MemoryLockingProvider(options);
            var expiryTime = 100;

            // Act
            _ = await provider.TryLockAsync("Resource", "Requester.1", out var @lock, TimeSpan.FromMilliseconds(expiryTime), true);
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
            var resultOne = await provider.TryLockAsync("Resource", "Requester.1", out _, TimeSpan.FromMilliseconds(100), false);
            var lockResultTask = provider.LockAsync("Resource", "Requester.2");
            await Helper.Async.Sleep(100 * 2);
            var resultTwo = await provider.TryLockAsync("Resource", "Requester.3", out _);

            // Assert
            Assert.IsTrue(resultOne);
            Assert.IsFalse(resultTwo);
            Assert.IsNotNull(lockResultTask);
            Assert.IsTrue(lockResultTask.IsCompleted);
            var lockResult = await lockResultTask;
            Assert.IsNotNull(lockResult);
            Assert.That(lockResult.Resource, Is.EqualTo("Resource"));
            Assert.That(lockResult.LockedBy, Is.EqualTo("Requester.2"));
            Assert.IsNull(lockResult.ExpiryDate);
        }
    }
}
