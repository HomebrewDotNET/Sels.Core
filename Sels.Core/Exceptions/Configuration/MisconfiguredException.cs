using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Exceptions.Configuration
{
    public class MisconfiguredException : Exception
    {
        // Constants
        private const string MessageFormat = "Configuration with key {0} in configuration file {1} is misconfigured: {2}";
        private const string MessageSectionFormat = "Configuration section {0} in configuration file {1} is misconfigured: {2}";

        // Properties
        /// <summary>
        /// Config key/section that was misconfigured.
        /// </summary>
        public string ConfigKey { get; }
        /// <summary>
        /// Configuration file that is missing the needed config key
        /// </summary>
        public string ConfigFile { get; }
        /// <summary>
        /// Reason that <see cref="ConfigKey"/> was misconfigured. 
        /// </summary>
        public string Reason { get; set; }


        public MisconfiguredException(string configKey, string section, string configFile, string reason) : base(MessageFormat.FormatString($"{section}.{configKey}", configFile, reason))
        {
            ConfigFile = configFile.ValidateArgumentNotNullOrWhitespace(nameof(configFile));
            ConfigKey = $"{section.ValidateArgumentNotNullOrWhitespace(nameof(section))}.{configKey.ValidateArgumentNotNullOrWhitespace(nameof(configKey))}";
            Reason = reason.ValidateArgumentNotNullOrWhitespace(nameof(reason));
        }

        public MisconfiguredException(string section, string configFile, string reason) : base(MessageSectionFormat.FormatString(section, configFile, reason))
        {
            ConfigFile = configFile.ValidateArgumentNotNullOrWhitespace(nameof(configFile));
            ConfigKey = section.ValidateArgumentNotNullOrWhitespace(nameof(section));
            Reason = reason.ValidateArgumentNotNullOrWhitespace(nameof(reason));
        }
    }
}
