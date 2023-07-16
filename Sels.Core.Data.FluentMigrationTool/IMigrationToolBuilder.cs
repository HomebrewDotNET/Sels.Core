using FluentMigrator;
using FluentMigrator.Runner;
using FluentMigrator.Runner.VersionTableInfo;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Data.FluentMigrationTool
{
    /// <summary>
    /// Builder for creating a tool to deploy database schema's
    /// </summary>
    public interface IMigrationToolRootBuilder
    {
        /// <summary>
        /// Configures the migration runner.
        /// </summary>
        /// <param name="action">The delegate that configures the migration runner</param>
        /// <returns>Current builder for method chaining</returns>
        IMigrationToolBuilder ConfigureRunner(Action<IMigrationRunnerBuilder> action);
    }
    /// <summary>
    /// Builder for creating a tool to deploy database schema's
    /// </summary>
    public interface IMigrationToolBuilder : IMigrationToolRootBuilder
    {
        /// <summary>
        /// Configures the service collection for the current tool.
        /// </summary>
        /// <param name="action">Delegate that configures the service collection</param>
        /// <returns>Current builder for method chaining</returns>
        IMigrationToolBuilder ConfigureServices(Action<IServiceCollection> action);

        /// <summary>
        /// Copies the services registrations from <paramref name="services"/> to the service collection of the tool.
        /// </summary>
        /// <param name="services">The service collection to inherit from</param>
        /// <returns>Current builder for method chaining</returns>
        IMigrationToolBuilder InheritFrom(IServiceCollection services) => ConfigureServices(x => x.Add(services.ValidateArgument(nameof(services))));
        /// <summary>
        /// Adds all migrations from the assembly where <typeparamref name="T"/> is located.
        /// </summary>
        /// <typeparam name="T">The type of the migration to get the assembly from</typeparam>
        /// <returns>Current builder for method chaining</returns>
        IMigrationToolBuilder AddMigrationsFrom<T>() where T : IMigration => ConfigureRunner(x => x.ScanIn(typeof(T).Assembly).For.Migrations());
        /// <summary>
        /// Overwrites the default version table metadata with <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the version table meta data to use</typeparam>
        /// <returns>Current builder for method chaining</returns>
        IMigrationToolBuilder UseVersionTableMetaData<T>() where T : class, IVersionTableMetaData => ConfigureServices(x => x.AddScoped<IVersionTableMetaData, T>());
        /// <summary>
        /// Overwrites the default version table metadata with <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the version table meta data to use</typeparam>
        /// <param name="factory">The delegate that creates the instance of <typeparamref name="T"/></param>
        /// <returns>Current builder for method chaining</returns>
        IMigrationToolBuilder UseVersionTableMetaData<T>(Func<IServiceProvider, T> factory) where T : class, IVersionTableMetaData => ConfigureServices(x => x.AddScoped<IVersionTableMetaData, T>(factory.ValidateArgument(nameof(factory))));
        /// <summary>
        /// Overwrites the default version table metadata with <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the version table meta data to use</typeparam>
        /// <param name="factory">The delegate that creates the instance of <typeparamref name="T"/></param>
        /// <returns>Current builder for method chaining</returns>
        IMigrationToolBuilder UseVersionTableMetaData<T>(Func<T> factory) where T : class, IVersionTableMetaData => ConfigureServices(x => x.AddScoped<IVersionTableMetaData, T>(x => factory.ValidateArgument(nameof(factory))()));

        /// <summary>
        /// Executes the current migration tool to deploy database changes.
        /// </summary>
        /// <param name="version">Option version to deploy, null means deploy to latest version</param>
        void Deploy(long? version = null);
        /// <summary>
        /// Executes the current migration tool to rollback to a certain version.
        /// </summary>
        /// <param name="version">The version to rollback to, null means rollback all changes (version 0)</param>
        void Rollback(long? version = null);
    }
}
