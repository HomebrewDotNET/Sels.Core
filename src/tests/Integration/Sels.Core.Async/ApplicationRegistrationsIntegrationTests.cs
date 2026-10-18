using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Sels.Core.Async.Throtting;

namespace Sels.Core.Async.IntegrationTests
{
    /// <summary>Integration tests for asynchronous service registration.</summary>
    [Collection(AsyncIntegrationCollection.Name)]
    [Trait("Category", "Integration")]
    public class ApplicationRegistrationsIntegrationTests : AsyncIntegrationTestBase
    {
        /// <inheritdoc/>
        protected override void ConfigureServices(IServiceCollection services)
        {
            RegisterTaskManager(services);
            services.AddMemoryCache();
            services.AddThrottler();
        }

        /// <summary>AddThrottler resolves a real singleton throttler through the registered provider.</summary>
        [Fact]
        public void AddThrottler_RegisteredServices_ResolvesIThrottler()
        {
            var first = ServiceProvider.GetRequiredService<IThrottler>();
            var second = ServiceProvider.GetRequiredService<IThrottler>();

            var same = ReferenceEquals(first, second);

            Assert.True(same);
        }
    }
}