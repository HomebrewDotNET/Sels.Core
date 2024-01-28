using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Mediator.Test
{
    internal static class TestHelper
    {
        public static IServiceProvider GetTestContainer(Action<IServiceCollection>? configurator = null)
        {
            var collection = new ServiceCollection()
                                 .AddNotifier()
                                 .AddMemoryCache();

            configurator?.Invoke(collection);
            return collection.BuildServiceProvider();
        }
    }
}
