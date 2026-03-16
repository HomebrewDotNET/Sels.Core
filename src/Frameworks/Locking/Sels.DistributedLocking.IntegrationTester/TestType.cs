using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.DistributedLocking.IntegrationTester
{
    /// <summary>
    /// Dictates which tests to execute.
    /// </summary>
    [Flags]
    public enum TestType
    {
        /// <summary>
        /// Run all tests.
        /// </summary>
        All = Functional | Concurrency | Benchmark,
        /// <summary>
        /// Runs a test on all features of the locking provider.
        /// </summary>
        Functional = 1,
        /// <summary>
        /// Runs a test to test concurrency on the locks.
        /// </summary>
        Concurrency = 2,
        /// <summary>
        /// Runs performance tests.
        /// </summary>
        Benchmark = 4
    }
}
