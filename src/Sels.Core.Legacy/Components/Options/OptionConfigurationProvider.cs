using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Options
{
    /// <summary>
    /// Custom options configurator that binds instances of <typeparamref name="TOptions"/> from <see cref="IConfiguration"/>.
    /// </summary>
    /// <typeparam name="TOptions">The type of the option to bind from config</typeparam>
    public class OptionConfigurationProvider<TOptions> : IConfigureOptions<TOptions>, IConfigureNamedOptions<TOptions> where TOptions : class
    {
        // Fields
        private readonly ConfigurationProviderNamedOptionBehaviour _nameBehaviour;
        private readonly bool _fallbackOnDefault;
        private readonly IConfiguration _configuration;
        private readonly string _sectionName;

        /// <inheritdoc cref="OptionConfigurationProvider{TOptions}"/>
        /// <param name="configuration">Used to access the application configuration</param>
        /// <param name="nameBehaviour"><inheritdoc cref="ConfigurationProviderNamedOptionBehaviour"/></param>
        /// <param name="fallbackOnDefault">Set to true to fallback to <paramref name="sectionName"/> if no configuration section exists for the name</param>
        /// <param name="sectionName">Optional section name to bind from. When not set the type name will be used as the section name</param>
        public OptionConfigurationProvider(IConfiguration configuration, ConfigurationProviderNamedOptionBehaviour nameBehaviour = ConfigurationProviderNamedOptionBehaviour.SubSection, bool fallbackOnDefault = true, string sectionName = null)
        {
            _configuration = configuration.ValidateArgument(nameof(configuration));
            _nameBehaviour = nameBehaviour;
            _fallbackOnDefault = fallbackOnDefault;
            _sectionName = sectionName.HasValue() ? sectionName : typeof(TOptions).Name;
        }

        /// <inheritdoc/>
        public void Configure(TOptions options)
        {
            options = options.ValidateArgument(nameof(options));
            var section = _configuration.GetSection(_sectionName);
            if (!section.Exists()) return;
            section.Bind(options, x => x.ErrorOnUnknownConfiguration = false);
        }
        /// <inheritdoc/>
        public void Configure(string name, TOptions options)
        {
            options = options.ValidateArgument(nameof(options));
            string sectionName = null;
            bool ignoreErrors = true;
            switch(_nameBehaviour)
            {
                case ConfigurationProviderNamedOptionBehaviour.Ignore:
                    Configure(options);
                    return;
                case ConfigurationProviderNamedOptionBehaviour.Prefix:
                    sectionName = $"{name}.{_sectionName}";
                    ignoreErrors = true;
                    break;
                case ConfigurationProviderNamedOptionBehaviour.Suffix:
                    sectionName = $"{_sectionName}.{name}";
                    ignoreErrors = true;
                    break;
                case ConfigurationProviderNamedOptionBehaviour.SubSection:
                    sectionName = $"{_sectionName}:{name}";
                    break;
                default:
                    throw new NotSupportedException($"Behaviour <{_nameBehaviour}> is not known");
            }

            var section = _configuration.GetSection(sectionName);
            if (!section.Exists() && !_fallbackOnDefault) { 
                return;
            }

            Configure(options);
            section.Bind(options, x => x.ErrorOnUnknownConfiguration = !ignoreErrors);
        }
    }
}
