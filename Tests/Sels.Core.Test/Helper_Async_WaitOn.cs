using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sels.Core.Test
{
    public class Helper_Async_WaitOn
    {
        [Test]
        [Timeout(10000)]
        public async Task ReturnsWhenTaskCompletes()
        {
            // Arrange

            // Act
            var delayedTask = Task.Delay(1000);
            await Helper.Async.WaitOn(delayedTask, TimeSpan.FromMinutes(1));

            // Assert
            Assert.IsTrue(delayedTask.IsCompleted);
        }

        [Test]
        [Timeout(10000)]
        public async Task ThrowsTimeoutExceptionWhenTaskDoesNotCompleteWithinTime()
        {
            // Arrange
            var taskSource = new TaskCompletionSource();
            Exception exception = null;

            // Act
            try
            {
                await Helper.Async.WaitOn(taskSource.Task, TimeSpan.FromMilliseconds(1));
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNotNull(exception);
            Assert.IsInstanceOf<TimeoutException>(exception);
        }

        [Test]
        [Timeout(10000)]
        public async Task ThrowsTaskCanceledExceptionWhenCancellationTokenIsCanceled()
        {
            // Arrange
            var taskSource = new TaskCompletionSource();
            var cancellationTokenSource = new CancellationTokenSource();
            Exception exception = null;

            // Act
            try
            {
                cancellationTokenSource.CancelAfter(1000);
                await Helper.Async.WaitOn(taskSource.Task, TimeSpan.FromDays(1), cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNotNull(exception);
            Assert.IsInstanceOf<TaskCanceledException>(exception);
        }

        [Test]
        [Timeout(10000)]
        public async Task GenericTask_ReturnsWhenTaskCompletes()
        {
            // Arrange
            var taskSource = new TaskCompletionSource<object>();

            // Act
            _ = Task.Run(async () =>
            {
                await Helper.Async.Sleep(1000);
                taskSource.SetResult(new object());
            });
            await Helper.Async.WaitOn(taskSource.Task, TimeSpan.FromMinutes(1));

            // Assert
            Assert.IsTrue(taskSource.Task.IsCompleted);
        }

        [Test]
        [Timeout(10000)]
        public async Task GenericTask_ThrowsTimeoutExceptionWhenTaskDoesNotCompleteWithinTime()
        {
            // Arrange
            var taskSource = new TaskCompletionSource<object>();
            Exception exception = null;

            // Act
            try
            {
                await Helper.Async.WaitOn(taskSource.Task, TimeSpan.FromMilliseconds(1));
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNotNull(exception);
            Assert.IsInstanceOf<TimeoutException>(exception);
        }

        [Test]
        [Timeout(10000)]
        public async Task GenericTask_ThrowsTaskCanceledExceptionWhenCancellationTokenIsCanceled()
        {
            // Arrange
            var taskSource = new TaskCompletionSource<object>();
            var cancellationTokenSource = new CancellationTokenSource();
            Exception exception = null;

            // Act
            try
            {
                cancellationTokenSource.CancelAfter(1000);
                await Helper.Async.WaitOn(taskSource.Task, TimeSpan.FromDays(1), cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNotNull(exception);
            Assert.IsInstanceOf<TaskCanceledException>(exception);
        }
    }
}
