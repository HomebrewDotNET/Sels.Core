using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Async.Test.Components.Queue
{
    public class WorkerQueue_OnQueueChanged
    {
        [TestCase(1u)]
        [TestCase(2u)]
        [TestCase(5u)]
        [TestCase(8u)]
        [Timeout(60000)]
        public async Task GetsTriggeredWhenQueueSizeGoesAbove(uint size)
        {
            // Arrange
            var provider = TestHelper.GetTaskManagerContainer();
            await using var scope = provider.CreateAsyncScope();
            provider = scope.ServiceProvider;
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
            Assert.AreEqual(1, triggeredAmount);
        }

        [TestCase(1u)]
        [TestCase(2u)]
        [TestCase(5u)]
        [TestCase(8u)]
        [Timeout(60000)]
        public async Task GetsTriggeredWhenQueueSizeGoesBelow(uint size)
        {
            // Arrange
            var provider = TestHelper.GetTaskManagerContainer();
            await using var scope = provider.CreateAsyncScope();
            provider = scope.ServiceProvider;
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
            Assert.AreEqual(1, triggeredAmount);
        }

        [Test, Timeout(60000)]
        public async Task CancellingSubscriptionStopsDelegateFromBeingCalled()
        {
            // Arrange
            var provider = TestHelper.GetTaskManagerContainer();
            await using var scope = provider.CreateAsyncScope();
            provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            await using var queue = new WorkerQueue<string>(taskManager, 1);
            int triggeredAmount = 0;
            var eventSubscription = queue.OnQueueAbove(1, t => { lock (provider) { triggeredAmount++; }; return Task.CompletedTask; });

            // Act
            eventSubscription.Dispose();
            await queue.EnqueueAsync(string.Empty);
            await Helper.Async.Sleep(2000);

            // Assert
            Assert.AreEqual(0, triggeredAmount);
        }
    }
}
