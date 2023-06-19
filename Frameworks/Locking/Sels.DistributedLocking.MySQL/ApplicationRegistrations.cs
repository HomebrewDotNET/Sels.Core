using Sels.Core.Extensions;
using Sels.DistributedLocking.Provider;
using Sels.DistributedLocking.SQL;
using Sels.Core.ServiceBuilder;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Sels.DistributedLocking.MySQL.Options;
using Sels.DistributedLocking.MySQL.Repository;
using Sels.Core.Contracts.Configuration;
using Microsoft.Extensions.Options;
using Sels.SQL.QueryBuilder;
using Sels.Core.Data.FluentMigrationTool;
using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using Sels.SQL.QueryBuilder.Builder.Compilation;
using Sels.SQL.QueryBuilder.MySQL;
using Microsoft.Extensions.Caching.Memory;
using Sels.SQL.QueryBuilder.Builder;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Contains extension methods for registering services.
    /// </summary>
    public static class ApplicationRegistrations
    {
        /// <summary>
        /// Adds all required services for placing distributed locks with <see cref="ILockingProvider"/> using a MySql database. Provides distributed locking for all applications using the same database.
        /// </summary>
        /// <param name="services">Collection to add the service registration to</param>
        /// <param name="configurator">Optional delegate for configuring the registration options</param>
        /// <param name="overwrite">True to overwrite previous registrations, otherwise false</param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        public static IServiceCollection AddMySqlLockingProvider(this IServiceCollection services, Action<IMySqlLockingProviderRegistrationOptions> configurator = null, bool overwrite = true)
        {
            services.ValidateArgument(nameof(services));
            var registrationOptions = new RegistrationOptions(configurator);

            // Add options
            services.AddOptions();

            // Required for config
            services.RegisterConfigurationService();

            // Required for migrations
            services.AddMigrationToolFactory();

            // Add configuration handler
            services.BindOptionsFromConfig<MySqlLockRepositoryDeploymentOptions>();

            // Add option validator
            services.AddValidationProfile<MySqlLockRepositoryDeploymentOptionsValidationProfile, string>(ServiceLifetime.Singleton);
            services.AddOptionProfileValidator<SqlLockingProviderOptions, MySqlLockRepositoryDeploymentOptionsValidationProfile>();

            // Configure deployment options
            if(registrationOptions.DeploymentConfigurator != null) services.Configure(registrationOptions.DeploymentConfigurator);

            // Add Sql Locking provider
            services.AddSqlLockingProvider(registrationOptions.ProviderConfigurator, overwrite);
            services.AddCachedMySqlQueryProvider(registrationOptions.CacheOptions, ExpressionCompileOptions.AppendSeparator, overwrite);

            // Add repository
            if (registrationOptions.UseMariaDbRepository)
            {
                services.New<ISqlLockRepository, MariaDbLockRepository>()
                        .ConstructWith(p =>
                        {
                            var connectionString = registrationOptions.ConnectionStringFactory(p);
                            return new MariaDbLockRepository(connectionString,
                                                            p.GetRequiredService<IOptions<MySqlLockRepositoryDeploymentOptions>>(),
                                                            p.GetRequiredService<ICachedSqlQueryProvider>(),
                                                            p.GetRequiredService<IMigrationToolFactory>(),
                                                            p.GetService<ILogger<MariaDbLockRepository>>());
                        })
                        .Trace(x => x.Duration.OfAll)
                        .AsSingleton()
                        .WithBehaviour(overwrite ? services.IsReadOnly ? RegisterBehaviour.Default : RegisterBehaviour.Replace : RegisterBehaviour.TryAdd)
                        .Register();
            }
            else
            {
                services.New<ISqlLockRepository, MySqlLockRepository>()
                        .ConstructWith(p =>
                        {
                            var connectionString = registrationOptions.ConnectionStringFactory(p);
                            return new MySqlLockRepository(connectionString,
                                                            p.GetRequiredService<IOptions<MySqlLockRepositoryDeploymentOptions>>(),
                                                            p.GetRequiredService<ICachedSqlQueryProvider>(),
                                                            p.GetRequiredService<IMigrationToolFactory>(),
                                                            p.GetService<ILogger<MySqlLockRepository>>());
                        })
                        .Trace(x => x.Duration.OfAll)
                        .AsSingleton()
                        .WithBehaviour(overwrite ? services.IsReadOnly ? RegisterBehaviour.Default : RegisterBehaviour.Replace : RegisterBehaviour.TryAdd)
                        .Register();
            }

            return services;
        }

        private class RegistrationOptions : IMySqlLockingProviderRegistrationOptions
        {
            // Properties
            public Action<MySqlLockRepositoryDeploymentOptions> DeploymentConfigurator { get; private set; }
            public Action<SqlLockingProviderOptions> ProviderConfigurator { get; private set; }
            public Action<MemoryCacheEntryOptions> CacheOptions { get; private set; }
            public Func<IServiceProvider, string> ConnectionStringFactory { get; private set; }
            public bool UseMariaDbRepository { get; private set; }

            public RegistrationOptions(Action<IMySqlLockingProviderRegistrationOptions> configurator)
            {
                // Defaults
                UseConnectionString(x => x.GetRequiredService<IConfigurationService>().GetConnectionString("MySql"));

                // Configure self
                configurator?.Invoke(this);
            }

            /// <inheritdoc/>
            public IMySqlLockingProviderRegistrationOptions ConfigureDeployment(Action<MySqlLockRepositoryDeploymentOptions> configurator)
            {
                DeploymentConfigurator = configurator.ValidateArgument(nameof(configurator));
                return this;
            }
            /// <inheritdoc/>
            public IMySqlLockingProviderRegistrationOptions ConfigureProvider(Action<SqlLockingProviderOptions> configurator)
            {
                ProviderConfigurator = configurator.ValidateArgument(nameof(configurator));
                return this;
            }
            /// <inheritdoc/>
            public IMySqlLockingProviderRegistrationOptions UseConnectionString(Func<IServiceProvider, string> connectionStringFactory)
            {
                ConnectionStringFactory = connectionStringFactory.ValidateArgument(nameof(connectionStringFactory));
                return this;
            }
            /// <inheritdoc/>
            public IMySqlLockingProviderRegistrationOptions UseMariaDb()
            {
                UseMariaDbRepository = true;
                return this;
            }
            /// <inheritdoc/>
            public IMySqlLockingProviderRegistrationOptions UseQueryCacheOptions(Action<MemoryCacheEntryOptions> cacheOptions)
            {
                CacheOptions = cacheOptions.ValidateArgument(nameof(cacheOptions));
                return this;
            }
        }
    }

    /// <summary>
    /// Exposes extra options when adding a MySql locking provider to a <see cref="IServiceCollection"/>.
    /// </summary>
    public interface IMySqlLockingProviderRegistrationOptions
    {
        /// <summary>
        /// Configures the options for the Sql <see cref="ILockingProvider"/>.
        /// </summary>
        /// <param name="configurator">Delegate for configuring the options</param>
        /// <returns>Current options for method chaining</returns>
        IMySqlLockingProviderRegistrationOptions ConfigureProvider(Action<SqlLockingProviderOptions> configurator);
        /// <summary>
        /// Configures the options for the MySql <see cref="ISqlLockRepository"/>.
        /// </summary>
        /// <param name="configurator">Delegate for configuring the options</param>
        /// <returns>Current options for method chaining</returns>
        IMySqlLockingProviderRegistrationOptions ConfigureDeployment(Action<MySqlLockRepositoryDeploymentOptions> configurator);
        /// <summary>
        /// Configures the options for caching of queries.
        /// </summary>
        /// <param name="cacheOptions">Delegate for configuring the options</param>
        /// <returns>Current options for method chaining</returns>
        IMySqlLockingProviderRegistrationOptions UseQueryCacheOptions(Action<MemoryCacheEntryOptions> cacheOptions);

        /// <summary>
        /// Use a <see cref="ISqlLockRepository"/> optimized for mariaDb.
        /// </summary>
        /// <returns>Current options for method chaining</returns>
        IMySqlLockingProviderRegistrationOptions UseMariaDb();
        /// <summary>
        /// Defines a delegate that returns the connection string for the lock repository.
        /// </summary>
        /// <param name="connectionStringFactory">Delegate that returns the connection string for the lock repository</param>
        /// <returns>Current options for method chaining</returns>
        IMySqlLockingProviderRegistrationOptions UseConnectionString(Func<IServiceProvider, string> connectionStringFactory);
        /// <summary>
        /// Defines the connection string for the lock repository.
        /// </summary>
        /// <param name="connectionString">The connection string for the lock repository</param>
        /// <returns>Current options for method chaining</returns>
        IMySqlLockingProviderRegistrationOptions UseConnectionString(string connectionString) => UseConnectionString(x => connectionString.ValidateArgumentNotNullOrWhitespace(nameof(connectionString)));
    }
}
