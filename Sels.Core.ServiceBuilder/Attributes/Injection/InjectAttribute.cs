using Microsoft.Extensions.DependencyInjection;
using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.ServiceBuilder.Injection
{
    /// <summary>
    /// Tells a <see cref="ServiceInjector"/> that the member it's defined on needs to be resolved using a DI container.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class InjectAttribute : Attribute
    {
        // Properties
        /// <summary>
        /// If the service is required. Set to false if the service is optional.
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// The method called by a <see cref="ServiceInjector"/> to resolve the service and assign it to <paramref name="member"/> on <paramref name="instance"/>.
        /// </summary>
        /// <param name="provider">The provider to resolve the service</param>
        /// <param name="member">The member the current attribute was defined on</param>
        /// <param name="instance">The instance to set the service on</param>
        /// <returns>True if the service was injected, otherwise false</returns>
        public bool Set(IServiceProvider provider, MemberInfo member, object instance)
        {
            provider.ValidateArgument(nameof(provider));
            member.ValidateArgument(nameof(member));
            instance.ValidateArgument(nameof(instance));

            // Get service type to inject.
            Type serviceType;
            switch (member.MemberType)
            {
                case MemberTypes.Field:
                    serviceType = ((FieldInfo)member).FieldType;
                    break;
                case MemberTypes.Property:
                    serviceType = ((PropertyInfo)member).PropertyType;
                    break;
                default:
                    throw new NotSupportedException($"Member type <{member.MemberType}> is not supported");
            }

            // Create service
            var service = CreateService(provider, serviceType, member, instance);

            // Set service if not null
            if (service != null) {
                switch (member.MemberType)
                {
                    case MemberTypes.Field:
                        ((FieldInfo)member).SetValue(instance, service);
                        break;
                    case MemberTypes.Property:
                        ((PropertyInfo)member).SetValue(instance, service);
                        break;
                    default:
                        throw new NotSupportedException($"Member type <{member.MemberType}> is not supported");
                }
                return true;
            }

            return false;
        }

        /// <summary>
        /// Creates the service instance to set on the provided member.
        /// </summary>
        /// <param name="provider">The provider to resolve the service</param>
        /// <param name="member">The member the current attribute was defined on</param>
        /// <param name="serviceType">The type of the service to resolve</param>
        /// <param name="instance">The instance to set the service on</param>
        /// <returns>The created service or null if the service couldn't be created</returns>
        protected virtual object CreateService(IServiceProvider provider, Type serviceType, MemberInfo member, object instance)
        {
            return Required ? provider.GetRequiredService(serviceType) : provider.GetService(serviceType);
        }
    }
}
