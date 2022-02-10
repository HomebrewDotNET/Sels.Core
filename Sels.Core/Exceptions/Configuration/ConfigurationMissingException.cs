using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Exceptions.Configuration
{
    /// <summary>
    /// Indicates that configuration was expected but missing in the config file.
    /// </summary>
    public class ConfigurationMissingException : Exception
    {
        // Constants
        private const string MessageFormat = "Configuration with key <{0}> is missing in configuration file <{1}>";
        private const string MessageSectionFormat = "Configuration section <{0}> is missing in configuration file <{1}>";

        /// <summary>
        /// Configuration key or section name that is missing from config.
        /// </summary>
        public string MissingKey { get; }
        /// <summary>
        /// Configuration file that is missing the needed config key/section
        /// </summary>
        public string ConfigFile { get; }

        /// <inheritdoc cref="ConfigurationMissingException"/>
        /// <param name="key">The section or config key that was missing</param>
        /// <param name="configFile">The config file where the config key is missing from</param>
        /// <param name="isMissingSection">If the current exception is thrown because of a missing section</param>
        /// <param name="sections">Optional parent sections for the missing config key</param>
        public ConfigurationMissingException(string key, string configFile, bool isMissingSection, params string[] sections) : base(BuildMessage(key, configFile, isMissingSection, sections))
        {
            ConfigFile = configFile;
            MissingKey = Helper.Configuration.BuildPathString(key, sections);           
        }

        private static string BuildMessage(string key, string configFile, bool isMissingSection, params string[] sections)
        {
            key.ValidateArgumentNotNullOrWhitespace(nameof(key));
            configFile.ValidateArgumentNotNullOrWhitespace(nameof(configFile));

            return (isMissingSection ? MessageSectionFormat : MessageFormat).FormatString(Helper.Configuration.BuildPathString(key, sections), configFile);
        }
    }
}
