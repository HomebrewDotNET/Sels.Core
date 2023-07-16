using Sels.Core.Extensions;

namespace Sels.Core.Configuration
{
    /// <summary>
    /// Contains extension methods related to reading application configuration.
    /// </summary>
    public static class ConfigurationExceptions
    {
        /// <summary>
        /// Reads <see cref="Constants.Configuration.Logging.MaxFileSize"/> from configuration where it it defined in filesize <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type of the file size in config</typeparam>
        /// <param name="configurationService">Service used to access the application configuration</param>
        /// <param name="required">Wether or not the filesize is expected to be set in config. When set to true and it's missing an exception will be thrown</param>
        /// <returns>The configured max file size or null if missing</returns>
        public static T GetMaxLogFileSize<T>(this IConfigurationService configurationService, bool required = true) where T : FileSize, new()
        {
            var size = configurationService
                    .ValidateArgument(nameof(configurationService))
                    .Get<decimal?>(Constants.Configuration.Logging.ArchiveDirectory,
                        x => x.FromSection(Constants.Configuration.Logging.Name),
                        required ? ConfigurationSettings.Required : ConfigurationSettings.None);

            return size.HasValue ? FileSize.CreateFromSize<T>(size.Value) : null;
        }
    }
}
