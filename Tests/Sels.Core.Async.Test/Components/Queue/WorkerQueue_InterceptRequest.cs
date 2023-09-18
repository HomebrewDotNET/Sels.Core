using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Async.Test.Components.Queue
{
    public class WorkerQueue_InterceptRequest
    {
        [Test, Timeout(60000)]
        public async Task RequestIsIntercepted()
        {
            // Arrange
            var provider = TestHelper.GetTaskManagerContainer();
            await using var scope = provider.CreateAsyncScope();
            provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            await using var queue = new WorkerQueue<string>(taskManager, 1);

            // Act
            using var subscription = queue.InterceptRequest(x => Task.FromResult(string.Empty));
            var result = await queue.DequeueAsync();

            // Assert
            Assert.That(result, Is.EqualTo(string.Empty));
        }

        [Test, Timeout(60000)]
        public async Task SecondInterceptorIsCalledWhenFirstReturnsNull()
        {
            // Arrange
            var provider = TestHelper.GetTaskManagerContainer();
            await using var scope = provider.CreateAsyncScope();
            provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            await using var queue = new WorkerQueue<string>(taskManager, 1);

            // Act
            using var subscription = queue.InterceptRequest(x => Task.FromResult<string?>(null));
            using var secondSubscription = queue.InterceptRequest(x => Task.FromResult(string.Empty));
            var result = await queue.DequeueAsync();

            // Assert
            Assert.That(result, Is.EqualTo(string.Empty));
        }
    }
}
