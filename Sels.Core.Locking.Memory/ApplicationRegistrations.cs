using Sels.Core.Extensions;
using Sels.Core.Locking.Memory;
using Sels.Core.Locking.Provider;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Contains extension methods for registering services.
    /// </summary>
    public static class ApplicationRegistrations
    {
        /// <summary>
        /// Adds a <see cref="ILockingProvider"/> that uses an in-memory store to keep lock state. Provides distributed locking within the same application.
        /// </summary>
        /// <param name="services">Collection to add the service registration to</param>
        /// <param name="configurator">Optional delegate for configuring the options</param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        public static IServiceCollection AddMemoryLockingProvider(this IServiceCollection services, Action<MemoryLockingProviderOptions> configurator = null)
        {
            services.ValidateArgument(nameof(services));

            services.AddOptions();

            // Add configuration handler
            services.BindOptionsFromConfig<MemoryLockingProviderOptions>();

            // Add option validator
            services.AddValidationProfile<ProviderOptionsValidationProfile, string>(ServiceLifetime.Singleton);
            services.AddOptionProfileValidator<MemoryLockingProviderOptions, ProviderOptionsValidationProfile>();

            // Add custom delegate
            if(configurator != null) services.Configure<MemoryLockingProviderOptions>(configurator);

            // Add provider
            services.AddSingleton<ILockingProvider, MemoryLockingProvider>();

            return services;
        }
    }
}
