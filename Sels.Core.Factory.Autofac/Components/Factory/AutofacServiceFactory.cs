using Autofac;
using Autofac.Core;
using Sels.Core.Contracts.Factory;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using Sels.Core.Extensions.Reflection;

namespace Sels.Core.Factory.Autofac.Factory
{
    /// <summary>
    /// Service factory implemented using Autofac.
    /// </summary>
    public class AutofacServiceFactory : IServiceFactory
    {
        // Fields
        private readonly ILifetimeScope _scope;

        /// <inheritdoc cref="AutofacServiceFactory"/>
        /// <param name="scope">The scope used to resolve services</param>
        public AutofacServiceFactory(ILifetimeScope scope)
        {
            _scope = Guard.IsNotNull(scope);
        }

        #region Resolve
        /// <inheritdoc/>
        public T Resolve<T>()
        {
#pragma warning disable CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.
            return _scope.Resolve<T>();
#pragma warning restore CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.
        }
        /// <inheritdoc/>
        public T Resolve<T>(string name)
        {
            Guard.IsNotNullOrWhitespace(name);
#pragma warning disable CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.
            return _scope.ResolveNamed<T>(name);
#pragma warning restore CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.
        }
        /// <inheritdoc/>
        public object Resolve(Type type)
        {
            Guard.IsNotNull(type);
            return _scope.Resolve(type);
        }
        /// <inheritdoc/>
        public object Resolve(Type type, string name)
        {
            Guard.IsNotNull(type);
            Guard.IsNotNullOrWhitespace(name);
            return _scope.ResolveNamed(name, type);
        }
        /// <inheritdoc/>
        public IEnumerable<T> ResolveAll<T>()
        {
            return _scope.Resolve<IEnumerable<T>>();
        }
        /// <inheritdoc/>
        public IEnumerable<object> ResolveAll(Type type)
        {
            Guard.IsNotNull(type);
            return _scope.Resolve(typeof(IEnumerable<>).MakeGenericType(type)).Cast<IEnumerable>().Enumerate();
        }
        #endregion

        #region Registered
        /// <inheritdoc/>
        public bool IsRegistered(Type type)
        {
            Guard.IsNotNull(type);
            return _scope.IsRegistered(type);
        }
        /// <inheritdoc/>
        public bool IsRegistered<T>()
        {
#pragma warning disable CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.
            return _scope.IsRegistered<T>();
#pragma warning restore CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.
        }
        /// <inheritdoc/>
        public bool IsRegistered(Type type, string name)
        {
            Guard.IsNotNull(type);
            Guard.IsNotNullOrWhitespace(name);
            return _scope.IsRegisteredWithName(name, type);
        }
        /// <inheritdoc/>
        public bool IsRegistered<T>(string name)
        {
            Guard.IsNotNullOrWhitespace(name);
#pragma warning disable CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.
            return _scope.IsRegisteredWithName<T>(name);
#pragma warning restore CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.
        }
        #endregion

        #region Implementer
        /// <inheritdoc/>
        public Type GetImplementerFor(Type type)
        {
            Guard.IsNotNull(type);

            return _scope.ComponentRegistry.Registrations
                                           .SelectMany(x => x.Services.OfType<IServiceWithType>(), (r, s) => (Activator: r.Activator , Service:s))                                           
                                           .Where(x => x.Service.ServiceType.Equals(type))
                                           .LastOrDefault().Activator?.LimitType;
        }
        /// <inheritdoc/>
        public Type GetImplementerFor<T>()
        {
            return GetImplementerFor(typeof(T));
        }
        /// <inheritdoc/>
        public Type GetImplementerFor(Type type, string name)
        {
            Guard.IsNotNull(type);
            Guard.IsNotNullOrWhitespace(name);

            return _scope.ComponentRegistry.Registrations
                                           .SelectMany(x => x.Services.OfType<KeyedService>(), (r, s) => (Activator: r.Activator, Service: s))
                                           .Where(x => x.Service.ServiceType.Equals(type) && x.Service.ServiceKey.GetType().IsString() && name.Equals(x.Service.ServiceKey))
                                           .LastOrDefault().Activator?.LimitType;
        }
        /// <inheritdoc/>
        public Type GetImplementerFor<T>(string name)
        {
            Guard.IsNotNullOrWhitespace(name);

            return GetImplementerFor(typeof(T), name);
        }
        #endregion

        /// <inheritdoc/>
        public IServiceFactoryScope CreateScope()
        {
            return new AutofacServiceFactory(_scope.BeginLifetimeScope());
        }
        /// <summary>
        /// Disposed of the current factory scope which in turn will dispose any resolved services (Taking into account the service lifetimes)
        /// </summary>
        public void Dispose() => _scope.Dispose();
    }
}
