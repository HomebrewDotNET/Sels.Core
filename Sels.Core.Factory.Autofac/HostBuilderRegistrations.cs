using Autofac;
using Sels.Core;
using Sels.Core.Contracts.Factory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Hosting
{
    /// <summary>
    /// Contains extension methods for setting up a host builder.
    /// </summary>
    public static class HostBuilderRegistrations
    {
        /// <summary>
        /// Configures autofac as the IoC container provider and registers a <see cref="IServiceFactory"/> using Autofac.
        /// </summary>
        /// <param name="hostBuilder">The builder to configure</param>
        /// <param name="containerBuilder">Optional delegate for configuring the autofac container</param>
        /// <returns><paramref name="hostBuilder"/> for method chaining</returns>
        public static IHostBuilder UseAutofacWithServiceFactory(this IHostBuilder hostBuilder, Action<HostBuilderContext, ContainerBuilder>? containerBuilder = null)
        {
            Guard.IsNotNull(hostBuilder);

            // Add autofac as the IoC provider
            hostBuilder.UseServiceProviderFactory(new AutofacServiceProviderFactory());

            // Configure container
            hostBuilder.ConfigureContainer<ContainerBuilder>((c, b) =>
            {
                containerBuilder?.Invoke(c, b);
            });

            // Add service factory
            hostBuilder.ConfigureServices((c, s) => s.AddAutofacServiceFactory());

            return hostBuilder;
        }
    }
}
