
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Sels.Core.Data.FluentMigrationTool;
using Sels.Core.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Contains extension methods for registering services
    /// </summary>
    public static class ApplicationRegistrations
    {
        /// <summary>
        /// Adds a <see cref="IMigrationToolFactory"/> that can be used to create migration tools for deploying database schema's.
        /// </summary>
        /// <param name="services">Collection to add the registration to</param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        public static IServiceCollection AddMigrationToolFactory(this IServiceCollection services)
        {
            services.ValidateArgument(nameof(services));

            services.AddSingleton<IMigrationToolFactory, MigrationToolFactory>(x => new MigrationToolFactory(services, x.GetService<ILoggerFactory>()));

            return services;
        }
    }
}
