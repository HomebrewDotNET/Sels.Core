using System;
using Microsoft.Extensions.Logging;
using Sels.Core.Components.Logging;
using Sels.Core.Extensions.Logging;
using Sels.Core.Extensions.Logging.Advanced;
using Microsoft.Extensions.DependencyInjection;
using Sels.Core.Mediator.Messaging;
using System.Threading.Tasks;
using Sels.Core.Contracts.Configuration;
using Sels.Core.Components.Configuration;
using Sels.Core.Deployment;
using Sels.Core.Data.MySQL.Models;
using Sels.Core.Conversion.Templates;
using Sels.Core.Conversion.Converters.Simple;
using System.Reflection.Emit;
using Autofac.Extensions.DependencyInjection;
using Autofac;
using Sels.Core.Conversion.Converters;
using Sels.Core.Contracts.Factory;
using Sels.Core.Factory;
using Sels.DistributedLocking.Provider;
using Sels.Core.Extensions.Conversion;
using Sels.DistributedLocking.Memory;
using Sels.Core.Conversion.Extensions;
using Sels.Core.Mediator.Event;
using Sels.Core.Mediator;
using Sels.SQL.QueryBuilder;
using Sels.SQL.QueryBuilder.MySQL;
using Sels.Core.TestTool.ExportEntities;
using Sels.SQL.QueryBuilder.Extensions;
using Sels.SQL.QueryBuilder.Builder;

namespace Sels.Core.TestTool
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await Helper.Console.RunAsync(() =>
            {
                TestMySqlQueryProvider();
                return Task.CompletedTask;
            });
        }

        internal static void TestInterceptors()
        {
            LoggingServices.RegisterLogger(CreateLogger(LogLevel.Trace));

            var provider = new ServiceCollection()
                                .AddLogging(builder =>
                                {
                                    builder.ClearProviders();
                                    builder.AddSimpleConsole(options =>
                                    {
                                        options.IncludeScopes = true;
                                        options.SingleLine = true;
                                        options.TimestampFormat = "hh:mm:ss ";
                                    }).SetMinimumLevel(LogLevel.Trace);
                                })
                                .AddSingleton(Helper.Configuration.BuildDefaultConfigurationFile())
                                .AddMemoryCache()
                                .AddDistributedMemoryCache()
                                .New<IConfigurationService, ConfigurationService>()
                                    .Trace(x => x.Duration.OfAll)
                                    .CacheDistributed(x => x.Method(m => m.Get(default, default, default)))
                                    .Register()
                                .New<BaseTypeConverter, GuidConverter>()
                                    .Trace(x => x.Duration.OfAll)
                                    .Cache(x => x.Method(m => m.ConvertTo(default, default, default)))
                                    .Register()
                                .BuildServiceProvider();

            var service = provider.GetRequiredService<IConfigurationService>();
            var value = service.Get("Test");
            value = service.Get("Test");

            var converter = provider.GetRequiredService<BaseTypeConverter>();
            var guid = Guid.NewGuid();
            var stringGuid = converter.ConvertTo(guid, typeof(string));
            stringGuid = converter.ConvertTo(guid, typeof(string));
        }

        internal static void TestMessanger()
        {
            LoggingServices.RegisterLogger(CreateLogger(LogLevel.Information));

            var provider = new ServiceCollection()
                                .AddMessanger()
                                .BuildServiceProvider();

            var messageSubscriber = provider.GetRequiredService<IMessageSubscriber>();
            var messager = provider.GetRequiredService<IMessanger<string>>();

            var handler = new object();
            messageSubscriber.Subscribe<string>(handler, (s, m, t) =>
            {
                Console.WriteLine($"Received message <{m}>");
                return Task.CompletedTask;
            });

            var received = messager.SendAsync(new object(), "Hello from messager").Result;
            Console.WriteLine($"Message received by <{received}> subscribers");
            messageSubscriber.Unsubscribe<string>(handler);
            received = messager.SendAsync(new object(), "Second message from messager").Result;
            Console.WriteLine($"Message received by <{received}> subscribers");
        }

        internal static async Task TestNotifier()
        {
            var provider = new ServiceCollection()
                                .AddLogging(builder =>
                                {
                                    builder.ClearProviders();
                                    builder.AddSimpleConsole(options =>
                                    {
                                        options.IncludeScopes = true;
                                        options.SingleLine = true;
                                        options.TimestampFormat = "hh:mm:ss ";
                                    }).SetMinimumLevel(LogLevel.Debug);
                                })
                                .AddEventListener(async (p, c, e, t) =>
                                {
                                    var logger = p.GetService<ILogger<Program>>();
                                    logger.Log($"Global waiting to commit for event <{e}>");
                                    await c.WaitForCommitAsync();
                                    logger.Log($"Global commited event transaction");
                                })
                                .AddEventListener<string>(async (p, c, e, t) =>
                                {
                                    var logger = p.GetService<ILogger<Program>>();
                                    logger.Log($"Waiting to commit for event <{e}>");
                                    await c.WaitForCommitAsync();
                                    logger.Log($"Commited event transaction");
                                })
                                .AddNotifier()
                                .BuildServiceProvider();

            var logger = provider.GetService<ILogger<Program>>();
            var eventSubscriber = provider.GetRequiredService<IEventSubscriber>();
            var notifier = provider.GetRequiredService<INotifier>();

            var handler = new object();
            eventSubscriber.Subscribe<string>(handler, (c, e, t) =>
            {
                logger.Log($"Received message <{e}>");
                return Task.CompletedTask;
            });

            var received = await notifier.RaiseEventAsync(new object(), "Hello from program");
            logger.Log($"Message received by <{received}> subscribers");
            eventSubscriber.Unsubscribe<string>(handler);
            received = await notifier.RaiseEventAsync(new object(), "Second message from program");
            logger.Log($"Message received by <{received}> subscribers");
            eventSubscriber.Unsubscribe(handler);
        }

        internal static void TestEnvironmentParser()
        {
            Environment.SetEnvironmentVariable("ConnectionString.Database", "localhost");
            Environment.SetEnvironmentVariable("ConnectionString.Port", "3039");
            Environment.SetEnvironmentVariable("ConnectionString.Username", "jenssels");
            Environment.SetEnvironmentVariable("ConnectionString.Password", "mysupersecretpassword");
            Environment.SetEnvironmentVariable("ConnectionString.SslMode", "Required");

            var connectionString = Deploy.Environment.ParseFrom<ConnectionString>(x => x.UsePrefix("ConnectionString"));

            Console.WriteLine($"Parsed <{connectionString}> from environment variables");
        }

        internal static void TestAutofacServiceFactory()
        {
            // Setup container
            var collection = new ServiceCollection()
                            .AddAutofacServiceFactory()
                            .AddConfigurationFromDirectory()
                            .RegisterConfigurationService()
                            .AddLogging(l =>
                            {
                                l.ClearProviders();
                                l.AddSimpleConsole(options =>
                                {
                                    options.IncludeScopes = true;
                                    options.SingleLine = true;
                                    options.TimestampFormat = "hh:mm:ss ";
                                }).SetMinimumLevel(LogLevel.Trace);
                            });

            var builder = new ContainerBuilder();
            builder.RegisterType<JsonConverter>().Named<ITypeConverter>("Json");
            builder.RegisterType<XmlConverter>().Named<ITypeConverter>("Xml");
            builder.Populate(collection);
            var container = builder.Build();

            var factory = container.Resolve<IServiceFactory>();
            var configurationProvider = factory.Resolve<IConfigurationService>();

            using (var scope = factory.Resolve<IServiceFactoryScope>())
            {
                var xmlImplementer = factory.GetImplementerFor<ITypeConverter>("Xml");
                var xmlConverter = factory.Resolve<ITypeConverter>("Xml");
            }

            using (var scope = factory.CreateScope())
            {
                var jsonImplementer = factory.GetImplementerFor<ITypeConverter>("Json");
                var jsonConverter = factory.Resolve<ITypeConverter>("Json");
            }

            var allConverters = factory.ResolveAll<ITypeConverter>();
        }

        internal static void TestMemoryLockingProviderOptions()
        {
            // Setup container
            var provider = new ServiceCollection()
                            .AddMemoryLockingProvider(x => x.ExpiryOffset = 1000)
                            .AddConfigurationFromDirectory()
                            .RegisterConfigurationService()
                            .AddLogging(l =>
                            {
                                l.ClearProviders();
                                l.AddSimpleConsole(options =>
                                {
                                    options.IncludeScopes = true;
                                    options.SingleLine = true;
                                    options.TimestampFormat = "hh:mm:ss ";
                                }).SetMinimumLevel(LogLevel.Debug);
                            })
                            .BuildServiceProvider();
            var memoryLockingProvider = provider.GetRequiredService<ILockingProvider>().CastTo<MemoryLockingProvider>();
            var options = memoryLockingProvider.OptionsMonitor.CurrentValue;
            Console.WriteLine(options.SerializeAsJson());
        }

        internal static void TestMySqlQueryProvider()
        {
            var provider = new ServiceCollection()
                               .AddMySqlQueryProvider()
                               .AddCachedMySqlQueryProvider()
                               .AddMemoryCache()
                               .AddLogging(l =>
                               {
                                   l.ClearProviders();
                                   l.AddSimpleConsole(options =>
                                   {
                                       options.IncludeScopes = true;
                                       options.SingleLine = true;
                                       options.TimestampFormat = "hh:mm:ss ";
                                   }).SetMinimumLevel(LogLevel.Trace);
                               })
                               .BuildServiceProvider();

            var sqlQueryProvider = provider.GetRequiredService<ISqlQueryProvider>();
            var cachedQueryProvider = provider.GetRequiredService<ICachedSqlQueryProvider>();
            var logger = provider.GetService<ILogger<Program>>();
            var selectQuery = sqlQueryProvider.Select<Person>()
                                                    .Column(p => p.FirstName)
                                                    .Column(p => p.LastName)
                                              .Where(w => w.Column(p => p.JobId).EqualTo.Value(5))
                                              .Build();
            logger.Log(selectQuery);
            var builderAction = new Func<ISqlQueryProvider, IQueryBuilder>(b =>
            {
                return b.With().Cte("Cte")
                                .Using(b.Select<Person>()
                                        .OrderBy(p => p.LastName)
                                        .Limit(5))
                               .Execute(b.Update<Person>()
                                         .Set.Column(p => p.JobId).To.Null()
                                         .InnerJoin().Table("CTE", datasetAlias: "C").On(x => x.Column(p => p.LastName).EqualTo.Column("C", "LastName")));
            });

            var updateQuery = cachedQueryProvider.GetQuery("Person.Update", builderAction);
            updateQuery = cachedQueryProvider.GetQuery("Person.Update", builderAction);
            logger.Log(selectQuery);
        }

        internal static ILogger CreateLogger(LogLevel level = LogLevel.Trace)
        {
            return LoggerFactory.Create(builder =>
            {
                builder.ClearProviders();
                builder.AddSimpleConsole(options =>
                {
                    options.IncludeScopes = true;
                    options.SingleLine = true;
                    options.TimestampFormat = "hh:mm:ss ";
                }).SetMinimumLevel(level);
            }).CreateLogger("Console");
        }
    }
}
