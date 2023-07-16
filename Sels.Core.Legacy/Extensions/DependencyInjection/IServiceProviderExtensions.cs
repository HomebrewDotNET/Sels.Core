using Microsoft.Extensions.DependencyInjection;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using System;

namespace Sels.Core.Extensions.DependencyInjection
{
    /// <summary>
    /// Contains extension methods for <see cref="IServiceProvider"/>
    /// </summary>
    public static class IServiceProviderExtensions
    {
        /// <summary>
        /// Attempts to create an instance of <paramref name="type"/> using <paramref name="provider"/> to resolve any dependencies.
        /// </summary>
        /// <param name="provider">The provider to use to resolve dependencies</param>
        /// <param name="type">The type to create an instance for</param>
        /// <returns>An instance of <paramref name="type"/></returns>
        public static object CreateInstance(this IServiceProvider provider, Type type)
        {
            provider.ValidateArgument(nameof(provider));
            type.ValidateArgument(nameof(type));

            return ActivatorUtilities.CreateInstance(provider, type);
        }

        /// <summary>
        /// Attempts to create an instance of <typeparamref name="T"/> using <paramref name="provider"/> to resolve any dependencies.
        /// </summary>
        /// <param name="provider">The provider to use to resolve dependencies</param>
        /// <returns>An instance of <typeparamref name="T"/></returns>
        /// <typeparam name="T">The type to create an instance for</typeparam>
        public static T CreateInstance<T>(this IServiceProvider provider)
        {
            provider.ValidateArgument(nameof(provider));

            return provider.CreateInstance(typeof(T)).CastTo<T>();
        }
    }
}
