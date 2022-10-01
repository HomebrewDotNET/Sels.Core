using Sels.Core.ServiceBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Contains extension methods for building server for dependency injection.
    /// </summary>
    public static class ServiceBuilderExtensions
    {
        /// <summary>
        /// Returns a builder for creating a new service to inject as dependency.
        /// </summary>
        /// <typeparam name="T">The service type</typeparam>
        /// <param name="collection">Collection to register the built service in</param>
        /// <returns>Builder for creating the service</returns>
        public static IServiceBuilder<T> New<T>(this IServiceCollection collection) where T : class
        {
            collection.ValidateArgument(nameof(collection));

            return new ServiceBuilder<T>(collection);
        }

        /// <summary>
        /// Returns a builder for creating a new service to inject as dependency.
        /// </summary>
        /// <typeparam name="T">The service type</typeparam>
        /// <typeparam name="TImpl">The implementation type for <typeparamref name="T"/></typeparam>
        /// <param name="collection">Collection to register the built service in</param>
        /// <returns>Builder for creating the service</returns>
        public static IServiceBuilder<T, TImpl> New<T, TImpl>(this IServiceCollection collection)
            where TImpl : class, T
            where T : class
        {
            collection.ValidateArgument(nameof(collection));

            return new ServiceBuilder<T, TImpl>(collection);
        }
    }
}
