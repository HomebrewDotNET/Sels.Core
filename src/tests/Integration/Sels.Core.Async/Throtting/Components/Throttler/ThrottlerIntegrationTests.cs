using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Sels.Core.Async.Throtting;

namespace Sels.Core.Async.IntegrationTests.Throtting.Components.Throttler
{
    /// <summary>Integration tests for real throttled workers and options validation.</summary>
    [Collection(AsyncIntegrationCollection.Name)]
    [Trait("Category", "Integration")]
    public class ThrottlerIntegrationTests : AsyncIntegrationTestBase
    {
        /// <inheritdoc/>
        protected override void ConfigureServices(IServiceCollection services)
        {
            RegisterTaskManager(services);
            services.AddMemoryCache();
            services.AddThrottler();
        }

        /// <summary>A registered throttler runs delegate work against its supplied state.</summary>
        [Fact]
        public async Task CreateWorker_ValidOptions_ExecutesDelegate()
        {
            var throttler = ServiceProvider.GetRequiredService<IThrottler>();
            var result = new WorkerResult();
            var context = new WorkerContext();
            await using var worker = throttler.CreateWorker(result, context, new ThrottledWorkerOptions
            {
                MaxConcurrentTasks = 1,
                MaxQueuedTasks = 1,
                StopOnError = false
            });

            await worker.EnqueueWorkAsync((actualResult, actualContext, _) =>
            {
                actualResult.Value = actualContext.Value;
                return Task.CompletedTask;
            }, wait: true).WaitAsync(TimeSpan.FromSeconds(10));

            Assert.Equal(42, result.Value);
        }

        /// <summary>Registered options validation rejects zero concurrency.</summary>
        [Fact]
        public void CreateWorker_InvalidOptions_ThrowsArgumentException()
        {
            var throttler = ServiceProvider.GetRequiredService<IThrottler>();

            var exception = Assert.Throws<ArgumentException>(() => throttler.CreateWorker(new WorkerResult(), new WorkerContext(), new ThrottledWorkerOptions
            {
                MaxConcurrentTasks = 0,
                MaxQueuedTasks = 1
            }));

            Assert.Equal("options", exception.ParamName);
        }

        /// <summary>StopOnError propagates a failed callback to the next worker operation.</summary>
        [Fact]
        public async Task EnqueueWorkAsync_StopOnError_PropagatesOriginalFailure()
        {
            var throttler = ServiceProvider.GetRequiredService<IThrottler>();
            var expected = new InvalidOperationException("worker failure");
            await using var worker = throttler.CreateWorker(new WorkerResult(), new WorkerContext(), new ThrottledWorkerOptions
            {
                MaxConcurrentTasks = 1,
                MaxQueuedTasks = 1,
                StopOnError = true
            });

            await worker.EnqueueWorkAsync((_, _, _) => throw expected, wait: true)
                .WaitAsync(TimeSpan.FromSeconds(10));
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => worker
                .EnqueueWorkAsync((_, _, _) => Task.CompletedTask));

            Assert.Same(expected, actual);
        }

        /// <summary>A worker configured to continue accepts later work after an earlier failure.</summary>
        [Fact]
        public async Task EnqueueWorkAsync_StopOnErrorFalse_ContinuesAfterFailure()
        {
            var throttler = ServiceProvider.GetRequiredService<IThrottler>();
            var result = new WorkerResult();
            await using var worker = throttler.CreateWorker(result, new WorkerContext(), new ThrottledWorkerOptions
            {
                MaxConcurrentTasks = 1,
                MaxQueuedTasks = 1,
                StopOnError = false
            });

            await worker.EnqueueWorkAsync((_, _, _) => Task.FromException(new InvalidOperationException("ignored")), wait: true)
                .WaitAsync(TimeSpan.FromSeconds(10));
            await worker.EnqueueWorkAsync((actualResult, _, _) =>
            {
                actualResult.Value = 52;
                return Task.CompletedTask;
            }, wait: true).WaitAsync(TimeSpan.FromSeconds(10));

            Assert.Equal(52, result.Value);
        }

        /// <summary>Worker cancellation reaches an active callback.</summary>
        [Fact]
        public async Task CreateWorker_CancellationTokenCancelled_CancelsCallback()
        {
            var throttler = ServiceProvider.GetRequiredService<IThrottler>();
            using var cancellation = new CancellationTokenSource();
            var started = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var cancelled = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            await using var worker = throttler.CreateWorker(new WorkerResult(), new WorkerContext(), new ThrottledWorkerOptions
            {
                MaxConcurrentTasks = 1,
                MaxQueuedTasks = 1,
                StopOnError = false
            }, cancellation.Token);

            try
            {
                await worker.EnqueueWorkAsync(async (_, _, token) =>
                {
                    started.TrySetResult(true);
                    try
                    {
                        await release.Task.WaitAsync(token).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        cancelled.TrySetResult(true);
                        throw;
                    }
                }).WaitAsync(TimeSpan.FromSeconds(10));
                await started.Task.WaitAsync(TimeSpan.FromSeconds(10));
                cancellation.Cancel();
                await cancelled.Task.WaitAsync(TimeSpan.FromSeconds(10));

                Assert.True(cancelled.Task.IsCompletedSuccessfully);
            }
            finally
            {
                release.TrySetResult(true);
            }
        }

        private sealed class WorkerResult
        {
            public int Value { get; set; }
        }

        private sealed class WorkerContext
        {
            public int Value { get; } = 42;
        }
    }
}