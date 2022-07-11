using Sels.Core.Extensions.Linq;
using Sels.Core.TestTool.Filter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Sels.Core.TestTool.ExportEntities;
using Sels.Core.Components.Parameters;
using Sels.Core.Components.Parameters.Parameters;
using Sels.Core.TestTool.Models;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Reflection;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using Sels.Core.Components.Logging;
using Sels.Core.Localization;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Sels.Core.Mediator.Messaging;
using System.Threading.Tasks;

namespace Sels.Core.TestTool
{
    class Program
    {
        static void Main(string[] args)
        {
            Helper.Console.Run(() => {
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
            });
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
