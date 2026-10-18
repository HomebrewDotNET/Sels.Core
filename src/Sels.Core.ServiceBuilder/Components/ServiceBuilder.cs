using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Linq;
using Sels.Core.ServiceBuilder.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using Sels.Core.Extensions.Reflection;
using Sels.Core.Extensions.DependencyInjection;
using Sels.Core.Extensions.Conversion;
using Sels.Core.ServiceBuilder.Template;
using System.Runtime.CompilerServices;

namespace Sels.Core.ServiceBuilder
{
    /// <inheritdoc cref="IServiceBuilder{T, TImpl}"/>
    public class ServiceBuilder<T, TImpl> : BaseProxyGenerator<T, TImpl, IServiceBuilder<T, TImpl>>, IServiceBuilder<T, TImpl>
        where TImpl : class, T
        where T : class
    {
        // Statics
        private static readonly ConditionalWeakTable<ServiceDescriptor, Type> _implementationTypes = new ConditionalWeakTable<ServiceDescriptor, Type>();

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
        private TImpl _externalInstance;
        private bool _usesExternalInstance;
        private ServiceLifetime _scope = ServiceLifetime.Scoped;
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
        /// <inheritdoc/>
        protected override IServiceBuilder<T, TImpl> Self => this;
        /// <summary>
        /// The service key that can be used to register the service with a specific key. This is useful for scenarios where multiple implementations of the same service type need to be registered and resolved based on a key.
        /// </summary>
        public object Key { get; private set; }

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
        public IServiceBuilder<T, TImpl> AsKeyed(object key)
        {
            Key = key.ValidateArgument(nameof(key));
            return this;
        }
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
        public IServiceBuilder<T, TImpl> WithExternalInstance(TImpl instance)
        {
            instance = instance.ValidateArgument(nameof(instance));
            ConstructWith(_ => instance);
            _externalInstance = instance;
            _usesExternalInstance = true;
            _scope = ServiceLifetime.Singleton;
            return this;
        }

        /// <inheritdoc/>
        public IServiceBuilder<T, TImpl> ConstructWith(Func<IServiceProvider, TImpl> factory)
        {
            factory.ValidateArgument(nameof(factory));

            _factory = factory;
            _externalInstance = default;
            _usesExternalInstance = false;

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

            if (_usesExternalInstance)
            {
                return RegisterExternalInstance();
            }

            // Create proxy if we have interceptors set
            if (_interceptorFactories.HasValue())
            {
                // Add singleton proxy generator
                _collection.TryAddSingleton<ProxyGenerator>();

                // Add registration
                if (ServiceType.IsInterface)
                {
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
                        var generator = p.GetRequiredService<ProxyGenerator>();

                        // Trigger created because proxy is not an instance of TImpl
                        OnCreatedEvent?.Invoke(p, instance);


                        return GenerateProxy(p, generator, instance);
                    });
                }
                else
                {
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
                        var generator = p.GetRequiredService<ProxyGenerator>();

                        return GenerateProxy(p, generator, instance);
                    });
                }                            
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
            OnAdd(_collection.Last());

            return _collection;
        }

        private IServiceCollection RegisterExternalInstance()
        {
            if (_scope != ServiceLifetime.Singleton || Key != null || _interceptorFactories.HasValue())
            {
                throw new InvalidOperationException(ServiceBuilderConstants.Registration.ExternalInstanceConfigurationMessage);
            }

            switch (_behaviour)
            {
                case RegisterBehaviour.Default:
                    _collection.Add(ServiceDescriptor.Singleton(typeof(T), _externalInstance));
                    break;
                case RegisterBehaviour.TryAdd:
                    if (!_collection.Any(descriptor => descriptor.ServiceType == typeof(T)))
                    {
                        _collection.Add(ServiceDescriptor.Singleton(typeof(T), _externalInstance));
                    }
                    break;
                case RegisterBehaviour.Replace:
                    var existing = _collection.Where(descriptor => descriptor.ServiceType == typeof(T)).ToArray();
                    existing.Execute(descriptor => _collection.Remove(descriptor));
                    _collection.Add(ServiceDescriptor.Singleton(typeof(T), _externalInstance));
                    break;
                case RegisterBehaviour.TryAddImplementation:
                    var implementation = GetByImplementationType(typeof(T), typeof(TImpl)).FirstOrDefault();
                    if (implementation == null)
                    {
                        _collection.Add(ServiceDescriptor.Singleton(typeof(T), _externalInstance));
                    }
                    break;
                default:
                    throw new NotSupportedException($"Register behaviour <{_behaviour}> is not supported for external instances");
            }
            OnAdd(_collection.Last());
            return _collection;
        }

        private void RegisterServiceWithFactory<TInstance>(Func<IServiceProvider, TInstance> factory) where TInstance : class, T
        { 
            if(Key != null)
            {
                RegisterServiceWithFactoryAndKey(factory, Key);
                return;
            }
            if (_behaviour == RegisterBehaviour.Replace)
            {
                // Remove existing implementations
                var existing = _collection.Where(x => x.ServiceType == typeof(T)).ToArray();
                existing.Execute(x => _collection.Remove(x));
            }

            var actualFactory = new Func<IServiceProvider, TInstance>(p =>
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
                    _collection.AddTransient<T, TInstance>(actualFactory);
                    break;
                case (ServiceLifetime.Transient, RegisterBehaviour.TryAdd):
                    var existing = _collection.FirstOrDefault(x => typeof(T).Equals(x.ServiceType));
                    if (existing == null) _collection.AddTransient<T, TInstance>(actualFactory);
                    break;
                case (ServiceLifetime.Scoped, RegisterBehaviour.Replace):
                case (ServiceLifetime.Scoped, RegisterBehaviour.Default):
                    _collection.AddScoped<T, TInstance>(actualFactory);
                    break;
                case (ServiceLifetime.Scoped, RegisterBehaviour.TryAdd):
                    existing = _collection.FirstOrDefault(x => typeof(T).Equals(x.ServiceType));
                    if (existing == null) _collection.AddScoped<T, TInstance>(actualFactory);
                    break;
                case (ServiceLifetime.Singleton, RegisterBehaviour.Replace):
                case (ServiceLifetime.Singleton, RegisterBehaviour.Default):
                    _collection.AddSingleton<T, TInstance>(actualFactory);
                    break;
                case (ServiceLifetime.Singleton, RegisterBehaviour.TryAdd):
                    existing = _collection.FirstOrDefault(x => typeof(T).Equals(x.ServiceType));
                    if (existing == null) _collection.AddSingleton<T, TInstance>(actualFactory);
                    break;
                case (ServiceLifetime.Transient, RegisterBehaviour.TryAddImplementation):
                    existing = GetByImplementationType(typeof(T), typeof(TImpl)).FirstOrDefault();
                    if (existing == null) _collection.AddTransient<T, TInstance>(actualFactory);
                    break;
                case (ServiceLifetime.Scoped, RegisterBehaviour.TryAddImplementation):
                    existing = GetByImplementationType(typeof(T), typeof(TImpl)).FirstOrDefault();
                    if (existing == null) _collection.AddScoped<T, TInstance>(actualFactory);
                    break;
                case (ServiceLifetime.Singleton, RegisterBehaviour.TryAddImplementation):
                    existing = GetByImplementationType(typeof(T), typeof(TImpl)).FirstOrDefault();
                    if (existing == null) _collection.AddSingleton<T, TInstance>(actualFactory);
                    break;
                default:
                    throw new NotSupportedException($"Scope <{_scope}> and behaviour {_behaviour} are not supported");
            }
        }

        private void RegisterServiceWithFactoryAndKey<TInstance>(Func<IServiceProvider, TInstance> factory, object key)
            where TInstance : class, T
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            if (_behaviour == RegisterBehaviour.Replace)
            {
                // Remove existing implementations
                var existing = _collection.Where(x => x.ServiceType == typeof(T))
                                          .Where(x => x.IsKeyedService && x.ServiceKey.Equals(key)).ToArray();
                existing.Execute(x => _collection.Remove(x));
            }
            var actualFactory = new Func<IServiceProvider, object?, TInstance>((p, k) =>
            {
                // Call registered factory
                var instance = factory(p);
                // Trigger on created event
                if (instance is TImpl implementation)
                {
                    OnCreatedEvent.Invoke(p, implementation);
                }
                return instance;
            });
            switch ((_scope, _behaviour))
            {
                case (ServiceLifetime.Transient, RegisterBehaviour.Replace):
                case (ServiceLifetime.Transient, RegisterBehaviour.Default):
                    _collection.AddKeyedTransient<T, TInstance>(key, actualFactory);
                    break;
                case (ServiceLifetime.Transient, RegisterBehaviour.TryAdd):
                    var existing = _collection.FirstOrDefault(x => typeof(T).Equals(x.ServiceType) && x.IsKeyedService && x.ServiceKey.Equals(key));
                    if (existing == null)
                    {
                        _collection.AddKeyedTransient<T, TInstance>(key, actualFactory);
                    }
                    break;
                case (ServiceLifetime.Scoped, RegisterBehaviour.Replace):
                case (ServiceLifetime.Scoped, RegisterBehaviour.Default):
                    _collection.AddKeyedScoped<T, TInstance>(key, actualFactory);
                    break;
                case (ServiceLifetime.Scoped, RegisterBehaviour.TryAdd):
                    existing = _collection.FirstOrDefault(x => typeof(T).Equals(x.ServiceType) && x.IsKeyedService && x.ServiceKey.Equals(key));
                    if (existing == null)
                    {
                        _collection.AddKeyedScoped<T, TInstance>(key, actualFactory);
                    }
                    break;
                case (ServiceLifetime.Singleton, RegisterBehaviour.Replace):
                case (ServiceLifetime.Singleton, RegisterBehaviour.Default):
                    _collection.AddKeyedSingleton<T, TInstance>(key, actualFactory);
                    break;
                case (ServiceLifetime.Singleton, RegisterBehaviour.TryAdd):
                    existing = _collection.FirstOrDefault(x => typeof(T).Equals(x.ServiceType) && x.IsKeyedService && x.ServiceKey.Equals(key));
                    if (existing == null)
                    {
                        _collection.AddKeyedSingleton<T, TInstance>(key, actualFactory);
                    }
                    break;
                case (ServiceLifetime.Transient, RegisterBehaviour.TryAddImplementation):
                    existing = GetByImplementationType(typeof(T), typeof(TImpl)).FirstOrDefault();
                    if (existing == null)
                    {
                        _collection.AddKeyedTransient<T, TInstance>(key, actualFactory);
                    }
                    break;
                case (ServiceLifetime.Scoped, RegisterBehaviour.TryAddImplementation):
                    existing = GetByImplementationType(typeof(T), typeof(TImpl)).FirstOrDefault();
                    if (existing == null)
                    {
                        _collection.AddKeyedScoped<T, TInstance>(key, actualFactory);
                    }
                    break;
                case (ServiceLifetime.Singleton, RegisterBehaviour.TryAddImplementation):
                    existing = GetByImplementationType(typeof(T), typeof(TImpl)).FirstOrDefault();
                    if (existing == null)
                    {
                        _collection.AddKeyedSingleton<T, TInstance>(key, actualFactory);
                    }
                    break;
                default:
                    throw new NotSupportedException($"Scope <{_scope}> and behaviour {_behaviour} are not supported");
            }
        }

        private void OnAdd(ServiceDescriptor descriptor)
        {
            if (descriptor == null) throw new ArgumentNullException(nameof(descriptor));
            if(!_implementationTypes.TryGetValue(descriptor, out _))
            {
                _implementationTypes.Add(descriptor, typeof(TImpl));
            }
        }
        private IEnumerable<ServiceDescriptor> GetByImplementationType(Type serviceType, Type implementationType)
        {
            serviceType.ValidateArgument(nameof(serviceType));
            implementationType.ValidateArgument(nameof(implementationType));
            return _collection.Where(x => x.ServiceType == serviceType && _implementationTypes.TryGetValue(x, out var type) && type == implementationType);
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
