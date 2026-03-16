using Microsoft.Extensions.DependencyInjection;
using Sels.Core.Extensions;
using Sels.Core.ServiceBuilder.Injection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.ServiceBuilder.Extensions
{
    /// <summary>
    /// Contains static extension methods for working with service builder events.
    /// </summary>
    public static class EventExtensions
    {
        /// <summary>
        /// Inject service dependencies using a <see cref="ServiceInjector{TImpl}"/>.
        /// </summary>
        /// <typeparam name="T">The service type that can be resolved as dependency</typeparam>
        /// <typeparam name="TImpl">The implementation type for <typeparamref name="T"/></typeparam>
        /// <param name="builder">Builder to add the interceptor to</param>
        /// <returns>Current builder for method chaining</returns>
        public static IServiceBuilder<T, TImpl> InjectServices<T, TImpl>(this IServiceBuilder<T, TImpl> builder)
            where TImpl : class, T
            where T : class
        {
            builder.ValidateArgument(nameof(builder));

            builder.Collection.AddServiceInjector<TImpl>();

            return builder.OnCreated<ServiceInjector<TImpl>>();
        }
    }
}
