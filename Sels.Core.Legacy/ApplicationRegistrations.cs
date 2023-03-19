using Sels.Core.Components.Configuration;
using Sels.Core.Contracts.Configuration;
using Sels.Core.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Contains extension methods for registering services
    /// </summary>
    public static class ApplicationRegistrations
    {
        /// <summary>
        /// Adds <see cref="ConfigurationService"/> to the service collection as <see cref="IConfigurationService"/>
        /// </summary>
        /// <param name="services">Collection to add the services to</param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        public static IServiceCollection RegisterConfigurationService(this IServiceCollection services)
        {
            services.ValidateArgument(nameof(services));

            services.AddSingleton<IConfigurationService, ConfigurationService>();

            return services;
        }
    }
}
