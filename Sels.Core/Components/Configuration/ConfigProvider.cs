using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using Sels.Core.Extensions;
using Sels.Core.Contracts.Configuration;
using Sels.Core.Exceptions.Configuration;
using Sels.Core.Extensions.Conversion;

namespace Sels.Core.Components.Configuration
{
    public class ConfigProvider : IConfigProvider
    {
        // Constants
        private const string AppSettingSection = Constants.Config.Sections.AppSettings;
        private const string ConnectionStringsSection = Constants.Config.Sections.ConnectionStrings;
        private const string ConfigFile = Constants.Config.DefaultAppSettingsFile;

        // Fields
        private readonly IConfiguration _config;

        public ConfigProvider(IConfiguration config)
        {
            config.ValidateArgument(nameof(config));

            _config = config;
        }

        public string GetAppSetting(string key, bool required = true, Predicate<string> validator = null, Func<string, string> misconfigurationReason = null)
        {
            key.ValidateArgumentNotNullOrWhitespace(nameof(key));

            return GetAppSetting<string>(key, required, validator, misconfigurationReason);
        }

        public T GetAppSetting<T>(string key, bool required = true, Predicate<T> validator = null, Func<T, string> misconfigurationReason = null)
        {
            key.ValidateArgumentNotNullOrWhitespace(nameof(key));

            return GetSectionSetting<T>(key, AppSettingSection, required, validator, misconfigurationReason);
        }

        public string GetConnectionString(string key, bool required = true)
        {
            key.ValidateArgumentNotNullOrWhitespace(nameof(key));

            var connectionstring = _config.GetConnectionString(key);

            if(required && !connectionstring.HasValue())
            {
                throw new MisconfiguredException(key, ConnectionStringsSection, ConfigFile, $"Connection string with name <{key}> is not properly defined");
            }

            return connectionstring;
        }

        public Dictionary<string, object> GetSectionAs(string section, bool required = true, Predicate<Dictionary<string, object>> validator = null, Func<Dictionary<string, object>, string> misconfigurationReason = null)
        {
            return GetSectionAs<Dictionary<string, object>>(section, required, validator, misconfigurationReason);
        }

        public Dictionary<string, object> GetSectionAs(bool required = true, Predicate<Dictionary<string, object>> validator = null, Func<Dictionary<string, object>, string> misconfigurationReason = null, params string[] sections)
        {
            return GetSectionAs<Dictionary<string, object>>(required, validator, misconfigurationReason, sections);
        }

        public T GetSectionAs<T>(string section, bool required = true, Predicate<T> validator = null, Func<T, string> misconfigurationReason = null)
        {
            section.ValidateArgumentNotNullOrWhitespace(nameof(section));

            var configSection = _config.GetSection(section);

            if (required && !configSection.Exists())
            {
                throw new ConfigurationMissingException(section, ConfigFile);
            }

            var value = configSection.Get<T>();

            return validator.HasValue() ? value.ValidateArgument(validator, x => new MisconfiguredException(section, ConfigFile, misconfigurationReason.HasValue() ? misconfigurationReason(x) : $"<{x.SerializeAsJson()}> is not a valid value")) : value;
        }

        public T GetSectionAs<T>(bool required = true, Predicate<T> validator = null, Func<T, string> misconfigurationReason = null, params string[] sections)
        {
            sections.ValidateArgumentNotNullOrEmpty(nameof(sections));

            IConfigurationSection currentSection = null;

            foreach (var section in sections)
            {
                if (currentSection.HasValue())
                {
                    currentSection = currentSection.GetSection(section);
                }
                else
                {
                    currentSection = _config.GetSection(section);
                }
            }

            if (required && !currentSection.Exists())
            {
                throw new ConfigurationMissingException(sections.JoinString("."), ConfigFile);
            }

            var value = currentSection.Get<T>();

            return validator.HasValue() ? value.ValidateArgument(validator, x => new MisconfiguredException(sections.JoinString("."), ConfigFile, misconfigurationReason.HasValue() ? misconfigurationReason(x) : $"<{x.SerializeAsJson()}> is not a valid value")) : value;
        }

        public string GetSectionSetting(string key, string section, bool required = true, Predicate<string> validator = null, Func<string, string> misconfigurationReason = null)
        {
            return GetSectionSetting<string>(key, section, required, validator, misconfigurationReason);
        }

        public string GetSectionSetting(string key, bool required = true, Predicate<string> validator = null, Func<string, string> misconfigurationReason = null, params string[] sections)
        {
            return GetSectionSetting<string>(key, required, validator, misconfigurationReason, sections);
        }

        public T GetSectionSetting<T>(string key, string section, bool required = true, Predicate<T> validator = null, Func<T, string> misconfigurationReason = null)
        {
            key.ValidateArgumentNotNullOrWhitespace(nameof(key));
            section.ValidateArgumentNotNullOrWhitespace(nameof(section));

            var configSection = _config.GetSection(AppSettingSection);

            if (required && !configSection.Exists())
            {
                throw new ConfigurationMissingException(key, section, ConfigFile);
            }

            var value = configSection.GetValue<T>(key);

            return validator.HasValue() ? value.ValidateArgument(validator, x => new MisconfiguredException(key, section, ConfigFile, misconfigurationReason.HasValue() ? misconfigurationReason(x) : $"<{x}> is not a valid value")) : value;
        }

        public T GetSectionSetting<T>(string key, bool required = true, Predicate<T> validator = null, Func<T, string> misconfigurationReason = null, params string[] sections)
        {
            key.ValidateArgumentNotNullOrWhitespace(nameof(key));
            sections.ValidateArgumentNotNullOrEmpty(nameof(sections));

            IConfigurationSection currentSection = null;

            foreach (var section in sections)
            {
                if (currentSection.HasValue())
                {
                    currentSection = currentSection.GetSection(section);
                }
                else
                {
                    currentSection = _config.GetSection(section);
                }
            }

            if (required && !currentSection.Exists())
            {
                throw new ConfigurationMissingException(key, sections.JoinString("."), ConfigFile);
            }

            var value =  currentSection.GetValue<T>(key);

            return validator.HasValue() ? value.ValidateArgument(validator, x => new MisconfiguredException(key, sections.JoinString("."), ConfigFile, misconfigurationReason.HasValue() ? misconfigurationReason(x) : $"<{x}> is not a valid value")) : value;
        }
    }
}
