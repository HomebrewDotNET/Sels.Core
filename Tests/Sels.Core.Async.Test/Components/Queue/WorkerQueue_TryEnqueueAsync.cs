using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sels.Core.Async.Test.Components.Queue
{
    public class WorkerQueue_TryEnqueueAsync
    {
        [Test, Timeout(10000)]
        public async Task ReturnsTrueWhenQueueIsNotAtMaxSize()
        {
            // Arrange
            await using var serviceProvider = TestHelper.GetTaskManagerContainer();
            await using var scope = serviceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            await using var queue = new WorkerQueue<string>(taskManager, 1);

            // Act
            var actual = await queue.TryEnqueueAsync(string.Empty);

            // Assert
            Assert.IsTrue(actual);
        }
        [Test, Timeout(10000)]
        public async Task ReturnsTrueWhenQueueDoesNotHaveMaxSize()
        {
            // Arrange
            await using var serviceProvider = TestHelper.GetTaskManagerContainer();
            await using var scope = serviceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            await using var queue = new WorkerQueue<string>(taskManager);

            // Act
            var actual = await queue.TryEnqueueAsync(string.Empty);

            // Assert
            Assert.IsTrue(actual);
        }
        [Test, Timeout(10000)]
        public async Task ReturnsFalseWhenQueueIsAtMaxSize()
        {
            // Arrange
            await using var serviceProvider = TestHelper.GetTaskManagerContainer();
            await using var scope = serviceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            await using var queue = new WorkerQueue<string>(taskManager, 1);

            // Act
            _ = await queue.TryEnqueueAsync(string.Empty);
            var actual = await queue.TryEnqueueAsync("2");

            // Assert
            Assert.IsFalse(actual);
        }
        [Test, Timeout(10000)]
        public async Task AddingToQueueIncreasesCount()
        {
            // Arrange
            const int amount = 5;
            await using var serviceProvider = TestHelper.GetTaskManagerContainer();
            await using var scope = serviceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            await using var queue = new WorkerQueue<string>(taskManager);

            // Act
            for(int i = 0; i < amount; i++)
            {
                await queue.TryEnqueueAsync($"{i}");
            }

            // Assert
            Assert.AreEqual(amount, queue.Count);
        }
        [Test, Timeout(10000)]
        public async Task ThrowsOperationCanceledExceptionWhenTokenIsCancelled()
        {
            // Arrange
            await using var serviceProvider = TestHelper.GetTaskManagerContainer();
            await using var scope = serviceProvider.CreateAsyncScope();
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
            Assert.IsNotNull(exception);
            Assert.That(exception, Is.AssignableTo<OperationCanceledException>());
        }
    }
}
