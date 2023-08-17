using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Async.Test.Components.Queue
{
    public class WorkerQueue_Subscribe
    {
        [Test, Timeout(60000)]
        public async Task ItemsGetAssignedToSubscriptionDelegate()
        {
            // Arrange
            const int item = 1998;
            var provider = TestHelper.GetTaskManagerContainer();
            await using var scope = provider.CreateAsyncScope();
            provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            await using var queue = new WorkerQueue<int>(taskManager, 1);
            int assigned = 0;
            using var subscription = queue.Subscribe(1, (i, t) => {  assigned = i; return Task.CompletedTask; });

            // Act
            await queue.EnqueueAsync(item);
            await Helper.Async.Sleep(2000);

            // Assert
            Assert.AreEqual(item, assigned);
        }

        [Test, Timeout(60000)]
        public async Task CancellingSubscriptionStopsDelegateFromBeingCalled()
        {
            // Arrange
            var provider = TestHelper.GetTaskManagerContainer();
            await using var scope = provider.CreateAsyncScope();
            provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            await using var queue = new WorkerQueue<int>(taskManager, 1);
            int triggeredAmount = 0;
            var subscription = queue.Subscribe(1, (i, t) => { lock (provider) { triggeredAmount++; }; return Task.CompletedTask; });

            // Act
            subscription.Dispose();
            await queue.EnqueueAsync(1);
            await Helper.Async.Sleep(2000);

            // Assert
            Assert.AreEqual(0, triggeredAmount);
        }
    }
}
