using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Sels.Core.Async.Throtting;
using Sels.Core.Validation;

namespace Sels.Core.Async.IntegrationTests.Throtting.Models.Throttler
{
    /// <summary>Integration tests for the registered worker option validation profile.</summary>
    [Collection(AsyncIntegrationCollection.Name)]
    [Trait("Category", "Integration")]
    public class ThrottledWorkerOptionsIntegrationTests : AsyncIntegrationTestBase
    {
        /// <inheritdoc/>
        protected override void ConfigureServices(IServiceCollection services)
        {
            RegisterTaskManager(services);
            services.AddMemoryCache();
            services.AddThrottler();
        }

        /// <summary>Positive concurrency and queue limits pass the real validation profile.</summary>
        [Fact]
        public void ValidationProfile_PositiveLimits_ReturnsNoErrors()
        {
            var validator = ServiceProvider.GetRequiredService<IValidator<ThrottledWorkerOptions, string, object>>();

            var errors = validator.Validate(new ThrottledWorkerOptions { MaxConcurrentTasks = 1, MaxQueuedTasks = 1 });

            Assert.Empty(errors);
        }

        /// <summary>Either zero limit fails the real validation profile.</summary>
        [Fact]
        public void ValidationProfile_ZeroLimit_ReturnsErrors()
        {
            var validator = ServiceProvider.GetRequiredService<IValidator<ThrottledWorkerOptions, string, object>>();

            var concurrencyErrors = validator.Validate(new ThrottledWorkerOptions { MaxConcurrentTasks = 0, MaxQueuedTasks = 1 });
            var queueErrors = validator.Validate(new ThrottledWorkerOptions { MaxConcurrentTasks = 1, MaxQueuedTasks = 0 });

            Assert.NotEmpty(concurrencyErrors);
            Assert.NotEmpty(queueErrors);
        }
    }
}