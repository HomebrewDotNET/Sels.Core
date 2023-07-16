using Sels.Core.Extensions;
using Sels.Core.Extensions.Text;
using System;

namespace Sels.Core.Exceptions.Configuration
{
    /// <summary>
    /// Indicates a value in config that is invalid.
    /// </summary>
    public class MisconfiguredException : Exception
    {
        // Constants
        private const string MessageFormat = "Configuration with key <{0}> in configuration file <{1}> is misconfigured: <{2}>";
        private const string MessageSectionFormat = "Configuration section <{0}> in configuration file <{1}> is misconfigured: <{2}>";

        // Properties
        /// <summary>
        /// Config key/section that was misconfigured.
        /// </summary>
        public string ConfigKey { get; }
        /// <summary>
        /// Configuration file that contains the invalid file.
        /// </summary>
        public string ConfigFile { get; }
        /// <summary>
        /// Reason that <see cref="ConfigKey"/> was misconfigured. 
        /// </summary>
        public string Reason { get; set; }

        /// <inheritdoc cref="MisconfiguredException"/>
        /// <param name="key">The config key or section that is invalid</param>
        /// <param name="configFile">The file containing the invalid value</param>
        /// <param name="reason"></param>
        /// <param name="isForSection"></param>
        /// <param name="sections"></param>
        public MisconfiguredException(string key, string configFile, string reason, bool isForSection, params string[] sections) : base(BuildMessage(key, configFile, reason, isForSection, sections))
        {
            ConfigFile = configFile;
            ConfigKey = Helper.Configuration.BuildPathString(key, sections);
            Reason = reason;
        }

        private static string BuildMessage(string key, string configFile, string reason, bool isForSection, params string[] sections)
        {
            key.ValidateArgumentNotNullOrWhitespace(nameof(key));
            configFile.ValidateArgumentNotNullOrWhitespace(nameof(configFile));
            reason.ValidateArgumentNotNullOrWhitespace(nameof(reason));

            return (isForSection ? MessageSectionFormat : MessageFormat).FormatString(Helper.Configuration.BuildPathString(key, sections), configFile, reason);
        }       
    }
}
