using Sels.Core.Exceptions.Configuration;
using System;
using System.Collections.Generic;
using static Sels.Core.Delegates;

namespace Sels.Core.Contracts.Configuration
{
    /// <summary>
    /// Service for accessing application configuration.
    /// </summary>
    public interface IConfigurationService
    {
        /// <summary>
        /// Fetches the connection string with name <paramref name="name"/> from the default connection strings section.
        /// </summary>
        /// <param name="name">The name of the connection string to fetch</param>
        /// <returns>The connection string with name <paramref name="name"/></returns>
        string GetConnectionString(string name);
        /// <summary>
        /// Fetches configuration value with key <paramref name="key"/>.
        /// </summary>
        /// <typeparam name="T">The type of the configuration value to fetch</typeparam>
        /// <param name="key">The key of the configuration value to fetch</param>
        /// <param name="options">Optional action for setting advanced actions that are to be executed when fetching the requested configuration value</param>
        /// <param name="settings">Optional settings that define extra actions to execute when fetching the requested configuration value</param>
        /// <returns>The configuration value with key <paramref name="key"/></returns>
        T Get<T>(string key, Action<IConfigurationOptions<T>> options = null, ConfigurationSettings settings = ConfigurationSettings.None);
        /// <summary>
        /// Fetches configuration value with key <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key of the configuration value to fetch</param>
        /// <param name="options">Optional action for setting advanced actions that are to be executed when fetching the requested configuration value</param>
        /// <param name="settings">Optional settings that define extra actions to execute when fetching the requested configuration value</param>
        /// <returns>The configuration value with key <paramref name="key"/></returns>
        string Get(string key, Action<IConfigurationOptions<string>> options = null, ConfigurationSettings settings = ConfigurationSettings.None);
        /// <summary>
        /// Fetches configuration value with key <paramref name="key"/> from the default app setting section.
        /// </summary>
        /// <typeparam name="T">The type of the configuration value to fetch</typeparam>
        /// <param name="key">The key of the configuration value to fetch</param>
        /// <param name="options">Optional action for setting advanced actions that are to be executed when fetching the requested configuration value</param>
        /// <param name="settings">Optional settings that define extra actions to execute when fetching the requested configuration value</param>
        /// <returns>The configuration value with key <paramref name="key"/></returns>
        T GetAppSetting<T>(string key, Action<IConfigurationOptions<T>> options = null, ConfigurationSettings settings = ConfigurationSettings.None);
        /// <summary>
        /// Fetches configuration value with key <paramref name="key"/> from the default app setting section.
        /// </summary>
        /// <param name="key">The key of the configuration value to fetch</param>
        /// <param name="options">Optional action for setting advanced actions that are to be executed when fetching the requested configuration value</param>
        /// <param name="settings">Optional settings that define extra actions to execute when fetching the requested configuration value</param>
        /// <returns>The configuration value with key <paramref name="key"/></returns>
        string GetAppSetting(string key, Action<IConfigurationOptions<string>> options = null, ConfigurationSettings settings = ConfigurationSettings.None);
        /// <summary>
        /// Fetches section with name <paramref name="section"/>.
        /// </summary>
        /// <typeparam name="T">The type that will be creating from the properties of the fetched section</typeparam>
        /// <param name="section">The name of the section to fetch</param>
        /// <param name="options">Optional action for setting advanced actions that are to be executed when fetching the requested configuration value</param>
        /// <param name="settings">Optional settings that define extra actions to execute when fetching the requested configuration value</param>
        /// <returns>The section with name <paramref name="section"/></returns>
        T GetSection<T>(string section, Action<IConfigurationOptions<T>> options = null, ConfigurationSettings settings = ConfigurationSettings.None);
        /// <summary>
        /// Fetches section with name <paramref name="section"/>.
        /// </summary>
        /// <param name="section">The name of the section to fetch</param>
        /// <param name="options">Optional action for setting advanced actions that are to be executed when fetching the requested configuration value</param>
        /// <param name="settings">Optional settings that define extra actions to execute when fetching the requested configuration value</param>
        /// <returns>The section with name <paramref name="section"/></returns>
        Dictionary<string, string> GetSection(string section, Action<IConfigurationOptions<Dictionary<string, string>>> options = null, ConfigurationSettings settings = ConfigurationSettings.None);
    }

    /// <summary>
    /// Gives more advanced settings when accessing application configuration.
    /// </summary>
    public interface IConfigurationOptions<T>
    {
        /// <summary>
        /// Sets the value that will be returned when the requested config value is missing. Is ignored when using <see cref="ConfigurationSettings.Required"/>.
        /// </summary>
        /// <param name="defaultValue">The default value to return</param>
        /// <returns>Current instance for method chaining</returns>
        IConfigurationOptions<T> SetDefault(T defaultValue);
        /// <summary>
        /// Defines a parent section that the requested value will be fetched from. The path will become: {section}:{key}.
        /// </summary>
        /// <param name="section">The name of the parent section to fetch the value from</param>
        /// <returns>Current instance for method chaining</returns>
        IConfigurationOptions<T> FromSection(string section);
        /// <summary>
        /// Defines parent sections that the requested value will be fetched from. The path will become: {section[0]}:{section[1]}:{section[2]}:{key}.
        /// </summary>
        /// <param name="sections">The parents sections to fetch the requested value from</param>
        /// <returns>Current instance for method chaining</returns>
        IConfigurationOptions<T> FromSection(params string[] sections);
        /// <summary>
        /// The fetched configuration value will be validated using <paramref name="condition"/>. The exception created from <paramref name="errorExceptionFunc"/> will be thrown when the value is invalid.
        /// </summary>
        /// <param name="condition">Delegate that checks if the fetched configuration value is valid</param>
        /// <param name="errorExceptionFunc">Delegate that creates the exception when <paramref name="condition"/> returns false</param>
        /// <returns>Current instance for method chaining</returns>
        IConfigurationOptions<T> ValidIf(Condition<(string Key, T Value, bool IsSection)> condition, Func<(string Key, T Value, bool IsSection), Exception> errorExceptionFunc);
        /// <summary>
        /// The fetched configuration value will be validated using <paramref name="condition"/>. An <see cref="MisconfiguredException"/> will be thrown using the created error message from <paramref name="errorMessageFunc"/>.
        /// </summary>
        /// <param name="condition">Delegate that checks if the fetched configuration value is valid</param>
        /// <param name="errorMessageFunc">Delegate that creates the error message when <paramref name="condition"/> returns false</param>
        /// <returns>Current instance for method chaining</returns>
        IConfigurationOptions<T> ValidIf(Condition<(string Key, T Value, bool IsSection)> condition, Func<(string Key, T Value, bool IsSection), string> errorMessageFunc);
    }

    /// <summary>
    /// Dictates what actions to execute when accessing application configuration.
    /// </summary>
    public enum ConfigurationSettings
    {
        /// <summary>
        /// No actions need to be executed.
        /// </summary>
        None = 0,
        /// <summary>
        /// An <see cref="ConfigurationMissingException"/> will be thrown when the requested configuration is missing.
        /// </summary>
        Required = 1
    }
}
