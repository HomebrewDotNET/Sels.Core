using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sels.Core;
using Sels.Core.Extensions;
using Sels.Core.Extensions.DependencyInjection;
using Sels.Core.Extensions.Fluent;
using Sels.Core.Mediator;
using Sels.Core.Mediator.Components;
using Sels.Core.Mediator.Event;
using Sels.Core.Mediator.Messaging;
using Sels.Core.Mediator.Request;
using Sels.Core.ServiceBuilder;
using static Sels.Core.Delegates;
using static Sels.Core.Delegates.Async;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Contains extension methods for registering services into a service collection.
    /// </summary>
    public static class ApplicationRegistrations
    {
        #region Obsolete
        /// <summary>
        /// Adds all the required services for sending messages and subscribing to them.
        /// </summary>
        /// <param name="services">The service collection to add the services to</param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        [Obsolete("Use the new Sels.Core.Mediator.Event components")]
        public static IServiceCollection AddMessanger(this IServiceCollection services)
        {
            services.ValidateArgument(nameof(services));

            services.TryAddScoped<IMessanger, Messenger>();
            services.New<IMessageSubscriber>().As<SubscriptionManager>().AsSingleton().Trace(x => x.Duration.OfAll).TryRegister();
            services.TryAddScoped(typeof(IMessanger<>), typeof(Messenger<>));

            return services;
        }

        /// <summary>
        /// Adds service <typeparamref name="TSubscriber"/> as global subscriber that can received messages of type <typeparamref name="TMessage"/> sent from other services.
        /// </summary>
        /// <typeparam name="TMessage">The type of message to subscribe to</typeparam>
        /// <typeparam name="TSubscriber">The type of the service to receive the messages with</typeparam>
        /// <param name="services">The service collection to add the services to</param>
        /// <param name="asForwardedService">If the instance needs to be resolved from the DI container instead. Set to true of <typeparamref name="TSubscriber"/> is already registered in the DI container</param>
        /// <param name="serviceScope">The scope for the registered service</param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        [Obsolete("Use the new Sels.Core.Mediator.Event components")]
        public static IServiceCollection AddSubscriber<TMessage, TSubscriber>(this IServiceCollection services, bool asForwardedService = false, ServiceLifetime serviceScope = ServiceLifetime.Scoped) where TSubscriber : class, ISubscriber<TMessage>
        {
            services.ValidateArgument(nameof(services));

            services.New<ISubscriber<TMessage>>().As<TSubscriber>()
                        .When(asForwardedService, b => b.AsForwardedService())
                        .WithLifetime(serviceScope)
                        .Register();

            return services;
        }

        /// <summary>
        /// Adds service <typeparamref name="THandler"/> as global subscriber that can received messages of type <typeparamref name="TMessage"/> sent from other services.
        /// </summary>
        /// <typeparam name="TMessage">The type of message to subscribe to</typeparam>
        /// <typeparam name="THandler">The type of the service to receive the messages with</typeparam>
        /// <param name="services">The service collection to add the services to</param>
        /// <param name="action">Delegate used to handle messages of type <typeparamref name="TMessage"/> using an instance of <typeparamref name="THandler"/></param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        [Obsolete("Use the new Sels.Core.Mediator.Event components")]
        public static IServiceCollection AddSubscriber<TMessage, THandler>(this IServiceCollection services, AsyncAction<THandler, object, TMessage, CancellationToken> action) where THandler : notnull
        {
            services.ValidateArgument(nameof(services));
            action.ValidateArgument(nameof(action));

            services.New<ISubscriber<TMessage>>().As<DelegateSubscriber<THandler, TMessage>>().ConstructWith(x => new DelegateSubscriber<THandler, TMessage>(x.GetRequiredService<THandler>(), action)).Register();

            return services;
        }
        /// <summary>
        /// Adds service <typeparamref name="THandler"/> as global subscriber that can received messages of type <typeparamref name="TMessage"/> sent from other services.
        /// </summary>
        /// <typeparam name="TMessage">The type of message to subscribe to</typeparam>
        /// <typeparam name="THandler">The type of the service to receive the messages with</typeparam>
        /// <param name="services">The service collection to add the services to</param>
        /// <param name="handlerBuilder">Delegate for configuring the handler</param>
        /// <param name="action">Delegate used to handle messages of type <typeparamref name="TMessage"/> using an instance of <typeparamref name="THandler"/></param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        [Obsolete("Use the new Sels.Core.Mediator.Event components")]
        public static IServiceCollection AddSubscriber<TMessage, THandler>(this IServiceCollection services, Func<IServiceBuilder<THandler>, IServiceBuilder> handlerBuilder, AsyncAction<THandler, object, TMessage, CancellationToken> action) where THandler : class
        {
            services.ValidateArgument(nameof(services));
            handlerBuilder.ValidateArgument(nameof(handlerBuilder));
            action.ValidateArgument(nameof(action));

            handlerBuilder(services.New<THandler>()).Register();

            services.New<ISubscriber<TMessage>>().As<DelegateSubscriber<THandler, TMessage>>().ConstructWith(x => new DelegateSubscriber<THandler, TMessage>(x.GetRequiredService<THandler>(), action)).Register();

            return services;
        }

        /// <summary>
        /// Adds service <typeparamref name="TSubscriber"/> as global subscriber that can received messages of all types sent from other services.
        /// </summary>
        /// <typeparam name="TSubscriber">The type of the service to receive the messages with</typeparam>
        /// <param name="services">The service collection to add the services to</param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        [Obsolete("Use the new Sels.Core.Mediator.Event components")]
        public static IServiceCollection AddSubscriber<TSubscriber>(this IServiceCollection services) where TSubscriber : class, ISubscriber
        {
            services.ValidateArgument(nameof(services));

            services.New<ISubscriber>().As<TSubscriber>().Register();

            return services;
        }
        /// <summary>
        /// Adds service <typeparamref name="THandler"/> as global subscriber that can received messages of all types sent from other services.
        /// </summary>
        /// <typeparam name="THandler">The type of the service to receive the messages with</typeparam>
        /// <param name="services">The service collection to add the services to</param>
        /// <param name="action">Delegate used to handle messages of all types using an instance of <typeparamref name="THandler"/></param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        [Obsolete("Use the new Sels.Core.Mediator.Event components")]
        public static IServiceCollection AddSubscriber<THandler>(this IServiceCollection services, AsyncAction<THandler, object, object, CancellationToken> action) where THandler : notnull
        {
            services.ValidateArgument(nameof(services));
            action.ValidateArgument(nameof(action));

            services.New<ISubscriber>().As<DelegateSubscriber<THandler>>().ConstructWith(x => new DelegateSubscriber<THandler>(x.GetRequiredService<THandler>(), action)).Register();

            return services;
        }
        /// <summary>
        /// Adds service <typeparamref name="THandler"/> as global subscriber that can received messages of all types sent from other services.
        /// </summary>
        /// <typeparam name="THandler">The type of the service to receive the messages with</typeparam>
        /// <param name="services">The service collection to add the services to</param>
        /// <param name="handlerBuilder">Delegate for configuring the handler</param>
        /// <param name="action">Delegate used to handle messages of all types using an instance of <typeparamref name="THandler"/></param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        [Obsolete("Use the new Sels.Core.Mediator.Event components")]
        public static IServiceCollection AddSubscriber<THandler>(this IServiceCollection services, Func<IServiceBuilder<THandler>, IServiceBuilder> handlerBuilder, AsyncAction<THandler, object, object, CancellationToken> action) where THandler : class
        {
            services.ValidateArgument(nameof(services));
            handlerBuilder.ValidateArgument(nameof(handlerBuilder));
            action.ValidateArgument(nameof(action));

            handlerBuilder(services.New<THandler>()).Register();

            services.New<ISubscriber>().As<DelegateSubscriber<THandler>>().ConstructWith(x => new DelegateSubscriber<THandler>(x.GetRequiredService<THandler>(), action)).Register();

            return services;
        }

        /// <summary>
        /// Scans <paramref name="assemblies"/> for all types that implement <see cref="ISubscriber{T}"/> or <see cref="ISubscriber"/> and registers them as scoped subscribers so they can receive the messages.
        /// </summary>
        /// <param name="services">The service collection to add the services to</param>
        /// <param name="assemblies">The assemblies to scan</param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        [Obsolete("Use the new Sels.Core.Mediator.Event components")]
        public static IServiceCollection ScanForSubscribersIn(this IServiceCollection services, params Assembly[] assemblies)
        {
            services.ValidateArgument(nameof(services));
            assemblies.ValidateArgumentNotNullOrEmpty(nameof(assemblies));

            return services.ScanForSubscribersIn((x, y) => true, assemblies);
        }
        /// <summary>
        /// Scans <paramref name="assemblies"/> for all types that implement <see cref="ISubscriber{T}"/> or <see cref="ISubscriber"/> and registers them as scoped subscribers so they can receive the messages.
        /// </summary>
        /// <param name="services">The service collection to add the services to</param>
        /// <param name="condition">Predicate that dictates if the type can be added. First arg is the implementation type and the second arg is the interface type</param>
        /// <param name="assemblies">The assemblies to scan</param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        [Obsolete("Use the new Sels.Core.Mediator.Event components")]
        public static IServiceCollection ScanForSubscribersIn(this IServiceCollection services, Condition<Type, Type> condition, params Assembly[] assemblies)
        {
            services.ValidateArgument(nameof(services));
            condition.ValidateArgument(nameof(condition));
            assemblies.ValidateArgumentNotNullOrEmpty(nameof(assemblies));

            foreach (var type in assemblies.SelectMany(x => x.ExportedTypes).Where(x => x.IsClass && !x.IsAbstract))
            {
                foreach (var typeInterface in type.GetInterfaces().Where(x => x.IsGenericType && x.IsGenericTypeDefinition))
                {
                    if (typeInterface.GetGenericTypeDefinition().Equals(typeof(ISubscriber<>)) && condition(type, typeInterface))
                    {
                        services.TryAddScoped(typeInterface, type);
                    }

                    if (typeInterface.Equals(typeof(ISubscriber)) && condition(type, typeInterface))
                    {
                        services.TryAddScoped(typeInterface, type);
                    }
                }
            }

            return services;
        }
        #endregion

        /// <summary>
        /// Adds all required services to make use of <see cref="INotifier"/> to raise requests/events and subscribe to them.
        /// </summary>
        /// <param name="services">The collection the service descriptions will be added to</param>
        /// <param name="options">Optional delegate to configure the options</param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        public static IServiceCollection AddNotifier(this IServiceCollection services, Action<NotifierOptions> options = null)
        {
            services.ValidateArgument(nameof(services));

            // Task manager
            services.AddTaskManager();

            // Options
            services.AddOptions();
            services.BindOptionsFromConfig<NotifierOptions>();
            services.AddValidationProfile<NotifierOptionsValidationProfile, string>(ServiceLifetime.Singleton);
            services.AddOptionProfileValidator<NotifierOptions, NotifierOptionsValidationProfile>();
            if(options != null) services.Configure<NotifierOptions>(options);

            services.TryAddScoped<INotifier, Notifier>();

            // Event
            services.TryAddSingleton<IEventSubscriber, EventSubscriptionManager>();
            services.TryAddSingleton(typeof(IEventSubscriber<>), typeof(EventSubscriptionManager<>));

            // Request
            services.TryAddSingleton<IRequestSubscriptionManager, RequestSubscriptionManager>();
            services.TryAddSingleton(typeof(IRequestSubscriber<>), typeof(RequestSubscriber<>));
            services.TryAddSingleton(typeof(IRequestSubscriber<,>), typeof(RequestSubscriber<,>));

            return services;
        }

        #region Event
        /// <summary>
        /// Adds a event listener of type <typeparamref name="TListener"/> that can react to events of type <typeparamref name="TEvent"/>.
        /// </summary>
        /// <typeparam name="TListener">The type of the event listener</typeparam>
        /// <typeparam name="TEvent">The type of the event to respond to</typeparam>
        /// <param name="services">The collection the service descriptions will be added to</param>
        /// <param name="serviceBuilder">Optional delegate for configuring the service registration</param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        public static IServiceCollection AddEventListener<TListener, TEvent>(this IServiceCollection services, Action<IServiceBuilder<IEventListener<TEvent>, TListener>> serviceBuilder = null)
            where TListener : class, IEventListener<TEvent>
        {
            services.ValidateArgument(nameof(services));
            services.AddNotifier();

            var builder = services.New<IEventListener<TEvent>, TListener>();
            serviceBuilder?.Invoke(builder);
            return builder.Register();
        }

        /// <summary>
        /// Adds a event listener that can react to events of type <typeparamref name="TEvent"/> using <paramref name="handler"/>.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event to respond to</typeparam>
        /// <param name="handler">The delegate that will be called to react to raised events</param>
        /// <param name="priority"><inheritdoc cref="Sels.Core.Mediator.IMessageHandler.Priority"/></param>
        /// <param name="services">The collection the service descriptions will be added to</param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        public static IServiceCollection AddEventListener<TEvent>(this IServiceCollection services, AsyncAction<IEventListenerContext, TEvent, CancellationToken> handler, uint? priority = null)
        {
            services.ValidateArgument(nameof(services));
            handler.ValidateArgument(nameof(handler));

            return services.AddEventListener<DelegateEventListener<TEvent>, TEvent>(x => x.AsSingleton()
                                                                                          .WithBehaviour(RegisterBehaviour.Default)
                                                                                          .Trace(x => x.Duration.OfAll)
                                                                                          .ConstructWith(p => new DelegateEventListener<TEvent>(handler) { Priority = priority })
                                                                                   );
        }

        /// <summary>
        /// Adds a event listener that can react to events of type <typeparamref name="TEvent"/> using <paramref name="handler"/>.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event to respond to</typeparam>
        /// <param name="handler">The delegate that will be called to react to raised events</param>
        /// <param name="priority"><inheritdoc cref="Sels.Core.Mediator.IMessageHandler.Priority"/></param>
        /// <param name="services">The collection the service descriptions will be added to</param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        public static IServiceCollection AddEventListener<TEvent>(this IServiceCollection services, AsyncAction<IServiceProvider, IEventListenerContext, TEvent, CancellationToken> handler, uint? priority = null)
        {
            services.ValidateArgument(nameof(services));
            handler.ValidateArgument(nameof(handler));

            return services.AddEventListener<DelegateEventListener<TEvent>, TEvent>(x => x.AsSingleton()
                                                                                          .WithBehaviour(RegisterBehaviour.Default)
                                                                                          .Trace(x => x.Duration.OfAll)
                                                                                          .ConstructWith(p => new DelegateEventListener<TEvent>((c, e, t) => handler(p, c, e, t)) { Priority = priority })
                                                                                   );
        }

        /// <summary>
        /// Adds a event listener of type <typeparamref name="TListener"/> that can react to all events.
        /// </summary>
        /// <typeparam name="TListener">The type of the event listener</typeparam>
        /// <param name="services">The collection the service descriptions will be added to</param>
        /// <param name="serviceBuilder">Optional delegate for configuring the service registration</param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        public static IServiceCollection AddEventListener<TListener>(this IServiceCollection services, Action<IServiceBuilder<IEventListener, TListener>> serviceBuilder = null)
            where TListener : class, IEventListener
        {
            services.ValidateArgument(nameof(services));
            services.AddNotifier();

            var builder = services.New<IEventListener, TListener>();
            serviceBuilder?.Invoke(builder);
            return builder.Register();
        }

        /// <summary>
        /// Adds a event listener that can react to all events using <paramref name="handler"/>.
        /// </summary>
        /// <param name="handler">The delegate that will be called to react to raised events</param>
        /// <param name="priority"><inheritdoc cref="Sels.Core.Mediator.IMessageHandler.Priority"/></param>
        /// <param name="services">The collection the service descriptions will be added to</param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        public static IServiceCollection AddEventListener(this IServiceCollection services, AsyncAction<IEventListenerContext, object, CancellationToken> handler, uint? priority = null)
        {
            services.ValidateArgument(nameof(services));
            handler.ValidateArgument(nameof(handler));

            return services.AddEventListener<DelegateEventListener>(x => x.AsSingleton()
                                                                          .WithBehaviour(RegisterBehaviour.Default)
                                                                          .Trace(x => x.Duration.OfAll)
                                                                          .ConstructWith(p => new DelegateEventListener(handler) { Priority = priority })
                                                                   );
        }

        /// <summary>
        /// Adds <paramref name="eventListener"/> as a singleton.
        /// </summary>
        /// <param name="eventListener">The event listener to add to the container</param>
        /// <param name="services">The collection the service descriptions will be added to</param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        public static IServiceCollection AddEventListener(this IServiceCollection services, IEventListener eventListener)
        {
            services.ValidateArgument(nameof(services));
            eventListener.ValidateArgument(nameof(eventListener));

            return services.AddSingleton(eventListener);
        }

        /// <summary>
        /// Adds <paramref name="eventListener"/> as a singleton.
        /// </summary>
        /// <typeparam name="TEvent">The type of event to listen to</typeparam>
        /// <param name="eventListener">The event listener to add to the container</param>
        /// <param name="services">The collection the service descriptions will be added to</param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        public static IServiceCollection AddEventListener<TEvent>(this IServiceCollection services, IEventListener<TEvent> eventListener)
        {
            services.ValidateArgument(nameof(services));
            eventListener.ValidateArgument(nameof(eventListener));

            return services.AddSingleton(eventListener);
        }

        /// <summary>
        /// Adds a event listener that can react to all events using <paramref name="handler"/>.
        /// </summary>
        /// <param name="handler">The delegate that will be called to react to raised events</param>
        /// <param name="priority"><inheritdoc cref="Sels.Core.Mediator.IMessageHandler.Priority"/></param>
        /// <param name="services">The collection the service descriptions will be added to</param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        public static IServiceCollection AddEventListener(this IServiceCollection services, AsyncAction<IServiceProvider, IEventListenerContext, object, CancellationToken> handler, uint? priority = null)
        {
            services.ValidateArgument(nameof(services));
            handler.ValidateArgument(nameof(handler));

            return services.AddEventListener<DelegateEventListener>(x => x.AsSingleton()
                                                                          .WithBehaviour(RegisterBehaviour.Default)
                                                                          .Trace(x => x.Duration.OfAll)
                                                                          .ConstructWith(p => new DelegateEventListener((c, e, t) => handler(p, c, e, t)) { Priority = priority })
                                                                   );
        }

        /// <summary>
        /// Scans <paramref name="assemblies"/> for all types that implement <see cref="IEventListener{T}"/> or <see cref="IEventListener"/> and registers them as <see cref="ServiceLifetime.Scoped"/>.
        /// </summary>
        /// <param name="services">The service collection to add the services to</param>
        /// <param name="assemblies">The assemblies to scan</param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        public static IServiceCollection ScanForEventListenersIn(this IServiceCollection services, params Assembly[] assemblies)
        {
            services.ValidateArgument(nameof(services));
            assemblies.ValidateArgument(nameof(assemblies));

            return services.ScanForEventListenersIn(x => true, x => ServiceLifetime.Scoped, assemblies);
        }
        /// <summary>
        /// Scans <paramref name="assemblies"/> for all types that implement <see cref="IEventListener{T}"/> or <see cref="IEventListener"/> and registers them.
        /// </summary>
        /// <param name="services">The service collection to add the services to</param>
        /// <param name="condition">Optional predicate that dictates if the type can be added. Arg is the implementation type</param>
        /// <param name="scopeSelector">Optional func that select the lifetime for the implementation</param>
        /// <param name="assemblies">The assemblies to scan</param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        public static IServiceCollection ScanForEventListenersIn(this IServiceCollection services, Condition<Type> condition, Func<Type, ServiceLifetime> scopeSelector, params Assembly[] assemblies)
        {
            services.ValidateArgument(nameof(services));
            assemblies.ValidateArgument(nameof(assemblies));

            foreach (var type in assemblies.SelectMany(x => x.ExportedTypes).Where(x => x.IsClass && !x.IsAbstract))
            {
                foreach (var typeInterface in type.GetInterfaces().Where(x => x.IsGenericType && !x.IsGenericTypeDefinition))
                {
                    if (typeInterface.GetGenericTypeDefinition().Equals(typeof(IEventListener<>)) && (condition?.Invoke(type) ?? true))
                    {
                        services.Register(typeInterface, type, scopeSelector?.Invoke(type) ?? ServiceLifetime.Scoped);
                    }

                    if (typeInterface.Equals(typeof(IEventListener)) && (condition?.Invoke(type) ?? true))
                    {
                        services.Register(typeInterface, type, scopeSelector?.Invoke(type) ?? ServiceLifetime.Scoped);
                    }
                }
            }

            return services;
        }
        #endregion

        #region Request
        /// <summary>
        /// Adds a request handler of type <typeparamref name="THandler"/> that can respond to requests of type <typeparamref name="TRequest"/> with <typeparamref name="TResponse"/>.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request to respond to</typeparam>
        /// <typeparam name="TResponse">The type of the response of an object</typeparam>
        /// <typeparam name="THandler">The type of the handler</typeparam>
        /// <param name="services">The collection the service descriptions will be added to</param>
        /// <param name="serviceBuilder">Optional delegate for configuring the service registration</param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        public static IServiceCollection AddRequestHandler<TRequest, TResponse, THandler>(this IServiceCollection services, Action<IServiceBuilder<IRequestHandler<TRequest, TResponse>, THandler>> serviceBuilder = null)
            where THandler : class, IRequestHandler<TRequest, TResponse>
        {
            services.ValidateArgument(nameof(services));
            services.AddNotifier();

            var builder = services.New<IRequestHandler<TRequest, TResponse>, THandler>();
            serviceBuilder?.Invoke(builder);
            return builder.Register();
        }

        /// <summary>
        /// Adds a request handler that can respond to requests of type <typeparamref name="TRequest"/> with <typeparamref name="TResponse"/> using <paramref name="handler"/>.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request to respond to</typeparam>
        /// <typeparam name="TResponse">The type of the response of an object</typeparam>
        /// <param name="handler">The delegate that will be called to react to raised requests</param>
        /// <param name="priority"><inheritdoc cref="Sels.Core.Mediator.IMessageHandler.Priority"/></param>
        /// <param name="services">The collection the service descriptions will be added to</param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        public static IServiceCollection AddRequestHandler<TRequest, TResponse>(this IServiceCollection services, AsyncFunc<IRequestHandlerContext, TRequest, CancellationToken, RequestResponse<TResponse>> handler, uint? priority = null)
        {
            services.ValidateArgument(nameof(services));
            handler.ValidateArgument(nameof(handler));

            return services.AddRequestHandler<TRequest, TResponse, DelegateRequestHandler<TRequest, TResponse>>(x => x.AsSingleton()
                                                                                          .WithBehaviour(RegisterBehaviour.Default)
                                                                                          .Trace(x => x.Duration.OfAll)
                                                                                          .ConstructWith(p => new DelegateRequestHandler<TRequest, TResponse>(handler) { Priority = priority })
                                                                                   );
        }

        /// <summary>
        /// Adds a request handler that can respond to requests of type <typeparamref name="TRequest"/> with <typeparamref name="TResponse"/> using <paramref name="handler"/>.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request to respond to</typeparam>
        /// <typeparam name="TResponse">The type of the response of an object</typeparam>
        /// <param name="handler">The delegate that will be called to react to raised requests</param>
        /// <param name="priority"><inheritdoc cref="Sels.Core.Mediator.IMessageHandler.Priority"/></param>
        /// <param name="services">The collection the service descriptions will be added to</param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        public static IServiceCollection AddRequestHandler<TRequest, TResponse>(this IServiceCollection services, AsyncFunc<IServiceProvider, IRequestHandlerContext, TRequest, CancellationToken, RequestResponse<TResponse>> handler, uint? priority = null)
        {
            services.ValidateArgument(nameof(services));
            handler.ValidateArgument(nameof(handler));

            return services.AddRequestHandler<TRequest, TResponse, DelegateRequestHandler<TRequest, TResponse>>(x => x.AsSingleton()
                                                                                          .WithBehaviour(RegisterBehaviour.Default)
                                                                                          .Trace(x => x.Duration.OfAll)
                                                                                          .ConstructWith(p => new DelegateRequestHandler<TRequest, TResponse>((c, e, t) => handler(p, c, e, t)) { Priority = priority })
                                                                                   );
        }

        /// <summary>
        /// Adds <paramref name="requestHandler"/> as a singleton.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request to respond to</typeparam>
        /// <typeparam name="TResponse">The type of the response of an object</typeparam>
        /// <param name="requestHandler">The request handler to add to the container</param>
        /// <param name="services">The collection the service descriptions will be added to</param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        public static IServiceCollection AddRequestHandler<TRequest, TResponse>(this IServiceCollection services, IRequestHandler<TRequest, TResponse> requestHandler)
        {
            services.ValidateArgument(nameof(services));
            requestHandler.ValidateArgument(nameof(requestHandler));

            return services.AddSingleton(requestHandler);
        }


        /// <summary>
        /// Adds a request handler of type <typeparamref name="THandler"/> that can acknowledge requests of type <typeparamref name="TRequest"/>.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request to respond to</typeparam>
        /// <typeparam name="THandler">The type of the handler</typeparam>
        /// <param name="services">The collection the service descriptions will be added to</param>
        /// <param name="serviceBuilder">Optional delegate for configuring the service registration</param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        public static IServiceCollection AddRequestHandler<TRequest, THandler>(this IServiceCollection services, Action<IServiceBuilder<IRequestHandler<TRequest>, THandler>> serviceBuilder = null)
            where THandler : class, IRequestHandler<TRequest>
        {
            services.ValidateArgument(nameof(services));
            services.AddNotifier();

            var builder = services.New<IRequestHandler<TRequest>, THandler>();
            serviceBuilder?.Invoke(builder);
            return builder.Register();
        }

        /// <summary>
        /// Adds a request handler that can acknowledge requests of type <typeparamref name="TRequest"/> using <paramref name="handler"/>.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request to respond to</typeparam>
        /// <param name="handler">The delegate that will be called to react to raised requests</param>
        /// <param name="priority"><inheritdoc cref="Sels.Core.Mediator.IMessageHandler.Priority"/></param>
        /// <param name="services">The collection the service descriptions will be added to</param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        public static IServiceCollection AddRequestHandler<TRequest>(this IServiceCollection services, AsyncFunc<IRequestHandlerContext, TRequest, CancellationToken, RequestAcknowledgement> handler, uint? priority = null)
        {
            services.ValidateArgument(nameof(services));
            handler.ValidateArgument(nameof(handler));

            return services.AddRequestHandler<TRequest, DelegateRequestHandler<TRequest>>(x => x.AsSingleton()
                                                                                          .WithBehaviour(RegisterBehaviour.Default)
                                                                                          .Trace(x => x.Duration.OfAll)
                                                                                          .ConstructWith(p => new DelegateRequestHandler<TRequest>(handler) { Priority = priority })
                                                                                   );
        }

        /// <summary>
        /// Adds a request handler that can acknowledge requests of type <typeparamref name="TRequest"/> using <paramref name="handler"/>.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request to respond to</typeparam>
        /// <param name="handler">The delegate that will be called to react to raised requests</param>
        /// <param name="priority"><inheritdoc cref="Sels.Core.Mediator.IMessageHandler.Priority"/></param>
        /// <param name="services">The collection the service descriptions will be added to</param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        public static IServiceCollection AddRequestHandler<TRequest>(this IServiceCollection services, AsyncFunc<IServiceProvider, IRequestHandlerContext, TRequest, CancellationToken, RequestAcknowledgement> handler, uint? priority = null)
        {
            services.ValidateArgument(nameof(services));
            handler.ValidateArgument(nameof(handler));

            return services.AddRequestHandler<TRequest, DelegateRequestHandler<TRequest>>(x => x.AsSingleton()
                                                                                          .WithBehaviour(RegisterBehaviour.Default)
                                                                                          .Trace(x => x.Duration.OfAll)
                                                                                          .ConstructWith(p => new DelegateRequestHandler<TRequest>((c, e, t) => handler(p, c, e, t)) { Priority = priority })
                                                                                   );
        }

        /// <summary>
        /// Adds <paramref name="requestHandler"/> as a singleton.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request to respond to</typeparam>
        /// <param name="requestHandler">The request handler to add to the container</param>
        /// <param name="services">The collection the service descriptions will be added to</param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        public static IServiceCollection AddRequestHandler<TRequest>(this IServiceCollection services, IRequestHandler<TRequest> requestHandler)
        {
            services.ValidateArgument(nameof(services));
            requestHandler.ValidateArgument(nameof(requestHandler));

            return services.AddSingleton(requestHandler);
        }


        /// <summary>
        /// Scans <paramref name="assemblies"/> for all types that implement <see cref="IRequestHandler{TRequest, TResponse}"/> or <see cref="IRequestHandler{TRequest}"/> and registers them.
        /// </summary>
        /// <param name="services">The service collection to add the services to</param>
        /// <param name="assemblies">The assemblies to scan</param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        public static IServiceCollection ScanForRequestHandlersIn(this IServiceCollection services, params Assembly[] assemblies)
        {
            services.ValidateArgument(nameof(services));
            assemblies.ValidateArgument(nameof(assemblies));

            return services.ScanForRequestHandlersIn(x => true, x => ServiceLifetime.Scoped, assemblies);
        }

        /// <summary>
        /// Scans <paramref name="assemblies"/> for all types that implement <see cref="IRequestHandler{TRequest, TResponse}"/> or <see cref="IRequestHandler{TRequest}"/> and registers them.
        /// </summary>
        /// <param name="services">The service collection to add the services to</param>
        /// <param name="condition">Optional predicate that dictates if the type can be added. Arg is the implementation type</param>
        /// <param name="scopeSelector">Optional func that select the lifetime for the implementation</param>
        /// <param name="assemblies">The assemblies to scan</param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        public static IServiceCollection ScanForRequestHandlersIn(this IServiceCollection services, Condition<Type> condition, Func<Type, ServiceLifetime> scopeSelector, params Assembly[] assemblies)
        {
            services.ValidateArgument(nameof(services));
            assemblies.ValidateArgument(nameof(assemblies));

            foreach (var type in assemblies.SelectMany(x => x.ExportedTypes).Where(x => x.IsClass && !x.IsAbstract))
            {
                foreach (var typeInterface in type.GetInterfaces().Where(x => x.IsGenericType && !x.IsGenericTypeDefinition))
                {
                    var genericTypeDefinition = typeInterface.GetGenericTypeDefinition();
                    if ((genericTypeDefinition.Equals(typeof(IRequestHandler<,>)) || genericTypeDefinition.Equals(typeof(IRequestHandler<>))) && (condition?.Invoke(type) ?? true))
                    {
                        services.Register(typeInterface, type, scopeSelector?.Invoke(type) ?? ServiceLifetime.Scoped);
                    }
                }
            }

            return services;
        }
        #endregion
    }
}
