using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Linq;
using Sels.Core.ServiceBuilder.Events;
using System;
using System.Collections.Generic;
using System.Linq;

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
        public event Action<IServiceBuilder<T, TImpl>> OnRegisteringEvent;
        /// <summary>
        /// Event that gets raised each time an instance of <typeparamref name="TImpl"/> is created using the current builder.
        /// </summary>
        public event Action<IServiceProvider, TImpl> OnCreatedEvent;

        // State
        private Func<IServiceProvider, TImpl> _factory;
        private ServiceLifetime _scope = ServiceLifetime.Scoped;
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
        /// <inheritdoc/>
        public IServiceCollection Collection => _collection;

        /// <inheritdoc cref="ServiceBuilder{T, TImpl}"/>
        /// <param name="collection"><inheritdoc cref="_collection"/></param>
        public ServiceBuilder(IServiceCollection collection)
        {
            _collection = collection.ValidateArgument(nameof(collection));

            // Add handler for globally defined handlers
            OnCreated((p, i) =>
            {
                // Typed handlers
                p.GetServices<IOnCreatedHandler<TImpl>>().Execute(x => x.Handle(p, i));

                // Untyped handlers
                p.GetServices<IOnCreatedHandler>().Execute(x => x.Handle(p, i));
            });
        }

        #region Building
        /// <inheritdoc/>
        public IServiceBuilder<T, TImpl> WithInstance(TImpl instance)
        {
            instance.ValidateArgument(nameof(instance));

            ConstructWith(x => instance);

            // Force scope to singleton since factory returns the same instance each time.
            OnRegisteringEvent += SetToSingleton;

            return this;
        }
        /// <inheritdoc/>
        public IServiceBuilder<T, TImpl> ConstructWith(Func<IServiceProvider, TImpl> factory)
        {
            factory.ValidateArgument(nameof(factory));

            _factory = factory;

            // Remove singleton handler if set
            OnRegisteringEvent -= SetToSingleton;
            return this;
        }

        /// <inheritdoc/>
        public IServiceBuilder<T, TImpl> WithLifetime(ServiceLifetime scope)
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
            OnRegisteringEvent?.Invoke(this);

            // Create proxy if we have interceptors set
            if (_interceptorFactories.HasValue())
            {
                // Add singleton proxy generator
                _collection.TryAddSingleton<ProxyGenerator>();
                              
                // Add registration
                RegisterServiceWithFactory(p =>
                {
                    // Factory for the actual implementation
                    var instanceFactory = _factory ?? new Func<IServiceProvider, TImpl>(prov =>
                    {
                        // Only try service provider if we have an abstraction. Otherwise factory will call itself
                        if (!IsAbstractionless)
                        {
                            return prov.GetService<TImpl>() ?? prov.CreateInstance<TImpl>();
                        }
                        else
                        {
                            return prov.CreateInstance<TImpl>();
                        }
                    });
                    var instance = instanceFactory(p);

                    // Create using defined factories
                    var interceptors = _interceptorFactories.Select(x => x(p)).Where(x => x != null).SelectMany(x => x).Where(x => x != null);
                    var generator = p.GetRequiredService<ProxyGenerator>();

                    if (ServiceType.IsInterface)
                    {
                        // Trigger created because proxy is not an instance of TImpl
                        OnCreatedEvent?.Invoke(p, instance);

                        return generator.CreateInterfaceProxyWithTargetInterface<T>(instance, interceptors.ToArray());
                    }
                    else
                    {
                        return generator.CreateClassProxyWithTarget(instance, interceptors.ToArray());
                    }                    
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
                    RegisterServiceWithFactory(p => p.CreateInstance<TImpl>());
                }
            }

            return _collection;
        }

        private void RegisterServiceWithFactory(Func<IServiceProvider, T> factory) 
        { 
            if(_behaviour == RegisterBehaviour.Replace)
            {
                // Remove existing implementations
                var existing = _collection.Where(x => x.ServiceType == typeof(T)).ToArray();
                existing.Execute(x => _collection.Remove(x));
            }

            var actualFactory = new Func<IServiceProvider, T>(p =>
            {
                // Call registered factory
                var instance = factory(p);

                // Trigger on created event
                if(instance is TImpl implementation) OnCreatedEvent.Invoke(p, implementation);

                return instance;
            });

            switch ((_scope, _behaviour))
            {
                case (ServiceLifetime.Transient, RegisterBehaviour.Replace):
                case (ServiceLifetime.Transient, RegisterBehaviour.Default):
                    _collection.AddTransient(actualFactory);
                    break;
                case (ServiceLifetime.Transient, RegisterBehaviour.TryAdd):
                    _collection.TryAddTransient(actualFactory);
                    break;
                case (ServiceLifetime.Scoped, RegisterBehaviour.Replace):
                case (ServiceLifetime.Scoped, RegisterBehaviour.Default):
                    _collection.AddScoped(actualFactory);
                    break;
                case (ServiceLifetime.Scoped, RegisterBehaviour.TryAdd):
                    _collection.TryAddScoped(actualFactory);
                    break;
                case (ServiceLifetime.Singleton, RegisterBehaviour.Replace):
                case (ServiceLifetime.Singleton, RegisterBehaviour.Default):
                    _collection.AddSingleton(actualFactory);
                    break;
                case (ServiceLifetime.Singleton, RegisterBehaviour.TryAdd):
                    _collection.TryAddSingleton(actualFactory);
                    break;
                default:
                    throw new NotSupportedException($"Scope <{_scope}> and behaviour {_behaviour} are not supported");
            }
        }

        #endregion

        #region Events
        /// <inheritdoc/>
        public IServiceBuilder<T, TImpl> OnCreated(Action<IServiceProvider, TImpl> action)
        {
            action.ValidateArgument(nameof(action));
            OnCreatedEvent += action;
            return this;
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
