using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sels.Core.Mediator.Messaging;
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
        /// <summary>
        /// Adds all the required services for sending messages and subscribing to them.
        /// </summary>
        /// <param name="services">The service collection to add the services to</param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        public static IServiceCollection AddMessanger(this IServiceCollection services)
        {
            services.ValidateArgument(nameof(services));

            services.New<IMessageSubscriber>().As<SubscriptionManager>().AsSingleton().Trace(x => x.Duration.OfAll).Register();
            services.AddScoped(typeof(IMessanger<>), typeof(Messenger<>));

            return services;
        }

        /// <summary>
        /// Adds service <typeparamref name="TSubscriber"/> as global subscriber that can received messages of type <typeparamref name="TMessage"/> sent from other services.
        /// </summary>
        /// <typeparam name="TMessage">The type of message to subscribe to</typeparam>
        /// <typeparam name="TSubscriber">The type of the service to receive the messages with</typeparam>
        /// <param name="services">The service collection to add the services to</param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        public static IServiceCollection AddSubscriber<TMessage, TSubscriber>(this IServiceCollection services) where TSubscriber : class, ISubscriber<TMessage>
        {
            services.ValidateArgument(nameof(services));

            services.New<ISubscriber<TMessage>>().As<TSubscriber>().Register();

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
    }
}
