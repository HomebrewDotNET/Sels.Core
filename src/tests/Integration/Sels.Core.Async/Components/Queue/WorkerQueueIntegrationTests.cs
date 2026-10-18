using Microsoft.Extensions.DependencyInjection;
using Sels.Core.Async.Queue;
using Sels.Core.Async.TaskManagement;
using Sels.Core.Extensions.Conversion;
using System.Drawing;
using System.Text;

namespace Sels.Core.Async.IntegrationTests.Components.Queue
{
    /// <summary>Integration tests for <see cref="WorkerQueue{T}"/> backed by a real task manager.</summary>
    [Collection(AsyncIntegrationCollection.Name)]
    [Trait("Category", "Integration")]
    public class WorkerQueueIntegrationTests : AsyncIntegrationTestBase
    {
        /// <inheritdoc/>
        protected override void ConfigureServices(IServiceCollection services) => RegisterTaskManager(services);

        /// <summary>Verifies DequeueAsync for queue not empty: returns item.</summary>
        [Theory(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        [InlineData("1")]
        [InlineData("0")]
        [InlineData("420")]
        [InlineData("-450459")]
        [InlineData("459234755")]
        public async Task DequeueAsync_QueueNotEmpty_ReturnsItem(string item)
        {
            // Arrange
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            await using var queue = new WorkerQueue<string>(taskManager, 1);

            // Act
            await queue.EnqueueAsync(item);
            var dequeued = await queue.DequeueAsync();

            // Assert
            Assert.Equal(item, dequeued);
        }

        /// <summary>Verifies DequeueAsync for queue empty: waits for item.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task DequeueAsync_QueueEmpty_WaitsForItem()
        {
            // Arrange
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            await using var queue = new WorkerQueue<string>(taskManager, 1);

            // Act
            var task = queue.DequeueAsync();
            await Helper.Async.Sleep(1000);

            // Assert
            Assert.NotNull(task);
            Assert.False(task.IsCompleted);
        }

        /// <summary>Verifies DequeueAsync for pending request: assigns enqueued item to request.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task DequeueAsync_PendingRequest_AssignsEnqueuedItemToRequest()
        {
            // Arrange
            const string item = "56";
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            await using var queue = new WorkerQueue<string>(taskManager, 1);

            // Act
            var task = queue.DequeueAsync();
            await queue.EnqueueAsync(item);

            // Assert
            Assert.NotNull(task);
            var actual = await task;
            Assert.Equal(item, actual);
            Assert.Equal(0, queue.Count);
        }

        /// <summary>Verifies DequeueAsync for token cancelled: throws operation canceled exception.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task DequeueAsync_TokenCancelled_ThrowsOperationCanceledException()
        {
            // Arrange
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            await using var queue = new WorkerQueue<string>(taskManager, 1);
            var tokenSource = new CancellationTokenSource();
            Exception exception = null;

            // Act
            try
            {
                tokenSource.Cancel();
                await queue.DequeueAsync(tokenSource.Token);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.NotNull(exception);
            Assert.IsAssignableFrom<OperationCanceledException>(exception);
        }

        /// <summary>Verifies EnqueueAsync for below max size: does not throw.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task EnqueueAsync_BelowMaxSize_DoesNotThrow()
        {
            // Arrange
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            await using var queue = new WorkerQueue<string>(taskManager, 1);
            Exception exception = null;

            // Act
            try
            {
                await queue.EnqueueAsync(string.Empty);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.Null(exception);
        }

        /// <summary>Verifies EnqueueAsync for no max size: does not throw.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task EnqueueAsync_NoMaxSize_DoesNotThrow()
        {
            // Arrange
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            await using var queue = new WorkerQueue<string>(taskManager);
            Exception exception = null;

            // Act
            try
            {
                await queue.EnqueueAsync(string.Empty);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.Null(exception);
        }

        /// <summary>Verifies EnqueueAsync for at max size: throws.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task EnqueueAsync_AtMaxSize_Throws()
        {
            // Arrange
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            await using var queue = new WorkerQueue<string>(taskManager, 1);
            Exception exception = null;

            // Act
            _ = await queue.TryEnqueueAsync(string.Empty);
            try
            {
                await queue.EnqueueAsync(string.Empty);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.NotNull(exception);
            Assert.IsAssignableFrom<InvalidOperationException>(exception);
        }

        /// <summary>Verifies EnqueueAsync for item: increases count.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task EnqueueAsync_Item_IncreasesCount()
        {
            // Arrange
            const int amount = 5;
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            await using var queue = new WorkerQueue<string>(taskManager);

            // Act
            for (int i = 0; i < amount; i++)
            {
                await queue.EnqueueAsync($"{i}");
            }

            // Assert
            Assert.Equal(amount, queue.Count);
        }

        /// <summary>Verifies EnqueueAsync for token cancelled: throws operation canceled exception.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task EnqueueAsync_TokenCancelled_ThrowsOperationCanceledException()
        {
            // Arrange
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            await using var queue = new WorkerQueue<string>(taskManager);
            var tokenSource = new CancellationTokenSource();
            Exception exception = null;

            // Act
            try
            {
                tokenSource.Cancel();
                await queue.EnqueueAsync(string.Empty, tokenSource.Token);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.NotNull(exception);
            Assert.IsAssignableFrom<OperationCanceledException>(exception);
        }

        /// <summary>Verifies InterceptRequest for interceptor returns item: intercepts request.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task InterceptRequest_InterceptorReturnsItem_InterceptsRequest()
        {
            // Arrange
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            await using var queue = new WorkerQueue<string>(taskManager, 1);

            // Act
            using var subscription = queue.InterceptRequest(x => Task.FromResult(string.Empty));
            var result = await queue.DequeueAsync();

            // Assert
            Assert.Equal(string.Empty, result);
        }

        /// <summary>Verifies InterceptRequest for first interceptor returns null: calls second interceptor.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task InterceptRequest_FirstInterceptorReturnsNull_CallsSecondInterceptor()
        {
            // Arrange
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            await using var queue = new WorkerQueue<string>(taskManager, 1);

            // Act
            using var subscription = queue.InterceptRequest(x => Task.FromResult<string?>(null));
            using var secondSubscription = queue.InterceptRequest(x => Task.FromResult(string.Empty));
            var result = await queue.DequeueAsync();

            // Assert
            Assert.Equal(string.Empty, result);
        }

        /// <summary>Verifies OnQueueAbove for size exceeded: triggers delegate.</summary>
        [Theory(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        [InlineData(1u)]
        [InlineData(2u)]
        [InlineData(5u)]
        [InlineData(8u)]
        public async Task OnQueueAbove_SizeExceeded_TriggersDelegate(uint size)
        {
            // Arrange
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            await using var queue = new WorkerQueue<string>(taskManager, size.ChangeType<int>());
            int triggeredAmount = 0;
            using var eventSubscription = queue.OnQueueAbove(size, t => { lock (provider) { triggeredAmount++; }; return Task.CompletedTask; });

            // Act
            for(int i = 0; i < size; i++)
            {
                await queue.EnqueueAsync($"{i}");
            }
            await Helper.Async.Sleep(2000);

            // Assert
            Assert.Equal(1, triggeredAmount);
        }

        /// <summary>Verifies OnQueueBelow for size drops below: triggers delegate.</summary>
        [Theory(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        [InlineData(1u)]
        [InlineData(2u)]
        [InlineData(5u)]
        [InlineData(8u)]
        public async Task OnQueueBelow_SizeDropsBelow_TriggersDelegate(uint size)
        {
            // Arrange
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            await using var queue = new WorkerQueue<string>(taskManager, size.ChangeType<int>()+1);
            int triggeredAmount = 0;
            using var eventSubscription = queue.OnQueueBelow(size, t => { lock (provider) { triggeredAmount++; }; return Task.CompletedTask; });

            // Act
            for (int i = 0; i < size+1; i++)
            {
                await queue.EnqueueAsync($"{i}");
            }
            while(queue.Count > size)
            {
                _ = await queue.DequeueAsync();
            }
            await Helper.Async.Sleep(2000);

            // Assert
            Assert.Equal(1, triggeredAmount);
        }

        /// <summary>Verifies OnQueueAbove for subscription cancelled: stops calling delegate.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task OnQueueAbove_SubscriptionCancelled_StopsCallingDelegate()
        {
            // Arrange
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            await using var queue = new WorkerQueue<string>(taskManager, 1);
            int triggeredAmount = 0;
            var eventSubscription = queue.OnQueueAbove(1, t => { lock (provider) { triggeredAmount++; }; return Task.CompletedTask; });

            // Act
            eventSubscription.Dispose();
            await queue.EnqueueAsync(string.Empty);
            await Helper.Async.Sleep(2000);

            // Assert
            Assert.Equal(0, triggeredAmount);
        }

        /// <summary>Verifies Subscribe for items enqueued: assigns items to delegate.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task Subscribe_ItemsEnqueued_AssignsItemsToDelegate()
        {
            // Arrange
            const string item = "1998";
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            await using var queue = new WorkerQueue<string>(taskManager, 1);
            string assigned = null;
            using var subscription = queue.Subscribe(1, (i, t) => {  assigned = i; return Task.CompletedTask; });

            // Act
            await queue.EnqueueAsync(item);
            await Helper.Async.Sleep(2000);

            // Assert
            Assert.Equal(item, assigned);
        }

        /// <summary>Verifies Subscribe for subscription cancelled: stops calling delegate.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task Subscribe_SubscriptionCancelled_StopsCallingDelegate()
        {
            // Arrange
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            await using var queue = new WorkerQueue<string>(taskManager, 1);
            int triggeredAmount = 0;
            var subscription = queue.Subscribe(1, (i, t) => { lock (provider) { triggeredAmount++; }; return Task.CompletedTask; });

            // Act
            subscription.Dispose();
            await queue.EnqueueAsync(string.Empty);
            await Helper.Async.Sleep(2000);

            // Assert
            Assert.Equal(0, triggeredAmount);
        }

        /// <summary>Verifies TryDequeueAsync for queue not empty: returns true with item.</summary>
        [Theory(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        [InlineData("1")]
        [InlineData("0")]
        [InlineData("420")]
        [InlineData("-450459")]
        [InlineData("459234755")]
        public async Task TryDequeueAsync_QueueNotEmpty_ReturnsTrueWithItem(string item)
        {
            // Arrange
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            await using var queue = new WorkerQueue<string>(taskManager, 1);

            // Act
            await queue.EnqueueAsync(item);
            var (wasDequeued, dequeued) = await queue.TryDequeueAsync();

            // Assert
            Assert.True(wasDequeued);
            Assert.Equal(item, dequeued);
        }

        /// <summary>Verifies TryDequeueAsync for queue empty: returns false.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task TryDequeueAsync_QueueEmpty_ReturnsFalse()
        {
            // Arrange
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            await using var queue = new WorkerQueue<string>(taskManager, 1);

            // Act
            var (wasDequeued, dequeued) = await queue.TryDequeueAsync();

            // Assert
            Assert.False(wasDequeued);
        }

        /// <summary>Verifies TryDequeueAsync for token cancelled: throws operation canceled exception.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task TryDequeueAsync_TokenCancelled_ThrowsOperationCanceledException()
        {
            // Arrange
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            await using var queue = new WorkerQueue<string>(taskManager, 1);
            var tokenSource = new CancellationTokenSource();
            Exception exception = null;

            // Act
            try
            {
                tokenSource.Cancel();
                await queue.TryDequeueAsync(tokenSource.Token);
            }
            catch(Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.NotNull(exception);
            Assert.IsAssignableFrom<OperationCanceledException>(exception);
        }

        /// <summary>Verifies TryEnqueueAsync for below max size: returns true.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task TryEnqueueAsync_BelowMaxSize_ReturnsTrue()
        {
            // Arrange
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            await using var queue = new WorkerQueue<string>(taskManager, 1);

            // Act
            var actual = await queue.TryEnqueueAsync(string.Empty);

            // Assert
            Assert.True(actual);
        }

        /// <summary>Verifies TryEnqueueAsync for no max size: returns true.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task TryEnqueueAsync_NoMaxSize_ReturnsTrue()
        {
            // Arrange
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            await using var queue = new WorkerQueue<string>(taskManager);

            // Act
            var actual = await queue.TryEnqueueAsync(string.Empty);

            // Assert
            Assert.True(actual);
        }

        /// <summary>Verifies TryEnqueueAsync for at max size: returns false.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task TryEnqueueAsync_AtMaxSize_ReturnsFalse()
        {
            // Arrange
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            await using var queue = new WorkerQueue<string>(taskManager, 1);

            // Act
            _ = await queue.TryEnqueueAsync(string.Empty);
            var actual = await queue.TryEnqueueAsync("2");

            // Assert
            Assert.False(actual);
        }

        /// <summary>Verifies TryEnqueueAsync for item: increases count.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task TryEnqueueAsync_Item_IncreasesCount()
        {
            // Arrange
            const int amount = 5;
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            await using var queue = new WorkerQueue<string>(taskManager);

            // Act
            for(int i = 0; i < amount; i++)
            {
                await queue.TryEnqueueAsync($"{i}");
            }

            // Assert
            Assert.Equal(amount, queue.Count);
        }

        /// <summary>Verifies TryEnqueueAsync for token cancelled: throws operation canceled exception.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task TryEnqueueAsync_TokenCancelled_ThrowsOperationCanceledException()
        {
            // Arrange
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            await using var queue = new WorkerQueue<string>(taskManager);
            var tokenSource = new CancellationTokenSource();
            Exception exception = null;

            // Act
            try
            {
                tokenSource.Cancel();
                await queue.TryEnqueueAsync(string.Empty, tokenSource.Token);
            }
            catch(Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.NotNull(exception);
            Assert.IsAssignableFrom<OperationCanceledException>(exception);
        }

        /// <summary>Verifies WaitUntilEmpty for queue becomes empty: completes.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task WaitUntilEmpty_QueueBecomesEmpty_Completes()
        {
            // Arrange
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            await using var queue = new WorkerQueue<string>(taskManager, 1);

            // Act
            await queue.EnqueueAsync("hello");
            var waitTask = queue.WaitUntilEmpty();
            var dequeued = await queue.DequeueAsync();

            // Assert
            Assert.NotNull(waitTask);
            await waitTask;
        }
    }
}
