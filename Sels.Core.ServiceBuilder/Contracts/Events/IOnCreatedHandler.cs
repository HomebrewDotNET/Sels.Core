using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.ServiceBuilder.Events
{
    /// <summary>
    /// Service that can perform actions on instances of <typeparamref name="T"/> each time they are created by a DI container.
    /// </summary>
    public interface IOnCreatedHandler<T>
    {
        /// <summary>
        /// Performs an action on <paramref name="instance"/> after it was created.
        /// </summary>
        /// <param name="provider">The provider used to create <paramref name="instance"/></param>
        /// <param name="instance">The object that was created</param>
        void Handle(IServiceProvider provider, T instance);
    }

    /// <summary>
    /// Service that can perform actions on instances each time they are created by a DI container.
    /// </summary>
    public interface IOnCreatedHandler
    {
        /// <summary>
        /// Performs an action on <paramref name="instance"/> after it was created.
        /// </summary>
        /// <param name="provider">The provider used to create <paramref name="instance"/></param>
        /// <param name="instance">The object that was created</param>
        void Handle(IServiceProvider provider, object instance);
    }
}
