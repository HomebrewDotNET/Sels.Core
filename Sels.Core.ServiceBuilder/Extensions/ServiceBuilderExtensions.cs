using Sels.Core;
using Sels.Core.Extensions.Reflection;
using Sels.Core.ServiceBuilder;

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

        /// <summary>
        /// Instead creating a new instance of <typeparamref name="TImpl"/> specifically, the builder will request an instance of <typeparamref name="TImpl"/> from the DI container instead when resolving <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The service type</typeparam>
        /// <typeparam name="TImpl">The implementation type for <typeparamref name="T"/></typeparam>
        /// <param name="builder">The builder to change into a forwarded service</param>
        /// <returns>Builder for creating the service</returns>
        public static IServiceBuilder<T, TImpl> AsForwardedService<T, TImpl>(this IServiceBuilder<T, TImpl> builder)
            where TImpl : class, T
            where T : class
        {
            Guard.IsNotNull(builder);

            var existing = builder.Collection.FirstOrDefault(x => x.ServiceType.Is<TImpl>());
            return builder.ConstructWith(p => p.GetRequiredService<TImpl>()).WithLifetime(existing?.Lifetime ?? ServiceLifetime.Scoped);
        }
    }
}
