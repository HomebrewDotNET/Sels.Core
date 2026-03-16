using Sels.DistributedLocking.Provider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.DistributedLocking.IntegrationTester
{
    /// <summary>
    /// Executes tests on a <see cref="ILockingProvider"/>.
    /// </summary>
    public interface ITester
    {
        /// <summary>
        /// Executes tests with <paramref name="lockingProvider"/>.
        /// </summary>
        /// <param name="provider">The provider being tested</param>
        /// <param name="lockingProvider">The provider to use in the tests</param>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>True if all tests executed successfully, otherwise false</returns>
        Task<bool> RunTests(TestProvider provider, ILockingProvider lockingProvider, CancellationToken token);
    }
}
