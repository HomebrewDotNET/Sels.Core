using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Async.Test.Components.TaskManagement
{
    public class TaskManager_ScheduleDelayed
    {
        [Test, Timeout(60000)]
        public async Task AnonymousTaskIsScheduledAndExecuted()
        {
            // Arrange
            var provider = TestHelper.GetTaskManagerContainer();
            await using var scope = provider.CreateAsyncScope();
            provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            bool executed = false;

            // Act
            var pendingTask = taskManager.ScheduleDelayed(TimeSpan.FromSeconds(2), (s, c) => s.ScheduleAnonymousAction(c => executed = true, token: c));

            // Assert
            Assert.IsNotNull(pendingTask);
            Assert.IsNotNull(pendingTask.Callback);
            var scheduledTask = await pendingTask.Callback;
            Assert.IsNotNull(scheduledTask);
            Assert.IsNotNull(scheduledTask.OnExecuted);
            await scheduledTask.OnExecuted;
            Assert.IsTrue(executed);
        }
        [Test, Timeout(60000)]
        public async Task ManagedTaskIsScheduledAndExecuted()
        {
            // Arrange
            var provider = TestHelper.GetTaskManagerContainer();
            await using var scope = provider.CreateAsyncScope();
            provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            bool executed = false;

            // Act
            var pendingTask = taskManager.ScheduleDelayed(TimeSpan.FromSeconds(2), (s, c) => s.ScheduleAction(this, c => executed = true, token: c));

            // Assert
            Assert.IsNotNull(pendingTask);
            Assert.IsNotNull(pendingTask.Callback);
            var scheduledTask = await pendingTask.Callback;
            Assert.IsNotNull(scheduledTask);
            Assert.IsNotNull(scheduledTask.OnExecuted);
            await scheduledTask.OnExecuted;
            Assert.IsTrue(executed);
        }

        [Test, Timeout(60000)]
        public async Task AnonymousTaskIsScheduledAndExecutedAfterDelay()
        {
            // Arrange
            var provider = TestHelper.GetTaskManagerContainer();
            await using var scope = provider.CreateAsyncScope();
            provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            DateTime executed = default;
            var delay = TimeSpan.FromSeconds(5);

            // Act
            var now = DateTime.Now;
            var pendingTask = taskManager.ScheduleDelayed(delay, (s, c) => s.ScheduleAnonymousAction(c => executed = DateTime.Now, token: c));

            // Assert
            Assert.IsNotNull(pendingTask);
            Assert.IsNotNull(pendingTask.Callback);
            var scheduledTask = await pendingTask.Callback;
            Assert.IsNotNull(scheduledTask);
            Assert.IsNotNull(scheduledTask.OnExecuted);
            await scheduledTask.OnExecuted;
            Assert.That(executed, Is.GreaterThanOrEqualTo(now.Add(delay)));
        }
        [Test, Timeout(60000)]
        public async Task ManagedTaskIsScheduledAndExecutedAfterDelay()
        {
            // Arrange
            var provider = TestHelper.GetTaskManagerContainer();
            await using var scope = provider.CreateAsyncScope();
            provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            DateTime executed = default;
            var delay = TimeSpan.FromSeconds(5);

            // Act
            var now = DateTime.Now;
            var pendingTask = taskManager.ScheduleDelayed(delay, (s, c) => s.ScheduleAction(this, c => executed = DateTime.Now, token: c));

            // Assert
            Assert.IsNotNull(pendingTask);
            Assert.IsNotNull(pendingTask.Callback);
            var scheduledTask = await pendingTask.Callback;
            Assert.IsNotNull(scheduledTask);
            Assert.IsNotNull(scheduledTask.OnExecuted);
            await scheduledTask.OnExecuted;
            Assert.That(executed, Is.GreaterThanOrEqualTo(now.Add(delay)));
        }
    }
}
