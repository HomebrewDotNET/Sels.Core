using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Sels.Core.Conversion.Converters;
using Sels.Core.Dispose;
using Sels.Core.Extensions;
using Sels.Core.ServiceBuilder;
using Sels.Core.ServiceBuilder.Interceptors;
using Sels.Core.ServiceBuilder.Contracts.Interceptors.Caching;
using Sels.Core.ServiceBuilder.Interceptors;
using Sels.Core.ServiceBuilder.Interceptors.Caching;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Contains static extension methods for working with interceptors
    /// </summary>
    public static class InterceptorExtensions
    {
        #region Tracing
        /// <summary>
        /// Adds an interceptor for tracing method duration and / or exceptions.
        /// </summary>
        /// <typeparam name="T">The service type that can be resolved as dependency</typeparam>
        /// <typeparam name="TImpl">The implementation type for <typeparamref name="T"/></typeparam>
        /// <param name="builder">Builder to add the interceptor to</param>
        /// <param name="interceptorBuilder">Builder for creating the interceptor</param>
        /// <param name="useFactory">If a <see cref="ILoggerFactory"/> can be used to create a logger with the same category as the target instance, otherwise a <see cref="ILogger{TCategoryName}"/> will be used using the interceptor category</param>
        /// <returns>Current builder for method chaining</returns>
        public static IServiceBuilder<T, TImpl> Trace<T, TImpl>(this IServiceBuilder<T, TImpl> builder, Func<ITracingInterceptorBuilder, object> interceptorBuilder, bool useFactory = true)
            where TImpl : class, T
            where T : class
        {
            builder.ValidateArgument(nameof(builder));
            interceptorBuilder.ValidateArgument(nameof(interceptorBuilder));

            return builder.InterceptedBy(x =>
            {
                var interceptor = useFactory ? new TracingInterceptor(x.GetService<ILoggerFactory>()) :  new TracingInterceptor(x.GetService<ILogger<TracingInterceptor>>());
                interceptorBuilder(interceptor);
                return interceptor;
            });
        }
        #endregion

        #region Caching
        /// <summary>
        /// Adds an interceptor for caching method return values using <see cref="IMemoryCache"/>.
        /// </summary>
        /// <typeparam name="T">The service type that can be resolved as dependency</typeparam>
        /// <typeparam name="TImpl">The implementation type for <typeparamref name="T"/></typeparam>
        /// <param name="builder">Builder to add the interceptor to</param>
        /// <param name="interceptorBuilder">Builder for creating the interceptor</param>
        /// <returns>Current builder for method chaining</returns>
        public static IServiceBuilder<T, TImpl> Cache<T, TImpl>(this IServiceBuilder<T, TImpl> builder, Func<ICachingInterceptorBuilder<TImpl, IMemoryCacheOptions>, object> interceptorBuilder)
            where TImpl : class, T
            where T : class
        {
            builder.ValidateArgument(nameof(builder));
            interceptorBuilder.ValidateArgument(nameof(interceptorBuilder));

            return builder.InterceptedBy(x =>
            {
                var provider = new MemoryCachingProvider(x.GetRequiredService<IMemoryCache>(), x.GetService<ILogger<MemoryCachingProvider>>());
                var interceptor = new CachingInterceptor<TImpl, IMemoryCacheOptions>(provider, x.GetService<ITypeConverter>(), x.GetService<ILoggerFactory>(), x.GetService<ILogger<CachingInterceptor<TImpl, IMemoryCacheOptions>>>());
                interceptorBuilder(interceptor);
                return interceptor;
            });
        }
        /// <summary>
        /// Adds an interceptor for caching method return values using a <see cref="IDistributedCache"/>.
        /// </summary>
        /// <typeparam name="T">The service type that can be resolved as dependency</typeparam>
        /// <typeparam name="TImpl">The implementation type for <typeparamref name="T"/></typeparam>
        /// <typeparam name="TEncoding">The encoding to use for the serialized strings</typeparam>
        /// <param name="builder">Builder to add the interceptor to</param>
        /// <param name="interceptorBuilder">Builder for creating the interceptor</param>
        /// <returns>Current builder for method chaining</returns>
        public static IServiceBuilder<T, TImpl> CacheDistributed<T, TImpl, TEncoding>(this IServiceBuilder<T, TImpl> builder, Func<ICachingInterceptorBuilder<TImpl, IDistributedCacheOptions>, object> interceptorBuilder) where TEncoding : Encoding, new()
            where TImpl : class, T
            where T : class
        {
            builder.ValidateArgument(nameof(builder));
            interceptorBuilder.ValidateArgument(nameof(interceptorBuilder));

            return builder.InterceptedBy(x =>
            {
                var provider = new DistributedCachingProvider<TEncoding>(x.GetRequiredService<IDistributedCache>(), x.GetService<ILogger<DistributedCachingProvider<TEncoding>>>());
                var interceptor = new CachingInterceptor<TImpl, IDistributedCacheOptions>(provider, x.GetService<ITypeConverter>(), x.GetService<ILoggerFactory>(), x.GetService<ILogger<CachingInterceptor<TImpl, IDistributedCacheOptions>>>());
                interceptorBuilder(interceptor);
                return interceptor;
            });
        }

        /// <inheritdoc cref="CacheDistributed{T, TImpl, TEncoding}(IServiceBuilder{T, TImpl}, Func{ICachingInterceptorBuilder{TImpl, IDistributedCacheOptions}, object})"/>
        public static IServiceBuilder<T, TImpl> CacheDistributed<T, TImpl>(this IServiceBuilder<T, TImpl> builder, Func<ICachingInterceptorBuilder<TImpl, IDistributedCacheOptions>, object> interceptorBuilder)
            where TImpl : class, T
            where T : class
        {
            return CacheDistributed<T, TImpl, UTF8Encoding>(builder, interceptorBuilder);
        }
        #endregion

        #region Dispose
        /// <summary>
        /// Adds an interceptor that throws an <see cref="ObjectDisposedException"/> when the target is disposing/is disposed.
        /// </summary>
        /// <typeparam name="T">The service type that can be resolved as dependency</typeparam>
        /// <typeparam name="TImpl">The implementation type for <typeparamref name="T"/></typeparam>
        /// <param name="builder">Builder to add the interceptor to</param>
        /// <returns>Current builder for method chaining</returns>
        public static IServiceBuilder<T, TImpl> HandleDisposed<T, TImpl>(this IServiceBuilder<T, TImpl> builder)
            where TImpl : class, T, IDisposableState
            where T : class
        {
            return builder.InterceptedBy(x => new DisposableInterceptor(x.GetService<ILogger<DisposableInterceptor>>()));
        }
        #endregion
    }
}
