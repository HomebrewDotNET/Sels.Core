using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sels.Core.Scope;
using Sels.Core.Configuration;
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
using Sels.Core.Extensions.Logging;

namespace Sels.DistributedLocking.IntegrationTester.Providers
{
    /// <summary>
    /// Sets up Sels.DistributedLocking.MySql.
    /// </summary>
    public class MySqlSetupProvider : ISetupProvider
    {
        // Fields
        /// <summary>
        /// Optiona logger for tracing.
        /// </summary>
        protected readonly ILogger? _logger;

        /// <inheritdoc cref="MySqlSetupProvider"/>
        /// <param name="logger"><inheritdoc cref="_logger"/></param>
        protected MySqlSetupProvider(ILogger? logger = null)
        {
            _logger = logger;
        }

        /// <inheritdoc cref="MySqlSetupProvider"/>
        /// <param name="logger"><inheritdoc cref="_logger"/></param>
        public MySqlSetupProvider(Logger<MySqlSetupProvider>? logger = null) : this(logger.CastToOrDefault<ILogger>())
        {
            
        }

        /// <inheritdoc/>
        public async Task<AsyncWrapper<ILockingProvider>> SetupProvider(IServiceCollection services, CancellationToken token)
        {
            var provider = BuildProvider(services);
            var scope = provider.CreateAsyncScope();

            var lockingProvider = scope.ServiceProvider.GetRequiredService<ILockingProvider>();
            var repository = scope.ServiceProvider.GetRequiredService<ISqlLockRepository>();

            var wrapper = new AsyncWrapper<ILockingProvider>(lockingProvider, async (p, t) => await SetupForTestRun(p, repository, t), async (p, t) => {
                await scope.DisposeAsync();
                await provider.DisposeAsync();
            });
            await wrapper.StartAsync(token);
            return wrapper;
        }

        /// <summary>
        /// Initializes the service provider to resolve the locking provider.
        /// </summary>
        /// <param name="services">The service collection to use to build the service provider</param>
        /// <returns>The service provider to resolve the locking provider</returns>
        protected virtual ServiceProvider BuildProvider(IServiceCollection services) => services.AddMySqlLockingProvider(x => x.ConfigureDeployment(c => c.IgnoreMigrationExceptions = false).ConfigureProvider(p => { })).BuildServiceProvider();

        /// <summary>
        /// Setup the environment for a next test run.
        /// </summary>
        /// <param name="lockingProvider">The locking provider that will be used for the test run</param>
        /// <param name="sqlLockRepository">The backing lock repository for <paramref name="lockingProvider"/></param>
        /// <returns>Task containing the execution state</returns>
        protected virtual async Task SetupForTestRun(ILockingProvider lockingProvider, ISqlLockRepository sqlLockRepository, CancellationToken token)
        {
            _logger.Log($"Clearing lock tables for next test run");

            await using (var transaction = await sqlLockRepository.CreateTransactionAsync(token))
            {
                await sqlLockRepository.ClearAllAsync(transaction, token);

                await transaction.CommitAsync(token);
            }

            _logger.Log($"Lock tables cleared for next test run");
        }
    }
}
