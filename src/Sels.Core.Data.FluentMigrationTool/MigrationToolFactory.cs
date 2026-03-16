using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Data.FluentMigrationTool
{
    /// <inheritdoc cref="IMigrationToolFactory"/>
    public class MigrationToolFactory : IMigrationToolFactory
    {
        // Fields
        private readonly IServiceCollection _services;
        private readonly ILoggerFactory _loggerFactory;

        /// <inheritdoc cref="MigrationToolFactory"/>
        /// <param name="services">Service collection used to transfer registration to the tool if self contained is not enabled</param>
        /// <param name="loggerFactory">Optional factory to create the loggers for the migration tools</param>
        public MigrationToolFactory(IServiceCollection services, ILoggerFactory loggerFactory = null)
        {
            _services = services.ValidateArgument(nameof(services));
            _loggerFactory = loggerFactory;
        }

        /// <inheritdoc/>
        public IMigrationToolRootBuilder Create(bool selfContained = false)
        {
            var tool = new MigrationTool(_loggerFactory?.CreateLogger<MigrationTool>());
            if(!selfContained) tool.CastTo<IMigrationToolBuilder>().InheritFrom(_services);
            return tool;
        }
    }
}
