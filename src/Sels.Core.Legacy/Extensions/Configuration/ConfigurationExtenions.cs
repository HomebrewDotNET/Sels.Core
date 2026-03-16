using Microsoft.Extensions.Logging;
using Sels.Core.Extensions;
using System.IO;

namespace Sels.Core.Configuration
{
    /// <summary>
    /// Contains extension methods for reading configuration.
    /// </summary>
    public static class ConfigurationExtenions
    {
        /// <summary>
        /// Reads <see cref="Constants.Configuration.Logging.Directory"/> from configuration.
        /// 
        /// </summary>
        /// <param name="configurationService">Service used to access the application configuration</param>
        /// <param name="required">Wether or not the directory is expected to be set in config. When set to true and it's missing an exception will be thrown</param>
        /// <returns>The configured logging directory</returns>
        public static DirectoryInfo GetLoggingDirectory(this IConfigurationService configurationService, bool required = true)
        {
            var path = configurationService
                    .ValidateArgument(nameof(configurationService))
                    .Get<string>(Constants.Configuration.Logging.Directory, 
                        x => x.FromSection(Constants.Configuration.Logging.Name),
                        required ? ConfigurationSettings.Required : ConfigurationSettings.None);
            return path.HasValue() ? new DirectoryInfo(path) : null;
        }

        /// <summary>
        /// Reads <see cref="Constants.Configuration.Logging.ArchiveDirectory"/> from configuration.
        /// </summary>
        /// <param name="configurationService">Service used to access the application configuration</param>
        /// <param name="required">Wether or not the directory is expected to be set in config. When set to true and it's missing an exception will be thrown</param>
        /// <returns>The configured archive logging directory</returns>
        public static DirectoryInfo GetArchiveLoggingDirectory(this IConfigurationService configurationService, bool required = true)
        {
            var path = configurationService
                    .ValidateArgument(nameof(configurationService))
                    .Get<string>(Constants.Configuration.Logging.ArchiveDirectory,
                        x => x.FromSection(Constants.Configuration.Logging.Name),
                        required ? ConfigurationSettings.Required : ConfigurationSettings.None);
            return path.HasValue() ? new DirectoryInfo(path) : null;
        }

        /// <summary>
        /// Reads <see cref="Constants.Configuration.Logging.MaxFileSize"/> from configuration.
        /// </summary>
        /// <param name="configurationService">Service used to access the application configuration</param>
        /// <param name="required">Wether or not the filesize is expected to be set in config. When set to true and it's missing an exception will be thrown</param>
        /// <returns>The configured max file size</returns>
        public static decimal? GetMaxLogFileSize(this IConfigurationService configurationService, bool required = true)
        {
            return configurationService
                    .ValidateArgument(nameof(configurationService))
                    .Get<decimal?>(Constants.Configuration.Logging.ArchiveDirectory,
                        x => x.FromSection(Constants.Configuration.Logging.Name),
                        required ? ConfigurationSettings.Required : ConfigurationSettings.None);
        }

        /// <summary>
        /// Reads <see cref="Constants.Configuration.Logging.LogLevel.Default"/> from configuration.
        /// </summary>
        /// <param name="configurationService">Service used to access the application configuration</param>
        /// <param name="defaultLogLevel">The default log level to return when missing from config</param>
        /// <returns>The configured default log level</returns>
        public static LogLevel GetDefaultLogLevel(this IConfigurationService configurationService, LogLevel defaultLogLevel = LogLevel.Information)
        {
            var loglevel = configurationService
                    .ValidateArgument(nameof(configurationService))
                    .Get<LogLevel?>(Constants.Configuration.Logging.LogLevel.Default,
                        x => x.FromSection(Constants.Configuration.Logging.Name, Constants.Configuration.Logging.LogLevel.Name)
                              .SetDefault(defaultLogLevel)
                        , ConfigurationSettings.None);
            return loglevel.HasValue ? loglevel.Value : LogLevel.Information;
        }
    }
}
