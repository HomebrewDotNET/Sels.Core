using System;
using Microsoft.Extensions.Logging;
using Sels.Core.Async.TaskManagement;
using Sels.Core.Async.Throtting;
using Sels.Core.Async.Throtting.Components.Throttler;
using Sels.Core.Extensions;
using Sels.Core.Validation;

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
                    .HandleDisposed()
                    .TryRegister();

            // Forwarded service
            services.New<ITaskManager, TaskManager>()
                    .AsForwardedService()
                    .TryRegister();
                    

            return services;
        }

        /// <summary>
        /// Adds a throttler injected as <see cref="IThrottler"/> to throttle work on the thread pool.
        /// </summary>
        /// <param name="services">The collection the service descriptions will be added to</param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        public static IServiceCollection AddThrottler(this IServiceCollection services)
        {
            services.ValidateArgument(nameof(services));

            // Throttler
            services.AddValidationProfile<ThrottledWorkerOptionsValidationProfile, string>();
            services.AddValidator<ThrottledWorkerOptions, string, ThrottledWorkerOptionsValidationProfile>();
            services.New<Throttler>()
                    .AsSingleton()
                    .ConstructWith(provider => new Throttler(
                        provider.GetRequiredService<ITaskManager>(),
                        provider.GetRequiredService<IValidator<ThrottledWorkerOptions, string, object>>(),
                        provider.GetService<ILoggerFactory>()))
                    .TryRegister();
            // Forwarded service
            services.New<IThrottler, Throttler>()
                    .AsForwardedService()
                    .Trace(x => x.WithScope.ForAll, true)
                    .TryRegister();
            return services;
        }
    }
}
