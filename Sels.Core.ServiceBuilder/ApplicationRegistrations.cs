using Sels.Core;
using Sels.Core.ServiceBuilder.Events;
using Sels.Core.ServiceBuilder.Injection;
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
        /// Adds a <see cref="ServiceInjector"/> that will be used for all services created with a service builder.
        /// </summary>
        /// <param name="services">The service collection to add the services to</param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        public static IServiceCollection AddServiceInjector(this IServiceCollection services)
        {
            Guard.IsNotNull(services);

            services.New<ServiceInjector>()
                    .AsSingleton()
                    .TryRegister();

            services.New<IOnCreatedHandler, ServiceInjector>().AsForwardedService()
                    .AsSingleton()
                    .Trace(x => x.Duration.OfAll)
                    .TryRegister();

            return services;
        }

        /// <summary>
        /// Adds a <see cref="ServiceInjector{TImpl}"/> that will be used for all services of type <typeparamref name="TImpl"/> created with a service builder.
        /// </summary>
        /// <param name="services">The service collection to add the services to</param>
        /// <typeparam name="TImpl">The type to inject for</typeparam>
        /// <returns><paramref name="services"/> for method chaining</returns>
        public static IServiceCollection AddServiceInjector<TImpl>(this IServiceCollection services)
        {
            Guard.IsNotNull(services);

            services.New<ServiceInjector<TImpl>>()
                    .AsSingleton()
                    .TryRegister();


            services.New<IOnCreatedHandler<TImpl>, ServiceInjector<TImpl>>().AsForwardedService()
                    .AsSingleton()
                    .Trace(x => x.Duration.OfAll)
                    .TryRegister();

            return services;
        }
    }
}
