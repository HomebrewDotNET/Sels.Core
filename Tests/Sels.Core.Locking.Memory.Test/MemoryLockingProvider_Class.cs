using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Locking.Memory.Test
{
    public class MemoryLockingProvider_Class
    {
        [Test]
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
                await provider.TryLockAsync(Guid.NewGuid().ToString(), "Me", out var locked);
                await locked.DisposeAsync();
            }

            // Act
            await Helper.Async.Sleep(100 * 2);
            while (provider.IsRunningCleanup)
            {
                await Helper.Async.Sleep(10);
            }
            var results = await provider.QueryAsync();

            // Assert
            Assert.That(results, Is.Not.Null);
            Assert.That(results.Length, Is.EqualTo(0));
        }

        [Test]
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
                await provider.TryLockAsync(Guid.NewGuid().ToString(), "Me", out var locked);
                await locked.DisposeAsync();
            }

            // Act
            await Helper.Async.Sleep(100 * 2);
            while (provider.IsRunningCleanup)
            {
                await Helper.Async.Sleep(10);
            }
            var results = await provider.QueryAsync();

            // Assert
            Assert.That(results, Is.Not.Null);
            Assert.That(results.Length, Is.EqualTo(0));
        }

        [Test]
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
                await provider.TryLockAsync(Guid.NewGuid().ToString(), "Me", out var locked);
                await locked.DisposeAsync();
            }

            // Act
            await Helper.Async.Sleep(100 * 2);
            while (provider.IsRunningCleanup)
            {
                await Helper.Async.Sleep(10);
            }
            var results = await provider.QueryAsync();

            // Assert
            Assert.That(results, Is.Not.Null);
            Assert.That(results.Length, Is.EqualTo(0));
        }

        [Test]
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
                await provider.TryLockAsync(Guid.NewGuid().ToString(), "Me", out var locked);
                await locked.DisposeAsync();
            }

            // Act
            await Helper.Async.Sleep(100 * 2);
            while (provider.IsRunningCleanup)
            {
                await Helper.Async.Sleep(10);
            }
            var results = await provider.QueryAsync();

            // Assert
            Assert.That(results, Is.Not.Null);
            Assert.That(results.Length, Is.EqualTo(0));
        }
    }
}
