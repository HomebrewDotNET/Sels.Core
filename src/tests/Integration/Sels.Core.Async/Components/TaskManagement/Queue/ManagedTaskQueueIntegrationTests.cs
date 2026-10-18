using Microsoft.Extensions.DependencyInjection;
using Sels.Core.Async.TaskManagement;
using Sels.Core.Models;

namespace Sels.Core.Async.IntegrationTests.Components.TaskManagement.Queue
{
    /// <summary>Integration tests for real managed local-queue behavior.</summary>
    [Collection(AsyncIntegrationCollection.Name)]
    [Trait("Category", "Integration")]
    public class ManagedTaskQueueIntegrationTests : AsyncIntegrationTestBase
    {
        /// <inheritdoc/>
        protected override void ConfigureServices(IServiceCollection services) => RegisterTaskManager(services);

        /// <summary>Queue wait completes after task acceptance, without waiting for gated execution.</summary>
        [Fact]
        public async Task WaitOnQueuedAsync_AcceptedWorkStillRunning_CompletesAtAcceptance()
        {
            var manager = ServiceProvider.GetRequiredService<ITaskManager>();
            using var queue = manager.CreateLocalQueue(this, 1);
            var started = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var pending = await queue.EnqueueAsync((taskManager, token) => taskManager.ScheduleAnonymousAction(async cancellationToken =>
            {
                started.TrySetResult(true);
                await release.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
            }, token: token));

            try
            {
                await started.Task.WaitAsync(TimeSpan.FromSeconds(10));

                await queue.WaitOnQueuedAsync().WaitAsync(TimeSpan.FromSeconds(10));

                Assert.True(pending.Callback.IsCompletedSuccessfully);
                Assert.False(pending.Callback.Result.OnFinalized.IsCompleted);
            }
            finally
            {
                release.TrySetResult(true);
            }

            await pending.WaitOnFinalization().WaitAsync(TimeSpan.FromSeconds(10));
        }

        /// <summary>Queue wait remains pending until a dequeued item receives an accepted managed-task handle.</summary>
        [Fact]
        public async Task WaitOnQueuedAsync_SchedulerAcceptanceGated_WaitsForHandle()
        {
            var manager = ServiceProvider.GetRequiredService<ITaskManager>();
            using var queue = manager.CreateLocalQueue(this, 1);
            var schedulerStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var accept = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var workStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var releaseWork = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            Func<ITaskManager, CancellationToken, Task<IManagedTask>> scheduler = async (taskManager, token) =>
            {
                schedulerStarted.TrySetResult(true);
                await accept.Task.WaitAsync(token).ConfigureAwait(false);
                return await taskManager.ScheduleAsync(this, "acceptance-probe", false, async cancellationToken =>
                {
                    workStarted.TrySetResult(true);
                    await releaseWork.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
                    return Null.Value;
                }, token: token).ConfigureAwait(false);
            };
            var pending = await queue.EnqueueAsync(scheduler);

            try
            {
                await schedulerStarted.Task.WaitAsync(TimeSpan.FromSeconds(10));
                var wait = queue.WaitOnQueuedAsync();

                Assert.False(wait.IsCompleted);

                accept.TrySetResult(true);
                await wait.WaitAsync(TimeSpan.FromSeconds(10));
                await workStarted.Task.WaitAsync(TimeSpan.FromSeconds(10));

                Assert.True(pending.Callback.IsCompletedSuccessfully);
                Assert.False(pending.Callback.Result.OnFinalized.IsCompleted);
            }
            finally
            {
                accept.TrySetResult(true);
                releaseWork.TrySetResult(true);
            }

            await pending.WaitOnFinalization().WaitAsync(TimeSpan.FromSeconds(10));
        }

        /// <summary>The scheduler failure is surfaced as the same exception from the pending callback.</summary>
        [Fact(Timeout = AsyncIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task EnqueueAsync_SchedulerThrows_PropagatesSameException()
        {
            var manager = ServiceProvider.GetRequiredService<ITaskManager>();
            using var queue = manager.CreateLocalQueue(this, 1);
            var expected = new InvalidOperationException("scheduler failed");
            Func<ITaskManager, CancellationToken, IManagedTask> scheduler = (_, _) => throw expected;
            var pending = await queue.EnqueueAsync(scheduler);

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => pending.Callback);

            Assert.Same(expected, actual);
        }

        /// <summary>A single-consumer queue does not start the next callback before the first finishes.</summary>
        [Fact]
        public async Task EnqueueAsync_ConcurrencyOne_RunsCallbacksSequentially()
        {
            var manager = ServiceProvider.GetRequiredService<ITaskManager>();
            using var queue = manager.CreateLocalQueue(this, 1);
            var firstStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var releaseFirst = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var secondStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var first = await queue.EnqueueAsync((taskManager, token) => taskManager.ScheduleAnonymousAction(async cancellationToken =>
            {
                firstStarted.TrySetResult(true);
                await releaseFirst.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
            }, token: token));
            var second = await queue.EnqueueAsync((taskManager, token) => taskManager.ScheduleAnonymousAction(cancellationToken =>
            {
                secondStarted.TrySetResult(true);
                return Task.CompletedTask;
            }, token: token));

            try
            {
                await firstStarted.Task.WaitAsync(TimeSpan.FromSeconds(10));

                Assert.False(secondStarted.Task.IsCompleted);
            }
            finally
            {
                releaseFirst.TrySetResult(true);
            }

            await Task.WhenAll(first.WaitOnFinalization(), second.WaitOnFinalization())
                .WaitAsync(TimeSpan.FromSeconds(10));
            Assert.True(secondStarted.Task.IsCompletedSuccessfully);
        }
    }
}