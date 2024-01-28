using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Async.Test.Components.TaskManagement
{
    public class TaskManager_CreateOrGetGlobalQueue
    {
        private const string GlobalQueueName = "UnitTests";

        [Test, Timeout(30000)]
        public async Task AnonymousTaskIsScheduledAndExecuted()
        {
            // Arrange
            await using var serviceProvider = TestHelper.GetTaskManagerContainer();
            await using var scope = serviceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            using var queue = taskManager.CreateOrGetGlobalQueue(GlobalQueueName, 1);
            bool executed = false;

            // Act
            var pendingTask = await queue.EnqueueAsync((s, c) => s.ScheduleAnonymousAction(c => executed = true, token: c));

            // Assert
            Assert.IsNotNull(pendingTask);
            Assert.IsNotNull(pendingTask.Callback);
            var scheduledTask = await pendingTask.Callback;
            Assert.IsNotNull(scheduledTask);
            Assert.IsNotNull(scheduledTask.OnExecuted);
            await scheduledTask.OnExecuted;
            Assert.IsTrue(executed);
        }
        [Test, Timeout(30000)]
        public async Task ManagedTaskIsScheduledAndExecuted()
        {
            // Arrange
            await using var serviceProvider = TestHelper.GetTaskManagerContainer();
            await using var scope = serviceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            using var queue = taskManager.CreateOrGetGlobalQueue(GlobalQueueName, 1);
            bool executed = false;

            // Act
            var pendingTask = await queue.EnqueueAsync((s, c) => s.ScheduleAction(this, c => executed = true, token: c));

            // Assert
            Assert.IsNotNull(pendingTask);
            Assert.IsNotNull(pendingTask.Callback);
            var scheduledTask = await pendingTask.Callback;
            Assert.IsNotNull(scheduledTask);
            Assert.IsNotNull(scheduledTask.OnExecuted);
            await scheduledTask.OnExecuted;
            Assert.IsTrue(executed);
        }
    }
}
