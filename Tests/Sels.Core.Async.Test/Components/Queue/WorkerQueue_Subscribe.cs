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
        [Test, Timeout(10000)]
        public async Task ItemsGetAssignedToSubscriptionDelegate()
        {
            // Arrange
            const string item = "1998";
            await using var serviceProvider = TestHelper.GetTaskManagerContainer();
            await using var scope = serviceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            await using var queue = new WorkerQueue<string>(taskManager, 1);
            string assigned = null;
            using var subscription = queue.Subscribe(1, (i, t) => {  assigned = i; return Task.CompletedTask; });

            // Act
            await queue.EnqueueAsync(item);
            await Helper.Async.Sleep(2000);

            // Assert
            Assert.AreEqual(item, assigned);
        }

        [Test, Timeout(10000)]
        public async Task CancellingSubscriptionStopsDelegateFromBeingCalled()
        {
            // Arrange
            await using var serviceProvider = TestHelper.GetTaskManagerContainer();
            await using var scope = serviceProvider.CreateAsyncScope();
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
            Assert.AreEqual(0, triggeredAmount);
        }
    }
}
