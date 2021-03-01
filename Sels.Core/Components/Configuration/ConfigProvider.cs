using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using Sels.Core.Extensions;
using Sels.Core.Extensions;
using Sels.Core.Components.Configuration.Exceptions;

namespace Sels.Core.Components.Configuration
{
    public class ConfigProvider : IConfigProvider
    {
        // Constants
        private const string AppSettingSection = "AppSettings";

        // Fields
        private readonly IConfiguration _config;

        public ConfigProvider(IConfiguration config)
        {
            config.ValidateVariable(nameof(config));

            _config = config;
        }

        public string GetAppSetting(string key)
        {
            key.ValidateVariable(nameof(key));

            return GetAppSetting<string>(key);
        }

        public T GetAppSetting<T>(string key)
        {
            key.ValidateVariable(nameof(key));

            return _config.GetSection(AppSettingSection).GetValue<T>(key);
        }

        public string GetConnectionString(string key)
        {
            key.ValidateVariable(nameof(key));

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
            section.ValidateVariable(nameof(section));

            return _config.GetSection(section).Get<T>();
        }

        public T GetSectionAs<T>(params string[] sections)
        {
            sections.ValidateVariable(x => x.HasValue(), () => "At least 1 section key must be supplied");
            Dictionary<string, object> sectionValue = new Dictionary<string, object>();

            IConfigurationSection currentSection = null;

            foreach (var section in sections)
            {
                if (currentSection.HasValue())
                {
                    currentSection = _config.GetSection(section);
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
            key.ValidateVariable(nameof(key));
            section.ValidateVariable(nameof(section));

            return _config.GetSection(section).GetValue<T>(key);
        }

        public T GetSectionSetting<T>(string key, params string[] sections)
        {
            key.ValidateVariable(nameof(key));
            sections.ValidateVariable(x => x.HasValue(), () => "At least 1 section key must be supplied");

            IConfigurationSection currentSection = null;

            foreach (var section in sections)
            {
                if (currentSection.HasValue())
                {
                    currentSection = _config.GetSection(section);
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
