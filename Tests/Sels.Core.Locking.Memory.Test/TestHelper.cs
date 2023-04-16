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
        public static IOptionsMonitor<MemoryLockingProviderOptions> GetProviderOptionsMock(Action<MemoryLockingProviderOptions>? setter = null)
        {
            var mock = new Mock<IOptionsMonitor<MemoryLockingProviderOptions>>();
            var instance = new MemoryLockingProviderOptions()
            {
                CleanupInterval = TimeSpan.Zero
            };
            setter?.Invoke(instance);
            mock.Setup(x => x.CurrentValue).Returns(instance);
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
