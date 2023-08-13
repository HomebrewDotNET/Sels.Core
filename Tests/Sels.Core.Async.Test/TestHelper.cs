using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Async.Test
{
    internal static class TestHelper
    {
        public static IServiceProvider GetTaskManagerContainer(Action<TaskManagerOptions>? configurator = null)
        {
            var collection = new ServiceCollection()
                                 .AddTaskManager(configurator);

            return collection.BuildServiceProvider();
        }
    }
}
