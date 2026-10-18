using Sels.Core.Async.Throtting;
using Sels.Core.Async;

namespace Sels.Core.Async.Tests.Throtting.Models.Throttler
{
    /// <summary>Contract tests for throttled worker option defaults.</summary>
    [Trait("Category", "Unit")]
    public class ThrottledWorkerOptionsTests
    {
        /// <summary>Explicit and shared defaults retain their documented values.</summary>
        [Fact]
        public void Defaults_ExplicitAndSharedOptions_MatchContract()
        {
            var options = new ThrottledWorkerOptions();
            var shared = ThrottledWorkerOptions.Default;

            var explicitValues = (options.MaxConcurrentTasks, options.MaxQueuedTasks, options.StopOnError);
            var sharedValues = (shared.MaxConcurrentTasks, shared.MaxQueuedTasks, shared.StopOnError);

            Assert.Equal((1, 100, false), explicitValues);
            Assert.Equal((Environment.ProcessorCount, Environment.ProcessorCount * 2, true), sharedValues);
            Assert.Equal(1, AsyncConstants.Throttling.MinimumTaskCount);
            Assert.Equal(2, AsyncConstants.Throttling.DefaultQueueProcessorMultiplier);
        }
    }
}