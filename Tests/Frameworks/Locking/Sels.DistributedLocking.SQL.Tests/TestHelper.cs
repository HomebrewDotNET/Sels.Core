using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Sels.Core.Mediator;
using Sels.Core.Mediator.Event;
using Sels.Core.Mediator.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.DistributedLocking.SQL.Test
{
    internal static class TestHelper
    {
        public static IOptionsMonitor<SqlLockingProviderOptions> GetProviderOptionsMock(Action<SqlLockingProviderOptions>? setter = null)
        {
            var mock = new Mock<IOptionsMonitor<SqlLockingProviderOptions>>();
            var instance = new SqlLockingProviderOptions()
            {
                MaintenanceInterval = TimeSpan.FromMinutes(1),
                IsCleanupEnabled = false
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

        public static Mock<INotifier> GetNotifierMock()
        {
            var notifierMock = new Mock<INotifier>();
            return notifierMock;
        }

        public static Mock<IEventSubscriber> GetSubscriberMock()
        {
            var subscriberMock = new Mock<IEventSubscriber>();
            return subscriberMock;
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
