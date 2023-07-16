using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.DistributedLocking.Memory.Test
{
    internal static class TestHelper
    {
        public static IOptionsMonitor<SqlLockingProviderOptions> GetProviderOptionsMock(Action<SqlLockingProviderOptions>? setter = null)
        {
            var mock = new Mock<IOptionsMonitor<SqlLockingProviderOptions>>();
            var instance = new SqlLockingProviderOptions()
            {
                MaintenanceInterval = TimeSpan.Zero
            };
            setter?.Invoke(instance);
            mock.Setup(x => x.CurrentValue).Returns(instance);
            return mock.Object;
        }

        public static Mock<ISqlLockRepository> GetRepositoryMock(Action<Mock<ISqlLockRepository>> configurator)
        {
            var repositoryMock = new Mock<ISqlLockRepository>();
            var transactionMock = new Mock<IRepositoryTransaction>();
            transactionMock.Setup(x => x.DisposeAsync()).Returns(ValueTask.CompletedTask);
            transactionMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            repositoryMock.Setup(x => x.CreateTransactionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(() => transactionMock.Object);

            configurator?.Invoke(repositoryMock);
            return repositoryMock;
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
