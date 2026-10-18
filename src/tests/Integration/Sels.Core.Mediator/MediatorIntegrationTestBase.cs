using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sels.Core.Testing;
using Sels.Core.Testing.Templates;

namespace Sels.Core.Mediator.IntegrationTests
{
    /// <summary>Shared integration setup for the real notifier services.</summary>
    public abstract class MediatorIntegrationTestBase : BaseIntegrationTest<IntegrationTestingSettings>, IAsyncLifetime
    {
        private Action<IServiceCollection>? _configureTestServices;

        /// <inheritdoc/>
        protected override void Configure(IConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>());
        }

        /// <summary>Registers additional services for the current test. Must be called before <see cref="BaseIntegrationTest{TSettings}.ServiceProvider"/> is first used.</summary>
        /// <param name="configurator">Delegate that registers the extra services.</param>
        protected void ConfigureTestServices(Action<IServiceCollection> configurator)
        {
            _configureTestServices = configurator;
        }

        /// <inheritdoc/>
        protected override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IConfiguration>(Configuration);
            services.AddLogging();
            services.AddNotifier().AddMemoryCache();
            _configureTestServices?.Invoke(services);
        }

        /// <inheritdoc/>
        public Task InitializeAsync() => Task.CompletedTask;

        /// <inheritdoc/>
        Task IAsyncLifetime.DisposeAsync() => DisposeAsync().AsTask();
    }
}
