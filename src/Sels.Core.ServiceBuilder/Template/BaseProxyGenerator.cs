using Castle.DynamicProxy;
using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sels.Core.ServiceBuilder.Template
{
    /// <summary>
    /// Base class for generating proxies configued by <see cref="IProxyBuilder{TProxy, TImpl, TDerived}"/>.
    /// </summary>
    /// <typeparam name="TProxy">The type being intercepted</typeparam>
    /// <typeparam name="TImpl">The type the proxy will wrap</typeparam>
    /// <typeparam name="TDerived">The type to return for the fluent syntax</typeparam>
    public abstract class BaseProxyGenerator<TProxy, TImpl, TDerived> : IProxyBuilder<TProxy, TImpl, TDerived> 
        where TImpl : class, TProxy
        where TProxy : class
    {
        // Fields
        /// <summary>
        /// The registered interceptors.
        /// </summary>
        protected readonly List<Func<IServiceProvider, IEnumerable<IInterceptor>>> _interceptorFactories = new List<Func<IServiceProvider, IEnumerable<IInterceptor>>>();

        // Properties
        /// <summary>
        /// THe instance to return for the proxy builder.
        /// </summary>
        protected abstract TDerived Self { get; }

        /// <inheritdoc/>
        public TDerived UsingInterceptor(Func<IServiceProvider, IEnumerable<IInterceptor>> factory)
        {
            factory.ValidateArgument(nameof(factory));

            _interceptorFactories.Add(factory);

            return Self;
        }

        /// <summary>
        /// Generates a new proxy that will be intercepted by any registered interceptor.
        /// </summary>
        /// <param name="serviceProvider">Service provider used to resolve the interceptors</param>
        /// <param name="generator">The proxy generator to use</param>
        /// <param name="target">The instance being proxied</param>
        /// <returns>A proxy where methods can be intercepted by any registered interceptor and where method calls will be handled by<paramref name="target"/></returns>
        protected TProxy GenerateProxy(IServiceProvider serviceProvider, ProxyGenerator generator, TImpl target)
        {
            serviceProvider.ValidateArgument(nameof(serviceProvider));
            generator.ValidateArgument(nameof(generator));
            target.ValidateArgument(nameof(target));

            var interceptors = _interceptorFactories.Select(x => x(serviceProvider)).Where(x => x != null).SelectMany(x => x).Where(x => x != null).ToArray();
            if (typeof(TProxy).IsInterface)
            {
                return generator.CreateInterfaceProxyWithTargetInterface<TProxy>(target, interceptors);
            }
            else
            {
                return generator.CreateClassProxyWithTarget(target, interceptors);
            }
        }
    }
}
