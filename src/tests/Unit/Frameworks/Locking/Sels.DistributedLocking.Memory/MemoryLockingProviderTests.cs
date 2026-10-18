using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Linq;
using Sels.DistributedLocking.Provider;
using System.Text;

namespace Sels.DistributedLocking.Memory.Tests
{
    /// <summary>Contract tests for <see cref="MemoryLockingProvider"/>.</summary>
    [Trait("Category", "Unit")]
    [Collection(DistributedLockingMemoryTestCollection.Name)]
    public class MemoryLockingProviderTests
    {
        /// <summary>Verifies Cleanup for method always: removes inactive locks.</summary>
        [Fact(Timeout = DistributedLockingMemoryTestConstants.TestTimeoutMilliseconds)]
        public async Task Cleanup_MethodAlways_RemovesInactiveLocks()
        {
            // Arrange
            var options = TestHelper.CreateProviderOptions(x =>
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
            Assert.NotNull(result);
            Assert.Equal(0, result.Results.Length);
        }

        /// <summary>Verifies Cleanup for method amount: removes inactive locks.</summary>
        [Fact(Timeout = DistributedLockingMemoryTestConstants.TestTimeoutMilliseconds)]
        public async Task Cleanup_MethodAmount_RemovesInactiveLocks()
        {
            // Arrange
            var options = TestHelper.CreateProviderOptions(x =>
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
            Assert.NotNull(result);
            Assert.Equal(0, result.Results.Length);
        }

        /// <summary>Verifies Cleanup for method time: removes inactive locks.</summary>
        [Fact(Timeout = DistributedLockingMemoryTestConstants.TestTimeoutMilliseconds)]
        public async Task Cleanup_MethodTime_RemovesInactiveLocks()
        {
            // Arrange
            var options = TestHelper.CreateProviderOptions(x =>
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
            Assert.NotNull(result);
            Assert.Equal(0, result.Results.Length);
        }

        /// <summary>Verifies Cleanup for method process memory: removes inactive locks.</summary>
        [Fact(Timeout = DistributedLockingMemoryTestConstants.TestTimeoutMilliseconds)]
        public async Task Cleanup_MethodProcessMemory_RemovesInactiveLocks()
        {
            // Arrange
            var options = TestHelper.CreateProviderOptions(x =>
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
            Assert.NotNull(result);
            Assert.Equal(0, result.Results.Length);
        }

        /// <summary>Verifies ForceUnlockAsync for locked resource: removes lock.</summary>
        [Fact(Timeout = DistributedLockingMemoryTestConstants.TestTimeoutMilliseconds)]
        public async Task ForceUnlockAsync_LockedResource_RemovesLock()
        {
            // Arrange
            var options = TestHelper.CreateProviderOptions();
            await using var provider = new MemoryLockingProvider(options);

            // Act
            var lockResult = await provider.TryLockAsync("Resource", "Me", TimeSpan.FromDays(1));
            await provider.ForceUnlockAsync("Resource", false);
            var result = await provider.GetAsync("Resource");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Resource", result.Resource);
            Assert.Null(result.LockedBy);
            Assert.Null(result.LockedAt);
            Assert.Null(result.ExpiryDate);
        }

        /// <summary>Verifies ForceUnlockAsync for pending requests: removes requests.</summary>
        [Fact(Timeout = DistributedLockingMemoryTestConstants.TestTimeoutMilliseconds)]
        public async Task ForceUnlockAsync_PendingRequests_RemovesRequests()
        {
            // Arrange
            var options = TestHelper.CreateProviderOptions();
            await using var provider = new MemoryLockingProvider(options);
            _ = await provider.LockAsync("Resource", "Requester.0");
            _ = provider.LockAsync("Resource", "Requester.1");
            _ = provider.LockAsync("Resource", "Requester.2");
            _ = provider.LockAsync("Resource", "Requester.3");

            // Act
            await provider.ForceUnlockAsync("Resource", true);
            var result = await provider.GetAsync("Resource");
            var pendingRequests = await provider.GetPendingRequestsAsync("Resource");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Resource", result.Resource);
            Assert.Null(result.LockedBy);
            Assert.Null(result.LockedAt);
            Assert.Null(result.ExpiryDate);
            Assert.Empty(pendingRequests);
        }

        /// <summary>Verifies GetAsync for locked resource: returns lock state.</summary>
        [Fact(Timeout = DistributedLockingMemoryTestConstants.TestTimeoutMilliseconds)]
        public async Task GetAsync_LockedResource_ReturnsLockState()
        {
            // Arrange
            var options = TestHelper.CreateProviderOptions();
            await using var provider = new MemoryLockingProvider(options);

            // Act
            var lockResult = await provider.TryLockAsync("Resource", "Me", TimeSpan.FromMinutes(5));
            var result = await provider.GetAsync("Resource");

            // Assert
            Assert.NotNull(lockResult);
            Assert.NotNull(result);
            Assert.Equal(lockResult.AcquiredLock.Resource, result.Resource);
            Assert.Equal(lockResult.AcquiredLock.LockedBy, result.LockedBy);
            Assert.Equal(lockResult.AcquiredLock.LockedAt, result.LockedAt);
            Assert.Equal(lockResult.AcquiredLock.ExpiryDate, result.ExpiryDate);
            Assert.Equal(lockResult.AcquiredLock.LastLockDate, result.LastLockDate);
            Assert.Equal(lockResult.AcquiredLock.PendingRequests, result.PendingRequests);
        }

        /// <summary>Verifies GetAsync for unknown resource: returns free lock.</summary>
        [Fact(Timeout = DistributedLockingMemoryTestConstants.TestTimeoutMilliseconds)]
        public async Task GetAsync_UnknownResource_ReturnsFreeLock()
        {
            // Arrange
            var options = TestHelper.CreateProviderOptions();
            await using var provider = new MemoryLockingProvider(options);

            // Act
            var result = await provider.GetAsync("Resource");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Resource", result.Resource);
            Assert.Null(result.LockedBy);
            Assert.Null(result.LockedAt);
            Assert.Null(result.ExpiryDate);
            Assert.Null(result.LastLockDate);
            Assert.Equal(0, result.PendingRequests);
        }

        /// <summary>Verifies GetPendingRequestsAsync for pending requests: returns all requests.</summary>
        [Fact(Timeout = DistributedLockingMemoryTestConstants.TestTimeoutMilliseconds)]
        public async Task GetPendingRequestsAsync_PendingRequests_ReturnsAllRequests()
        {
            // Arrange
            var options = TestHelper.CreateProviderOptions();
            await using var provider = new MemoryLockingProvider(options);

            // Act
            _ = await provider.LockAsync("Resource", "Requester.0");
            _ = provider.LockAsync("Resource", "Requester.1");
            _ = provider.LockAsync("Resource", "Requester.2");
            _ = provider.LockAsync("Resource", "Requester.3");
            var pendingRequests = await provider.GetPendingRequestsAsync("Resource");

            // Assert
            Assert.NotNull(pendingRequests);
            Assert.Equal(3, pendingRequests.Length);
            for (int i = 0; i < pendingRequests.Length; i++)
            {
                Assert.NotNull(pendingRequests[i]);
                Assert.Equal("Resource", pendingRequests[i].Resource);
                Assert.Equal($"Requester.{i + 1}", pendingRequests[i].Requester);
                Assert.Null(pendingRequests[i].ExpiryTime);
                Assert.Null(pendingRequests[i].Timeout);
                Assert.False(pendingRequests[i].KeepAlive);
            }
        }

        /// <summary>Verifies GetPendingRequestsAsync for no pending requests: returns empty array.</summary>
        [Fact(Timeout = DistributedLockingMemoryTestConstants.TestTimeoutMilliseconds)]
        public async Task GetPendingRequestsAsync_NoPendingRequests_ReturnsEmptyArray()
        {
            // Arrange
            var options = TestHelper.CreateProviderOptions();
            await using var provider = new MemoryLockingProvider(options);

            // Act
            _ = await provider.LockAsync("Resource", "Requester.0");
            var pendingRequests = await provider.GetPendingRequestsAsync("Resource");

            // Assert
            Assert.NotNull(pendingRequests);
            Assert.Equal(0, pendingRequests.Length);
        }

        /// <summary>Verifies GetPendingRequestsAsync for unknown resource: returns empty array.</summary>
        [Fact(Timeout = DistributedLockingMemoryTestConstants.TestTimeoutMilliseconds)]
        public async Task GetPendingRequestsAsync_UnknownResource_ReturnsEmptyArray()
        {
            // Arrange
            var options = TestHelper.CreateProviderOptions();
            await using var provider = new MemoryLockingProvider(options);

            // Act
            var pendingRequests = await provider.GetPendingRequestsAsync("NonExistant");

            // Assert
            Assert.NotNull(pendingRequests);
            Assert.Equal(0, pendingRequests.Length);
        }

        /// <summary>Verifies LockAsync for free resource: locks for requester.</summary>
        [Theory(Timeout = DistributedLockingMemoryTestConstants.TestTimeoutMilliseconds)]
        [InlineData("Jens")]
        [InlineData("Thread.1")]
        [InlineData("Database.Deployer")]
        [InlineData("BackgroundJob.56")]
        [InlineData("sels.homebrewit.csautomatron")]
        public async Task LockAsync_FreeResource_LocksForRequester(string requester)
        {
            // Arrange
            var options = TestHelper.CreateProviderOptions();
            await using var provider = new MemoryLockingProvider(options);
            var lockingProvider = provider.CastTo<ILockingProvider>();

            // Act
            var @lock = await lockingProvider.LockAndWaitAsync("Resource", requester);

            // Assert
            Assert.NotNull(@lock);
            Assert.Equal("Resource", @lock.Resource);
            Assert.Equal(requester, @lock.LockedBy);
            Assert.Null(@lock.ExpiryDate);
        }

        /// <summary>Verifies LockAsync for locked resource: places request.</summary>
        [Theory(Timeout = DistributedLockingMemoryTestConstants.TestTimeoutMilliseconds)]
        [InlineData("Jens")]
        [InlineData("Thread.1")]
        [InlineData("Database.Deployer")]
        [InlineData("BackgroundJob.56")]
        [InlineData("sels.homebrewit.csautomatron")]
        public async Task LockAsync_LockedResource_PlacesRequest(string requester)
        {
            // Arrange
            var options = TestHelper.CreateProviderOptions();
            await using var provider = new MemoryLockingProvider(options);
            var lockingProvider = provider.CastTo<ILockingProvider>();

            // Act
            var lockOne = await lockingProvider.LockAndWaitAsync("Resource", "Requester");
            var lockTask = provider.LockAsync("Resource", requester);
            var pendingRequests = await provider.GetPendingRequestsAsync("Resource");

            // Assert
            Assert.NotNull(lockOne);
            Assert.Equal("Resource", lockOne.Resource);
            Assert.Equal("Requester", lockOne.LockedBy);
            Assert.Null(lockOne.ExpiryDate);
            Assert.NotNull(lockTask);
            Assert.NotNull(pendingRequests);
            Assert.Equal(1, pendingRequests.Length);
            var request = pendingRequests[0];
            Assert.NotNull(request);
            Assert.Equal("Resource", request.Resource);
            Assert.Equal(requester, request.Requester);
            Assert.Null(request.ExpiryTime);
            Assert.Null(request.Timeout);
            Assert.False(request.KeepAlive);
        }

        /// <summary>Verifies LockAsync for locked resource multiple callers: creates request per caller.</summary>
        [Fact(Timeout = DistributedLockingMemoryTestConstants.TestTimeoutMilliseconds)]
        public async Task LockAsync_LockedResourceMultipleCallers_CreatesRequestPerCaller()
        {
            // Arrange
            var options = TestHelper.CreateProviderOptions();
            await using var provider = new MemoryLockingProvider(options);
            var lockingProvider = provider.CastTo<ILockingProvider>();

            // Act
            var lockOne = await lockingProvider.LockAndWaitAsync("Resource", "Requester.0");
            _ = provider.LockAsync("Resource", "Requester.1");
            _ = provider.LockAsync("Resource", "Requester.2");
            _ = provider.LockAsync("Resource", "Requester.3");
            var pendingRequests = await provider.GetPendingRequestsAsync("Resource");

            // Assert
            Assert.NotNull(lockOne);
            Assert.Equal("Resource", lockOne.Resource);
            Assert.Equal("Requester.0", lockOne.LockedBy);
            Assert.Null(lockOne.ExpiryDate);
            Assert.NotNull(pendingRequests);
            Assert.Equal(3, pendingRequests.Length);
            for(int i =  0; i < pendingRequests.Length; i++)
            {
                Assert.NotNull(pendingRequests[i]);
                Assert.Equal("Resource", pendingRequests[i].Resource);
                Assert.Equal($"Requester.{i+1}", pendingRequests[i].Requester);
                Assert.Null(pendingRequests[i].ExpiryTime);
                Assert.Null(pendingRequests[i].Timeout);
                Assert.False(pendingRequests[i].KeepAlive);
            }
        }

        /// <summary>Verifies LockAsync for after unlock: locks resource.</summary>
        [Fact(Timeout = DistributedLockingMemoryTestConstants.TestTimeoutMilliseconds)]
        public async Task LockAsync_AfterUnlock_LocksResource()
        {
            // Arrange
            var options = TestHelper.CreateProviderOptions();
            await using var provider = new MemoryLockingProvider(options);
            var lockingProvider = provider.CastTo<ILockingProvider>();

            // Act
            var lockOne = await lockingProvider.LockAndWaitAsync("Resource", "Requester.1");
            await lockOne.UnlockAsync();
            var lockTwo = await lockingProvider.LockAndWaitAsync("Resource", "Requester.2");

            // Assert
            Assert.NotNull(lockOne);
            Assert.Equal("Requester.1", lockOne.LockedBy);
            Assert.Equal("Resource", lockOne.Resource);
            Assert.Null(lockOne.ExpiryDate);
            Assert.NotNull(lockTwo);
            Assert.Equal("Requester.2", lockTwo.LockedBy);
            Assert.Equal("Resource", lockTwo.Resource);
            Assert.Null(lockTwo.ExpiryDate);
        }

        /// <summary>Verifies LockAsync for after expiry: locks resource.</summary>
        [Fact(Timeout = DistributedLockingMemoryTestConstants.TestTimeoutMilliseconds)]
        public async Task LockAsync_AfterExpiry_LocksResource()
        {
            // Arrange
            var options = TestHelper.CreateProviderOptions();
            await using var provider = new MemoryLockingProvider(options);
            var lockingProvider = provider.CastTo<ILockingProvider>();
            var expiryTime = 500;

            // Act
            var lockOne = await lockingProvider.LockAndWaitAsync("Resource", "Requester.1", TimeSpan.FromMilliseconds(expiryTime));
            await Helper.Async.Sleep(expiryTime * 2);
            var lockTwo = await lockingProvider.LockAndWaitAsync("Resource", "Requester.2");

            // Assert
            Assert.NotNull(lockOne);
            Assert.Equal("Requester.1", lockOne.LockedBy);
            Assert.NotNull(lockTwo);
            Assert.Equal("Requester.2", lockTwo.LockedBy);
        }

        /// <summary>Verifies LockAsync for keep alive enabled: extends expiry date.</summary>
        [Fact(Timeout = DistributedLockingMemoryTestConstants.TestTimeoutMilliseconds)]
        public async Task LockAsync_KeepAliveEnabled_ExtendsExpiryDate()
        {
            // Arrange
            var options = TestHelper.CreateProviderOptions();
            await using var provider = new MemoryLockingProvider(options);
            var lockingProvider = provider.CastTo<ILockingProvider>();
            var expiryTime = 100;

            // Act
            var @lock = await lockingProvider.LockAndWaitAsync("Resource", "Requester.1", TimeSpan.FromMilliseconds(expiryTime), true);
            var initialExpiry = @lock.ExpiryDate;
            await Helper.Async.Sleep(expiryTime * 2);
            var currentExpiry = @lock.ExpiryDate;

            // Assert
            Assert.NotEqual(currentExpiry, initialExpiry);
            Assert.True(currentExpiry > initialExpiry);
        }

        /// <summary>Verifies LockAsync for pending request: takes priority over direct calls.</summary>
        [Fact(Timeout = DistributedLockingMemoryTestConstants.TestTimeoutMilliseconds)]
        public async Task LockAsync_PendingRequest_TakesPriorityOverDirectCalls()
        {
            // Arrange
            var options = TestHelper.CreateProviderOptions(x => x.ExpiryOffset = 10000000);
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
            Assert.NotNull(lockResult);
            Assert.False(lockResult.Success);
            Assert.NotNull(lockRequest);
            var @lock = await Helper.Async.WaitOn(lockRequest.Callback, TimeSpan.FromMinutes(1));
            Assert.NotNull(lockResult);
            Assert.Equal("Resource", @lock.Resource);
            Assert.Equal("Requester.2", @lock.LockedBy);
            Assert.Null(@lock.ExpiryDate);
        }

        /// <summary>Verifies LockAsync for request timeout elapsed: times out request.</summary>
        [Fact(Timeout = DistributedLockingMemoryTestConstants.TestTimeoutMilliseconds)]
        public async Task LockAsync_RequestTimeoutElapsed_TimesOutRequest()
        {
            // Arrange
            var options = TestHelper.CreateProviderOptions();
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
            Assert.NotNull(exception);
            Assert.IsAssignableFrom<LockTimeoutException>(exception);
            var timeoutException = exception as LockTimeoutException;
            Assert.NotNull(timeoutException);
            Assert.Equal("Resource", timeoutException.Resource);
            Assert.Equal("Requester.2", timeoutException.Requester);
            Assert.Equal(TimeSpan.FromMilliseconds(100), timeoutException.Timeout);
        }

        /// <summary>Verifies LockAsync for token cancelled: throws operation canceled exception.</summary>
        [Fact(Timeout = DistributedLockingMemoryTestConstants.TestTimeoutMilliseconds)]
        public async Task LockAsync_TokenCancelled_ThrowsOperationCanceledException()
        {
            // Arrange
            var options = TestHelper.CreateProviderOptions();
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
            Assert.NotNull(exception);
            Assert.IsAssignableFrom<OperationCanceledException>(exception);
        }

        /// <summary>Verifies LockAsync for lock expires: assigns next request.</summary>
        [Fact(Timeout = DistributedLockingMemoryTestConstants.TestTimeoutMilliseconds)]
        public async Task LockAsync_LockExpires_AssignsNextRequest()
        {
            // Arrange
            var options = TestHelper.CreateProviderOptions(x => x.ExpiryOffset = 0);
            await using var provider = new MemoryLockingProvider(options);
            var lockingProvider = provider.CastTo<ILockingProvider>();

            // Act
            _ = await lockingProvider.LockAndWaitAsync("Resource", "Requester.1", TimeSpan.FromMilliseconds(100));
            var lockRequest = await lockingProvider.LockAsync("Resource", "Requester.2");
            
            // Assert
            Assert.NotNull(lockRequest);
            var @lock = await Helper.Async.WaitOn(lockRequest.Callback, TimeSpan.FromMinutes(1));
            Assert.NotNull(@lock);
            Assert.Equal("Resource", @lock.Resource);
            Assert.Equal("Requester.2", @lock.LockedBy);
        }

        /// <summary>Verifies QueryAsync for filter on resource: returns matching locks.</summary>
        [Theory(Timeout = DistributedLockingMemoryTestConstants.TestTimeoutMilliseconds)]
        [InlineData("System.", new string[] { "Resource", "System.Monitor", "System.Heartbeat", "RecurringJob.1-1" }, 2)]
        [InlineData("RecurringJob", new string[] { "Resource", "System.Monitor", "System.Heartbeat", "RecurringJob.1-1" }, 1)]
        [InlineData("Database.Deployment", new string[] { "Resource", "System.Monitor", "System.Heartbeat", "RecurringJob.1-1" }, 0)]
        public async Task QueryAsync_FilterOnResource_ReturnsMatchingLocks(string filter, string[] locks, int expected)
        {
            // Arrange
            var options = TestHelper.CreateProviderOptions();
            await using var provider = new MemoryLockingProvider(options);
            foreach (var @lock in locks)
            {
                await provider.TryLockAsync(@lock, "Me");
            }

            // Act
            var result = await provider.QueryAsync(x => x.WithFilterOnResource(filter));

            // Assert
            Assert.NotNull(result.Results);
            Assert.Equal(expected, result.Results.Length);
            foreach (var lockResult in result.Results)
            {
                Assert.NotNull(lockResult);
                Assert.Contains(lockResult.Resource, locks);
            }
        }

        /// <summary>Verifies QueryAsync for filter on locked by: returns matching locks.</summary>
        [Theory(Timeout = DistributedLockingMemoryTestConstants.TestTimeoutMilliseconds)]
        [InlineData("Thread", new string[] { "Thread.1", "Thread.2", "BackgroundJob.1", "BackgroundJob.2", "RecurringJob.1" }, 2)]
        [InlineData("Thread.2", new string[] { "Thread.1", "Thread.2", "BackgroundJob.1", "BackgroundJob.2", "RecurringJob.1" }, 1)]
        [InlineData("Thread.3", new string[] { "Thread.1", "Thread.2", "BackgroundJob.1", "BackgroundJob.2", "RecurringJob.1" }, 0)]
        [InlineData("RecurringJob", new string[] { "Thread.1", "Thread.2", "BackgroundJob.1", "BackgroundJob.2", "RecurringJob.1" }, 1)]
        [InlineData("Me", new string[] { "Thread.1", "Thread.2", "BackgroundJob.1", "BackgroundJob.2", "RecurringJob.1" }, 0)]
        public async Task QueryAsync_FilterOnLockedBy_ReturnsMatchingLocks(string filter, string[] requesters, int expected)
        {
            // Arrange
            var options = TestHelper.CreateProviderOptions();
            await using var provider = new MemoryLockingProvider(options);
            foreach (var requester in requesters)
            {
                await provider.TryLockAsync(Guid.NewGuid().ToString(), requester);
            }

            // Act
            var result = await provider.QueryAsync(x => x.WithFilterOnLockedBy(filter));

            // Assert
            Assert.NotNull(result.Results);
            Assert.Equal(expected, result.Results.Length);
            foreach (var lockResult in result.Results)
            {
                Assert.NotNull(lockResult);
                Assert.Contains(lockResult.LockedBy, requesters);
            }
        }

        /// <summary>Verifies QueryAsync for equal to on locked by: returns matching locks.</summary>
        [Theory(Timeout = DistributedLockingMemoryTestConstants.TestTimeoutMilliseconds)]
        [InlineData("Thread.1", new string?[] { "Thread.1", "Thread.1", null, "BackgroundJob.2", null }, 2)]
        [InlineData(null, new string?[] { "Thread.1", "Thread.1", null, "BackgroundJob.2", null }, 2)]
        [InlineData("Thread.3", new string?[] { "Thread.1", "Thread.1", null, "BackgroundJob.2", null }, 0)]
        [InlineData("BackgroundJob.2", new string?[] { "Thread.1", "Thread.1", null, "BackgroundJob.2", null }, 1)]
        [InlineData("Me", new string?[] { "Thread.1", "Thread.1", null, "BackgroundJob.2", null }, 0)]
        public async Task QueryAsync_EqualToOnLockedBy_ReturnsMatchingLocks(string? equalTo, string?[] requesters, int expected)
        {
            // Arrange
            var options = TestHelper.CreateProviderOptions();
            await using var provider = new MemoryLockingProvider(options);
            var lockingProvider = provider.CastTo<ILockingProvider>();
            foreach (var requester in requesters)
            {
                if(requester == null)
                {
                    var @lock = await lockingProvider.LockAndWaitAsync(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
                    await @lock.DisposeAsync();
                }
                else
                {
                    await provider.TryLockAsync(Guid.NewGuid().ToString(), requester);
                }
            }

            // Act
            var result = await provider.QueryAsync(x => x.WithLockedByEqualTo(equalTo));

            // Assert
            Assert.NotNull(result.Results);
            Assert.Equal(expected, result.Results.Length);
            foreach (var lockResult in result.Results)
            {
                Assert.NotNull(lockResult);
            }
        }

        /// <summary>Verifies QueryAsync for filter on pending requests: returns matching locks.</summary>
        [Theory(Timeout = DistributedLockingMemoryTestConstants.TestTimeoutMilliseconds)]
        [InlineData(5, 0, 4)]
        [InlineData(3, 1, 1)]
        [InlineData(10, 5, 4)]
        public async Task QueryAsync_FilterOnPendingRequests_ReturnsMatchingLocks(int amountOfLocks, int threshold, int expected)
        {
            // Arrange
            var options = TestHelper.CreateProviderOptions();
            await using var provider = new MemoryLockingProvider(options);

            for(int i = 0; i < amountOfLocks; i++)
            {
                await provider.LockAsync($"Resource.{i}", Guid.NewGuid().ToString());
                for (int y = 0; y < i; y++)
                {
                    _ = provider.LockAsync($"Resource.{i}", $"Requester,{y}");
                }
            }
            await Helper.Async.Sleep(TimeSpan.FromSeconds(1));

            // Act
            var result = await provider.QueryAsync(x => x.WithPendingRequestsLargerThan(threshold));

            // Assert
            Assert.NotNull(result.Results);
            Assert.Equal(expected, result.Results.Length);
        }

        /// <summary>Verifies QueryAsync for filter only expired: returns matching locks.</summary>
        [Theory(Timeout = DistributedLockingMemoryTestConstants.TestTimeoutMilliseconds)]
        [InlineData(10, 5)]
        [InlineData(1, 0)]
        [InlineData(100, 10)]
        public async Task QueryAsync_FilterOnlyExpired_ReturnsMatchingLocks(int amountOfLocks, int amountOfExpiredLocks)
        {
            // Arrange
            var options = TestHelper.CreateProviderOptions();
            await using var provider = new MemoryLockingProvider(options);

            for (int i = 0; i < amountOfLocks; i++)
            {
                await provider.LockAsync($"Resource.{i}", Guid.NewGuid().ToString(), i < amountOfExpiredLocks ? TimeSpan.Zero : TimeSpan.FromHours(24));
            }
            await Helper.Async.Sleep(TimeSpan.FromSeconds(1));

            // Act
            var result = await provider.QueryAsync(x => x.WithOnlyExpired());

            // Assert
            Assert.NotNull(result.Results);
            Assert.Equal(amountOfExpiredLocks, result.Results.Length);
        }

        /// <summary>Verifies QueryAsync for filter only not expired: returns matching locks.</summary>
        [Theory(Timeout = DistributedLockingMemoryTestConstants.TestTimeoutMilliseconds)]
        [InlineData(10, 5)]
        [InlineData(1, 0)]
        [InlineData(100, 10)]
        public async Task QueryAsync_FilterOnlyNotExpired_ReturnsMatchingLocks(int amountOfLocks, int amountOfExpiredLocks)
        {
            // Arrange
            var options = TestHelper.CreateProviderOptions();
            await using var provider = new MemoryLockingProvider(options);

            for (int i = 0; i < amountOfLocks; i++)
            {
                await provider.LockAsync($"Resource.{i}", Guid.NewGuid().ToString(), i < amountOfExpiredLocks ? TimeSpan.Zero : TimeSpan.FromHours(24));
            }
            await Helper.Async.Sleep(TimeSpan.FromSeconds(1));

            // Act
            var result = await provider.QueryAsync(x => x.WithOnlyNotExpired());

            // Assert
            Assert.NotNull(result.Results);
            Assert.Equal(amountOfLocks-amountOfExpiredLocks, result.Results.Length);
        }

        /// <summary>Verifies QueryAsync for no filter: returns all locks.</summary>
        [Fact(Timeout = DistributedLockingMemoryTestConstants.TestTimeoutMilliseconds)]
        public async Task QueryAsync_NoFilter_ReturnsAllLocks()
        {
            // Arrange
            var options = TestHelper.CreateProviderOptions();
            await using var provider = new MemoryLockingProvider(options);
            var locks = new string[] { "Resource", "System.Monitor", "System.Heartbeat", "RecurringJob.1-1" };
            foreach (var @lock in locks)
            {
                await provider.TryLockAsync(@lock, "Me");
            }

            // Act
            var result = await provider.QueryAsync(x => { });

            // Assert
            Assert.NotNull(result);
            foreach (var lockResult in result.Results)
            {
                Assert.NotNull(lockResult);
                Assert.Contains(lockResult.Resource, locks);
            }
        }

        /// <summary>Verifies QueryAsync for pagination: returns requested page.</summary>
        [Theory(Timeout = DistributedLockingMemoryTestConstants.TestTimeoutMilliseconds)]
        [InlineData(1, 2, new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9" }, new string[] { "1", "2" })]
        [InlineData(1, 100, new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9" }, new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9" })]
        [InlineData(2, 5, new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9" }, new string[] { "6", "7", "8", "9" })]
        [InlineData(6, 2, new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9" }, new string[] { })]
        public async Task QueryAsync_Pagination_ReturnsRequestedPage(int page, int pageSize, string[] locks, string[] expected)
        {
            // Arrange
            var options = TestHelper.CreateProviderOptions();
            await using var provider = new MemoryLockingProvider(options);
            foreach (var @lock in locks)
            {
                await provider.TryLockAsync(@lock, "Me");
            }

            // Act
            var result = await provider.QueryAsync(x => x.WithPagination(page, pageSize));

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expected, result.Results.Select(x => x.Resource));
        }

        /// <summary>Verifies QueryAsync for sort on resource: returns sorted locks.</summary>
        [Theory(Timeout = DistributedLockingMemoryTestConstants.TestTimeoutMilliseconds)]
        [InlineData(new string[] { "1", "2", "3", "4", "5" }, new string[] { "1","2","3","4", "5" } , false)]
        [InlineData(new string[] { "1", "2", "3", "4", "5" }, new string[] { "5", "4", "3", "2", "1" }, true)]
        public async Task QueryAsync_SortOnResource_ReturnsSortedLocks(string[] locks, string[] expected, bool sortDescending)
        {
            // Arrange
            var options = TestHelper.CreateProviderOptions();
            await using var provider = new MemoryLockingProvider(options);
            foreach (var @lock in locks)
            {
                await provider.TryLockAsync(@lock, Guid.NewGuid().ToString());
            }

            // Act
            var results = await provider.QueryAsync(x => x.OrderByResource(sortDescending));

            // Assert
            Assert.NotNull(results);
            Assert.Equal(expected, results.Results.Select(x => x.Resource));
        }

        /// <summary>Verifies QueryAsync for sort on locked by: returns sorted locks.</summary>
        [Theory(Timeout = DistributedLockingMemoryTestConstants.TestTimeoutMilliseconds)]
        [InlineData(new string?[] { "1", "2", "3", "4", "5", null }, new string?[] { null, "1", "2", "3", "4", "5" }, false)]
        [InlineData(new string?[] { "1", "2", "3", "4", "5", null }, new string?[] { "5", "4", "3", "2", "1", null }, true)]
        public async Task QueryAsync_SortOnLockedBy_ReturnsSortedLocks(string?[] requesters, string?[] expected, bool sortDescending)
        {
            // Arrange
            var options = TestHelper.CreateProviderOptions();
            await using var provider = new MemoryLockingProvider(options);
            foreach (var @lock in requesters)
            {
                // Unlock lock to simulate empty LockedBy property
                if(@lock == null)
                {
                    var lockResult = await provider.TryLockAsync(Guid.NewGuid().ToString(), "Nullable");
                    await lockResult.AcquiredLock.DisposeAsync();
                }
                else
                {
                    await provider.TryLockAsync(Guid.NewGuid().ToString(), @lock);
                }
                
            }

            // Act
            var results = await provider.QueryAsync(x => x.OrderByLockedBy(sortDescending));

            // Assert
            Assert.NotNull(results);
            Assert.Equal(expected, results.Results.Select(x => x.LockedBy));
        }

        /// <summary>Verifies QueryAsync for sort on last lock date: returns sorted locks.</summary>
        [Fact(Timeout = DistributedLockingMemoryTestConstants.TestTimeoutMilliseconds)]
        public async Task QueryAsync_SortOnLastLockDate_ReturnsSortedLocks()
        {
            // Arrange
            var options = TestHelper.CreateProviderOptions();
            await using var provider = new MemoryLockingProvider(options);
            var lockingProvider = provider.CastTo<ILockingProvider>();
            foreach (var i in Enumerable.Range(0, 100))
            {
                await using(await lockingProvider.LockAndWaitAsync(i.ToString(), "Random"))
                {

                }
            }

            // Act
            var results = await provider.QueryAsync(x => x.OrderByLastLockDate());

            // Assert
            Assert.NotNull(results);
            Assert.Equal(Enumerable.Range(0, 100).Select(x => x.ToString()), results.Results.Select(x => x.Resource));
        }

        /// <summary>Verifies QueryAsync for sort on lock date: returns sorted locks.</summary>
        [Fact(Timeout = DistributedLockingMemoryTestConstants.TestTimeoutMilliseconds)]
        public async Task QueryAsync_SortOnLockDate_ReturnsSortedLocks()
        {
            // Arrange
            var options = TestHelper.CreateProviderOptions();
            await using var provider = new MemoryLockingProvider(options);
            var lockingProvider = provider.CastTo<ILockingProvider>();
            foreach (var i in Enumerable.Range(0, 100))
            {
                _ = await lockingProvider.LockAndWaitAsync(i.ToString(), "Random");
            }

            // Act
            var results = await provider.QueryAsync(x => x.OrderByLockedAt());

            // Assert
            Assert.NotNull(results);
            Assert.Equal(Enumerable.Range(0, 100).Select(x => x.ToString()), results.Results.Select(x => x.Resource));
        }

        /// <summary>Verifies QueryAsync for sort on expiry date: returns sorted locks.</summary>
        [Fact(Timeout = DistributedLockingMemoryTestConstants.TestTimeoutMilliseconds)]
        public async Task QueryAsync_SortOnExpiryDate_ReturnsSortedLocks()
        {
            // Arrange
            var options = TestHelper.CreateProviderOptions();
            await using var provider = new MemoryLockingProvider(options);
            var lockingProvider = provider.CastTo<ILockingProvider>();
            foreach (var i in Enumerable.Range(0, 100))
            {
                _ = await lockingProvider.LockAndWaitAsync(i.ToString(), "Random", TimeSpan.FromMilliseconds(10));
                await Helper.Async.Sleep(TimeSpan.FromMilliseconds(5));
            }

            // Act
            var results = await provider.QueryAsync(x => x.OrderByLockedAt());

            // Assert
            Assert.NotNull(results);
            Assert.Equal(Enumerable.Range(0, 100).Select(x => x.ToString()), results.Results.Select(x => x.Resource));
        }

        /// <summary>Verifies TryLockAsync for free resource: locks for requester.</summary>
        [Theory(Timeout = DistributedLockingMemoryTestConstants.TestTimeoutMilliseconds)]
        [InlineData("Jens")]
        [InlineData("Thread.1")]
        [InlineData("Database.Deployer")]
        [InlineData("BackgroundJob.56")]
        [InlineData("sels.homebrewit.csautomatron")]
        public async Task TryLockAsync_FreeResource_LocksForRequester(string requester)
        {
            // Arrange
            var options = TestHelper.CreateProviderOptions();
            await using  var provider = new MemoryLockingProvider(options);

            // Act
            var lockResult = await provider.TryLockAsync("Resource", requester);

            // Assert
            Assert.True(lockResult.Success);
            Assert.NotNull(requester);
            Assert.NotNull(lockResult.AcquiredLock);
            Assert.Equal("Resource", lockResult.AcquiredLock.Resource);
            Assert.Equal(requester, lockResult.AcquiredLock.LockedBy);
            Assert.Null(lockResult.AcquiredLock.ExpiryDate);
        }

        /// <summary>Verifies TryLockAsync for resource locked by other: returns failure.</summary>
        [Fact(Timeout = DistributedLockingMemoryTestConstants.TestTimeoutMilliseconds)]
        public async Task TryLockAsync_ResourceLockedByOther_ReturnsFailure()
        {
            // Arrange
            var options = TestHelper.CreateProviderOptions();
            await using  var provider = new MemoryLockingProvider(options);

            // Act
            var lockResult = await provider.TryLockAsync("Resource", "Requester.1");
            var lockResultTwo = await provider.TryLockAsync("Resource", "Requester.2");

            // Assert
            Assert.True(lockResult.Success);
            Assert.NotNull(lockResult.AcquiredLock);
            Assert.False(lockResultTwo.Success);
            Assert.Null(lockResultTwo.AcquiredLock);
        }

        /// <summary>Verifies TryLockAsync for parallel calls: only one acquires lock.</summary>
        [Fact(Timeout = DistributedLockingMemoryTestConstants.TestTimeoutMilliseconds)]
        public async Task TryLockAsync_ParallelCalls_OnlyOneAcquiresLock()
        {
            // Arrange
            var options = TestHelper.CreateProviderOptions();
            await using  var provider = new MemoryLockingProvider(options);
            var threads = 500;
            List<Task<bool>> _tasks = new List<Task<bool>>();

            // Act
            for(int i = 0; i < threads; i++)
            {
                var currentI = i;
                _tasks.Add(Task.Run(async () => (await provider.TryLockAsync("Resource", $"Thread.{currentI}")).Success));
            }
            var results = await Task.WhenAll(_tasks);

            // Assert
            Assert.True(results.Length == threads);
            Assert.Equal(1, results.Count(x => x));
        }

        /// <summary>Verifies TryLockAsync for requester already holds lock: returns success.</summary>
        [Fact(Timeout = DistributedLockingMemoryTestConstants.TestTimeoutMilliseconds)]
        public async Task TryLockAsync_RequesterAlreadyHoldsLock_ReturnsSuccess()
        {
            // Arrange
            var options = TestHelper.CreateProviderOptions();
            await using var provider = new MemoryLockingProvider(options);

            // Act
            var lockResult = await provider.TryLockAsync("Resource", "Requester.1");
            var lockResultTwo = await provider.TryLockAsync("Resource", "Requester.1");

            // Assert
            Assert.True(lockResult.Success);
            Assert.NotNull(lockResult.AcquiredLock);
            Assert.True(lockResultTwo.Success);
            Assert.NotNull(lockResultTwo.AcquiredLock);
        }

        /// <summary>Verifies TryLockAsync for after unlock: locks resource.</summary>
        [Fact(Timeout = DistributedLockingMemoryTestConstants.TestTimeoutMilliseconds)]
        public async Task TryLockAsync_AfterUnlock_LocksResource()
        {
            // Arrange
            var options = TestHelper.CreateProviderOptions();
            await using  var provider = new MemoryLockingProvider(options);

            // Act
            var lockResult = await provider.TryLockAsync("Resource", "Requester.1");
            await lockResult.AcquiredLock.UnlockAsync();
            var lockResultTwo = await provider.TryLockAsync("Resource", "Requester.2");

            // Assert
            Assert.True(lockResult.Success);
            Assert.NotNull(lockResult.AcquiredLock);
            Assert.Equal("Requester.1", lockResult.AcquiredLock.LockedBy);
            Assert.Equal("Resource", lockResult.AcquiredLock.Resource);
            Assert.Null(lockResult.AcquiredLock.ExpiryDate);
            Assert.True(lockResultTwo.Success);
            Assert.NotNull(lockResultTwo.AcquiredLock);
            Assert.Equal("Requester.2", lockResultTwo.AcquiredLock.LockedBy);
            Assert.Equal("Resource", lockResultTwo.AcquiredLock.Resource);
            Assert.Null(lockResultTwo.AcquiredLock.ExpiryDate);
        }

        /// <summary>Verifies TryLockAsync for after expiry: locks resource.</summary>
        [Fact(Timeout = DistributedLockingMemoryTestConstants.TestTimeoutMilliseconds)]
        public async Task TryLockAsync_AfterExpiry_LocksResource()
        {
            // Arrange
            var options = TestHelper.CreateProviderOptions();
            await using  var provider = new MemoryLockingProvider(options);
            var expiryTime = 500;

            // Act
            var lockResult = await provider.TryLockAsync("Resource", "Requester.1", TimeSpan.FromMilliseconds(expiryTime));
            await Helper.Async.Sleep(expiryTime * 2);
            var lockResultTwo = await provider.TryLockAsync("Resource", "Requester.2");

            // Assert
            Assert.True(lockResult.Success);
            Assert.NotNull(lockResult.AcquiredLock);
            Assert.Equal("Requester.1", lockResult.AcquiredLock.LockedBy);
            Assert.True(lockResultTwo.Success);
            Assert.NotNull(lockResultTwo.AcquiredLock);
            Assert.Equal("Requester.2", lockResultTwo.AcquiredLock.LockedBy);
        }

        /// <summary>Verifies TryLockAsync for keep alive enabled: extends expiry date.</summary>
        [Fact(Timeout = DistributedLockingMemoryTestConstants.TestTimeoutMilliseconds)]
        public async Task TryLockAsync_KeepAliveEnabled_ExtendsExpiryDate()
        {
            // Arrange
            var options = TestHelper.CreateProviderOptions();
            await using  var provider = new MemoryLockingProvider(options);
            var expiryTime = 100;

            // Act
            var lockResult = await provider.TryLockAsync("Resource", "Requester.1", TimeSpan.FromMilliseconds(expiryTime), true);
            var initialExpiry = lockResult.AcquiredLock.ExpiryDate;
            await Helper.Async.Sleep(expiryTime * 2);
            var currentExpiry = lockResult.AcquiredLock.ExpiryDate;

            // Assert
            Assert.NotEqual(currentExpiry, initialExpiry);
            Assert.True(currentExpiry > initialExpiry);
        }

        /// <summary>Verifies TryLockAsync for pending request: takes priority over direct calls.</summary>
        [Fact(Timeout = DistributedLockingMemoryTestConstants.TestTimeoutMilliseconds)]
        public async Task TryLockAsync_PendingRequest_TakesPriorityOverDirectCalls()
        {
            // Arrange
            var options = TestHelper.CreateProviderOptions(x => x.ExpiryOffset = 10000000);
            await using var provider = new MemoryLockingProvider(options);
            var lockingProvider = provider.CastTo<ILockingProvider>();

            // Act
            var resultOne = (await lockingProvider.TryLockAsync("Resource", "Requester.1", TimeSpan.FromMilliseconds(100), false)).Success;
            var lockResultTask = lockingProvider.LockAndWaitAsync("Resource", "Requester.2");
            await Helper.Async.Sleep(100 * 2);
            var resultTwo = (await lockingProvider.TryLockAsync("Resource", "Requester.3")).Success;

            // Assert
            Assert.True(resultOne);
            Assert.False(resultTwo);
            Assert.NotNull(lockResultTask);
            Assert.True(lockResultTask.IsCompleted);
            var lockResult = await lockResultTask;
            Assert.NotNull(lockResult);
            Assert.Equal("Resource", lockResult.Resource);
            Assert.Equal("Requester.2", lockResult.LockedBy);
            Assert.Null(lockResult.ExpiryDate);
        }
    }
}
