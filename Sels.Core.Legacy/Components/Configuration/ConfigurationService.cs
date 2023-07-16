using Microsoft.Extensions.Configuration;
using Sels.Core.Configuration;
using Sels.Core.Exceptions.Configuration;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sels.Core.Configuration
{
    /// <inheritdoc cref="IConfigurationService"/>
    public class ConfigurationService : IConfigurationService
    {
        // Fields
        private readonly IConfiguration _configuration;

        /// <inheritdoc cref="ConfigurationService"/>
        /// <param name="configuration">The key/value pairs to access application configuration</param>
        public ConfigurationService(IConfiguration configuration)
        {
            _configuration = configuration.ValidateArgument(nameof(configuration));
        }

        /// <inheritdoc/>
        public string GetConnectionString(string name)
        {
            name.ValidateArgumentNotNullOrWhitespace(nameof(name));

            return Get(name, x => x.FromSection(Constants.Configuration.Sections.ConnectionStrings), ConfigurationSettings.Required);
        }
        /// <inheritdoc/>
        public string Get(string key, Action<IConfigurationOptions<string>> options = null, ConfigurationSettings settings = ConfigurationSettings.None)
        {
            key.ValidateArgumentNotNullOrWhitespace(nameof(key));

            return Get<string>(key, options, settings);
        }
        /// <inheritdoc/>
        public T Get<T>(string key, Action<IConfigurationOptions<T>> options = null, ConfigurationSettings settings = ConfigurationSettings.None)
        {
            key.ValidateArgumentNotNullOrWhitespace(nameof(key));

            return GetConfig<T>(key, false, options, settings);
        }
        /// <inheritdoc/>
        public string GetAppSetting(string key, Action<IConfigurationOptions<string>> options = null, ConfigurationSettings settings = ConfigurationSettings.None)
        {
            key.ValidateArgumentNotNullOrWhitespace(nameof(key));

            return GetAppSetting<string>(key, options, settings);
        }
        /// <inheritdoc/>
        public T GetAppSetting<T>(string key, Action<IConfigurationOptions<T>> options = null, ConfigurationSettings settings = ConfigurationSettings.None)
        {
            key.ValidateArgumentNotNullOrWhitespace(nameof(key));

            return Get<T>(key, x =>
            {
                x.FromSection(Constants.Configuration.Sections.AppSettings);
                if (options != null) options(x);
            }, settings);
        }
        /// <inheritdoc/>
        public Dictionary<string, string> GetSection(string section, Action<IConfigurationOptions<Dictionary<string, string>>> options = null, ConfigurationSettings settings = ConfigurationSettings.None)
        {
            section.ValidateArgumentNotNullOrWhitespace(nameof(section));

            return GetSection<Dictionary<string, string>>(section, options, settings);
        }
        /// <inheritdoc/>
        public T GetSection<T>(string section, Action<IConfigurationOptions<T>> options = null, ConfigurationSettings settings = ConfigurationSettings.None)
        {
            section.ValidateArgumentNotNullOrWhitespace(nameof(section));

            return GetConfig<T>(section, true, options, settings);
        }

        private T GetConfig<T>(string key, bool isForSection, Action<IConfigurationOptions<T>> options = null, ConfigurationSettings settings = ConfigurationSettings.None)
        {
            key.ValidateArgumentNotNullOrWhitespace(nameof(key));

            // Create option objct and configure it if delegate is provided
            var configOptions = new ConfigurationOptions<T>(isForSection, settings);
            if(options != null) options(configOptions);

            // Go through parent sections if provided
            IConfigurationSection currentSection = null;
            if (configOptions.Sections.HasValue())
            {
                foreach(var section in configOptions.Sections)
                {
                    if (currentSection == null)
                    {
                        currentSection = _configuration.GetSection(section);
                    }
                    else 
                    {
                        currentSection = currentSection.GetSection(section);
                    }

                    if(!currentSection.Exists())
                    {
                        if (configOptions.IsRequired) throw new ConfigurationMissingException(key, Constants.Configuration.DefaultAppSettingsFile, true, configOptions.Sections);
                        return configOptions.DefaultValue;
                    }
                }              
            }

            // Fetch, bind and validate the configuration value
            var configValueSection = (currentSection ?? _configuration).GetSection(key);
            if (configValueSection.Exists())
            {
                var value = configValueSection.Get<T>();
                configOptions.ThrowIfInvalid(key, value);
                return value;
            }
            else if (configOptions.IsRequired)
            {
                throw new ConfigurationMissingException(key, Constants.Configuration.DefaultAppSettingsFile, isForSection, configOptions.Sections);
            }

            return configOptions.DefaultValue;
        }

        private class ConfigurationOptions<T> : IConfigurationOptions<T>
        {
            // Fields
            private bool _isForSection;
            private List<string> _sections = new List<string>();
            private List<(Delegates.Condition<(string Key, T Value, bool IsSection)> Condition, Func<(string Key, T Value, bool IsSection), Exception> ErrorExceptionFunc)> _validators = new List<(Delegates.Condition<(string Key, T Value, bool IsSection)> Condition, Func<(string Key, T Value, bool IsSection), Exception> ErrorExceptionFunc)>(); 

            // Properties
            public T DefaultValue { get; private set; }
            public bool IsRequired { get; }
            public string[] Sections => _sections.ToArray();

            public ConfigurationOptions(bool isForSection, ConfigurationSettings settings)
            {
                _isForSection = isForSection;
                IsRequired = settings.HasFlag(ConfigurationSettings.Required);
            }

            public void ThrowIfInvalid(string key, T value)
            {
                _validators.Where(x => !x.Condition((key, value, _isForSection))).Execute(x => throw x.ErrorExceptionFunc((key, value, _isForSection)));
            }

            public IConfigurationOptions<T> FromSection(string section)
            {
                section.ValidateArgumentNotNullOrWhitespace(nameof(section));

                _sections.Add(section);

                return this;
            }

            public IConfigurationOptions<T> FromSection(params string[] sections)
            {
                sections.ValidateArgumentNotNullOrEmpty(nameof(sections));
                sections.Execute(x => FromSection(x));
                return this;
            }

            public IConfigurationOptions<T> SetDefault(T defaultValue)
            {
                DefaultValue = defaultValue;
                return this;
            }

            public IConfigurationOptions<T> ValidIf(Delegates.Condition<(string Key, T Value, bool IsSection)> condition, Func<(string Key, T Value, bool IsSection), Exception> errorExceptionFunc)
            {
                condition.ValidateArgument(nameof(condition));
                errorExceptionFunc.ValidateArgument(nameof(errorExceptionFunc));

                _validators.Add((condition, errorExceptionFunc));
                return this;
            }

            public IConfigurationOptions<T> ValidIf(Delegates.Condition<(string Key, T Value, bool IsSection)> condition, Func<(string Key, T Value, bool IsSection), string> errorMessageFunc)
            {
                condition.ValidateArgument(nameof(condition));
                errorMessageFunc.ValidateArgument(nameof(errorMessageFunc));

                return ValidIf(condition, x => new MisconfiguredException(x.Key, Constants.Configuration.DefaultAppSettingsFile, errorMessageFunc(x), _isForSection, Sections));
            }
        }
    }
}
