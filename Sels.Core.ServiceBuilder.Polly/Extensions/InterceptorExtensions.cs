using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Polly;
using Sels.Core.Conversion.Converters;
using Sels.Core.Dispose;
using Sels.Core.Extensions;
using Sels.Core.ServiceBuilder;
using Sels.Core.ServiceBuilder.Interceptors;
using Sels.Core.ServiceBuilder.Contracts.Interceptors.Caching;
using Sels.Core.ServiceBuilder.Interceptors.Caching;
using Sels.Core.ServiceBuilder.Polly;
using System;
using System.Collections.Generic;
using System.Text;
using Castle.DynamicProxy;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Contains static extension methods for working with interceptors
    /// </summary>
    public static class InterceptorExtensions
    {
        #region Execute
        /// <summary>
        /// Adds an interceptor that executes methods using Polly policies.
        /// </summary>
        /// <typeparam name="T">The service type that can be resolved as dependency</typeparam>
        /// <typeparam name="TImpl">The implementation type for <typeparamref name="T"/></typeparam>
        /// <typeparam name="TDerived">The type to return for the fluent syntax</typeparam>
        /// <param name="builder">Builder to add the interceptor to</param>
        /// <param name="interceptorBuilder">Builder for creating the interceptor</param>
        /// <returns>Current builder for method chaining</returns>
        public static TDerived ExecuteWithPolly<T, TImpl, TDerived>(this IProxyBuilder<T, TImpl, TDerived> builder, Func<IServiceProvider, IRootPolicyInterceptorBuilder<T>, object> interceptorBuilder)
            where TImpl : class, T
            where T : class
        {
            builder.ValidateArgument(nameof(builder));
            interceptorBuilder.ValidateArgument(nameof(interceptorBuilder));

            return builder.InterceptedBy(x => {
                var interceptor = new PollyExecutorInterceptor<T>(x.GetService<ILogger<PollyExecutorInterceptor<T>>>());
                interceptorBuilder(x, interceptor);
                return interceptor.ToInterceptor();
            });
        }
        /// <summary>
        /// Adds an interceptor that executes methods using Polly policies.
        /// </summary>
        /// <typeparam name="T">The service type that can be resolved as dependency</typeparam>
        /// <typeparam name="TImpl">The implementation type for <typeparamref name="T"/></typeparam>
        /// <typeparam name="TDerived">The type to return for the fluent syntax</typeparam>
        /// <param name="builder">Builder to add the interceptor to</param>
        /// <param name="interceptorBuilder">Builder for creating the interceptor</param>
        /// <returns>Current builder for method chaining</returns>
        public static TDerived ExecuteWithPolly<T, TImpl, TDerived>(this IProxyBuilder<T, TImpl, TDerived> builder, Func<IRootPolicyInterceptorBuilder<T>, object> interceptorBuilder)
            where TImpl : class, T
            where T : class
        {
            builder.ValidateArgument(nameof(builder));
            interceptorBuilder.ValidateArgument(nameof(interceptorBuilder));

            return builder.ExecuteWithPolly((p, b) => interceptorBuilder(b));
        }
        #endregion
    }
}
