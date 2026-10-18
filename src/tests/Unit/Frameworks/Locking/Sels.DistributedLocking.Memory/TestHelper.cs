using Microsoft.Extensions.Options;

namespace Sels.DistributedLocking.Memory.Tests
{
    /// <summary>Creates the shared test doubles of the memory locking provider tests.</summary>
    internal static class TestHelper
    {
        /// <summary>Creates a substituted options monitor that returns options with cleanup disabled by default.</summary>
        /// <param name="setter">Optional delegate that modifies the options returned by the monitor.</param>
        /// <returns>The substituted options monitor.</returns>
        public static IOptionsMonitor<MemoryLockingProviderOptions> CreateProviderOptions(Action<MemoryLockingProviderOptions>? setter = null)
        {
            var options = Substitute.For<IOptionsMonitor<MemoryLockingProviderOptions>>();
            var instance = new MemoryLockingProviderOptions()
            {
                CleanupInterval = TimeSpan.Zero
            };
            setter?.Invoke(instance);
            options.CurrentValue.Returns(instance);
            return options;
        }
    }
}
