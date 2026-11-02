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
        private readonly string _parentSectionName;

        /// <inheritdoc cref="OptionConfigurationProvider{TOptions}"/>
        /// <param name="configuration">Used to access the application configuration</param>
        /// <param name="nameBehaviour"><inheritdoc cref="ConfigurationProviderNamedOptionBehaviour"/></param>
        /// <param name="fallbackOnDefault">Set to true to fallback to <paramref name="sectionName"/> if no configuration section exists for the name</param>
        /// <param name="sectionName">Optional root/parent name to bind from. When not set the type name will be used as the section name</param>
        /// <param name="parentSectionName">Optional parent section to bind from. <paramref name="sectionName"/> will be used as sub section and this section will become the root. Meaning <paramref name="nameBehaviour"/> applies to it and not <paramref name="sectionName"/></param>
        public OptionConfigurationProvider(IConfiguration configuration, ConfigurationProviderNamedOptionBehaviour nameBehaviour = ConfigurationProviderNamedOptionBehaviour.SubSection, bool fallbackOnDefault = true, string sectionName = null, string parentSectionName = null)
        {
            _configuration = configuration.ValidateArgument(nameof(configuration));
            _nameBehaviour = nameBehaviour;
            _fallbackOnDefault = fallbackOnDefault;
            _sectionName = sectionName.HasValue() ? sectionName : typeof(TOptions).Name;
            _parentSectionName = parentSectionName;
        }

        /// <inheritdoc/>
        public void Configure(TOptions options)
        {
            options = options.ValidateArgument(nameof(options));
            var root = _parentSectionName.HasValue() ? _parentSectionName : _sectionName;

            var section = _configuration.GetSection(root);
            if (!section.Exists()) return;
            if (_parentSectionName.HasValue()) {
                section = section.GetSection(_sectionName);
            }
            if (!section.Exists()) return;
            section.Bind(options, x => x.ErrorOnUnknownConfiguration = false);
        }
        /// <inheritdoc/>
        public void Configure(string name, TOptions options)
        {
            options = options.ValidateArgument(nameof(options));
            if (!name.HasValue())
            {
                Configure(options);
                return;
            }
            string sectionName = null;
            bool ignoreErrors = true;
            var root = _parentSectionName.HasValue() ? _parentSectionName : _sectionName;
            switch (_nameBehaviour)
            {
                case ConfigurationProviderNamedOptionBehaviour.Ignore:
                    Configure(options);
                    return;
                case ConfigurationProviderNamedOptionBehaviour.Prefix:
                    sectionName = $"{name}.{root}";
                    ignoreErrors = true;
                    break;
                case ConfigurationProviderNamedOptionBehaviour.Suffix:
                    sectionName = $"{root}.{name}";
                    ignoreErrors = true;
                    break;
                case ConfigurationProviderNamedOptionBehaviour.SubSection:
                    sectionName = $"{root}:{name}";
                    break;
                default:
                    throw new NotSupportedException($"Behaviour <{_nameBehaviour}> is not known");
            }

            var section = _configuration.GetSection(sectionName);
            if (!section.Exists() && !_fallbackOnDefault) { 
                return;
            }
            if (_parentSectionName.HasValue())
            {
                section = section.GetSection(_sectionName);
            }
            if (!section.Exists() && !_fallbackOnDefault)
            {
                return;
            }

            Configure(options);
            section.Bind(options, x => x.ErrorOnUnknownConfiguration = !ignoreErrors);
        }
    }
}
