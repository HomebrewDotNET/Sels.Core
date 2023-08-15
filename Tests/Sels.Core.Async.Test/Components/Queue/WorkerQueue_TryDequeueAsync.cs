using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Async.Test.Components.Queue
{
    public class WorkerQueue_TryDequeueAsync
    {
        [TestCase(1)]
        [TestCase(0)]
        [TestCase(420)]
        [TestCase(-450459)]
        [TestCase(459234755)]
        [Timeout(10000)]
        public async Task ReturnsTrueWithItemWhenQueueIsNotEmpty(int item)
        {
            // Arrange
            var provider = TestHelper.GetTaskManagerContainer();
            await using var scope = provider.CreateAsyncScope();
            provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            await using var queue = new WorkerQueue<int>(taskManager, 1);

            // Act
            await queue.EnqueueAsync(item);
            var (wasDequeued, dequeued) = await queue.TryDequeueAsync();

            // Assert
            Assert.IsTrue(wasDequeued);
            Assert.AreEqual(item, dequeued);
        }

        [Test, Timeout(10000)]
        public async Task ReturnsFalseWhenQueueIsEmpty()
        {
            // Arrange
            var provider = TestHelper.GetTaskManagerContainer();
            await using var scope = provider.CreateAsyncScope();
            provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            await using var queue = new WorkerQueue<int>(taskManager, 1);

            // Act
            var (wasDequeued, dequeued) = await queue.TryDequeueAsync();

            // Assert
            Assert.IsFalse(wasDequeued);
        }

        [Test, Timeout(10000)]
        public async Task ThrowsOperationCanceledExceptionWhenTokenGetsCancelled()
        {
            // Arrange
            var provider = TestHelper.GetTaskManagerContainer();
            await using var scope = provider.CreateAsyncScope();
            provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            await using var queue = new WorkerQueue<int>(taskManager, 1);
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
            Assert.IsNotNull(exception);
            Assert.That(exception, Is.AssignableTo<OperationCanceledException>());
        }
    }
}
