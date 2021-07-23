using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Exceptions.Configuration
{
    /// <summary>
    /// Indicates that configuration was expected but missing in the config file
    /// </summary>
    public class ConfigurationMissingException : Exception
    {
        // Constants
        private const string MessageFormat = "Configuration with key {0} is missing in configuration file {1}";
        private const string MessageSectionFormat = "Configuration section {0} is missing in configuration file {1}";

        /// <summary>
        /// Configuration key or section name that is missing from config
        /// </summary>
        public string MissingKey { get; }
        /// <summary>
        /// Configuration file that is missing the needed config key/section
        /// </summary>
        public string ConfigFile { get; }

        public ConfigurationMissingException(string configKey, string section, string configFile) : base(MessageFormat.FormatString($"{section}.{configKey}", configFile))
        {
            ConfigFile = configFile.ValidateArgumentNotNullOrWhitespace(nameof(configFile));
            MissingKey = $"{section.ValidateArgumentNotNullOrWhitespace(nameof(section))}.{configKey.ValidateArgumentNotNullOrWhitespace(nameof(configKey))}";           
        }

        public ConfigurationMissingException(string section, string configFile) : base(MessageSectionFormat.FormatString(section, configFile))
        {
            ConfigFile = configFile.ValidateArgumentNotNullOrWhitespace(nameof(configFile));
            MissingKey = section.ValidateArgumentNotNullOrWhitespace(nameof(section));
        }
    }
}
