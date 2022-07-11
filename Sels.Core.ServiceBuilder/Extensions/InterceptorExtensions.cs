using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Sels.Core.Conversion.Converters;
using Sels.Core.ServiceBuilder;
using Sels.Core.ServiceBuilder.Interceptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Contains static extension methods for working with interceptors
    /// </summary>
    public static class InterceptorExtensions
    {
        #region Tracing
        /// <summary>
        /// Addds an interceptor for tracing method duration and / or exceptions.
        /// </summary>
        /// <typeparam name="T">The service type that can be resolved as dependency</typeparam>
        /// <typeparam name="TImpl">The implementation type for <typeparamref name="T"/></typeparam>
        /// <param name="builder">Builder to add the interceptor to</param>
        /// <param name="interceptorBuilder">Builder for creating the interceptor</param>
        /// <returns>Current builder for method tracing</returns>
        public static IServiceBuilder<T, TImpl> Trace<T, TImpl>(this IServiceBuilder<T, TImpl> builder, Func<ITracingInterceptorBuilder, ITracingInterceptorBuilder> interceptorBuilder)
            where TImpl : class, T
            where T : class
        {
            builder.ValidateArgument(nameof(builder));
            interceptorBuilder.ValidateArgument(nameof(interceptorBuilder));

            return builder.InterceptedBy(x =>
            {
                var factory = x.GetService<ILoggerFactory>();
                var interceptor = new TracingInterceptor(factory);
                interceptorBuilder(interceptor);
                return interceptor;
            });
        }

        /// <summary>
        /// Addds an interceptor for tracing method duration and / or exceptions.
        /// </summary>
        /// <typeparam name="T">The service type that can be resolved as dependency</typeparam>
        /// <typeparam name="TImpl">The implementation type for <typeparamref name="T"/></typeparam>
        /// <param name="builder">Builder to add the interceptor to</param>
        /// <param name="interceptorBuilder">Builder for creating the interceptor</param>
        /// <param name="loggers">The loggers to use for tracing</param>
        /// <returns>Current builder for method tracing</returns>
        public static IServiceBuilder<T, TImpl> Trace<T, TImpl>(this IServiceBuilder<T, TImpl> builder, Func<ITracingInterceptorBuilder, ITracingInterceptorBuilder> interceptorBuilder, IEnumerable<ILogger?>? loggers)
            where TImpl : class, T
            where T : class
        {
            builder.ValidateArgument(nameof(builder));
            interceptorBuilder.ValidateArgument(nameof(interceptorBuilder));

            return builder.InterceptedBy(x =>
            {
                var interceptor = new TracingInterceptor(loggers);
                interceptorBuilder(interceptor);
                return interceptor;
            });
        }
        #endregion

        #region Caching
        /// <summary>
        /// Adds an interceptor for caching method return values.
        /// </summary>
        /// <typeparam name="T">The service type that can be resolved as dependency</typeparam>
        /// <typeparam name="TImpl">The implementation type for <typeparamref name="T"/></typeparam>
        /// <param name="builder">Builder to add the interceptor to</param>
        /// <param name="interceptorBuilder">Builder for creating the interceptor</param>
        /// <returns>Current builder for method tracing</returns>
        public static IServiceBuilder<T, TImpl> Cache<T, TImpl>(this IServiceBuilder<T, TImpl> builder, Func<ICachingInterceptorBuilder, ICachingInterceptorBuilder> interceptorBuilder)
            where TImpl : class, T
            where T : class
        {
            builder.ValidateArgument(nameof(builder));
            interceptorBuilder.ValidateArgument(nameof(interceptorBuilder));

            return builder.InterceptedBy(x =>
            {
                var interceptor = new CachingInterceptor(x.GetRequiredService<IDistributedCache>(), x.GetService<ITypeConverter>(), x.GetService<ILoggerFactory>());
                interceptorBuilder(interceptor);
                return interceptor;
            });
        }
        /// <summary>
        /// Adds an interceptor for caching method return values.
        /// </summary>
        /// <typeparam name="T">The service type that can be resolved as dependency</typeparam>
        /// <typeparam name="TImpl">The implementation type for <typeparamref name="T"/></typeparam>
        /// <param name="builder">Builder to add the interceptor to</param>
        /// <param name="interceptorBuilder">Builder for creating the interceptor</param>
        /// <param name="loggers">The loggers to use for tracing</param>
        /// <returns>Current builder for method tracing</returns>
        public static IServiceBuilder<T, TImpl> Cache<T, TImpl>(this IServiceBuilder<T, TImpl> builder, Func<ICachingInterceptorBuilder, ICachingInterceptorBuilder> interceptorBuilder, IEnumerable<ILogger?>? loggers)
            where TImpl : class, T
            where T : class
        {
            builder.ValidateArgument(nameof(builder));
            interceptorBuilder.ValidateArgument(nameof(interceptorBuilder));

            return builder.InterceptedBy(x =>
            {
                var interceptor = new CachingInterceptor(x.GetRequiredService<IDistributedCache>(), x.GetService<ITypeConverter>(), loggers);
                interceptorBuilder(interceptor);
                return interceptor;
            });
        }
        #endregion
    }
}
