using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using Sels.Core.Extensions;
using Sels.Core.Contracts.Configuration;

namespace Sels.Core.Components.Configuration
{
    public class ConfigProvider : IConfigProvider
    {
        // Constants
        private const string AppSettingSection = Constants.Config.Sections.AppSettings;

        // Fields
        private readonly IConfiguration _config;

        public ConfigProvider(IConfiguration config)
        {
            config.ValidateArgument(nameof(config));

            _config = config;
        }

        public string GetAppSetting(string key)
        {
            key.ValidateArgumentNotNullOrWhitespace(nameof(key));

            return GetAppSetting<string>(key);
        }

        public T GetAppSetting<T>(string key)
        {
            key.ValidateArgumentNotNullOrWhitespace(nameof(key));

            return _config.GetSection(AppSettingSection).GetValue<T>(key);
        }

        public string GetConnectionString(string key)
        {
            key.ValidateArgumentNotNullOrWhitespace(nameof(key));

            return _config.GetConnectionString(key);
        }

        public Dictionary<string, object> GetSectionAs(string section)
        {
            return GetSectionAs<Dictionary<string, object>>(section);
        }

        public Dictionary<string, object> GetSectionAs(params string[] sections)
        {
            return GetSectionAs<Dictionary<string, object>>(sections);
        }

        public T GetSectionAs<T>(string section)
        {
            section.ValidateArgumentNotNullOrWhitespace(nameof(section));

            return _config.GetSection(section).Get<T>();
        }

        public T GetSectionAs<T>(params string[] sections)
        {
            sections.ValidateArgumentNotNullOrEmpty(nameof(sections));
            Dictionary<string, object> sectionValue = new Dictionary<string, object>();

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

            return currentSection.Get<T>();
        }

        public string GetSectionSetting(string key, string section)
        {
            return GetSectionSetting<string>(key, section);
        }

        public string GetSectionSetting(string key, params string[] sections)
        {
            return GetSectionSetting<string>(key, sections);
        }

        public T GetSectionSetting<T>(string key, string section)
        {
            key.ValidateArgumentNotNullOrWhitespace(nameof(key));
            section.ValidateArgumentNotNullOrWhitespace(nameof(section));

            return _config.GetSection(section).GetValue<T>(key);
        }

        public T GetSectionSetting<T>(string key, params string[] sections)
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

            return currentSection.GetValue<T>(key);
        }
    }
}
