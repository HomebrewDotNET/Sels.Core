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
    public class OptionConfigurationProvider<TOptions> : IConfigureOptions<TOptions> where TOptions : class
    {
        // Fields
        private readonly IConfiguration _configuration;
        private readonly string _sectionName;

        /// <inheritdoc cref="OptionConfigurationProvider{TOptions}"/>
        /// <param name="configuration">Used to access the application configuration</param>
        /// <param name="sectionName">Optional section name to bind from. When not set the type name will be used as the section name</param>
        public OptionConfigurationProvider(IConfiguration configuration, string sectionName = null)
        {
            _configuration = configuration.ValidateArgument(nameof(configuration));
            _sectionName = sectionName.HasValue() ? sectionName : typeof(TOptions).Name;
        }

        /// <inheritdoc/>
        public void Configure(TOptions options)
        {
            var section = _configuration.GetSection(_sectionName);
            if (!section.Exists()) return;
            section.Bind(options);
        }
    }
}
