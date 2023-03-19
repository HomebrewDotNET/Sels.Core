using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sels.Core.Cli;
using Sels.Core.Components.Logging;
using System.Reflection;
using SelsCommandLine = Sels.Core.Cli.CommandLine;

namespace Sels.Core.Data.MySQL.Migrator
{
    /// <summary>
    /// Cli for running database migration using Fluent Migrator
    /// </summary>
    public static class MigrationTool
    {
        /// <summary>
        /// Runs all migration located in <paramref name="migrationAssembly"/> and optionally <paramref name="migrationAssemblies"/>.
        /// </summary>
        /// <param name="args">The command line arguments. Will be parsed to <see cref="MigratorArguments"/></param>
        /// <param name="lockerName">The name that will be used to lock the database. Use unique names when running multiple instances</param>
        /// <param name="migrationAssembly">Assembly containing the migrations</param>
        /// <param name="migrationAssemblies">Optional additional assemblies containing migrations</param>
        /// <returns>The status code of the cli</returns>
        public static Task<int> RunAsync(string[] args, string lockerName, Assembly migrationAssembly, params Assembly[] migrationAssemblies)
        {
            if (args == null || !args.HasValue()) throw new CommandLineException(2, "No command line argument provided");
            lockerName.ValidateArgumentNotNullOrWhitespace(nameof(lockerName));
            migrationAssembly.ValidateArgument(nameof(migrationAssembly));
            migrationAssemblies.ValidateArgumentNotNullOrEmpty(nameof(migrationAssemblies));

            return RunAsync<MigratorArguments>(args, lockerName, null, migrationAssembly, migrationAssemblies);
        }

        /// <summary>
        /// Runs all migration located in <paramref name="migrationAssembly"/> and optionally <paramref name="migrationAssemblies"/>.
        /// </summary>
        /// <typeparam name="TArg">The type to parse <paramref name="args"/> to</typeparam>
        /// <param name="args">The command line arguments. Will be parsed to <typeparamref name="TArg"/></param>
        /// <param name="lockerName">The name that will be used to lock the database. Use unique names when running multiple instances</param>
        /// <param name="onParsed">Optional delegate that is triggered after <paramref name="args"/> was parsed to an instance of <typeparamref name="TArg"/></param>
        /// <param name="migrationAssembly">Assembly containing the migrations</param>
        /// <param name="migrationAssemblies">Optional additional assemblies containing migrations</param>
        /// <returns>The status code of the cli</returns>
        public async static Task<int> RunAsync<TArg>(string[] args, string lockerName, Action<TArg>? onParsed, Assembly migrationAssembly, params Assembly[] migrationAssemblies) where TArg : MigratorArguments, new()
        {
            if (args == null || !args.HasValue()) throw new CommandLineException(2, "No command line argument provided");
            lockerName.ValidateArgumentNotNullOrWhitespace(nameof(lockerName));
            migrationAssembly.ValidateArgument(nameof(migrationAssembly));

            return await SelsCommandLine.CreateAsyncTool<TArg>()
                               .RegisterServices((services, args) =>
                               {
                                   if (args == null) throw new CommandLineException(SelsCommandLine.InvalidArgumentsExitCode, "No command line argument provided");

                                   var assemblies = Helper.Collection.Enumerate(migrationAssembly, migrationAssemblies).Where(x => x != null);

                                   // Add logging
                                   services.AddLogging(x =>
                                   {
                                       x.SetMinimumLevel(args.Debug ? LogLevel.Trace : args.Verbose ? LogLevel.Information : LogLevel.Warning)
                                       .AddConsole(c =>
                                       {
                                           c.LogToStandardErrorThreshold = LogLevel.Trace;
                                       })
                                       .AddSimpleConsole(c =>
                                       {
                                           c.SingleLine = true;
                                       });
                                   });

                                   // Setup migrator
                                   services
                                        .AddFluentMigratorCore()
                                        .ConfigureRunner(rb =>
                                            rb.WithGlobalConnectionString(args.ConnectionString).AddMySql5().ScanIn(Helper.Collection.Enumerate(Assembly.GetExecutingAssembly(), assemblies).ToArray())
                                        );
                               })
                               .Execute(async (provider, args, token) =>
                               {
                                   if (args == null) throw new CommandLineException(SelsCommandLine.InvalidArgumentsExitCode, "No command line argument provided");

                                   // Trigger on parsed
                                   if (onParsed != null) onParsed(args);

                                   // Setup logging
                                   var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("Sels.Core.Data.MySQL.Migrator.MigrationRunner");
                                   LoggingServices.RegisterLogger(logger);

                                   // Create database if it does not yet exist
                                   Console.WriteLine("Creating database if it does not yet exists");
                                   await MySql.Database.EnsureDatabaseExistsAsync(args.ConnectionString, token);
                                   // Wait for exclusive lock on database and run migrations while lock is held
                                   Console.WriteLine($"Waiting for exclusive lock on database");
                                   await using (await MySql.Database.Locking.LockAsync(args.ConnectionString, "DatabaseDeployer", lockerName, 10, logger.AsEnumerable(), token))
                                   {
                                       // Run migrations
                                       var runner = provider.GetRequiredService<IMigrationRunner>();
                                       Console.WriteLine("Deploying schema changes to database");
                                       runner.MigrateUp();
                                       Console.WriteLine("Succesfully deployed migrations");
                                   }                                 
                               })
                               .RunAsync(args);
        }

    } 
}
