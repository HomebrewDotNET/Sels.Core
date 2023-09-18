using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Async.Test.Components.Queue
{
    public class WorkerQueue_WaitUntilEmpty
    {
        [Test, Timeout(60000)]
        public async Task TaskCompletesWhenQueueBecomesEmpty()
        {
            // Arrange
            var provider = TestHelper.GetTaskManagerContainer();
            await using var scope = provider.CreateAsyncScope();
            provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            await using var queue = new WorkerQueue<string>(taskManager, 1);

            // Act
            await queue.EnqueueAsync("hello");
            var waitTask = queue.WaitUntilEmpty();
            var dequeued = await queue.DequeueAsync();

            // Assert
            Assert.IsNotNull(waitTask);
            await waitTask;
        }
    }
}
