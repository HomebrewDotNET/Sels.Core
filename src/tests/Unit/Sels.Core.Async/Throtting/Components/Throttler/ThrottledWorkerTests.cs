using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Sels.Core.Async;
using Sels.Core.Async.Throtting;
using Sels.Core.Async.Throtting.Components.Throttler;
using Sels.Core.Extensions.Logging;

namespace Sels.Core.Async.Tests.Throtting.Components.Throttler
{
    /// <summary>Contract tests for controlled throttled worker execution.</summary>
    [Trait("Category", "Unit")]
    [Collection(AsyncTestCollection.Name)]
    public class ThrottledWorkerTests
    {
        /// <summary>Enqueue without waiting returns after acceptance, before the callback completes.</summary>
        [Fact]
        public async Task EnqueueWorkAsync_NoWait_ReturnsBeforeWorkCompletes()
        {
            var result = new WorkerResult();
            var context = new WorkerContext("context");
            await using var harness = TestHelper.CreateWorkerHarness(result, context);
            var started = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            await harness.Worker.EnqueueWorkAsync(async (actualResult, actualContext, _) =>
            {
                Assert.Same(result, actualResult);
                Assert.Same(context, actualContext);
                started.TrySetResult(true);
                await release.Task.ConfigureAwait(false);
                actualResult.Value = 12;
            }).WaitAsync(TimeSpan.FromMilliseconds(AsyncTestConstants.AsyncTestTimeoutMilliseconds));
            var work = harness.StartScheduledWork();
            await started.Task.WaitAsync(TimeSpan.FromMilliseconds(AsyncTestConstants.AsyncTestTimeoutMilliseconds));

            try
            {
                Assert.False(work.IsCompleted);
                Assert.Equal(0, result.Value);
            }
            finally
            {
                release.TrySetResult(true);
            }

            await work.WaitAsync(TimeSpan.FromMilliseconds(AsyncTestConstants.AsyncTestTimeoutMilliseconds));
            Assert.Equal(12, result.Value);
        }

        /// <summary>Enqueue with waiting remains pending until scheduled work finishes.</summary>
        [Fact]
        public async Task EnqueueWorkAsync_WaitTrue_WaitsForCallbackCompletion()
        {
            var result = new WorkerResult();
            await using var harness = TestHelper.CreateWorkerHarness(result, new WorkerContext("context"));
            var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            var enqueue = harness.Worker.EnqueueWorkAsync(async (actualResult, _, _) =>
            {
                await release.Task.ConfigureAwait(false);
                actualResult.Value = 23;
            }, wait: true);
            var work = harness.StartScheduledWork();

            try
            {
                Assert.False(enqueue.IsCompleted);
                Assert.False(work.IsCompleted);
            }
            finally
            {
                release.TrySetResult(true);
            }

            await work.WaitAsync(TimeSpan.FromMilliseconds(AsyncTestConstants.AsyncTestTimeoutMilliseconds));
            harness.CompleteFinalization();
            await enqueue.WaitAsync(TimeSpan.FromMilliseconds(AsyncTestConstants.AsyncTestTimeoutMilliseconds));
            Assert.Equal(23, result.Value);
        }

        /// <summary>A worker with no queued work returns its result immediately.</summary>
        [Fact]
        public async Task WaitForWorkAsync_NoQueuedWork_ReturnsResult()
        {
            var result = new WorkerResult();
            await using var harness = TestHelper.CreateWorkerHarness(result, new WorkerContext("context"));

            var actual = await harness.Worker.WaitForWorkAsync()
                .WaitAsync(TimeSpan.FromMilliseconds(AsyncTestConstants.AsyncTestTimeoutMilliseconds));

            Assert.Same(result, actual);
        }

        /// <summary>A child worker shares its parent's queue and returns its own result after work completes.</summary>
        [Fact]
        public async Task CreateSubWorker_EnqueuedWork_CompletesOnSharedWorkerQueue()
        {
            var parentResult = new WorkerResult();
            var childResult = new ChildResult();
            await using var harness = TestHelper.CreateWorkerHarness(parentResult, new WorkerContext("context"));
            var child = harness.Worker.CreateSubWorker(childResult);

            await child.EnqueueWorkAsync((actualResult, _, _) =>
            {
                actualResult.Value = 37;
                return Task.CompletedTask;
            }).WaitAsync(TimeSpan.FromMilliseconds(AsyncTestConstants.AsyncTestTimeoutMilliseconds));
            var work = harness.StartScheduledWork();
            await work.WaitAsync(TimeSpan.FromMilliseconds(AsyncTestConstants.AsyncTestTimeoutMilliseconds));
            var childCompletion = await child.WaitForWorkAsync()
                .WaitAsync(TimeSpan.FromMilliseconds(AsyncTestConstants.AsyncTestTimeoutMilliseconds));
            var parentCompletion = await harness.Worker.WaitForWorkAsync()
                .WaitAsync(TimeSpan.FromMilliseconds(AsyncTestConstants.AsyncTestTimeoutMilliseconds));

            Assert.Same(childResult, childCompletion);
            Assert.Same(parentResult, parentCompletion);
            Assert.Equal(37, childResult.Value);
        }

        /// <summary>Cancelling a work wait does not cancel the already running callback.</summary>
        [Fact]
        public async Task WaitForWorkAsync_WaitTokenCancelled_LeavesCallbackRunning()
        {
            var result = new WorkerResult();
            await using var harness = TestHelper.CreateWorkerHarness(result, new WorkerContext("context"));
            var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            await harness.Worker.EnqueueWorkAsync(async (actualResult, _, _) =>
            {
                await release.Task.ConfigureAwait(false);
                actualResult.Value = 41;
            }).WaitAsync(TimeSpan.FromMilliseconds(AsyncTestConstants.AsyncTestTimeoutMilliseconds));
            var work = harness.StartScheduledWork();
            using var waitCancellation = new CancellationTokenSource();
            harness.Queue.WaitOnQueuedAsync(Arg.Any<CancellationToken>())
                .Returns(call => Task.Delay(Timeout.Infinite, call.Arg<CancellationToken>()));

            var wait = harness.Worker.WaitForWorkAsync(waitCancellation.Token);
            waitCancellation.Cancel();
            var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => wait);

            try
            {
                Assert.IsAssignableFrom<OperationCanceledException>(exception);
                Assert.False(work.IsCompleted);
                Assert.Equal(0, result.Value);
            }
            finally
            {
                release.TrySetResult(true);
            }

            await work.WaitAsync(TimeSpan.FromMilliseconds(AsyncTestConstants.AsyncTestTimeoutMilliseconds));
            Assert.Equal(41, result.Value);
        }

        /// <summary>Parent cancellation reaches a controlled worker callback.</summary>
        [Fact]
        public async Task EnqueueWorkAsync_ParentTokenCancelled_CancelsCallback()
        {
            using var cancellation = new CancellationTokenSource();
            var harness = TestHelper.CreateWorkerHarness(
                new WorkerResult(),
                new WorkerContext("context"),
                cancellationToken: cancellation.Token);
            await using (harness)
            {
                var started = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                var cancelled = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                Task? work = null;

                try
                {
                    await harness.Worker.EnqueueWorkAsync(async (_, _, token) =>
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
                    }).WaitAsync(TimeSpan.FromMilliseconds(AsyncTestConstants.AsyncTestTimeoutMilliseconds));
                    work = harness.StartScheduledWork();
                    await started.Task.WaitAsync(TimeSpan.FromMilliseconds(AsyncTestConstants.AsyncTestTimeoutMilliseconds));
                    cancellation.Cancel();
                    await cancelled.Task.WaitAsync(TimeSpan.FromMilliseconds(AsyncTestConstants.AsyncTestTimeoutMilliseconds));
                }
                finally
                {
                    release.TrySetResult(true);
                    if (work != null)
                    {
                        await Assert.ThrowsAsync<TaskCanceledException>(async () => await work.WaitAsync(TimeSpan.FromMilliseconds(AsyncTestConstants.AsyncCleanupTimeoutMilliseconds)));
                    }
                }

                Assert.True(cancelled.Task.IsCompletedSuccessfully);
            }
        }

        /// <summary>WaitForWorkAsync returns a pending task while accepted work remains gated.</summary>
        [Fact]
        public async Task WaitForWorkAsync_AcceptedWorkRunning_ReturnsPendingTaskBeforeRelease()
        {
            var result = new WorkerResult();
            await using var harness = TestHelper.CreateWorkerHarness(result, new WorkerContext("context"));
            var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var queueDrained = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            // The default stub completes immediately, which would make the worker spin on its pending-work check
            harness.Queue.WaitOnQueuedAsync(Arg.Any<CancellationToken>()).Returns(queueDrained.Task);
            await harness.Worker.EnqueueWorkAsync(async (actualResult, _, _) =>
            {
                await release.Task.ConfigureAwait(false);
                actualResult.Value = 31;
            }).WaitAsync(TimeSpan.FromMilliseconds(AsyncTestConstants.AsyncTestTimeoutMilliseconds));
            var work = harness.StartScheduledWork();

            var wait = harness.Worker.WaitForWorkAsync();
            try
            {
                Assert.False(wait.IsCompleted);
            }
            finally
            {
                release.TrySetResult(true);
            }

            await work.WaitAsync(TimeSpan.FromMilliseconds(AsyncTestConstants.AsyncTestTimeoutMilliseconds));
            queueDrained.TrySetResult(true);
            var actual = await wait.WaitAsync(TimeSpan.FromMilliseconds(AsyncTestConstants.AsyncTestTimeoutMilliseconds));

            Assert.Same(result, actual);
            Assert.Equal(31, result.Value);
        }

        /// <summary>A rejected enqueue propagates the same scheduler exception.</summary>
        [Fact(Timeout = AsyncTestConstants.AsyncTestTimeoutMilliseconds)]
        public async Task EnqueueWorkAsync_QueueRejectsRequest_PropagatesSameException()
        {
            await using var harness = TestHelper.CreateWorkerHarness(new WorkerResult(), new WorkerContext("context"));
            var expected = new InvalidOperationException("queue rejected");
            harness.Queue.EnqueueAsync(
                    Arg.Any<Func<Sels.Core.Async.TaskManagement.ITaskManager, CancellationToken, Sels.Core.Async.TaskManagement.IManagedAnonymousTask>>(),
                    Arg.Any<CancellationToken>())
                .Returns(_ => Task.FromException<Sels.Core.Async.TaskManagement.IPendingTask<Sels.Core.Async.TaskManagement.IManagedAnonymousTask>>(expected));

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => harness.Worker.EnqueueWorkAsync((_, _, _) => Task.CompletedTask));

            Assert.Same(expected, actual);
        }

        /// <summary>The canonical worker constructor accepts no logger.</summary>
        [Fact(Timeout = AsyncTestConstants.AsyncTestTimeoutMilliseconds)]
        public async Task Constructor_NoLogger_CreatesWorker()
        {
            var manager = Substitute.For<Sels.Core.Async.TaskManagement.ITaskManager>();
            manager.CreateLocalQueue(Arg.Any<object>(), Arg.Any<int>())
                .Returns(Substitute.For<Sels.Core.Async.TaskManagement.IManagedTaskLocalQueue>());

            await using var worker = new ThrottledWorker<WorkerResult, WorkerContext>(
                manager,
                new WorkerResult(),
                new WorkerContext("context"),
                new ThrottledWorkerOptions(),
                CancellationToken.None);

            Assert.NotNull(worker);
        }

        /// <summary>The original positional logger constructor forwards to the canonical worker constructor.</summary>
        [Fact(Timeout = AsyncTestConstants.AsyncTestTimeoutMilliseconds)]
        public async Task Constructor_LegacyLoggerOrder_CreatesWorker()
        {
            var manager = Substitute.For<Sels.Core.Async.TaskManagement.ITaskManager>();
            manager.CreateLocalQueue(Arg.Any<object>(), Arg.Any<int>())
                .Returns(Substitute.For<Sels.Core.Async.TaskManagement.IManagedTaskLocalQueue>());

            await using var worker = new ThrottledWorker<WorkerResult, WorkerContext>(
                manager,
                new WorkerResult(),
                new WorkerContext("context"),
                new ThrottledWorkerOptions(),
                Microsoft.Extensions.Logging.Abstractions.NullLogger<ThrottledWorker<WorkerResult, WorkerContext>>.Instance,
                CancellationToken.None);

            Assert.NotNull(worker);
        }

        /// <summary>The legacy worker constructor retains its positional logger and cancellation parameters.</summary>
        [Fact(Timeout = AsyncTestConstants.AsyncTestTimeoutMilliseconds)]
        public async Task Constructor_LegacyOverload_PreservesCallShape()
        {
            var manager = Substitute.For<Sels.Core.Async.TaskManagement.ITaskManager>();
            manager.CreateLocalQueue(Arg.Any<object>(), Arg.Any<int>())
                .Returns(Substitute.For<Sels.Core.Async.TaskManagement.IManagedTaskLocalQueue>());

            await using var worker = new ThrottledWorker<WorkerResult, WorkerContext>(
                manager,
                new WorkerResult(),
                new WorkerContext("context"),
                new ThrottledWorkerOptions(),
                NullLogger<ThrottledWorker<WorkerResult, WorkerContext>>.Instance,
                CancellationToken.None);

            Assert.NotNull(worker);
        }

        /// <summary>Sels logging extensions contain logger failures without escaping.</summary>
        [Fact]
        public void LogException_LoggerThrows_DoesNotEscape()
        {
            var logger = new ThrowingLogger();
            var exception = new InvalidOperationException("work failed");

            var actual = Record.Exception(() => logger.LogException(LogLevel.Error, "Work failed", exception));

            Assert.Null(actual);
        }

        /// <summary>Structured logging preserves error level, exception identity, and named fields.</summary>
        [Fact]
        public void LogException_StructuredParameters_EmitsNamedValues()
        {
            var logger = new RecordingLogger();
            var exception = new InvalidOperationException("work failed");
            var template = $"Work failed for <{{{AsyncLog.Throttling.ResultType}}}> with context <{{{AsyncLog.Throttling.ContextType}}}>";

            logger.LogException(LogLevel.Error, template, exception, "Result", "Context");

            Assert.Equal(LogLevel.Error, logger.Level);
            Assert.Same(exception, logger.Exception);
            Assert.Equal("Sels.Core.Async.Throttling.ResultType", AsyncLog.Throttling.ResultType);
            Assert.Equal("Sels.Core.Async.Throttling.ContextType", AsyncLog.Throttling.ContextType);
            Assert.Equal("Sels.Core.Async.Throttling.Result", AsyncLog.Throttling.Result);
            Assert.Equal("Sels.Core.Async.Throttling.Context", AsyncLog.Throttling.Context);
            Assert.Equal("Sels.Core.Async.Throttling.PendingWork", AsyncLog.Throttling.PendingWork);
            Assert.Equal("Result", logger.State[AsyncLog.Throttling.ResultType]);
            Assert.Equal("Context", logger.State[AsyncLog.Throttling.ContextType]);
        }

        private sealed class WorkerResult
        {
            public int Value { get; set; }
        }

        private sealed class WorkerContext
        {
            public string Name { get; }

            public WorkerContext(string name)
            {
                Name = name;
            }
        }

        private sealed class ChildResult
        {
            public int Value { get; set; }
        }

        private sealed class RecordingLogger : ILogger
        {
            public LogLevel Level { get; private set; }

            public Exception? Exception { get; private set; }

            public Dictionary<string, object?> State { get; } = new();

            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                Level = logLevel;
                Exception = exception;
                if (state is IEnumerable<KeyValuePair<string, object?>> values)
                {
                    foreach (var pair in values)
                    {
                        State[pair.Key] = pair.Value;
                    }
                }
            }
        }

        private sealed class ThrowingLogger : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                throw new InvalidOperationException("logger failed");
            }
        }
    }
}