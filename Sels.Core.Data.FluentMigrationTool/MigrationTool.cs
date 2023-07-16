using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Logging.Advanced;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Data.FluentMigrationTool
{
    /// <summary>
    /// Migration tool for deploying database changes.
    /// </summary>
    public class MigrationTool : IMigrationToolBuilder
    {
        // Fields
        private Action<IMigrationRunnerBuilder> _runnerBuilderAction = new Action<IMigrationRunnerBuilder>(x => { });
        private Action<IServiceCollection> _collectionBuilderActions = new Action<IServiceCollection>(x => { });
        private readonly ILogger _logger;

        /// <inheritdoc cref="MigrationTool"/>
        /// <param name="logger">Optional logger for tracing</param>
        public MigrationTool(ILogger<MigrationTool> logger = null)
        {
            _logger = logger;
        }

        /// <inheritdoc/>
        public IMigrationToolBuilder ConfigureRunner(Action<IMigrationRunnerBuilder> action)
        {
            action.ValidateArgument(nameof(action));
            _runnerBuilderAction += action;
            return this;
        }
        /// <inheritdoc/>
        public IMigrationToolBuilder ConfigureServices(Action<IServiceCollection> action)
        {
            action.ValidateArgument(nameof(action));
            _collectionBuilderActions += action;
            return this;
        }

        /// <inheritdoc/>
        public void Deploy(long? version = null)
        {
            using (_logger.TraceMethod(this))
            {
                Run(version, false);
            }
        }
        /// <inheritdoc/>
        public void Rollback(long? version = null)
        {
            using (_logger.TraceMethod(this))
            {
                Run(version, true);
            }
        }

        private void Run(long? version, bool isRollback) 
        {
            _logger.Log($"Migration tool starting up");
            _logger.Debug($"Creating service collection");
            var serviceCollection = new ServiceCollection()
                                    .AddFluentMigratorCore()
                                    .ConfigureRunner(x => _runnerBuilderAction(x));
            _collectionBuilderActions(serviceCollection);
            _logger.Debug($"Building service provider");
            
            using (var provider = serviceCollection.BuildServiceProvider())
            {
                _logger.Debug($"Resolving migration runner");
                var runner = provider.GetRequiredService<IMigrationRunner>();

                if (!isRollback)
                {
                    _logger.Log($"Deploying database schema to {(version.HasValue ? $"version <{version.Value}>" : "latest version")}");
                    if (!version.HasValue) runner.MigrateUp();
                    else runner.MigrateUp(version.Value);
                    _logger.Log($"Deployed database schema to {(version.HasValue ? $"version <{version.Value}>" : "latest version")}");
                }
                else
                {
                    if (version.HasValue)
                    {
                        _logger.Log($"Rolling back database schema to version <{version.Value}>");
                        runner.MigrateDown(version.Value);
                        _logger.Log($"Rolled back database schema to version <{version.Value}>");
                    }
                    else
                    {
                        _logger.Log($"Rolling back full database schema");
                        runner.MigrateDown(0);
                        _logger.Log($"Fully rollbacked database schema");
                    }

                }
            }
        }
    }
}
