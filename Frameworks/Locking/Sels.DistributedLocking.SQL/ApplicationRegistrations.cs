using Sels.Core.Extensions;
using Sels.DistributedLocking.Provider;
using Sels.DistributedLocking.SQL;
using Sels.Core.ServiceBuilder;
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
        /// Adds a <see cref="ILockingProvider"/> that uses an <see cref="ISqlLockRepository"/> to execute SQL queries to manage distributed locks. Provides distributed locking for all applications using the same database.
        /// </summary>
        /// <param name="services">Collection to add the service registration to</param>
        /// <param name="configurator">Optional delegate for configuring the options</param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        public static IServiceCollection AddSqlLockingProvider(this IServiceCollection services, Action<SqlLockingProviderOptions> configurator = null)
        {
            services.ValidateArgument(nameof(services));

            services.AddOptions();

            // Add configuration handler
            services.BindOptionsFromConfig<SqlLockingProviderOptions>();

            // Add option validator
            services.AddValidationProfile<ProviderOptionsValidationProfile, string>(ServiceLifetime.Singleton);
            services.AddOptionProfileValidator<SqlLockingProviderOptions, ProviderOptionsValidationProfile>();

            // Add custom delegate
            if(configurator != null) services.Configure<SqlLockingProviderOptions>(configurator);

            // Add provider
            services.New<ILockingProvider, SqlLockingProvider>()
                    .Trace(x => x.Duration.OfAll)
                    .AsSingleton()
                    .Register();

            return services;
        }
    }
}
