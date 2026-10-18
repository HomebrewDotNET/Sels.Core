using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sels.Core.Async.TaskManagement;
using Sels.Core.Testing;
using Sels.Core.Testing.Templates;

namespace Sels.Core.Async.IntegrationTests
{
    /// <summary>Shared integration setup for real asynchronous task services.</summary>
    public abstract class AsyncIntegrationTestBase : BaseIntegrationTest<IntegrationTestingSettings>, IAsyncLifetime
    {
        /// <inheritdoc/>
        protected override void Configure(IConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>());
        }

        /// <summary>Registers real task-manager services and the inherited configuration instance.</summary>
        /// <param name="services">Collection to register the services in.</param>
        /// <param name="configurator">Optional delegate to configure the task-manager options.</param>
        protected void RegisterTaskManager(IServiceCollection services, Action<TaskManagerOptions>? configurator = null)
        {
            services.AddSingleton<IConfiguration>(Configuration);
            services.AddLogging();
            services.AddTaskManager(configurator);
        }

        /// <inheritdoc/>
        public Task InitializeAsync() => Task.CompletedTask;

        /// <inheritdoc/>
        Task IAsyncLifetime.DisposeAsync() => DisposeAsync().AsTask();
    }
}