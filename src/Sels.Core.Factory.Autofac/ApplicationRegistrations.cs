using Microsoft.Extensions.DependencyInjection.Extensions;
using Sels.Core;
using Sels.Core.Factory;
using Sels.Core.Factory;
using Sels.Core.Factory.Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Contains extension methods for registering services into a service collection.
    /// </summary>
    public static class ApplicationRegistrations
    {
        /// <summary>
        /// Adds <see cref="AutofacServiceFactory"/> as <see cref="IServiceFactory"/>.
        /// </summary>
        /// <param name="services">Collection to add the service registration to</param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        public static IServiceCollection AddAutofacServiceFactory(this IServiceCollection services)
        {
            Guard.IsNotNull(services);

            services.AddServiceFactoryScope();
            services.New<IServiceFactory, AutofacServiceFactory>()
                    .AsSingleton()
                    .Trace(x => x.Duration.OfAll.WithDefaultThresholds())
                    .Register();

            return services;
        }
    }
}
