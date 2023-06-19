using Microsoft.Extensions.DependencyInjection;
using Sels.Core.Components.Scope;
using Sels.DistributedLocking.Provider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.DistributedLocking.IntegrationTester
{
    /// <summary>
    /// Responsible for setting up the <see cref="ILockingProvider"/> for a test run.
    /// </summary>
    public interface ISetupProvider
    {
        /// <summary>
        /// Returns the locking provider to use for testing and sets up the environment for a test run (Cleanup old data, ...)
        /// </summary>
        /// <param name="services">Service collection that can be used to create the provider</param>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>The locking provider to use</returns>
        public Task<AsyncWrapper<ILockingProvider>> SetupProvider(IServiceCollection services, CancellationToken token);
    }
}
