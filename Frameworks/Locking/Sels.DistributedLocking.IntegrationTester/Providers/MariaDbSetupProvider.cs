using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sels.Core.Components.Scope;
using Sels.Core.Contracts.Configuration;
using Sels.Core.Extensions.Conversion;
using Sels.DistributedLocking.MySQL.Options;
using Sels.DistributedLocking.Provider;
using Sels.DistributedLocking.SQL;
using Sels.SQL.QueryBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sels.Core.Extensions.Logging.Advanced;

namespace Sels.DistributedLocking.IntegrationTester.Providers
{
    /// <summary>
    /// Sets up Sels.DistributedLocking.MySql.
    /// </summary>
    public class MariaDbSetupProvider : MySqlSetupProvider
    {
        /// <inheritdoc cref="MariaDbSetupProvider"/>
        /// <param name="logger"><inheritdoc cref="_logger"/></param>
        public MariaDbSetupProvider(Logger<MariaDbSetupProvider>? logger = null) : base(logger.CastToOrDefault<ILogger>())
        {

        }

        /// <inheritdoc/>
        protected override ServiceProvider BuildProvider(IServiceCollection services) => services.AddMySqlLockingProvider(x => x.ConfigureDeployment(c => c.IgnoreMigrationExceptions = false).UseMariaDb().ConfigureProvider(p => p.RequestPollingRate = 500)).BuildServiceProvider();
    }
}
