using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using Sels.Core.Extensions.Logging.Advanced;
using Sels.Core.Extensions.Reflection;
using Sels.Core.ServiceBuilder.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.ServiceBuilder.Injection
{
    /// <summary>
    /// Injects services after a instance is created using a service builder on all members decorated with attribute <see cref="InjectAttribute"/>.
    /// </summary>
    public class ServiceInjector : IOnCreatedHandler
    {
        // Fields
        /// <summary>
        /// Optional logger for tracing.
        /// </summary>
        protected readonly ILogger<ServiceInjector>? _logger;

        /// <inheritdoc cref="ServiceInjector"/>
        /// <param name="logger"><inheritdoc cref="_logger"/></param>
        public ServiceInjector(ILogger<ServiceInjector>? logger = null)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public void Handle(IServiceProvider provider, object instance)
        {
            Guard.IsNotNull(provider);
            Guard.IsNotNull(instance);
            var type = instance.GetType();

            using(_logger.TraceAction($"Injecting services on instance of type <{type.GetDisplayName()}>"))
            {
                foreach(var injectableMember in GetInjectableMembers(instance))
                {
                    var member = injectableMember.Member;
                    var attribute = injectableMember.Attribute;
                    _logger.Trace($"Injecting service for member <{member.Name}> on instance <{instance}> using a injector attribute of type <{attribute.GetType().GetDisplayName()}>");
                    
                    if(attribute.Set(provider, member, instance))
                    {
                        _logger.Debug($"Member <{member.Name}> on instance of type <{instance.GetType().GetDisplayName()}> was injected with a new service instance");
                    }
                    else
                    {
                        _logger.Debug($"No service was injected for member <{member.Name}> on instance of type <{instance.GetType().GetDisplayName()}>");
                    }
                }
            }
        }

        /// <summary>
        /// Returns all members that can be injected with the defined injection attribute.
        /// </summary>
        /// <param name="instance">The instance to get the  members from</param>
        /// <returns>An enumerator returning all members to inject</returns>
        protected virtual IEnumerable<(MemberInfo Member, InjectAttribute Attribute)> GetInjectableMembers(object instance)
        {
            Guard.IsNotNull(instance);
            var type = instance.GetType();

            return GetInjectableMembers(type);
        }

        /// <summary>
        /// Returns all members that can be injected with the defined injection attribute.
        /// </summary>
        /// <param name="type">The type to get the  members from</param>
        /// <returns>An enumerator returning all members to inject</returns>
        protected IEnumerable<(MemberInfo Member, InjectAttribute Attribute)> GetInjectableMembers(Type type)
        {
            Guard.IsNotNull(type);

            // Fields
            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var attribute = field.GetCustomAttribute<InjectAttribute>();

                if (attribute != null) yield return (field, attribute);
            }

            // Properties
            foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(x => x.CanWrite))
            {
                var attribute = property.GetCustomAttribute<InjectAttribute>();

                if (attribute != null) yield return (property, attribute);
            }
        }
    }

    /// <inheritdoc cref="ServiceInjector"/>
    /// <typeparam name="TImpl">The type of the instance to inject for</typeparam>
    public class ServiceInjector<TImpl> : ServiceInjector, IOnCreatedHandler<TImpl>
    {
        // Fields
        private Dictionary<MemberInfo, InjectAttribute> _membersToInject = new Dictionary<MemberInfo, InjectAttribute>();

        /// <inheritdoc cref="ServiceInjector{TImpl}"/>
        /// <param name="logger"><inheritdoc cref="ServiceInjector._logger"/></param>
        public ServiceInjector(ILogger<ServiceInjector<TImpl>>? logger = null) : base(logger)
        {
            // Cache the members to inject
            _membersToInject = base.GetInjectableMembers(typeof(TImpl)).ToDictionary(x => x.Member, x => x.Attribute);
        }

        /// <inheritdoc />
        public void Handle(IServiceProvider provider, TImpl instance) => Handle(provider, (object)instance);
        /// <inheritdoc />
        protected override IEnumerable<(MemberInfo Member, InjectAttribute Attribute)> GetInjectableMembers(object instance)
        {
            Guard.IsNotNull(instance);

            return instance.GetType().Is(typeof(TImpl)) ? _membersToInject.Select(x => (x.Key, x.Value)) : base.GetInjectableMembers(instance);
        }
    }
}
