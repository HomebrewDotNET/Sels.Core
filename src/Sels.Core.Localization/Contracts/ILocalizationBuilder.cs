using System.Reflection;
using Microsoft.Extensions.Logging;
using Sels.Core.Extensions.Conversion;
using static Sels.Core.Delegates;

namespace Sels.Core.Localization
{
    /// <summary>
    /// Builder for setting up a localizer.
    /// </summary>
    public interface ILocalizationBuilder
    {
        #region Resource
        /// <summary>
        /// Uses localization defined in resource with name <paramref name="resourceName"/>.
        /// </summary>
        /// <param name="assembly">The assembly that contains the resource</param>
        /// <param name="resourceName">The name of the resource (without extension)</param>
        /// <returns>Current builder for method chaining</returns>
        ILocalizationBuilder Use(Assembly assembly, string resourceName);
        /// <summary>
        /// Uses localization defined in all resource contained within <paramref name="assembly"/>.
        /// </summary>
        /// <param name="assembly">The assembly to scan</param>
        /// <param name="canLoad">Optional delegate that checks if the resource can be loaded.  When set to null everything is loaded. Arg is the resource name</param>
        /// <returns>Current builder for method chaining</returns>
        ILocalizationBuilder ScanIn(Assembly assembly, Predicate<string>? canLoad = null);
        /// <summary>
        /// Uses localization defined in all resources in all currently loaded assemblies.
        /// </summary>
        /// <param name="canLoad">Optional delegate that checks if the resource can be loaded.  When set to null everything is loaded. First arg is the assembly, second arg is the resource name</param>
        /// <returns>Current builder for method chaining</returns>
        ILocalizationBuilder ScanInAllLoaded(Condition<Assembly, string>? canLoad = null);
        #endregion

        #region Logging
        /// <summary>
        /// Allows the localizer to trace using <paramref name="logger"/>.
        /// </summary>
        /// <param name="logger">The logger to use for tracing</param>
        /// <returns>Current builder for method chaining</returns>
        ILocalizationBuilder UseLogger(ILogger? logger) => UseLoggers(logger.AsArrayOrDefault());
        /// <summary>
        /// Allows the localizer to trace using <paramref name="loggers"/>.
        /// </summary>
        /// <param name="loggers">The loggers to use for tracing</param>
        /// <returns>Current builder for method chaining</returns>
        ILocalizationBuilder UseLoggers(IEnumerable<ILogger?>? loggers);
        #endregion

        #region Default
        /// <summary>
        /// Sets the global default options that will be used for every request.
        /// </summary>
        /// <param name="options">Delegate that configures the default options</param>
        /// <returns>Current builder for method chaining</returns>
        ILocalizationBuilder WithDefaultOptions(Action<LocalizationOptions> options);
        #endregion
    }
}
