using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sels.Core.Components.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sels.Core.Extensions.Linq;

namespace Sels.Core.ServiceBuilder
{
    /// <inheritdoc cref="IServiceBuilder{T, TImpl}"/>
    public class ServiceBuilder<T, TImpl> : IServiceBuilder<T, TImpl>
        where TImpl : class, T
        where T : class
    {
        // Fields
        /// <summary>
        /// The collection to add the built service to
        /// </summary>
        protected readonly IServiceCollection _collection;
        /// <summary>
        /// Event that gets raised before adding the service to the service collection.
        /// </summary>
        public event Action<IServiceBuilder<T, TImpl>> OnRegistering;

        // State
        private Func<IServiceProvider, TImpl> _factory;
        private ServiceScope _scope = ServiceScope.Scoped;
        private readonly List<Func<IServiceProvider, IEnumerable<IInterceptor>>> _interceptorFactories = new List<Func<IServiceProvider, IEnumerable<IInterceptor>>>();
        private RegisterBehaviour _behaviour = RegisterBehaviour.Default;

        // Properties
        /// <summary>
        /// If the current service registration is not an implementation for a service type but just a registration of the concrete type.
        /// </summary>
        public bool IsAbstractionless => typeof(T).Equals(typeof(TImpl));
        /// <inheritdoc/>
        public Type ServiceType => typeof(T);
        /// <inheritdoc/>
        public Type ImplementationType => typeof(TImpl);

        /// <inheritdoc cref="ServiceBuilder{T, TImpl}"/>
        /// <param name="collection"><inheritdoc cref="_collection"/></param>
        public ServiceBuilder(IServiceCollection collection)
        {
            _collection = collection.ValidateArgument(nameof(collection));
        }

        #region Building
        /// <inheritdoc/>
        public IServiceBuilder<T, TImpl> WithInstance(TImpl instance)
        {
            instance.ValidateArgument(nameof(instance));

            ConstructWith(x => instance);

            // Force scope to singleton since factory returns instance.
            OnRegistering += SetToSingleton;

            return this;
        }
        /// <inheritdoc/>
        public IServiceBuilder<T, TImpl> ConstructWith(Func<IServiceProvider, TImpl> factory)
        {
            factory.ValidateArgument(nameof(factory));

            _factory = factory;

            // Remove singleton handler if set
            OnRegistering -= SetToSingleton;
            return this;
        }

        /// <inheritdoc/>
        public IServiceBuilder<T, TImpl> WithScope(ServiceScope scope)
        {
            _scope = scope;
            return this;
        }

        /// <inheritdoc/>
        public IServiceBuilder<T, TImpl> UsingInterceptors(Func<IServiceProvider, IEnumerable<IInterceptor>> factory)
        {
            factory.ValidateArgument(nameof(factory));

            _interceptorFactories.Add(factory);

            return this;
        }
        /// <inheritdoc/>
        public IServiceBuilder<T, TImpl> WithBehaviour(RegisterBehaviour behaviour)
        {
            if (_collection.IsReadOnly && _behaviour == RegisterBehaviour.Replace) throw new NotSupportedException($"Service collection is read only so behaviour {behaviour} is not supported");

            _behaviour = behaviour;
            return this;
        }
        #endregion

        #region Registering
        /// <inheritdoc/>
        public IServiceCollection Register()
        {
            // Trigger events
            OnRegistering?.Invoke(this);

            // Create proxy if we have interceptors set
            if (_interceptorFactories.HasValue())
            {
                // Add singleton proxy generator
                _collection.TryAddSingleton<ProxyGenerator>();
                               
                if(_factory == null)
                {
                    // Registering type without service type (just the concrete type) but we have no factory
                    if (IsAbstractionless)
                    {
                        throw new NotSupportedException($"Interceptors are set but implementation <{typeof(TImpl)}> doesn't have a factory defined so we can't create the instance for the proxy class");
                    }

                    // Add registration for actual implementation
                    _collection.Register<TImpl>(_scope);
                }

                // Add registration
                RegisterServiceWithFactory(p =>
                {
                    // Factory for the actual implementation
                    var instanceFactory = _factory;
                    // Create using defined factories
                    var interceptors = _interceptorFactories.Select(x => x(p)).Where(x => x != null).SelectMany(x => x).Where(x => x != null);
                    var generator = p.GetRequiredService<ProxyGenerator>();
                    return generator.CreateInterfaceProxyWithTargetInterface<T>(instanceFactory != null ? instanceFactory(p) : p.GetRequiredService<TImpl>(), interceptors.ToArray());
                });               
            }
            // Standard registration
            else
            {
                if(_factory != null)
                {
                    RegisterServiceWithFactory(_factory);
                }
                else
                {
                    RegisterService();
                }
            }

            return _collection;
        }

        private void RegisterServiceWithFactory(Func<IServiceProvider, T> factory) 
        { 
            if(_behaviour == RegisterBehaviour.Replace)
            {
                // Remove existing implementations
                var existing = _collection.Where(x => x.ServiceType == typeof(T));
                existing.Execute(x => _collection.Remove(x));
            }

            switch((_scope, _behaviour))
            {
                case (ServiceScope.Transient, RegisterBehaviour.Replace):
                case (ServiceScope.Transient, RegisterBehaviour.Default):
                    _collection.AddTransient(factory);
                    break;
                case (ServiceScope.Transient, RegisterBehaviour.TryAdd):
                    _collection.TryAddTransient(factory);
                    break;
                case (ServiceScope.Scoped, RegisterBehaviour.Replace):
                case (ServiceScope.Scoped, RegisterBehaviour.Default):
                    _collection.AddScoped(factory);
                    break;
                case (ServiceScope.Scoped, RegisterBehaviour.TryAdd):
                    _collection.TryAddScoped(factory);
                    break;
                case (ServiceScope.Singleton, RegisterBehaviour.Replace):
                case (ServiceScope.Singleton, RegisterBehaviour.Default):
                    _collection.AddSingleton(factory);
                    break;
                case (ServiceScope.Singleton, RegisterBehaviour.TryAdd):
                    _collection.TryAddSingleton(factory);
                    break;
                default:
                    throw new NotSupportedException($"Scope <{_scope}> and behaviour {_behaviour} are not supported");
            }
        }

        private void RegisterService()
        {
            if (_behaviour == RegisterBehaviour.Replace)
            {
                // Remove existing implementations
                var existing = _collection.Where(x => x.ServiceType == typeof(T));
                existing.Execute(x => _collection.Remove(x));
            }

            switch ((_scope, _behaviour))
            {
                case (ServiceScope.Transient, RegisterBehaviour.Replace):
                case (ServiceScope.Transient, RegisterBehaviour.Default):
                    if (IsAbstractionless) _collection.AddTransient<T>(); else _collection.AddTransient<T, TImpl>();
                    break;
                case (ServiceScope.Transient, RegisterBehaviour.TryAdd):
                    if (IsAbstractionless) _collection.TryAddTransient<T>(); else _collection.TryAddTransient<T, TImpl>();
                    break;
                case (ServiceScope.Scoped, RegisterBehaviour.Replace):
                case (ServiceScope.Scoped, RegisterBehaviour.Default):
                    if (IsAbstractionless) _collection.AddScoped<T>(); else _collection.AddScoped<T, TImpl>();
                    break;
                case (ServiceScope.Scoped, RegisterBehaviour.TryAdd):
                    if(IsAbstractionless) _collection.TryAddScoped<T>(); else _collection.TryAddScoped<T, TImpl>();
                    break;
                case (ServiceScope.Singleton, RegisterBehaviour.Replace):
                case (ServiceScope.Singleton, RegisterBehaviour.Default):
                    if (IsAbstractionless) _collection.AddSingleton<T>(); else _collection.AddSingleton<T, TImpl>();
                    break;
                case (ServiceScope.Singleton, RegisterBehaviour.TryAdd):
                    if (IsAbstractionless) _collection.TryAddSingleton<T>(); else _collection.TryAddSingleton<T, TImpl>();
                    break;
                default:
                    throw new NotSupportedException($"Scope <{_scope}> and behaviour {_behaviour} are not supported");
            }
        }

        #endregion

        private void SetToSingleton(IServiceBuilder<T, TImpl> builder)
        {
            builder.ValidateArgument(nameof(builder));
            builder.AsSingleton();
        }
    }

    /// <inheritdoc cref="IServiceBuilder{T}"/>
    public class ServiceBuilder<T> : ServiceBuilder<T, T>, IServiceBuilder<T>
        where T : class
    {
        /// <inheritdoc cref="ServiceBuilder{T}"/>
        /// <param name="collection"><inheritdoc cref="ServiceBuilder{T, TImpl}._collection"/></param>
        public ServiceBuilder(IServiceCollection collection) : base(collection)
        {
        }

        /// <inheritdoc/>
        IServiceBuilder<T, TImpl> IServiceBuilder<T>.As<TImpl>()
        {
            return new ServiceBuilder<T, TImpl>(_collection);
        }
    }
}
