using System;
using Sels.Core.Async.TaskManagement;
using Sels.Core.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Contains extension methods for registering services into a service collection.
    /// </summary>
    public static class ApplicationRegistrations
    {
        /// <summary>
        /// Adds a task manager injected as <see cref="ITaskManager"/> to schedule work on the thread pool.
        /// </summary>
        /// <param name="services">The collection the service descriptions will be added to</param>
        /// <param name="configurator">Optional delegate to configure the options</param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        public static IServiceCollection AddTaskManager(this IServiceCollection services, Action<TaskManagerOptions>? configurator = null)
        {
            services.ValidateArgument(nameof(services));

            // Options
            services.AddOptions();
            services.BindOptionsFromConfig<TaskManagerOptions>();
            if(configurator != null) services.Configure<TaskManagerOptions>(configurator);

            // Task manager
            services.New<TaskManager>()
                    .AsSingleton()
                    .Trace(x => x.Duration.OfAll.WithDefaultThresholds())
                    .HandleDisposed()
                    .TryRegister();

            // Forwarded service
            services.New<ITaskManager, TaskManager>()
                    .AsForwardedService()
                    .TryRegister();
                    

            return services;
        }
    }
}
