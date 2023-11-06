using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sels.Core.Async.Test.Components.TaskManagement
{
    public class TaskManager_ScheduleAsync
    {
        [Test, Timeout(60000)]
        public async Task TaskIsScheduledAndExecuted()
        {
            // Arrange
            const string ScheduledName = "MainTask";
            var provider = TestHelper.GetTaskManagerContainer();
            await using var scope = provider.CreateAsyncScope();
            provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            bool executed = false;

            // Act
            var scheduledTask = await taskManager.ScheduleAsync(this, ScheduledName, false, () => executed = true);

            // Assert
            Assert.IsNotNull(scheduledTask);
            Assert.IsNotNull(scheduledTask.OnExecuted);
            Assert.That(scheduledTask.Name, Is.EqualTo(ScheduledName));
            await scheduledTask.OnExecuted;
            Assert.IsTrue(executed);
        }

        [TestCase(1)]
        [TestCase(5)]
        [TestCase(10)]
        [TestCase(69)]
        [TestCase(420)]
        [Timeout(60000)]
        public async Task CorrectOutputIsReturnedFromTask(int output)
        {
            // Arrange
            const string ScheduledName = "MainTask";
            var provider = TestHelper.GetTaskManagerContainer();
            await using var scope = provider.CreateAsyncScope();
            provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();

            // Act
            var scheduledTask = await taskManager.ScheduleAsync(this, ScheduledName, false, () => output);

            // Assert
            Assert.IsNotNull(scheduledTask);
            Assert.IsNotNull(scheduledTask.OnExecuted);
            Assert.That(scheduledTask.Name, Is.EqualTo(ScheduledName));
            await scheduledTask.OnExecuted;
            var actual = scheduledTask.Result.CastToOrDefault<int>();
            Assert.AreEqual(output, actual);
        }

        [Test, Timeout(60000)]
        public async Task CorrectExceptionIsReturnedFromTask()
        {
            // Arrange
            const string ScheduledName = "MainTask";
            var provider = TestHelper.GetTaskManagerContainer();
            await using var scope = provider.CreateAsyncScope();
            provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();

            // Act
            var scheduledTask = await taskManager.ScheduleActionAsync(this, ScheduledName, false, () => throw new DivideByZeroException());

            // Assert
            Assert.IsNotNull(scheduledTask);
            Assert.IsNotNull(scheduledTask.OnExecuted);
            Assert.That(scheduledTask.Name, Is.EqualTo(ScheduledName));
            await scheduledTask.OnExecuted;
            var actual = scheduledTask.Result.CastToOrDefault<Exception>();
            Assert.IsNotNull(actual);
            Assert.That(actual, Is.AssignableTo<DivideByZeroException>());
        }

        [Test, Timeout(60000)]
        public async Task TaskIsCancelledWhenTokenGetsCancelled()
        {
            // Arrange
            const string ScheduledName = "MainTask";
            var provider = TestHelper.GetTaskManagerContainer();
            await using var scope = provider.CreateAsyncScope();
            provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            var tokenSource = new CancellationTokenSource();
            Exception exception = null;

            // Act
            var scheduledTask = await taskManager.ScheduleActionAsync(this, ScheduledName, false, () => Task.Delay(TimeSpan.FromMinutes(1), tokenSource.Token), token: tokenSource.Token);
            tokenSource.Cancel();

            // Assert
            Assert.IsNotNull(scheduledTask);
            Assert.IsNotNull(scheduledTask.OnExecuted);
            Assert.That(scheduledTask.Name, Is.EqualTo(ScheduledName));
            await scheduledTask.OnExecuted;
            exception = scheduledTask.Result.CastToOrDefault<Exception>();
            Assert.IsNotNull(exception);
            Assert.That(exception, Is.AssignableTo<OperationCanceledException>());
        }

        [Test, Timeout(60000)]
        public async Task PreAndPostExecutionAreExecuted()
        {
            // Arrange
            const string ScheduledName = "MainTask";
            var provider = TestHelper.GetTaskManagerContainer();
            await using var scope = provider.CreateAsyncScope();
            provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            bool preExecuted = false;
            bool executed = false;
            bool postExecuted = false;

            // Act
            var scheduledTask = await taskManager.ScheduleAsync(this, ScheduledName, false, () => executed = true, x => x.ExecuteFirst(() => preExecuted = true).ExecuteAfter(() => postExecuted = true));

            // Assert
            Assert.IsNotNull(scheduledTask);
            Assert.IsNotNull(scheduledTask.OnExecuted);
            Assert.That(scheduledTask.Name, Is.EqualTo(ScheduledName));
            await scheduledTask.OnExecuted;
            Assert.IsTrue(preExecuted);
            Assert.IsTrue(executed);
            Assert.IsTrue(postExecuted);
        }

        [Test, Timeout(60000)]
        public async Task ExceptionThrownInPreActionIsCaptured()
        {
            // Arrange
            const string ScheduledName = "MainTask";
            var provider = TestHelper.GetTaskManagerContainer();
            await using var scope = provider.CreateAsyncScope();
            provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();

            // Act
            var scheduledTask = await taskManager.ScheduleActionAsync(this, ScheduledName, false, () => Task.CompletedTask, x => x.ExecuteFirst(() => throw new AbandonedMutexException()));

            // Assert
            Assert.IsNotNull(scheduledTask);
            Assert.IsNotNull(scheduledTask.OnExecuted);
            Assert.That(scheduledTask.Name, Is.EqualTo(ScheduledName));
            await scheduledTask.OnExecuted;
            var actual = scheduledTask.Result.CastToOrDefault<Exception>();
            Assert.IsNotNull(actual);
            Assert.That(actual, Is.AssignableTo<AbandonedMutexException>());
        }

        [Test, Timeout(60000)]
        public async Task ExceptionThrownInPostActionIsCaptured()
        {
            // Arrange
            const string ScheduledName = "MainTask";
            var provider = TestHelper.GetTaskManagerContainer();
            await using var scope = provider.CreateAsyncScope();
            provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();

            // Act
            var scheduledTask = await taskManager.ScheduleActionAsync(this, ScheduledName, false, () => Task.CompletedTask, x => x.ExecuteAfter(() => throw new AbandonedMutexException()));

            // Assert
            Assert.IsNotNull(scheduledTask);
            Assert.IsNotNull(scheduledTask.OnExecuted);
            Assert.That(scheduledTask.Name, Is.EqualTo(ScheduledName));
            await scheduledTask.OnExecuted;
            var actual = scheduledTask.Result.CastToOrDefault<Exception>();
            Assert.IsNotNull(actual);
            Assert.That(actual, Is.AssignableTo<AbandonedMutexException>());
        }

        [TestCase(ManagedTaskOptions.GracefulCancellation)]
        [TestCase(ManagedTaskOptions.AutoRestart | ManagedTaskOptions.KeepAlive)]
        [TestCase(ManagedTaskOptions.None)]
        [Timeout(60000)]
        public async Task TaskIsScheduledWithExpectedManagedTaskOptions(ManagedTaskOptions taskOptions)
        {
            // Arrange
            const string ScheduledName = "MainTask";
            var provider = TestHelper.GetTaskManagerContainer();
            await using var scope = provider.CreateAsyncScope();
            provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();

            // Act
            var scheduledTask = await taskManager.ScheduleActionAsync(this, ScheduledName, false, () => Task.CompletedTask, x => x.WithManagedOptions(taskOptions));

            // Assert
            Assert.IsNotNull(scheduledTask);
            Assert.AreEqual(taskOptions, scheduledTask.Options);
        }

        [Test, Timeout(60000)]
        public async Task ManagedContinuationIsTriggered()
        {
            // Arrange
            const string ScheduledName = "MainTask";
            var provider = TestHelper.GetTaskManagerContainer();
            await using var scope = provider.CreateAsyncScope();
            provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            IManagedTask managedTask = null;

            // Act
            var scheduledTask = await taskManager.ScheduleActionAsync(this, ScheduledName, false, () => Task.CompletedTask, x => x.ContinueWith((s, m, r, c) =>
            {
                managedTask = s.TryScheduleAction(this, ScheduledName, false, () => Task.CompletedTask);
                return managedTask;
            }));

            // Assert
            Assert.IsNotNull(scheduledTask);
            Assert.IsNotNull(scheduledTask.OnExecuted);
            await scheduledTask.OnExecuted;
            Assert.That(scheduledTask.Continuations.Length, Is.EqualTo(1));
            Assert.That(scheduledTask.Continuations[0], Is.EqualTo(managedTask));
        }

        [Test, Timeout(60000)]
        public async Task ManagedNamedContinuationIsTriggered()
        {
            // Arrange
            const string ScheduledName = "MainTask";
            const string TaskName = "Hello";
            var provider = TestHelper.GetTaskManagerContainer();
            await using var scope = provider.CreateAsyncScope();
            provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            IManagedTask managedTask = null;

            // Act
            var scheduledTask = await taskManager.ScheduleActionAsync(this, ScheduledName, false, () => Task.CompletedTask, x => x.ContinueWith(async (s, m, r, c) =>
            {
                managedTask = await s.ScheduleActionAsync(this, TaskName, false, () => Task.CompletedTask, x => x.WithPolicy(NamedManagedTaskPolicy.WaitAndStart));
                return managedTask;
            }));

            // Assert
            Assert.IsNotNull(scheduledTask);
            Assert.IsNotNull(scheduledTask.OnExecuted);
            await scheduledTask.OnExecuted;
            Assert.That(scheduledTask.Continuations.Length, Is.EqualTo(1));
            Assert.That(scheduledTask.Continuations[0], Is.EqualTo(managedTask));
            Assert.That(scheduledTask.Continuations[0].Name, Is.EqualTo(TaskName));
        }

        [Test, Timeout(60000)]
        public async Task AnonymousContinuationIsTriggered()
        {
            // Arrange
            const string ScheduledName = "MainTask";
            var provider = TestHelper.GetTaskManagerContainer();
            await using var scope = provider.CreateAsyncScope();
            provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            IManagedAnonymousTask managedTask = null;

            // Act
            var scheduledTask = await taskManager.ScheduleActionAsync(this, ScheduledName, false, () => Task.CompletedTask, x => x.ContinueWith((s, m, r, c) =>
            {
                managedTask = s.ScheduleAnonymousAction(() => Task.CompletedTask);
                return managedTask;
            }));

            // Assert
            Assert.IsNotNull(scheduledTask);
            Assert.IsNotNull(scheduledTask.OnExecuted);
            await scheduledTask.OnExecuted;
            Assert.That(scheduledTask.AnonymousContinuations.Length, Is.EqualTo(1));
            Assert.That(scheduledTask.AnonymousContinuations[0], Is.EqualTo(managedTask));
        }

        [TestCase(NamedManagedTaskPolicy.GracefulCancelAndStart)]
        [TestCase(NamedManagedTaskPolicy.CancelAndStart)]
        [Timeout(60000)]
        public async Task NamedPolicyWithCancellationIsCorrectlyApplied(NamedManagedTaskPolicy policy)
        {
            // Arrange
            const string ScheduledName = "MainTask";
            var provider = TestHelper.GetTaskManagerContainer(x =>
            {
                x.GracefulCancellationWaitTime = TimeSpan.FromSeconds(1);
                x.LongRunningGracefulCancellationWaitTime = TimeSpan.FromSeconds(1);
            });
            await using var scope = provider.CreateAsyncScope();
            provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            bool executed = false;
            Exception exception = null;

            // Act
            var firstTask = taskManager.TryScheduleAction(this, ScheduledName, false, async t =>
            {
                try
                {
                    await Task.Delay(TimeSpan.FromHours(1), t);
                }
                catch(Exception ex)
                {
                    exception = ex;
                    throw;
                }
            });
            var scheduledTask = await taskManager.ScheduleActionAsync(this, ScheduledName, false, () => executed = true, x => x.WithPolicy(policy));

            // Assert
            Assert.IsNotNull(scheduledTask);
            Assert.IsNotNull(scheduledTask.OnExecuted);
            await scheduledTask.OnExecuted;
            Assert.IsTrue(executed);
            Assert.IsNotNull(exception);
            Assert.That(exception, Is.AssignableTo<OperationCanceledException>());
        }

        [Test, Timeout(60000)]
        public async Task WaitAndStartPolicyIsApplied()
        {
            // Arrange
            const string ScheduledName = "MainTask";
            var provider = TestHelper.GetTaskManagerContainer(x =>
            {
                x.GracefulCancellationWaitTime = TimeSpan.FromSeconds(1);
                x.LongRunningGracefulCancellationWaitTime = TimeSpan.FromSeconds(1);
            });
            await using var scope = provider.CreateAsyncScope();
            provider = scope.ServiceProvider;
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
            Assert.IsNotNull(scheduledTask);
            Assert.IsNotNull(scheduledTask.OnExecuted);
            await scheduledTask.OnExecuted;
            Assert.IsTrue(executed);
            Assert.IsNull(exception);
        }
        [Test, Timeout(60000)]
        public async Task ExceptionPolicyIsApplied()
        {
            // Arrange
            const string ScheduledName = "MainTask";
            var provider = TestHelper.GetTaskManagerContainer(x =>
            {
                x.GracefulCancellationWaitTime = TimeSpan.FromSeconds(1);
                x.LongRunningGracefulCancellationWaitTime = TimeSpan.FromSeconds(1);
            });
            await using var scope = provider.CreateAsyncScope();
            provider = scope.ServiceProvider;
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
            Assert.IsNotNull(exception);
            Assert.That(exception, Is.AssignableTo<InvalidOperationException>());   
        }

        [TestCase(NamedManagedTaskPolicy.GracefulCancelAndStart)]
        [TestCase(NamedManagedTaskPolicy.CancelAndStart)]
        [Timeout(60000)]
        public async Task NamedPolicyWithCancellationIsCorrectlyAppliedForGlobalTask(NamedManagedTaskPolicy policy)
        {
            // Arrange
            const string ScheduledName = "MainTask";
            var provider = TestHelper.GetTaskManagerContainer(x =>
            {
                x.GracefulCancellationWaitTime = TimeSpan.FromSeconds(1);
                x.LongRunningGracefulCancellationWaitTime = TimeSpan.FromSeconds(1);
            });
            await using var scope = provider.CreateAsyncScope();
            provider = scope.ServiceProvider;
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
            Assert.IsNotNull(scheduledTask);
            Assert.IsNotNull(scheduledTask.OnExecuted);
            await scheduledTask.OnExecuted;
            Assert.IsTrue(executed);
            Assert.IsNotNull(exception);
            Assert.That(exception, Is.AssignableTo<OperationCanceledException>());
        }

        [Test, Timeout(60000)]
        public async Task WaitAndStartPolicyIsAppliedForGlobalTask()
        {
            // Arrange
            const string ScheduledName = "MainTask";
            var provider = TestHelper.GetTaskManagerContainer(x =>
            {
                x.GracefulCancellationWaitTime = TimeSpan.FromSeconds(1);
                x.LongRunningGracefulCancellationWaitTime = TimeSpan.FromSeconds(1);
            });
            await using var scope = provider.CreateAsyncScope();
            provider = scope.ServiceProvider;
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
            Assert.IsNotNull(scheduledTask);
            Assert.IsNotNull(scheduledTask.OnExecuted);
            await scheduledTask.OnExecuted;
            Assert.IsTrue(executed);
            Assert.IsNull(exception);
        }
        [Test, Timeout(60000)]
        public async Task ExceptionPolicyIsAppliedForGlobalTask()
        {
            // Arrange
            const string ScheduledName = "MainTask";
            var provider = TestHelper.GetTaskManagerContainer(x =>
            {
                x.GracefulCancellationWaitTime = TimeSpan.FromSeconds(1);
                x.LongRunningGracefulCancellationWaitTime = TimeSpan.FromSeconds(1);
            });
            await using var scope = provider.CreateAsyncScope();
            provider = scope.ServiceProvider;
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
            Assert.IsNotNull(exception);
            Assert.That(exception, Is.AssignableTo<InvalidOperationException>());
        }
    }
}
