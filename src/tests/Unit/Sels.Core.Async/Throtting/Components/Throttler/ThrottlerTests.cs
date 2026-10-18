using Microsoft.Extensions.Logging.Abstractions;
using Sels.Core.Async.TaskManagement;
using Sels.Core.Async.Throtting;
using Sels.Core.Async.Throtting.Components.Throttler;
using Sels.Core.Validation;
using ThrottlerComponent = Sels.Core.Async.Throtting.Components.Throttler.Throttler;

namespace Sels.Core.Async.Tests.Throtting.Components.Throttler
{
    /// <summary>Contract tests for throttler worker creation.</summary>
    [Trait("Category", "Unit")]
    [Collection(AsyncTestCollection.Name)]
    public class ThrottlerTests
    {
        /// <summary>A valid worker request creates an asynchronous worker with the supplied state.</summary>
        [Fact(Timeout = AsyncTestConstants.AsyncTestTimeoutMilliseconds)]
        public async Task CreateWorker_ValidInputs_ReturnsWorker()
        {
            var manager = Substitute.For<ITaskManager>();
            manager.CreateLocalQueue(Arg.Any<object>(), Arg.Any<int>()).Returns(Substitute.For<IManagedTaskLocalQueue>());
            var validator = Substitute.For<IValidator<ThrottledWorkerOptions, string, object>>();
            validator.Validate(Arg.Any<ThrottledWorkerOptions>(), Arg.Any<object>()).Returns(Array.Empty<string>());
            var sut = new ThrottlerComponent(NullLoggerFactory.Instance, manager, validator);
            var result = new WorkerResult();
            var context = new WorkerContext();

            var worker = sut.CreateWorker(result, context, new ThrottledWorkerOptions());

            Assert.IsType<ThrottledWorker<WorkerResult, WorkerContext>>(worker);
            await worker.DisposeAsync();
        }

        /// <summary>Worker creation rejects options that the validator reports as invalid.</summary>
        [Fact]
        public void CreateWorker_InvalidOptions_ThrowsArgumentException()
        {
            var manager = Substitute.For<ITaskManager>();
            var validator = Substitute.For<IValidator<ThrottledWorkerOptions, string, object>>();
            validator.Validate(Arg.Any<ThrottledWorkerOptions>(), Arg.Any<object>()).Returns(new[] { "invalid" });
            var sut = new ThrottlerComponent(NullLoggerFactory.Instance, manager, validator);

            var exception = Assert.Throws<ArgumentException>(() =>
            {
                _ = sut.CreateWorker(new WorkerResult(), new WorkerContext(), new ThrottledWorkerOptions());
            });

            Assert.Contains("invalid", exception.Message);
            Assert.Equal("options", exception.ParamName);
        }

        /// <summary>Worker creation validates required result and context arguments.</summary>
        [Fact]
        public void CreateWorker_NullRequiredArgument_ThrowsArgumentNullException()
        {
            var manager = Substitute.For<ITaskManager>();
            var validator = Substitute.For<IValidator<ThrottledWorkerOptions, string, object>>();
            var sut = new ThrottlerComponent(NullLoggerFactory.Instance, manager, validator);

            var resultException = Assert.Throws<ArgumentNullException>(() =>
            {
                _ = sut.CreateWorker<WorkerResult, WorkerContext>(null!, new WorkerContext());
            });
            var contextException = Assert.Throws<ArgumentNullException>(() =>
            {
                _ = sut.CreateWorker(new WorkerResult(), (WorkerContext)null!);
            });

            Assert.Equal("result", resultException.ParamName);
            Assert.Equal("context", contextException.ParamName);
        }

        /// <summary>The canonical throttler constructor accepts absent logging dependencies.</summary>
        [Fact(Timeout = AsyncTestConstants.AsyncTestTimeoutMilliseconds)]
        public async Task Constructor_NoLoggerFactory_CreatesWorker()
        {
            var manager = Substitute.For<ITaskManager>();
            manager.CreateLocalQueue(Arg.Any<object>(), Arg.Any<int>()).Returns(Substitute.For<IManagedTaskLocalQueue>());
            var validator = Substitute.For<IValidator<ThrottledWorkerOptions, string, object>>();
            validator.Validate(Arg.Any<ThrottledWorkerOptions>(), Arg.Any<object>()).Returns(Array.Empty<string>());
            var sut = new ThrottlerComponent(manager, validator);

            var worker = sut.CreateWorker(new WorkerResult());

            Assert.NotNull(worker);
            await worker.DisposeAsync();
        }

        /// <summary>The legacy constructor accepts an explicitly null logger factory.</summary>
        [Fact(Timeout = AsyncTestConstants.AsyncTestTimeoutMilliseconds)]
        public async Task Constructor_LegacyOverloadWithNullFactory_CreatesWorker()
        {
            var manager = Substitute.For<ITaskManager>();
            manager.CreateLocalQueue(Arg.Any<object>(), Arg.Any<int>()).Returns(Substitute.For<IManagedTaskLocalQueue>());
            var validator = Substitute.For<IValidator<ThrottledWorkerOptions, string, object>>();
            validator.Validate(Arg.Any<ThrottledWorkerOptions>(), Arg.Any<object>()).Returns(Array.Empty<string>());
            var sut = new ThrottlerComponent(null!, manager, validator);

            var worker = sut.CreateWorker(new WorkerResult());

            Assert.NotNull(worker);
            await worker.DisposeAsync();
        }

        private sealed class WorkerResult
        {
        }

        private sealed class WorkerContext
        {
        }
    }
}