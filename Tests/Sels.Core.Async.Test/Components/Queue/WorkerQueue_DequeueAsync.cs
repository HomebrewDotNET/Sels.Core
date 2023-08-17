using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Async.Test.Components.Queue
{
    public class WorkerQueue_DequeueAsync
    {
        [TestCase(1)]
        [TestCase(0)]
        [TestCase(420)]
        [TestCase(-450459)]
        [TestCase(459234755)]
        [Timeout(60000)]
        public async Task ReturnsCorrectItemWhenQueueIsNotEmpty(int item)
        {
            // Arrange
            var provider = TestHelper.GetTaskManagerContainer();
            await using var scope = provider.CreateAsyncScope();
            provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            await using var queue = new WorkerQueue<int>(taskManager, 1);

            // Act
            await queue.EnqueueAsync(item);
            var dequeued = await queue.DequeueAsync();

            // Assert
            Assert.AreEqual(item, dequeued);
        }
        [Test, Timeout(60000)]
        public async Task CallbackTaskBlocksWhenQueueIsEmpty()
        {
            // Arrange
            var provider = TestHelper.GetTaskManagerContainer();
            await using var scope = provider.CreateAsyncScope();
            provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            await using var queue = new WorkerQueue<int>(taskManager, 1);

            // Act
            var task = queue.DequeueAsync();
            await Helper.Async.Sleep(1000);

            // Assert
            Assert.IsNotNull(task);
            Assert.IsFalse(task.IsCompleted);
        }
        [Test, Timeout(60000)]
        public async Task ItemGetsAssignedToRequestAndNotQueue()
        {
            // Arrange
            const int item = 56;
            var provider = TestHelper.GetTaskManagerContainer();
            await using var scope = provider.CreateAsyncScope();
            provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            await using var queue = new WorkerQueue<int>(taskManager, 1);

            // Act
            var task = queue.DequeueAsync();
            await queue.EnqueueAsync(item);

            // Assert
            Assert.IsNotNull(task);
            var actual = await task;
            Assert.AreEqual(item, actual);
            Assert.AreEqual(0, queue.Count);
        }
        [Test, Timeout(60000)]
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
                await queue.DequeueAsync(tokenSource.Token);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNotNull(exception);
            Assert.That(exception, Is.AssignableTo<OperationCanceledException>());
        }
    }
}
