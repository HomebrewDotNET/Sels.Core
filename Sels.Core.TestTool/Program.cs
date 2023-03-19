using System;
using Microsoft.Extensions.Logging;
using Sels.Core.Components.Logging;
using Microsoft.Extensions.DependencyInjection;
using Sels.Core.Mediator.Messaging;
using System.Threading.Tasks;
using Sels.Core.Contracts.Configuration;
using Sels.Core.Components.Configuration;
using Sels.Core.Deployment;
using Sels.Core.Data.MySQL.Models;
using Sels.Core.Conversion.Templates;
using Sels.Core.Conversion.Converters.Simple;

namespace Sels.Core.TestTool
{
    class Program
    {
        static void Main(string[] args)
        {
            Helper.Console.Run(() => {
                TestInterceptors();
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
