using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.DistributedLocking.Memory.Test
{
    public class MemoryLockingProvider_Class
    {
        [Test, Timeout(10000)]
        public async Task InActiveLocksAreCleanedUpWithCleanupMethodSetToAlways()
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock(x =>
            {
                x.CleanupMethod = MemoryLockCleanupMethod.Always;
                x.CleanupInterval = TimeSpan.FromMilliseconds(10);
            });
            await using var provider = new MemoryLockingProvider(options);
            for (int i = 0; i < 10; i++)
            {
                var lockResult = await provider.TryLockAsync(Guid.NewGuid().ToString(), "Me");
                await lockResult.AcquiredLock.DisposeAsync();
            }

            // Act
            await Helper.Async.Sleep(100 * 2);
            while (provider.IsRunningCleanup)
            {
                await Helper.Async.Sleep(10);
            }
            var result = await provider.QueryAsync(x => { });

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Results.Length, Is.EqualTo(0));
        }

        [Test, Timeout(10000)]
        public async Task InActiveLocksAreCleanedUpWithCleanupMethodSetToAmount()
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock(x =>
            {
                x.CleanupMethod = MemoryLockCleanupMethod.Amount;
                x.CleanupAmount = 10;
                x.CleanupInterval = TimeSpan.FromMilliseconds(10);
            });
            await using var provider = new MemoryLockingProvider(options);
            for (int i = 0; i < 20; i++)
            {
                var lockResult = await provider.TryLockAsync(Guid.NewGuid().ToString(), "Me");
                await lockResult.AcquiredLock.DisposeAsync();
            }

            // Act
            await Helper.Async.Sleep(100 * 2);
            while (provider.IsRunningCleanup)
            {
                await Helper.Async.Sleep(10);
            }
            var result = await provider.QueryAsync(x => { });

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Results.Length, Is.EqualTo(0));
        }

        [Test, Timeout(10000)]
        public async Task InActiveLocksAreCleanedUpWithCleanupMethodSetToTime()
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock(x =>
            {
                x.CleanupMethod = MemoryLockCleanupMethod.Time;
                x.CleanupAmount = 100;
                x.CleanupInterval = TimeSpan.FromMilliseconds(10);
            });
            await using var provider = new MemoryLockingProvider(options);
            for (int i = 0; i < 20; i++)
            {
                var lockResult = await provider.TryLockAsync(Guid.NewGuid().ToString(), "Me");
                await lockResult.AcquiredLock.DisposeAsync();
            }

            // Act
            await Helper.Async.Sleep(100 * 2);
            while (provider.IsRunningCleanup)
            {
                await Helper.Async.Sleep(10);
            }
            var result = await provider.QueryAsync(x => { });

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Results.Length, Is.EqualTo(0));
        }

        [Test, Timeout(10000)]
        public async Task InActiveLocksAreCleanedUpWithCleanupMethodSetToProcessMemory()
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock(x =>
            {
                x.CleanupMethod = MemoryLockCleanupMethod.ProcessMemory;
                x.CleanupAmount = 1;
                x.CleanupInterval = TimeSpan.FromMilliseconds(10);
            });
            await using var provider = new MemoryLockingProvider(options);
            for (int i = 0; i < 20; i++)
            {
                var lockResult = await provider.TryLockAsync(Guid.NewGuid().ToString(), "Me");
                await lockResult.AcquiredLock.DisposeAsync();
            }

            // Act
            await Helper.Async.Sleep(100 * 2);
            while (provider.IsRunningCleanup)
            {
                await Helper.Async.Sleep(10);
            }
            var result = await provider.QueryAsync(x => { });

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Results.Length, Is.EqualTo(0));
        }
    }
}
