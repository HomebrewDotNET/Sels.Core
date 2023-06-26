﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sels.Core;
using Sels.Core.Components.Configuration;
using Sels.Core.Contracts.Configuration;
using Sels.Core.Extensions;
using Sels.DistributedLocking.IntegrationTester;
using SelsCommandLine = Sels.Core.Cli.CommandLine;
using Sels.Core.Extensions.Logging.Advanced;
using Sels.Core.Components.Logging;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sels.DistributedLocking.Provider;
using Sels.DistributedLocking.IntegrationTester.Providers;
using Sels.Core.Components.Scope;
using System.Diagnostics;
using Sels.DistributedLocking.IntegrationTester.Tests;

static IEnumerable<(TestProvider, ISetupProvider)> GetProviders(IServiceProvider serviceProvider, TestProvider testProviders)
{
    if (testProviders.HasFlag(TestProvider.Memory)) yield return (TestProvider.Memory, serviceProvider.GetRequiredService<MemorySetupProvider>());
    if (testProviders.HasFlag(TestProvider.MySql)) yield return (TestProvider.MySql, serviceProvider.GetRequiredService<MySqlSetupProvider>());
    if (testProviders.HasFlag(TestProvider.MariaDb)) yield return (TestProvider.MariaDb, serviceProvider.GetRequiredService<MariaDbSetupProvider>());
}

static IEnumerable<(TestType, ITester)> GetTesters(IServiceProvider serviceProvider, TestType types)
{
    if (types.HasFlag(TestType.Functional)) yield return (TestType.Functional, serviceProvider.GetRequiredService<AssertionTester>());    
    //if (types.HasFlag(TestType.Concurrency)) yield return (TestType.Concurrency, serviceProvider.GetRequiredService<MySqlSetupProvider>());
    //if (types.HasFlag(TestType.Benchmark)) yield return (TestType.Benchmark, serviceProvider.GetRequiredService<MariaDbSetupProvider>());
}


var exitCode = await SelsCommandLine.CreateAsyncTool<CliArguments>()
               .ConfigureServices((s, a) =>
               {
                   // Create configuration 
                   IConfiguration configuration;
                   if (a.ConfigDirectories.HasValue())
                   {
                       configuration = Helper.Configuration.BuildConfigurationFromDirectories(a.ConfigDirectories.Select(x => new DirectoryInfo(x)), x => x.Name.Contains("settings", StringComparison.OrdinalIgnoreCase), true);
                   }
                   else
                   {
                       configuration = Helper.Configuration.BuildConfigurationFromDirectory(x => x.Name.Contains("settings", StringComparison.OrdinalIgnoreCase), true);
                   }

                   // Add config service
                   var configurationService = new ConfigurationService(configuration);
                   s.AddSingleton(configuration);
                   s.AddSingleton<IConfigurationService, ConfigurationService>(x => configurationService);

                   // Load argument from config and merge
                   var configArguments = configurationService.GetSection<CliArguments>("TesterSettings");
                   if(configArguments != null) a.MergeFrom(configArguments);

                   // Add services so we can access it later
                   s.AddSingleton(s);

                   // Setup logging
                   s.AddLogging(x =>
                   {
                       x.SetMinimumLevel(a.Debug ? LogLevel.Trace : a.Verbose ? LogLevel.Information : LogLevel.Warning)
                       .AddConsole(c =>
                       {
                           c.LogToStandardErrorThreshold = LogLevel.Trace;
                       })
                       .AddSimpleConsole(c =>
                       {
                           //c.SingleLine = true;
                       });
                   });

                   // Providers
                   s.AddScoped<MemorySetupProvider>();
                   s.AddScoped<MySqlSetupProvider>();
                   s.AddScoped<MariaDbSetupProvider>();

                   // Testers
                   s.AddScoped<AssertionTester>();
                   s.AddOptions<AssertionTesterOptions>();
                   s.AddValidationProfile<AssertionTesterOptionsValidationProfile, string>();
                   s.AddOptionProfileValidator<AssertionTesterOptions, AssertionTesterOptionsValidationProfile>();
                   s.BindOptionsFromConfig<AssertionTesterOptions>();
               })
               .Execute(async (p, a, t) =>
               {
                   var logger = p.GetService<ILogger<Program>>();
                   var services = p.GetRequiredService<IServiceCollection>();

                   logger.Log($"Preparing the test providers");
                   var provider = a.Provider ?? TestProvider.All;
                   var testType = a.Type ?? TestType.Functional;
                   List<Exception> exceptions = new List<Exception>();

                   foreach (var (type, tester) in GetTesters(p, testType))
                   {
                       foreach (var (providerKey, testProvider) in GetProviders(p, provider))
                       {
                           try
                           {
                               logger.Log($"Setting up provider <{providerKey}> for next test run");

                               var providerServices = new ServiceCollection();
                               foreach (var service in services)
                               {
                                   providerServices.Add(service);
                               }
                               AsyncWrapper<ILockingProvider> setupProvider = null;
                               try
                               {
                                   setupProvider = (await testProvider.SetupProvider(providerServices, t)) ?? throw new InvalidOperationException($"Provider <{nameof(providerKey)}> did not return a locking provider");
                               }
                               catch (Exception ex)
                               {
                                   logger.Log($"Could not setup provider <{providerKey}>", ex);
                                   exceptions.Add(ex);
                                   continue;
                               }

                               try
                               {
                                   await using (setupProvider)
                                   {
                                       logger.Log($"Executing test <{testType}> for provider <{providerKey}>");
                                       await tester.RunTests(providerKey, setupProvider.Value, t);
                                   }
                               }
                               catch (Exception ex)
                               {
                                   logger.Log($"Could not execute test <{testType}> for provider <{providerKey}>", ex);
                                   exceptions.Add(ex);
                               }
                           }
                           catch(Exception ex)
                           {
                               logger.Log($"Unhandled error occured for tester <{type}> for provider <{providerKey}>", ex);
                               exceptions.Add(ex);
                           }                           
                       }
                   }

                   
                   if (exceptions.HasValue()) throw new AggregateException(exceptions);

                   logger.Log($"Tests executed. Tool stopping");
               })
               .RunAsync(args);

if(Debugger.IsAttached && Environment.UserInteractive)
{
    Console.WriteLine($"Press any key to close");
    Console.ReadLine();
}

return exitCode;

