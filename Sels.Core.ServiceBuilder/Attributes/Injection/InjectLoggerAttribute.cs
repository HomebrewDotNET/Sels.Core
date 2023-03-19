using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sels.Core.Extensions.Reflection;
using Sels.Core.ServiceBuilder.Injection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.ServiceBuilder.Attributes.Injection
{
    /// <summary>
    /// Injects the decorated member of type <see cref="ILogger"/> with a logger where the category is equal to the full name of the instance.
    /// </summary>
    public class InjectLoggerAttribute : InjectAttribute
    {
        /// <inheritdoc/>
        protected override object? CreateService(IServiceProvider provider, Type serviceType, MemberInfo member, object instance)
        {
            if(serviceType.Is<ILogger>())
            {
                var factory = provider.GetRequiredService<ILoggerFactory>();
                return factory.CreateLogger(instance.GetType().FullName);
            }
            else
            {
                return base.CreateService(provider, serviceType, member, instance);
            }
        }
    }
}
