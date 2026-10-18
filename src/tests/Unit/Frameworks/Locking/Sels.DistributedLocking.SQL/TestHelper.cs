using Microsoft.Extensions.Options;
using Sels.Core.Async.TaskManagement;
using Sels.Core.Mediator;
using Sels.Core.Mediator.Event;

namespace Sels.DistributedLocking.SQL.Tests
{
    /// <summary>Creates the shared test doubles of the SQL locking provider tests.</summary>
    internal static class TestHelper
    {
        /// <summary>Creates a substituted options monitor that returns options with maintenance and cleanup disabled by default.</summary>
        /// <param name="setter">Optional delegate that modifies the options returned by the monitor.</param>
        /// <returns>The substituted options monitor.</returns>
        public static IOptionsMonitor<SqlLockingProviderOptions> CreateProviderOptions(Action<SqlLockingProviderOptions>? setter = null)
        {
            var options = Substitute.For<IOptionsMonitor<SqlLockingProviderOptions>>();
            var instance = new SqlLockingProviderOptions()
            {
                MaintenanceInterval = TimeSpan.FromMinutes(1),
                IsCleanupEnabled = false
            };
            setter?.Invoke(instance);
            options.CurrentValue.Returns(instance);
            return options;
        }

        /// <summary>Creates a substituted repository whose transactions can be created, committed and disposed.</summary>
        /// <param name="configurator">Optional delegate that configures the substituted repository further.</param>
        /// <returns>The substituted repository.</returns>
        public static ISqlLockRepository CreateRepository(Action<ISqlLockRepository>? configurator = null)
        {
            var repository = Substitute.For<ISqlLockRepository>();
            var transaction = Substitute.For<IRepositoryTransaction>();
            transaction.DisposeAsync().Returns(ValueTask.CompletedTask);
            transaction.CommitAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
            repository.CreateTransactionAsync(Arg.Any<CancellationToken>()).Returns(_ => Task.FromResult(transaction));

            configurator?.Invoke(repository);
            return repository;
        }

        /// <summary>Creates a substituted notifier.</summary>
        /// <returns>The substituted notifier.</returns>
        public static INotifier CreateNotifier()
        {
            return Substitute.For<INotifier>();
        }

        /// <summary>Creates a substituted event subscriber.</summary>
        /// <returns>The substituted event subscriber.</returns>
        public static IEventSubscriber CreateSubscriber()
        {
            return Substitute.For<IEventSubscriber>();
        }

        /// <summary>Creates a substituted task manager.</summary>
        /// <returns>The substituted task manager.</returns>
        public static ITaskManager CreateTaskManager()
        {
            return Substitute.For<ITaskManager>();
        }
    }
}
