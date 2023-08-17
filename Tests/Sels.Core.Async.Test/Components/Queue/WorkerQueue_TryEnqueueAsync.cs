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
        [Test, Timeout(60000)]
        public async Task ReturnsTrueWhenQueueIsNotAtMaxSize()
        {
            // Arrange
            var provider = TestHelper.GetTaskManagerContainer();
            await using var scope = provider.CreateAsyncScope();
            provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            await using var queue = new WorkerQueue<int>(taskManager, 1);

            // Act
            var actual = await queue.TryEnqueueAsync(1);

            // Assert
            Assert.IsTrue(actual);
        }
        [Test, Timeout(60000)]
        public async Task ReturnsTrueWhenQueueDoesNotHaveMaxSize()
        {
            // Arrange
            var provider = TestHelper.GetTaskManagerContainer();
            await using var scope = provider.CreateAsyncScope();
            provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            await using var queue = new WorkerQueue<int>(taskManager);

            // Act
            var actual = await queue.TryEnqueueAsync(1);

            // Assert
            Assert.IsTrue(actual);
        }
        [Test, Timeout(60000)]
        public async Task ReturnsFalseWhenQueueIsAtMaxSize()
        {
            // Arrange
            var provider = TestHelper.GetTaskManagerContainer();
            await using var scope = provider.CreateAsyncScope();
            provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            await using var queue = new WorkerQueue<int>(taskManager, 1);

            // Act
            _ = await queue.TryEnqueueAsync(1);
            var actual = await queue.TryEnqueueAsync(2);

            // Assert
            Assert.IsFalse(actual);
        }
        [Test, Timeout(60000)]
        public async Task AddingToQueueIncreasesCount()
        {
            // Arrange
            const int amount = 5;
            var provider = TestHelper.GetTaskManagerContainer();
            await using var scope = provider.CreateAsyncScope();
            provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            await using var queue = new WorkerQueue<int>(taskManager);

            // Act
            for(int i = 0; i < amount; i++)
            {
                await queue.TryEnqueueAsync(i);
            }

            // Assert
            Assert.AreEqual(amount, queue.Count);
        }
        [Test, Timeout(60000)]
        public async Task ThrowsOperationCanceledExceptionWhenTokenIsCancelled()
        {
            // Arrange
            var provider = TestHelper.GetTaskManagerContainer();
            await using var scope = provider.CreateAsyncScope();
            provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            await using var queue = new WorkerQueue<int>(taskManager);
            var tokenSource = new CancellationTokenSource();
            Exception exception = null;

            // Act
            try
            {
                tokenSource.Cancel();
                await queue.TryEnqueueAsync(1, tokenSource.Token);
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
