using Microsoft.Extensions.DependencyInjection;
using Sels.Core.Async.TaskManagement;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Threading;
using Sels.Core.Models;
using System.Text;

namespace Sels.Core.Async.IntegrationTests.Components.TaskManagement
{
    /// <summary>Integration tests for real task-manager ownership partitions.</summary>
    [Collection(AsyncIntegrationCollection.Name)]
    [Trait("Category", "Integration")]
    public class TaskManagerIntegrationTests : AsyncIntegrationTestBase
    {
        private Action<TaskManagerOptions>? _configureTaskManager;

        /// <inheritdoc/>
        protected override void ConfigureServices(IServiceCollection services) => RegisterTaskManager(services, _configureTaskManager);

        /// <summary>Owners with colliding, negative, and minimum hashes still schedule independent tasks.</summary>
        [Fact]
        public async Task ScheduleAction_OwnerHashCollisions_ExecutesEachTask()
        {
            var manager = ServiceProvider.GetRequiredService<ITaskManager>();
            var ownerHashes = new[] { -1, int.MinValue, -1 };
            var completed = new bool[ownerHashes.Length];
            var tasks = new List<IManagedTask>();

            try
            {
                for (var index = 0; index < ownerHashes.Length; index++)
                {
                    var taskIndex = index;
                    tasks.Add(manager.ScheduleAction(new HashOwner(ownerHashes[index]), _ => completed[taskIndex] = true));
                }

                await Task.WhenAll(tasks.Select(task => task.OnFinalized))
                    .WaitAsync(TimeSpan.FromSeconds(10));

                Assert.All(completed, Assert.True);
            }
            finally
            {
                foreach (var task in tasks)
                {
                    task.Dispose();
                }
            }
        }

        /// <summary>Distinct global task names execute independently despite equal owner hashes.</summary>
        [Fact]
        public async Task ScheduleActionAsync_DistinctGlobalNames_ExecutesBothTasks()
        {
            var manager = ServiceProvider.GetRequiredService<ITaskManager>();
            var firstOwner = new HashOwner(-1);
            var secondOwner = new HashOwner(-1);
            var completed = new bool[2];
            var first = manager.ScheduleActionAsync(firstOwner, "global-partition-first", true, () =>
            {
                completed[0] = true;
                return Task.CompletedTask;
            });
            var second = manager.ScheduleActionAsync(secondOwner, "global-partition-second", true, () =>
            {
                completed[1] = true;
                return Task.CompletedTask;
            });

            try
            {
                await Task.WhenAll(first, second).WaitAsync(TimeSpan.FromSeconds(10));

                Assert.All(completed, Assert.True);
            }
            finally
            {
                first.Dispose();
                second.Dispose();
            }
        }

        /// <summary>Verifies CreateLocalQueue for anonymous task: executes task.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task CreateLocalQueue_AnonymousTask_ExecutesTask()
        {
            // Arrange
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            using var queue = taskManager.CreateLocalQueue(this, 1);
            bool executed = false;

            // Act
            var pendingTask = await queue.EnqueueAsync((s, c) => s.ScheduleAnonymousAction(c => executed = true, token: c));

            // Assert
            Assert.NotNull(pendingTask);
            Assert.NotNull(pendingTask.Callback);
            var scheduledTask = await pendingTask.Callback;
            Assert.NotNull(scheduledTask);
            Assert.NotNull(scheduledTask.OnExecuted);
            await scheduledTask.OnExecuted;
            Assert.True(executed);
        }

        /// <summary>Verifies CreateLocalQueue for managed task: executes task.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task CreateLocalQueue_ManagedTask_ExecutesTask()
        {
            // Arrange
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            using var queue = taskManager.CreateLocalQueue(this, 1);
            bool executed = false;

            // Act
            var pendingTask = await queue.EnqueueAsync((s, c) => s.ScheduleAction(this, c => executed = true, token: c));

            // Assert
            Assert.NotNull(pendingTask);
            Assert.NotNull(pendingTask.Callback);
            var scheduledTask = await pendingTask.Callback;
            Assert.NotNull(scheduledTask);
            Assert.NotNull(scheduledTask.OnExecuted);
            await scheduledTask.OnExecuted;
            Assert.True(executed);
        }

        private const string GlobalQueueName = "UnitTests";

        /// <summary>Verifies CreateOrGetGlobalQueue for anonymous task: executes task.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task CreateOrGetGlobalQueue_AnonymousTask_ExecutesTask()
        {
            // Arrange
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            using var queue = taskManager.CreateOrGetGlobalQueue(GlobalQueueName, 1);
            bool executed = false;

            // Act
            var pendingTask = await queue.EnqueueAsync((s, c) => s.ScheduleAnonymousAction(c => executed = true, token: c));

            // Assert
            Assert.NotNull(pendingTask);
            Assert.NotNull(pendingTask.Callback);
            var scheduledTask = await pendingTask.Callback;
            Assert.NotNull(scheduledTask);
            Assert.NotNull(scheduledTask.OnExecuted);
            await scheduledTask.OnExecuted;
            Assert.True(executed);
        }

        /// <summary>Verifies CreateOrGetGlobalQueue for managed task: executes task.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task CreateOrGetGlobalQueue_ManagedTask_ExecutesTask()
        {
            // Arrange
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            using var queue = taskManager.CreateOrGetGlobalQueue(GlobalQueueName, 1);
            bool executed = false;

            // Act
            var pendingTask = await queue.EnqueueAsync((s, c) => s.ScheduleAction(this, c => executed = true, token: c));

            // Assert
            Assert.NotNull(pendingTask);
            Assert.NotNull(pendingTask.Callback);
            var scheduledTask = await pendingTask.Callback;
            Assert.NotNull(scheduledTask);
            Assert.NotNull(scheduledTask.OnExecuted);
            await scheduledTask.OnExecuted;
            Assert.True(executed);
        }

        /// <summary>Verifies ScheduleDelayed for anonymous task: executes task.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task ScheduleDelayed_AnonymousTask_ExecutesTask()
        {
            // Arrange
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            bool executed = false;

            // Act
            var pendingTask = taskManager.ScheduleDelayed(TimeSpan.FromSeconds(2), (s, c) => s.ScheduleAnonymousAction(c => executed = true, token: c));

            // Assert
            Assert.NotNull(pendingTask);
            Assert.NotNull(pendingTask.Callback);
            var scheduledTask = await pendingTask.Callback;
            Assert.NotNull(scheduledTask);
            Assert.NotNull(scheduledTask.OnExecuted);
            await scheduledTask.OnExecuted;
            Assert.True(executed);
        }

        /// <summary>Verifies ScheduleDelayed for managed task: executes task.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task ScheduleDelayed_ManagedTask_ExecutesTask()
        {
            // Arrange
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            bool executed = false;

            // Act
            var pendingTask = taskManager.ScheduleDelayed(TimeSpan.FromSeconds(2), (s, c) => s.ScheduleAction(this, c => executed = true, token: c));

            // Assert
            Assert.NotNull(pendingTask);
            Assert.NotNull(pendingTask.Callback);
            var scheduledTask = await pendingTask.Callback;
            Assert.NotNull(scheduledTask);
            Assert.NotNull(scheduledTask.OnExecuted);
            await scheduledTask.OnExecuted;
            Assert.True(executed);
        }

        /// <summary>Verifies ScheduleDelayed for anonymous task with delay: executes after delay.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task ScheduleDelayed_AnonymousTaskWithDelay_ExecutesAfterDelay()
        {
            // Arrange
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            DateTime executed = default;
            var delay = TimeSpan.FromSeconds(5);

            // Act
            var now = DateTime.Now;
            var pendingTask = taskManager.ScheduleDelayed(delay, (s, c) => s.ScheduleAnonymousAction(c => executed = DateTime.Now, token: c));

            // Assert
            Assert.NotNull(pendingTask);
            Assert.NotNull(pendingTask.Callback);
            var scheduledTask = await pendingTask.Callback;
            Assert.NotNull(scheduledTask);
            Assert.NotNull(scheduledTask.OnExecuted);
            await scheduledTask.OnExecuted;
            Assert.True(executed >= now.Add(delay));
        }

        /// <summary>Verifies ScheduleDelayed for managed task with delay: executes after delay.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task ScheduleDelayed_ManagedTaskWithDelay_ExecutesAfterDelay()
        {
            // Arrange
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            DateTime executed = default;
            var delay = TimeSpan.FromSeconds(5);

            // Act
            var now = DateTime.Now;
            var pendingTask = taskManager.ScheduleDelayed(delay, (s, c) => s.ScheduleAction(this, c => executed = DateTime.Now, token: c));

            // Assert
            Assert.NotNull(pendingTask);
            Assert.NotNull(pendingTask.Callback);
            var scheduledTask = await pendingTask.Callback;
            Assert.NotNull(scheduledTask);
            Assert.NotNull(scheduledTask.OnExecuted);
            await scheduledTask.OnExecuted;
            Assert.True(executed >= now.Add(delay));
        }

        /// <summary>Verifies Schedule for action: executes task.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task Schedule_Action_ExecutesTask()
        {
            // Arrange
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            bool executed = false;

            // Act
            var scheduledTask = taskManager.Schedule(this, () => executed = true);

            // Assert
            Assert.NotNull(scheduledTask);
            Assert.NotNull(scheduledTask.OnExecuted);
            await scheduledTask.OnExecuted;
            Assert.True(executed);
        }

        /// <summary>Verifies Schedule for task returns value: returns output.</summary>
        [Theory(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(69)]
        [InlineData(420)]
        public async Task Schedule_TaskReturnsValue_ReturnsOutput(int output)
        {
            // Arrange
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();

            // Act
            var scheduledTask = taskManager.Schedule(this, () => output);

            // Assert
            Assert.NotNull(scheduledTask);
            Assert.NotNull(scheduledTask.OnExecuted);
            await scheduledTask.OnExecuted;
            var actual = scheduledTask.Result.CastToOrDefault<int>();
            Assert.Equal(output, actual);
        }

        /// <summary>Verifies Schedule for task throws: returns exception.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task Schedule_TaskThrows_ReturnsException()
        {
            // Arrange
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();

            // Act
            var scheduledTask = taskManager.ScheduleAction(this, () => throw new DivideByZeroException());

            // Assert
            Assert.NotNull(scheduledTask);
            Assert.NotNull(scheduledTask.OnExecuted);
            await scheduledTask.OnExecuted;
            var actual = scheduledTask.Result.CastToOrDefault<Exception>();
            Assert.NotNull(actual);
            Assert.IsAssignableFrom<DivideByZeroException>(actual);
        }

        /// <summary>Verifies Schedule for token cancelled: cancels task.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task Schedule_TokenCancelled_CancelsTask()
        {
            // Arrange
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            var tokenSource = new CancellationTokenSource();
            Exception exception = null;

            // Act
            var scheduledTask = taskManager.ScheduleAction(this, () => Task.Delay(TimeSpan.FromMinutes(1), tokenSource.Token), token: tokenSource.Token);
            tokenSource.Cancel();

            // Assert
            Assert.NotNull(scheduledTask);
            Assert.NotNull(scheduledTask.OnExecuted);
            await scheduledTask.OnExecuted;
            exception = scheduledTask.Result.CastToOrDefault<Exception>();
            Assert.NotNull(exception);
            Assert.IsAssignableFrom<OperationCanceledException>(exception);
        }

        /// <summary>Verifies Schedule for pre and post actions: executes both.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task Schedule_PreAndPostActions_ExecutesBoth()
        {
            // Arrange
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            bool preExecuted = false;
            bool executed = false;
            bool postExecuted = false;

            // Act
            var scheduledTask = taskManager.Schedule(this, () => executed = true, x => x.ExecuteFirst(() => preExecuted = true).ExecuteAfter(() => postExecuted = true));

            // Assert
            Assert.NotNull(scheduledTask);
            Assert.NotNull(scheduledTask.OnExecuted);
            await scheduledTask.OnExecuted;
            Assert.True(preExecuted);
            Assert.True(executed);
            Assert.True(postExecuted);
        }

        /// <summary>Verifies Schedule for pre action throws: captures exception.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task Schedule_PreActionThrows_CapturesException()
        {
            // Arrange
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();

            // Act
            var scheduledTask = taskManager.ScheduleAction(this, () => Task.CompletedTask, x => x.ExecuteFirst(() => throw new AbandonedMutexException()));

            // Assert
            Assert.NotNull(scheduledTask);
            Assert.NotNull(scheduledTask.OnExecuted);
            await scheduledTask.OnExecuted;
            var actual = scheduledTask.Result.CastToOrDefault<Exception>();
            Assert.NotNull(actual);
            Assert.IsAssignableFrom<AbandonedMutexException>(actual);
        }

        /// <summary>Verifies Schedule for post action throws: captures exception.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task Schedule_PostActionThrows_CapturesException()
        {
            // Arrange
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();

            // Act
            var scheduledTask = taskManager.ScheduleAction(this, () => Task.CompletedTask, x => x.ExecuteAfter(() => throw new AbandonedMutexException()));

            // Assert
            Assert.NotNull(scheduledTask);
            Assert.NotNull(scheduledTask.OnExecuted);
            await scheduledTask.OnExecuted;
            var actual = scheduledTask.Result.CastToOrDefault<Exception>();
            Assert.NotNull(actual);
            Assert.IsAssignableFrom<AbandonedMutexException>(actual);
        }

        /// <summary>Verifies Schedule for managed task options: applies options.</summary>
        [Theory(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        [InlineData(ManagedTaskOptions.GracefulCancellation)]
        [InlineData(ManagedTaskOptions.AutoRestart | ManagedTaskOptions.KeepAlive)]
        [InlineData(ManagedTaskOptions.None)]
        public async Task Schedule_ManagedTaskOptions_AppliesOptions(ManagedTaskOptions taskOptions)
        {
            // Arrange
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();

            // Act
            var scheduledTask = taskManager.ScheduleAction(this, () => Task.CompletedTask, x => x.WithManagedOptions(taskOptions));

            // Assert
            Assert.NotNull(scheduledTask);
            Assert.Equal(taskOptions, scheduledTask.Options);
        }

        /// <summary>Verifies Schedule for managed continuation: triggers continuation.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task Schedule_ManagedContinuation_TriggersContinuation()
        {
            // Arrange
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            IManagedTask managedTask = null;

            // Act
            var scheduledTask = taskManager.ScheduleAction(this, () => Task.CompletedTask, x => x.ContinueWith((s, m, r, c) =>
            {
                managedTask = s.ScheduleAction(this, () => Task.CompletedTask);
                return managedTask;
            }));

            // Assert
            Assert.NotNull(scheduledTask);
            Assert.NotNull(scheduledTask.OnExecuted);
            await scheduledTask.OnExecuted;
            Assert.Equal(1, scheduledTask.Continuations.Length);
            Assert.Equal(managedTask, scheduledTask.Continuations[0]);
        }

        /// <summary>Verifies Schedule for managed named continuation: triggers continuation.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task Schedule_ManagedNamedContinuation_TriggersContinuation()
        {
            // Arrange
            const string TaskName = "Hello";
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            IManagedTask managedTask = null;

            // Act
            var scheduledTask = taskManager.ScheduleAction(this, () => Task.CompletedTask, x => x.ContinueWith(async (s, m, r, c) =>
            {
                managedTask = await s.ScheduleActionAsync(this, TaskName, false, () => Task.CompletedTask, x => x.WithPolicy(NamedManagedTaskPolicy.WaitAndStart));
                return managedTask;
            }));

            // Assert
            Assert.NotNull(scheduledTask);
            Assert.NotNull(scheduledTask.OnExecuted);
            await scheduledTask.OnExecuted;
            Assert.Equal(1, scheduledTask.Continuations.Length);
            Assert.Equal(managedTask, scheduledTask.Continuations[0]);
            Assert.Equal(TaskName, scheduledTask.Continuations[0].Name);
        }

        /// <summary>Verifies Schedule for anonymous continuation: triggers continuation.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task Schedule_AnonymousContinuation_TriggersContinuation()
        {
            // Arrange
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            IManagedAnonymousTask managedTask = null;

            // Act
            var scheduledTask = taskManager.ScheduleAction(this, () => Task.CompletedTask, x => x.ContinueWith((s, m, r, c) =>
            {
                managedTask = s.ScheduleAnonymousAction(() => Task.CompletedTask);
                return managedTask;
            }));

            // Assert
            Assert.NotNull(scheduledTask);
            Assert.NotNull(scheduledTask.OnExecuted);
            await scheduledTask.OnExecuted;
            Assert.Equal(1, scheduledTask.AnonymousContinuations.Length);
            Assert.Equal(managedTask, scheduledTask.AnonymousContinuations[0]);
        }

        /// <summary>Verifies ScheduleAnonymous for action: executes task.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task ScheduleAnonymous_Action_ExecutesTask()
        {
            // Arrange
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            bool executed = false;

            // Act
            var scheduledTask = taskManager.ScheduleAnonymous(() => executed = true);

            // Assert
            Assert.NotNull(scheduledTask);
            Assert.NotNull(scheduledTask.OnExecuted);
            await scheduledTask.OnExecuted;
            Assert.True(executed);
        }

        /// <summary>Verifies ScheduleAnonymous for task returns value: returns output.</summary>
        [Theory(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(69)]
        [InlineData(420)]
        public async Task ScheduleAnonymous_TaskReturnsValue_ReturnsOutput(int output)
        {
            // Arrange
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();

            // Act
            var scheduledTask = taskManager.ScheduleAnonymous(() => output);

            // Assert
            Assert.NotNull(scheduledTask);
            Assert.NotNull(scheduledTask.OnExecuted);
            await scheduledTask.OnExecuted;
            var actual = scheduledTask.Result.CastToOrDefault<int>();
            Assert.Equal(output, actual);
        }

        /// <summary>Verifies ScheduleAnonymous for task throws: returns exception.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task ScheduleAnonymous_TaskThrows_ReturnsException()
        {
            // Arrange
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();

            // Act
            var scheduledTask = taskManager.ScheduleAnonymousAction(() => throw new DivideByZeroException());

            // Assert
            Assert.NotNull(scheduledTask);
            Assert.NotNull(scheduledTask.OnExecuted);
            await scheduledTask.OnExecuted;
            var actual = scheduledTask.Result.CastToOrDefault<Exception>();
            Assert.NotNull(actual);
            Assert.IsAssignableFrom<DivideByZeroException>(actual);
        }

        /// <summary>Verifies ScheduleAnonymous for token cancelled: cancels task.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task ScheduleAnonymous_TokenCancelled_CancelsTask()
        {
            // Arrange
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            var tokenSource = new CancellationTokenSource();
            Exception exception = null;

            // Act
            var scheduledTask = taskManager.ScheduleAnonymousAction(() => Task.Delay(TimeSpan.FromMinutes(1), tokenSource.Token), token: tokenSource.Token);
            tokenSource.Cancel();

            // Assert
            Assert.NotNull(scheduledTask);
            Assert.NotNull(scheduledTask.OnExecuted);
            await scheduledTask.OnExecuted;
            exception = scheduledTask.Result.CastToOrDefault<Exception>();
            Assert.NotNull(exception);
            Assert.IsAssignableFrom<OperationCanceledException>(exception);
        }

        /// <summary>Verifies ScheduleAnonymous for pre and post actions: executes both.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task ScheduleAnonymous_PreAndPostActions_ExecutesBoth()
        {
            // Arrange
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            bool preExecuted = false;
            bool executed = false;
            bool postExecuted = false;

            // Act
            var scheduledTask = taskManager.ScheduleAnonymous(() => executed = true, x => x.ExecuteFirst(() => preExecuted = true).ExecuteAfter(() => postExecuted = true));

            // Assert
            Assert.NotNull(scheduledTask);
            Assert.NotNull(scheduledTask.OnExecuted);
            await scheduledTask.OnExecuted;
            Assert.True(preExecuted);
            Assert.True(executed);
            Assert.True(postExecuted);
        }

        /// <summary>Verifies ScheduleAnonymous for pre action throws: captures exception.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task ScheduleAnonymous_PreActionThrows_CapturesException()
        {
            // Arrange
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();

            // Act
            var scheduledTask = taskManager.ScheduleAnonymousAction(() => Task.CompletedTask, x => x.ExecuteFirst(() => throw new AbandonedMutexException()));

            // Assert
            Assert.NotNull(scheduledTask);
            Assert.NotNull(scheduledTask.OnExecuted);
            await scheduledTask.OnExecuted;
            var actual = scheduledTask.Result.CastToOrDefault<Exception>();
            Assert.NotNull(actual);
            Assert.IsAssignableFrom<AbandonedMutexException>(actual);
        }

        /// <summary>Verifies ScheduleAnonymous for post action throws: captures exception.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task ScheduleAnonymous_PostActionThrows_CapturesException()
        {
            // Arrange
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();

            // Act
            var scheduledTask = taskManager.ScheduleAnonymousAction(() => Task.CompletedTask, x => x.ExecuteAfter(() => throw new AbandonedMutexException()));

            // Assert
            Assert.NotNull(scheduledTask);
            Assert.NotNull(scheduledTask.OnExecuted);
            await scheduledTask.OnExecuted;
            var actual = scheduledTask.Result.CastToOrDefault<Exception>();
            Assert.NotNull(actual);
            Assert.IsAssignableFrom<AbandonedMutexException>(actual);
        }

        /// <summary>Verifies ScheduleAnonymous for managed task options: applies options.</summary>
        [Theory(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        [InlineData(ManagedTaskOptions.GracefulCancellation)]
        [InlineData(ManagedTaskOptions.AutoRestart | ManagedTaskOptions.KeepAlive)]
        [InlineData(ManagedTaskOptions.None)]
        public async Task ScheduleAnonymous_ManagedTaskOptions_AppliesOptions(ManagedTaskOptions taskOptions)
        {
            // Arrange
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();

            // Act
            var scheduledTask = taskManager.ScheduleAnonymousAction(() => Task.CompletedTask, x => x.WithManagedOptions(taskOptions));

            // Assert
            Assert.NotNull(scheduledTask);
            Assert.Equal(taskOptions, scheduledTask.Options);
        }

        /// <summary>Verifies ScheduleAnonymous for managed continuation: triggers continuation.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task ScheduleAnonymous_ManagedContinuation_TriggersContinuation()
        {
            // Arrange
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            IManagedTask managedTask = null;

            // Act
            var scheduledTask = taskManager.ScheduleAnonymousAction(() => Task.CompletedTask, x => x.ContinueWith((s, m, r, c) =>
            {
                managedTask = s.ScheduleAction(this, () => Task.CompletedTask);
                return managedTask;
            }));

            // Assert
            Assert.NotNull(scheduledTask);
            Assert.NotNull(scheduledTask.OnExecuted);
            await scheduledTask.OnExecuted;
            Assert.Equal(1, scheduledTask.Continuations.Length);
            Assert.Equal(managedTask, scheduledTask.Continuations[0]);
        }

        /// <summary>Verifies ScheduleAnonymous for managed named continuation: triggers continuation.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task ScheduleAnonymous_ManagedNamedContinuation_TriggersContinuation()
        {
            // Arrange
            const string TaskName = "Hello";
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            IManagedTask managedTask = null;

            // Act
            var scheduledTask = taskManager.ScheduleAnonymousAction(() => Task.CompletedTask, x => x.ContinueWith(async (s, m, r, c) =>
            {
                managedTask = await s.ScheduleActionAsync(this, TaskName, false, () => Task.CompletedTask, x => x.WithPolicy(NamedManagedTaskPolicy.WaitAndStart));
                return managedTask;
            }));

            // Assert
            Assert.NotNull(scheduledTask);
            Assert.NotNull(scheduledTask.OnExecuted);
            await scheduledTask.OnExecuted;
            Assert.Equal(1, scheduledTask.Continuations.Length);
            Assert.Equal(managedTask, scheduledTask.Continuations[0]);
            Assert.Equal(TaskName, scheduledTask.Continuations[0].Name);
        }

        /// <summary>Verifies ScheduleAnonymous for anonymous continuation: triggers continuation.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task ScheduleAnonymous_AnonymousContinuation_TriggersContinuation()
        {
            // Arrange
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            IManagedAnonymousTask managedTask = null;

            // Act
            var scheduledTask = taskManager.ScheduleAnonymousAction(() => Task.CompletedTask, x => x.ContinueWith((s, m, r, c) =>
            {
                managedTask = s.ScheduleAnonymousAction(() => Task.CompletedTask);
                return managedTask;
            }));

            // Assert
            Assert.NotNull(scheduledTask);
            Assert.NotNull(scheduledTask.OnExecuted);
            await scheduledTask.OnExecuted;
            Assert.Equal(1, scheduledTask.AnonymousContinuations.Length);
            Assert.Equal(managedTask, scheduledTask.AnonymousContinuations[0]);
        }

        /// <summary>Verifies ScheduleAsync for action: executes task.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task ScheduleAsync_Action_ExecutesTask()
        {
            // Arrange
            const string ScheduledName = "MainTask";
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            bool executed = false;

            // Act
            var scheduledTask = await taskManager.ScheduleAsync(this, ScheduledName, false, () => executed = true);

            // Assert
            Assert.NotNull(scheduledTask);
            Assert.NotNull(scheduledTask.OnExecuted);
            Assert.Equal(ScheduledName, scheduledTask.Name);
            await scheduledTask.OnExecuted;
            Assert.True(executed);
        }

        /// <summary>Verifies ScheduleAsync for task returns value: returns output.</summary>
        [Theory(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(69)]
        [InlineData(420)]
        public async Task ScheduleAsync_TaskReturnsValue_ReturnsOutput(int output)
        {
            // Arrange
            const string ScheduledName = "MainTask";
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();

            // Act
            var scheduledTask = await taskManager.ScheduleAsync(this, ScheduledName, false, () => output);

            // Assert
            Assert.NotNull(scheduledTask);
            Assert.NotNull(scheduledTask.OnExecuted);
            Assert.Equal(ScheduledName, scheduledTask.Name);
            await scheduledTask.OnExecuted;
            var actual = scheduledTask.Result.CastToOrDefault<int>();
            Assert.Equal(output, actual);
        }

        /// <summary>Verifies ScheduleAsync for task throws: returns exception.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task ScheduleAsync_TaskThrows_ReturnsException()
        {
            // Arrange
            const string ScheduledName = "MainTask";
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();

            // Act
            var scheduledTask = await taskManager.ScheduleActionAsync(this, ScheduledName, false, () => throw new DivideByZeroException());

            // Assert
            Assert.NotNull(scheduledTask);
            Assert.NotNull(scheduledTask.OnExecuted);
            Assert.Equal(ScheduledName, scheduledTask.Name);
            await scheduledTask.OnExecuted;
            var actual = scheduledTask.Result.CastToOrDefault<Exception>();
            Assert.NotNull(actual);
            Assert.IsAssignableFrom<DivideByZeroException>(actual);
        }

        /// <summary>Verifies ScheduleAsync for token cancelled: cancels task.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task ScheduleAsync_TokenCancelled_CancelsTask()
        {
            // Arrange
            const string ScheduledName = "MainTask";
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            var tokenSource = new CancellationTokenSource();
            Exception exception = null;

            // Act
            var scheduledTask = await taskManager.ScheduleActionAsync(this, ScheduledName, false, () => Task.Delay(TimeSpan.FromMinutes(1), tokenSource.Token), token: tokenSource.Token);
            tokenSource.Cancel();

            // Assert
            Assert.NotNull(scheduledTask);
            Assert.NotNull(scheduledTask.OnExecuted);
            Assert.Equal(ScheduledName, scheduledTask.Name);
            await scheduledTask.OnExecuted;
            exception = scheduledTask.Result.CastToOrDefault<Exception>();
            Assert.NotNull(exception);
            Assert.IsAssignableFrom<OperationCanceledException>(exception);
        }

        /// <summary>Verifies ScheduleAsync for pre and post actions: executes both.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task ScheduleAsync_PreAndPostActions_ExecutesBoth()
        {
            // Arrange
            const string ScheduledName = "MainTask";
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            bool preExecuted = false;
            bool executed = false;
            bool postExecuted = false;

            // Act
            var scheduledTask = await taskManager.ScheduleAsync(this, ScheduledName, false, () => executed = true, x => x.ExecuteFirst(() => preExecuted = true).ExecuteAfter(() => postExecuted = true));

            // Assert
            Assert.NotNull(scheduledTask);
            Assert.NotNull(scheduledTask.OnExecuted);
            Assert.Equal(ScheduledName, scheduledTask.Name);
            await scheduledTask.OnExecuted;
            Assert.True(preExecuted);
            Assert.True(executed);
            Assert.True(postExecuted);
        }

        /// <summary>Verifies ScheduleAsync for pre action throws: captures exception.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task ScheduleAsync_PreActionThrows_CapturesException()
        {
            // Arrange
            const string ScheduledName = "MainTask";
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();

            // Act
            var scheduledTask = await taskManager.ScheduleActionAsync(this, ScheduledName, false, () => Task.CompletedTask, x => x.ExecuteFirst(() => throw new AbandonedMutexException()));

            // Assert
            Assert.NotNull(scheduledTask);
            Assert.NotNull(scheduledTask.OnExecuted);
            Assert.Equal(ScheduledName, scheduledTask.Name);
            await scheduledTask.OnExecuted;
            var actual = scheduledTask.Result.CastToOrDefault<Exception>();
            Assert.NotNull(actual);
            Assert.IsAssignableFrom<AbandonedMutexException>(actual);
        }

        /// <summary>Verifies ScheduleAsync for post action throws: captures exception.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task ScheduleAsync_PostActionThrows_CapturesException()
        {
            // Arrange
            const string ScheduledName = "MainTask";
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();

            // Act
            var scheduledTask = await taskManager.ScheduleActionAsync(this, ScheduledName, false, () => Task.CompletedTask, x => x.ExecuteAfter(() => throw new AbandonedMutexException()));

            // Assert
            Assert.NotNull(scheduledTask);
            Assert.NotNull(scheduledTask.OnExecuted);
            Assert.Equal(ScheduledName, scheduledTask.Name);
            await scheduledTask.OnExecuted;
            var actual = scheduledTask.Result.CastToOrDefault<Exception>();
            Assert.NotNull(actual);
            Assert.IsAssignableFrom<AbandonedMutexException>(actual);
        }

        /// <summary>Verifies ScheduleAsync for managed task options: applies options.</summary>
        [Theory(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        [InlineData(ManagedTaskOptions.GracefulCancellation)]
        [InlineData(ManagedTaskOptions.AutoRestart | ManagedTaskOptions.KeepAlive)]
        [InlineData(ManagedTaskOptions.None)]
        public async Task ScheduleAsync_ManagedTaskOptions_AppliesOptions(ManagedTaskOptions taskOptions)
        {
            // Arrange
            const string ScheduledName = "MainTask";
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();

            // Act
            var scheduledTask = await taskManager.ScheduleActionAsync(this, ScheduledName, false, () => Task.CompletedTask, x => x.WithManagedOptions(taskOptions));

            // Assert
            Assert.NotNull(scheduledTask);
            Assert.Equal(taskOptions, scheduledTask.Options);
        }

        /// <summary>Verifies ScheduleAsync for managed continuation: triggers continuation.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task ScheduleAsync_ManagedContinuation_TriggersContinuation()
        {
            // Arrange
            const string ScheduledName = "MainTask";
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            IManagedTask managedTask = null;

            // Act
            var scheduledTask = await taskManager.ScheduleActionAsync(this, ScheduledName, false, () => Task.CompletedTask, x => x.ContinueWith((s, m, r, c) =>
            {
                managedTask = s.TryScheduleAction(this, ScheduledName, false, () => Task.CompletedTask);
                return managedTask;
            }));

            // Assert
            Assert.NotNull(scheduledTask);
            Assert.NotNull(scheduledTask.OnExecuted);
            await scheduledTask.OnExecuted;
            Assert.Equal(1, scheduledTask.Continuations.Length);
            Assert.Equal(managedTask, scheduledTask.Continuations[0]);
        }

        /// <summary>Verifies ScheduleAsync for managed named continuation: triggers continuation.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task ScheduleAsync_ManagedNamedContinuation_TriggersContinuation()
        {
            // Arrange
            const string ScheduledName = "MainTask";
            const string TaskName = "Hello";
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            IManagedTask managedTask = null;

            // Act
            var scheduledTask = await taskManager.ScheduleActionAsync(this, ScheduledName, false, () => Task.CompletedTask, x => x.ContinueWith(async (s, m, r, c) =>
            {
                managedTask = await s.ScheduleActionAsync(this, TaskName, false, () => Task.CompletedTask, x => x.WithPolicy(NamedManagedTaskPolicy.WaitAndStart));
                return managedTask;
            }));

            // Assert
            Assert.NotNull(scheduledTask);
            Assert.NotNull(scheduledTask.OnExecuted);
            await scheduledTask.OnExecuted;
            Assert.Equal(1, scheduledTask.Continuations.Length);
            Assert.Equal(managedTask, scheduledTask.Continuations[0]);
            Assert.Equal(TaskName, scheduledTask.Continuations[0].Name);
        }

        /// <summary>Verifies ScheduleAsync for anonymous continuation: triggers continuation.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task ScheduleAsync_AnonymousContinuation_TriggersContinuation()
        {
            // Arrange
            const string ScheduledName = "MainTask";
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            IManagedAnonymousTask managedTask = null;

            // Act
            var scheduledTask = await taskManager.ScheduleActionAsync(this, ScheduledName, false, () => Task.CompletedTask, x => x.ContinueWith((s, m, r, c) =>
            {
                managedTask = s.ScheduleAnonymousAction(() => Task.CompletedTask);
                return managedTask;
            }));

            // Assert
            Assert.NotNull(scheduledTask);
            Assert.NotNull(scheduledTask.OnExecuted);
            await scheduledTask.OnExecuted;
            Assert.Equal(1, scheduledTask.AnonymousContinuations.Length);
            Assert.Equal(managedTask, scheduledTask.AnonymousContinuations[0]);
        }

        /// <summary>Verifies ScheduleAsync for named policy with cancellation: applies policy.</summary>
        [Theory(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        [InlineData(NamedManagedTaskPolicy.GracefulCancelAndStart)]
        [InlineData(NamedManagedTaskPolicy.CancelAndStart)]
        public async Task ScheduleAsync_NamedPolicyWithCancellation_AppliesPolicy(NamedManagedTaskPolicy policy)
        {
            // Arrange
            const string ScheduledName = "MainTask";
            _configureTaskManager = x =>
            {
                x.GracefulCancellationWaitTime = TimeSpan.FromSeconds(1);
                x.LongRunningGracefulCancellationWaitTime = TimeSpan.FromSeconds(1);
            };
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            bool executed = false;
            Exception exception = null;

            // Act
            var firstTask = taskManager.TryScheduleAction(this, ScheduledName, false, async t =>
            {
                await Task.Delay(TimeSpan.FromHours(1), t);
            });
            var scheduledTask = await taskManager.ScheduleActionAsync(this, ScheduledName, false, () => executed = true, x => x.WithPolicy(policy));

            // Assert
            Assert.NotNull(scheduledTask);
            Assert.NotNull(scheduledTask.OnExecuted);
            await scheduledTask.OnExecuted;
            Assert.True(executed);
            Assert.NotNull(firstTask);
            Assert.NotNull(firstTask.OnExecuted);
            exception = firstTask.Result.CastToOrDefault<Exception>();
            Assert.NotNull(exception);
            Assert.IsAssignableFrom<OperationCanceledException>(exception);
        }

        /// <summary>Verifies ScheduleAsync for wait and start policy: applies policy.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task ScheduleAsync_WaitAndStartPolicy_AppliesPolicy()
        {
            // Arrange
            const string ScheduledName = "MainTask";
            _configureTaskManager = x =>
            {
                x.GracefulCancellationWaitTime = TimeSpan.FromSeconds(1);
                x.LongRunningGracefulCancellationWaitTime = TimeSpan.FromSeconds(1);
            };
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            bool executed = false;
            Exception exception = null;

            // Act
            var firstTask = taskManager.TryScheduleAction(this, ScheduledName, false, async t =>
            {
                try
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(100), t);
                }
                catch (Exception ex)
                {
                    exception = ex;
                    throw;
                }
            });
            var scheduledTask = await taskManager.ScheduleActionAsync(this, ScheduledName, false, () => executed = true, x => x.WithPolicy(NamedManagedTaskPolicy.WaitAndStart));

            // Assert
            Assert.NotNull(scheduledTask);
            Assert.NotNull(scheduledTask.OnExecuted);
            await scheduledTask.OnExecuted;
            Assert.True(executed);
            Assert.Null(exception);
        }

        /// <summary>Verifies ScheduleAsync for exception policy: applies policy.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task ScheduleAsync_ExceptionPolicy_AppliesPolicy()
        {
            // Arrange
            const string ScheduledName = "MainTask";
            _configureTaskManager = x =>
            {
                x.GracefulCancellationWaitTime = TimeSpan.FromSeconds(1);
                x.LongRunningGracefulCancellationWaitTime = TimeSpan.FromSeconds(1);
            };
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            Exception exception = null;

            // Act
            var firstTask = taskManager.TryScheduleAction(this, ScheduledName, false, t => Task.Delay(TimeSpan.FromHours(1), t));
            try
            {
                _ = await taskManager.ScheduleActionAsync(this, ScheduledName, false, () => Task.CompletedTask, x => x.WithPolicy(NamedManagedTaskPolicy.Exception));
            }
            catch(Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.NotNull(exception);
            Assert.IsAssignableFrom<InvalidOperationException>(exception);   
        }

        /// <summary>Verifies ScheduleAsync for global named policy with cancellation: applies policy.</summary>
        [Theory(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        [InlineData(NamedManagedTaskPolicy.GracefulCancelAndStart)]
        [InlineData(NamedManagedTaskPolicy.CancelAndStart)]
        public async Task ScheduleAsync_GlobalNamedPolicyWithCancellation_AppliesPolicy(NamedManagedTaskPolicy policy)
        {
            // Arrange
            const string ScheduledName = "MainTask";
            _configureTaskManager = x =>
            {
                x.GracefulCancellationWaitTime = TimeSpan.FromSeconds(1);
                x.LongRunningGracefulCancellationWaitTime = TimeSpan.FromSeconds(1);
            };
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            bool executed = false;
            Exception exception = null;

            // Act
            var firstTask = taskManager.TryScheduleAction(new object(), ScheduledName, true, async t =>
            {
                try
                {
                    await Task.Delay(TimeSpan.FromHours(1), t);
                }
                catch (Exception ex)
                {
                    exception = ex;
                    throw;
                }
            });
            var scheduledTask = await taskManager.ScheduleActionAsync(new object(), ScheduledName, true, () => executed = true, x => x.WithPolicy(policy));

            // Assert
            Assert.NotNull(scheduledTask);
            Assert.NotNull(scheduledTask.OnExecuted);
            await scheduledTask.OnExecuted;
            Assert.True(executed);
            Assert.NotNull(exception);
            Assert.IsAssignableFrom<OperationCanceledException>(exception);
        }

        /// <summary>Verifies ScheduleAsync for global wait and start policy: applies policy.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task ScheduleAsync_GlobalWaitAndStartPolicy_AppliesPolicy()
        {
            // Arrange
            const string ScheduledName = "MainTask";
            _configureTaskManager = x =>
            {
                x.GracefulCancellationWaitTime = TimeSpan.FromSeconds(1);
                x.LongRunningGracefulCancellationWaitTime = TimeSpan.FromSeconds(1);
            };
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            bool executed = false;
            Exception exception = null;

            // Act
            var firstTask = taskManager.TryScheduleAction(new object(), ScheduledName, true, async t =>
            {
                try
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(100), t);
                }
                catch (Exception ex)
                {
                    exception = ex;
                    throw;
                }
            });
            var scheduledTask = await taskManager.ScheduleActionAsync(new object(), ScheduledName, true, () => executed = true, x => x.WithPolicy(NamedManagedTaskPolicy.WaitAndStart));

            // Assert
            Assert.NotNull(scheduledTask);
            Assert.NotNull(scheduledTask.OnExecuted);
            await scheduledTask.OnExecuted;
            Assert.True(executed);
            Assert.Null(exception);
        }

        /// <summary>Verifies ScheduleAsync for global exception policy: applies policy.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task ScheduleAsync_GlobalExceptionPolicy_AppliesPolicy()
        {
            // Arrange
            const string ScheduledName = "MainTask";
            _configureTaskManager = x =>
            {
                x.GracefulCancellationWaitTime = TimeSpan.FromSeconds(1);
                x.LongRunningGracefulCancellationWaitTime = TimeSpan.FromSeconds(1);
            };
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            Exception exception = null;

            // Act
            var firstTask = taskManager.TryScheduleAction(new object(), ScheduledName, true, t => Task.Delay(TimeSpan.FromHours(1), t));
            try
            {
                _ = await taskManager.ScheduleActionAsync(new object(), ScheduledName, true, () => Task.CompletedTask, x => x.WithPolicy(NamedManagedTaskPolicy.Exception));
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.NotNull(exception);
            Assert.IsAssignableFrom<InvalidOperationException>(exception);
        }

        /// <summary>Verifies TrySchedule for action: executes task.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task TrySchedule_Action_ExecutesTask()
        {
            // Arrange
            const string ScheduledName = "MainTask";
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            bool executed = false;

            // Act
            var scheduledTask = taskManager.TrySchedule(this, ScheduledName, false, () => executed = true);

            // Assert
            Assert.NotNull(scheduledTask);
            Assert.NotNull(scheduledTask.OnExecuted);
            Assert.Equal(ScheduledName, scheduledTask.Name);
            await scheduledTask.OnExecuted;
            Assert.True(executed);
        }

        /// <summary>Verifies TrySchedule for task returns value: returns output.</summary>
        [Theory(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(69)]
        [InlineData(420)]
        public async Task TrySchedule_TaskReturnsValue_ReturnsOutput(int output)
        {
            // Arrange
            const string ScheduledName = "MainTask";
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();

            // Act
            var scheduledTask = taskManager.TrySchedule(this, ScheduledName, false, () => output);

            // Assert
            Assert.NotNull(scheduledTask);
            Assert.NotNull(scheduledTask.OnExecuted);
            Assert.Equal(ScheduledName, scheduledTask.Name);
            await scheduledTask.OnExecuted;
            var actual = scheduledTask.Result.CastToOrDefault<int>();
            Assert.Equal(output, actual);
        }

        /// <summary>Verifies TrySchedule for task throws: returns exception.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task TrySchedule_TaskThrows_ReturnsException()
        {
            // Arrange
            const string ScheduledName = "MainTask";
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();

            // Act
            var scheduledTask = taskManager.TryScheduleAction(this, ScheduledName, false, () => throw new DivideByZeroException());

            // Assert
            Assert.NotNull(scheduledTask);
            Assert.NotNull(scheduledTask.OnExecuted);
            Assert.Equal(ScheduledName, scheduledTask.Name);
            await scheduledTask.OnExecuted;
            var actual = scheduledTask.Result.CastToOrDefault<Exception>();
            Assert.NotNull(actual);
            Assert.IsAssignableFrom<DivideByZeroException>(actual);
        }

        /// <summary>Verifies TrySchedule for token cancelled: cancels task.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task TrySchedule_TokenCancelled_CancelsTask()
        {
            // Arrange
            const string ScheduledName = "MainTask";
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            var tokenSource = new CancellationTokenSource();
            Exception exception = null;

            // Act
            var scheduledTask = taskManager.TryScheduleAction(this, ScheduledName, false, () => Task.Delay(TimeSpan.FromMinutes(1), tokenSource.Token), token: tokenSource.Token);
            tokenSource.Cancel();

            // Assert
            Assert.NotNull(scheduledTask);
            Assert.NotNull(scheduledTask.OnExecuted);
            Assert.Equal(ScheduledName, scheduledTask.Name);
            await scheduledTask.OnExecuted;
            exception = scheduledTask.Result.CastToOrDefault<Exception>();
            Assert.NotNull(exception);
            Assert.IsAssignableFrom<OperationCanceledException>(exception);
        }

        /// <summary>Verifies TrySchedule for pre and post actions: executes both.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task TrySchedule_PreAndPostActions_ExecutesBoth()
        {
            // Arrange
            const string ScheduledName = "MainTask";
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            bool preExecuted = false;
            bool executed = false;
            bool postExecuted = false;

            // Act
            var scheduledTask = taskManager.TrySchedule(this, ScheduledName, false, () => executed = true, x => x.ExecuteFirst(() => preExecuted = true).ExecuteAfter(() => postExecuted = true));

            // Assert
            Assert.NotNull(scheduledTask);
            Assert.NotNull(scheduledTask.OnExecuted);
            Assert.Equal(ScheduledName, scheduledTask.Name);
            await scheduledTask.OnExecuted;
            Assert.True(preExecuted);
            Assert.True(executed);
            Assert.True(postExecuted);
        }

        /// <summary>Verifies TrySchedule for pre action throws: captures exception.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task TrySchedule_PreActionThrows_CapturesException()
        {
            // Arrange
            const string ScheduledName = "MainTask";
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();

            // Act
            var scheduledTask = taskManager.TryScheduleAction(this, ScheduledName, false, () => Task.CompletedTask, x => x.ExecuteFirst(() => throw new AbandonedMutexException()));

            // Assert
            Assert.NotNull(scheduledTask);
            Assert.NotNull(scheduledTask.OnExecuted);
            Assert.Equal(ScheduledName, scheduledTask.Name);
            await scheduledTask.OnExecuted;
            var actual = scheduledTask.Result.CastToOrDefault<Exception>();
            Assert.NotNull(actual);
            Assert.IsAssignableFrom<AbandonedMutexException>(actual);
        }

        /// <summary>Verifies TrySchedule for post action throws: captures exception.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task TrySchedule_PostActionThrows_CapturesException()
        {
            // Arrange
            const string ScheduledName = "MainTask";
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();

            // Act
            var scheduledTask = taskManager.TryScheduleAction(this, ScheduledName, false, () => Task.CompletedTask, x => x.ExecuteAfter(() => throw new AbandonedMutexException()));

            // Assert
            Assert.NotNull(scheduledTask);
            Assert.NotNull(scheduledTask.OnExecuted);
            Assert.Equal(ScheduledName, scheduledTask.Name);
            await scheduledTask.OnExecuted;
            var actual = scheduledTask.Result.CastToOrDefault<Exception>();
            Assert.NotNull(actual);
            Assert.IsAssignableFrom<AbandonedMutexException>(actual);
        }

        /// <summary>Verifies TrySchedule for managed task options: applies options.</summary>
        [Theory(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        [InlineData(ManagedTaskOptions.GracefulCancellation)]
        [InlineData(ManagedTaskOptions.AutoRestart | ManagedTaskOptions.KeepAlive)]
        [InlineData(ManagedTaskOptions.None)]
        public async Task TrySchedule_ManagedTaskOptions_AppliesOptions(ManagedTaskOptions taskOptions)
        {
            // Arrange
            const string ScheduledName = "MainTask";
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();

            // Act
            var scheduledTask = taskManager.TryScheduleAction(this, ScheduledName, false, () => Task.CompletedTask, x => x.WithManagedOptions(taskOptions));

            // Assert
            Assert.NotNull(scheduledTask);
            Assert.Equal(taskOptions, scheduledTask.Options);
        }

        /// <summary>Verifies TrySchedule for managed continuation: triggers continuation.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task TrySchedule_ManagedContinuation_TriggersContinuation()
        {
            // Arrange
            const string ScheduledName = "MainTask";
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            IManagedTask managedTask = null;

            // Act
            var scheduledTask = taskManager.TryScheduleAction(this, ScheduledName, false, () => Task.CompletedTask, x => x.ContinueWith((s, m, r, c) =>
            {
                managedTask = s.TryScheduleAction(this, ScheduledName, false, () => Task.CompletedTask);
                return managedTask;
            }));

            // Assert
            Assert.NotNull(scheduledTask);
            Assert.NotNull(scheduledTask.OnExecuted);
            await scheduledTask.OnExecuted;
            Assert.Equal(1, scheduledTask.Continuations.Length);
            Assert.Equal(managedTask, scheduledTask.Continuations[0]);
        }

        /// <summary>Verifies TrySchedule for managed named continuation: triggers continuation.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task TrySchedule_ManagedNamedContinuation_TriggersContinuation()
        {
            // Arrange
            const string ScheduledName = "MainTask";
            const string TaskName = "Hello";
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            IManagedTask managedTask = null;

            // Act
            var scheduledTask = taskManager.TryScheduleAction(this, ScheduledName, false, () => Task.CompletedTask, x => x.ContinueWith(async (s, m, r, c) =>
            {
                managedTask = await s.ScheduleActionAsync(this, TaskName, false, () => Task.CompletedTask, x => x.WithPolicy(NamedManagedTaskPolicy.WaitAndStart));
                return managedTask;
            }));

            // Assert
            Assert.NotNull(scheduledTask);
            Assert.NotNull(scheduledTask.OnExecuted);
            await scheduledTask.OnExecuted;
            Assert.Equal(1, scheduledTask.Continuations.Length);
            Assert.Equal(managedTask, scheduledTask.Continuations[0]);
            Assert.Equal(TaskName, scheduledTask.Continuations[0].Name);
        }

        /// <summary>Verifies TrySchedule for anonymous continuation: triggers continuation.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task TrySchedule_AnonymousContinuation_TriggersContinuation()
        {
            // Arrange
            const string ScheduledName = "MainTask";
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            IManagedAnonymousTask managedTask = null;

            // Act
            var scheduledTask = taskManager.TryScheduleAction(this, ScheduledName, false, () => Task.CompletedTask, x => x.ContinueWith((s, m, r, c) =>
            {
                managedTask = s.ScheduleAnonymousAction(() => Task.CompletedTask);
                return managedTask;
            }));

            // Assert
            Assert.NotNull(scheduledTask);
            Assert.NotNull(scheduledTask.OnExecuted);
            await scheduledTask.OnExecuted;
            Assert.Equal(1, scheduledTask.AnonymousContinuations.Length);
            Assert.Equal(managedTask, scheduledTask.AnonymousContinuations[0]);
        }

        private sealed class HashOwner
        {
            private readonly int _hashCode;

            public HashOwner(int hashCode)
            {
                _hashCode = hashCode;
            }

            public override int GetHashCode() => _hashCode;
        }
    }
}