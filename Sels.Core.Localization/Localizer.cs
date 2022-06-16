using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Linq;
using Sels.Core.Extensions.Logging.Advanced;
using Sels.Core.Extensions.Reflection;

namespace Sels.Core.Localization
{
    /// <summary>
    /// Contains static helper methods for localization.
    /// </summary>
    public static class Localizer
    {
        // Fields
        private static readonly object _lock = new object();
        private static LocalizationOptions _defaultOptions = new LocalizationOptions();
        private static readonly List<ILogger> _loggers = new List<ILogger>();
        private static readonly List<ResourceManager> _resources = new List<ResourceManager>();

        // Properties
        /// <summary>
        /// The currently loaded resources that will be used for localization.
        /// </summary>
        public static ResourceManager[] Resources => _resources.ToArray();

        /// <summary>
        /// Configures the localizer using <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">Delegate to configure the current builder</param>
        /// <param name="clear">If previous configuration should be cleared</param>
        public static void Setup(Action<ILocalizationBuilder> builder, bool clear = true)
        {
            builder.ValidateArgument(nameof(builder));

            lock (_lock)
            {
                if (clear)
                {
                    _defaultOptions = new LocalizationOptions();
                    _loggers.Clear();
                    _resources.Clear();
                }
                
                var locBuilder = new LocBuilder();
                builder(locBuilder);

                _loggers.AddRange(locBuilder.Loggers.Where(x => x != null));
                locBuilder.Resources.Execute(x => _resources.Add(new ResourceManager(x.Name, x.Assembly)));
                if (locBuilder.OptionsBuilder != null) locBuilder.OptionsBuilder(_defaultOptions);
            }
        }

        #region Get
        /// <summary>
        /// Fetches the localized string for <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key of the localization entry to fetch</param>
        /// <param name="parameters">Optional parameters for formatting the localized string if it is not missing</param>
        /// <returns>The localized string or another result based on <see cref="LocalizationOptions.MissingKeySettings"/></returns>
        /// <exception cref="LocalizationMissingException"></exception>
        public static string? Get(string key, params object[]? parameters) => Get(key, null, null, parameters);
        /// <summary>
        /// Fetches the localized string for <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key of the localization entry to fetch</param>
        /// <param name="culture">Optional culture to get the localized string in. If set to null either the default defined in <see cref="LocalizationOptions"/> is used or the default thread ui culture</param>
        /// <param name="parameters">Optional parameters for formatting the localized string if it is not missing</param>
        /// <returns>The localized string or another result based on <see cref="LocalizationOptions.MissingKeySettings"/></returns>
        /// <exception cref="LocalizationMissingException"></exception>
        public static string? Get(string key, string? culture, params object[]? parameters) => Get(key, culture, null, parameters);
        /// <summary>
        /// Fetches the localized string for <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key of the localization entry to fetch</param>
        /// <param name="culture">Optional culture to get the localized string in. If set to null either the default defined in <see cref="LocalizationOptions"/> is used or the default thread ui culture</param>
        /// <param name="options">Optional delegate for configuring the options</param>
        /// <param name="parameters">Optional parameters for formatting the localized string if it is not missing</param>
        /// <returns>The localized string or another result based on <see cref="LocalizationOptions.MissingKeySettings"/></returns>
        /// <exception cref="LocalizationMissingException"></exception>
        public static string? Get(string key, string? culture, Action<LocalizationOptions>? options, params object[]? parameters)
        {
            using (_loggers.TraceMethod(typeof(Localizer)))
            {
                key.ValidateArgument(nameof(key));

                var locOptions = _defaultOptions;
                if(options != null)
                {
                    locOptions = _defaultOptions with { };
                    options(locOptions);
                }

                var cultures = Helper.Collection.Enumerate(culture.HasValue() ? new CultureInfo(culture) : null, locOptions.DefaultCulture.HasValue() ? new CultureInfo(locOptions.DefaultCulture) : null).Where(x => x != null);

                var result = Get(key, cultures, culture == null);

                if(result == null)
                {
                    var cultureName = (cultures.FirstOrDefault() ?? Thread.CurrentThread.CurrentUICulture).Name;

                    switch (locOptions.MissingKeySettings)
                    {
                        case MissingLocalizationSettings.Default:
                            return $"{key}_{culture}";
                        case MissingLocalizationSettings.Key:
                            return key;
                        case MissingLocalizationSettings.Exception:
                            throw new LocalizationMissingException(key, cultureName);
                        case MissingLocalizationSettings.Null:
                            return null;
                        default:
                            throw new NotSupportedException($"Setting <{locOptions.MissingKeySettings}> is not supported");
                    }
                }

                return parameters.HasValue() ? result.FormatString(parameters) : result;
            }         
        }

        private static string? Get(string key, IEnumerable<CultureInfo>? cultures, bool useDefault)
        {
            key.ValidateArgument(nameof(key));

            using (_loggers.TraceAction(LogLevel.Debug, $"Localizing <{key}>"))
            {
                if (cultures.HasValue())
                {
                    foreach (var culture in cultures)
                    {
                        foreach (var resource in _resources)
                        {
                            _loggers.Debug($"Attempting to localize <{key}> using culture <{culture.Name}> and resource <{resource.BaseName}>");
                            var result = resource.GetString(key, culture);
                            if (result != null) {
                                _loggers.Trace($"Localized <{key}> using culture <{culture.Name}> and resource <{resource.BaseName}> to: {result}");
                                return result;
                            } 
                        }
                    }
                }

                if (useDefault)
                {
                    var culture = Thread.CurrentThread.CurrentUICulture; 
                    foreach (var resource in _resources)
                    {
                        _loggers.Debug($"Attempting to localize <{key}> using ui culture <{culture.Name}> and resource <{resource.BaseName}>");
                        var result = resource.GetString(key, culture);
                        if (result != null)
                        {
                            _loggers.Trace($"Localized <{key}> using ui culture <{culture.Name}> and resource <{resource.BaseName}> to: {result}");
                            return result;
                        }
                    }
                }

                _loggers.Debug($"Could not localize <{key}>");
                return null;
            }           
        }
        #endregion

        /// <summary>
        /// Contains static helper methods for localizing objects.
        /// </summary>
        public static class Object
        {
            /// <summary>
            /// Returns the localized name for object <paramref name="objectType"/>.
            /// </summary>
            /// <param name="objectType">The type of the object to localize</param>
            /// <param name="culture">Optional culture to get the localized string in. If set to null either the default defined in <see cref="LocalizationOptions"/> is used or the default thread ui culture</param>
            /// <param name="options">Optional delegate for configuring the options</param>
            /// <returns>The localized string or another result based on <see cref="LocalizationOptions.MissingKeySettings"/></returns>
            public static string? Get(Type objectType, string? culture = null, Action<LocalizationOptions>? options = null)
            {
                objectType.ValidateArgument(nameof(objectType));

                return GetObjectKey(GetTypeAlias(objectType), culture, options, objectType.Name);
            }
            /// <summary>
            /// Returns the localized name for <paramref name="property"/>.
            /// </summary>
            /// <param name="property">The name of the property to localize</param>
            /// <param name="culture">Optional culture to get the localized string in. If set to null either the default defined in <see cref="LocalizationOptions"/> is used or the default thread ui culture</param>
            /// <param name="options">Optional delegate for configuring the options</param>
            /// <returns>The localized string or another result based on <see cref="LocalizationOptions.MissingKeySettings"/></returns>
            public static string? Get(PropertyInfo property, string? culture = null, Action<LocalizationOptions>? options = null)
            {
                property.ValidateArgument(nameof(property));

                return GetObjectKey($"{GetTypeAlias(property.DeclaringType)}.{property.Name}", culture, options, property.Name);
            }

            /// <summary>
            /// Returns the localized value name for a value assigned to <paramref name="property"/>.
            /// </summary>
            /// <param name="property">The name of the property to localize the value for</param>
            /// <param name="value">The property value to get the localized name for</param>
            /// <param name="culture">Optional culture to get the localized string in. If set to null either the default defined in <see cref="LocalizationOptions"/> is used or the default thread ui culture</param>
            /// <param name="options">Optional delegate for configuring the options</param>
            /// <returns>The localized string or another result based on <see cref="LocalizationOptions.MissingKeySettings"/></returns>
            public static string? GetValue(PropertyInfo property, object value, string? culture = null, Action<LocalizationOptions>? options = null)
            {
                property.ValidateArgument(nameof(property));
                var valueString = value.ValidateArgument(nameof(value)).ToString();

                return GetObjectKey($"{GetTypeAlias(property.DeclaringType)}.{property.Name}.{valueString}", culture, options, valueString);
            }

            /// <summary>
            /// Returns the localized name for enum <paramref name="value"/>.
            /// </summary>
            /// <param name="value">The enum value to localize</param>
            /// <param name="culture">Optional culture to get the localized string in. If set to null either the default defined in <see cref="LocalizationOptions"/> is used or the default thread ui culture</param>
            /// <param name="options">Optional delegate for configuring the options</param>
            /// <returns>The localized string or another result based on <see cref="LocalizationOptions.MissingKeySettings"/></returns>
            public static string? Enum<T>(T value, string? culture = null, Action<LocalizationOptions>? options = null) where T : Enum
            {
                return GetObjectKey($"{GetTypeAlias(value.GetType())}.{value}", culture, options, value.ToString());
            }

            private static string? GetObjectKey(string key, string? culture, Action<LocalizationOptions>? options, string defaultValue)
            {
                key.ValidateArgumentNotNullOrWhitespace(nameof(key));

                bool isDefault = false;

                var result = Localizer.Get(key, culture, x =>
                {
                    if (options != null)
                    {
                        options(x);  
                    }
                    // Intercept default handling. Instead of the formatted key we return the supplied default value.
                    isDefault = x.MissingKeySettings == MissingLocalizationSettings.Default;
                    x.MissingKeySettings = MissingLocalizationSettings.Null;
                });

                if(isDefault && result == null)
                {
                    return defaultValue;
                }

                return result;
            }

            #region Alias
            /// <summary>
            /// The prefix to place in front of a type name to define an alias for the type.
            /// </summary>
            public const string AliasKeyPrefix = "TypeAlias.";
            private static string GetTypeAlias(Type type)
            {
                using (_loggers.TraceMethod(typeof(Localizer)))
                {
                    type.ValidateArgument(nameof(type));

                    var typeName = type.GetDisplayName(true);
                    _loggers.Debug($"Getting alias for type <{typeName}>");

                    var alias = Localizer.Get(AliasKeyPrefix + typeName, null, true);
                    
                    if (!alias.HasValue() && type.IsGenericType && !type.IsGenericTypeDefinition)
                    {
                        typeName = type.GetGenericTypeDefinition().GetDisplayName();
                        _loggers.Debug($"Type <{type}> is generic type. Searching for generic type definition <{typeName}>");
                        alias = Localizer.Get(AliasKeyPrefix + typeName, null, true);
                    }

                    if (alias.HasValue())
                    {
                        _loggers.Debug($"Type alias <{alias}> defined for type <{typeName}>");
                        return alias;
                    }

                    _loggers.Debug($"No type alias defined for type <{type}>. Using full type display name as default");
                    return type.GetDisplayName(true);
                }
            }
            #endregion
        }

        private class LocBuilder : ILocalizationBuilder
        {
            // Constants
            public const string ResourceExtension = ".resources";

            // Properties
            public List<(Assembly Assembly, string Name)> Resources { get;  } = new List<(Assembly Assembly, string Name)>();
            public List<string> Cultures { get; } = new List<string>();
            public List<ILogger> Loggers { get; } = new List<ILogger>();
            public Action<LocalizationOptions> OptionsBuilder { get; private set; }

            /// <inheritdoc/>
            public ILocalizationBuilder Use(Assembly assembly, string resourceName)
            {
                assembly.ValidateArgument(nameof(assembly));
                resourceName.ValidateArgument(nameof(resourceName));

                var resourceInfo = assembly.GetManifestResourceInfo(resourceName);
                if (resourceInfo == null) throw new InvalidOperationException($"Could not find resource <{resourceName}> in assembly <{assembly}>");
                Resources.Add((assembly, resourceName.Substring(0, resourceName.IndexOf(ResourceExtension))));

                return this;
            }
            /// <inheritdoc/>
            public ILocalizationBuilder ScanIn(Assembly assembly, Predicate<string>? canLoad = null)
            {
                assembly.ValidateArgument(nameof(assembly));

                foreach (var resource in assembly.GetManifestResourceNames().Where(x => x.EndsWith(ResourceExtension, StringComparison.OrdinalIgnoreCase)))
                {
                    if (canLoad != null ? canLoad(resource) : true)
                    {
                        Resources.Add((assembly, resource.Substring(0, resource.IndexOf(ResourceExtension))));
                    }
                }

                return this;
            }
            /// <inheritdoc/>
            public ILocalizationBuilder ScanInAllLoaded(Delegates.Condition<Assembly, string>? canLoad = null)
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();

                foreach(var assembly in assemblies)
                {
                    ScanIn(assembly, x => canLoad != null ? canLoad(assembly, x) : true);
                }

                return this;
            }
            /// <inheritdoc/>
            public ILocalizationBuilder WithCultures(IEnumerable<string> cultures)
            {
                cultures.ValidateArgumentNotNullOrEmpty(nameof(cultures));

                Cultures.AddRange(cultures.Where(x => x.HasValue()));

                return this;
            }
            /// <inheritdoc/>
            public ILocalizationBuilder UseLoggers(IEnumerable<ILogger?>? loggers)
            {
                if(loggers != null)
                {
                    Loggers.AddRange(loggers.Where(x => x != null));
                }

                return this;
            }
            /// <inheritdoc/>
            public ILocalizationBuilder WithDefaultOptions(Action<LocalizationOptions> options)
            {
                options.ValidateArgument(nameof(options));

                OptionsBuilder = options;

                return this;
            }
        }
    }
}
