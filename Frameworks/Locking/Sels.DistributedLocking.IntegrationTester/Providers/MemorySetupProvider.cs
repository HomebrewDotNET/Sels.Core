using Microsoft.Extensions.DependencyInjection;
using Sels.Core.Components.Scope;
using Sels.DistributedLocking.Provider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.DistributedLocking.IntegrationTester.Providers
{
    /// <summary>
    /// Sets up Sels.DistributedLocking.Memory.
    /// </summary>
    public class MemorySetupProvider : ISetupProvider
    {
        /// <inheritdoc/>
        public Task<AsyncWrapper<ILockingProvider>> SetupProvider(IServiceCollection services, CancellationToken token)
        {
            var provider = services.AddMemoryLockingProvider().BuildServiceProvider();
            var lockingProvider = provider.GetRequiredService<ILockingProvider>();

            // In-memory store so no need to do cleanup

            return Task.FromResult(new AsyncWrapper<ILockingProvider>(lockingProvider, stopAction: async (p, t) => await provider.DisposeAsync()));
        }
    }
}
