using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Configuration
{
    /// <summary>
    /// Helps with adding app settings to the service collection that can be used during application runtime as opposed to only startup.
    /// This is used by <see cref="AppSettings{TOptions}"/> to add the necessary services to resolve the app settings during application runtime.
    /// </summary>
    public interface IAppSettingsRegistrar<TOptions> where TOptions : IAppSettings<TOptions>
    {
        /// <summary>
        /// Also registers the app settings as the specified type.
        /// Mainly used if <typeparamref name="TOptions"/> also needs to be registered as a base class.
        /// Makes it possible to resolve the app settings as both <typeparamref name="TOptions"/> and <typeparamref name="T"/>.
        /// </summary>
        /// <remarks>
        /// <typeparamref name="TOptions"/> must be assignable to <typeparamref name="T"/> for this to work, otherwise an exception will be thrown during registration.
        /// </remarks>
        /// <typeparam name="T">The base class of <typeparamref name="TOptions"/> that also needs to be known as an appsetting</typeparam>
        /// <returns>The current instance for method chaining</returns>
        IAppSettingsRegistrar<TOptions> As<T>() where T : class, IAppSettings<TOptions>;
    }
}
