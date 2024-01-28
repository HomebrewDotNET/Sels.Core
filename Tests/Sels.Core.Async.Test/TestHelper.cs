using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Async.Test
{
    internal static class TestHelper
    {
        public static ServiceProvider GetTaskManagerContainer(Action<TaskManagerOptions>? configurator = null)
        {
            var collection = new ServiceCollection()
                                 //.AddLogging(x => x.SetMinimumLevel(LogLevel.Debug).AddConsole())
                                 .AddTaskManager(configurator);

            return collection.BuildServiceProvider();
        }
    }
}
