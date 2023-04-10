using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Locking.Memory.Test
{
    internal static class TestHelper
    {
        public static IOptions<MemoryLockingProviderOptions> GetProviderOptionsMock(Action<MemoryLockingProviderOptions>? setter = null)
        {
            var mock = new Mock<IOptions<MemoryLockingProviderOptions>>();
            var instance = new MemoryLockingProviderOptions()
            {
                CleanupInterval = null
            };
            setter?.Invoke(instance);
            mock.Setup(x => x.Value).Returns(instance);
            return mock.Object;
        }

        public static ILogger<T> GetDebugLogger<T>()
        {
            return LoggerFactory.Create(x =>
            {
                x.SetMinimumLevel(LogLevel.Trace);
                x.AddConsole();
            }).CreateLogger<T>();
        }
    }
}
